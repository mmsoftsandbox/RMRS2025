using System;
using System.Configuration;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RMRS.ConsoleApp.Helpers;
using Microsoft.Data.SqlClient;
using RMRS.ConsoleApp.Context;

namespace RMRS.ConsoleApp
{
    class Program
    {
        // Mutex для проверки на повторный запуск приложения 
        static Mutex? InstanceMutex;

        // Конфигурируем
        private static IConfiguration Configuration => new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Использовать EF
        private static bool UseEntityFramework => Configuration.GetValue<bool>("UseEntityFramework");

        private static void ConfigureServices(IServiceCollection services)
        {
            if (UseEntityFramework)
            {

                services.AddDbContext<EmployeeDBContext>(options =>
                {
                    options.UseSqlServer(
                        Configuration.GetValue<String>("ConnectionStrings:EmployeeDBConnectionString"),
                        b =>
                        {
                            b.EnableRetryOnFailure();
                            var commandTimeout = Configuration.GetValue<int>("CommandTimeout");
                            b.CommandTimeout(commandTimeout > 0 ? commandTimeout : 50);
                        }
                    );

                });

                services.AddTransient<IDataLayer, DataLayerEF>();
            }
            else // Использовать SqlClient
            {
                services.AddTransient(provider =>
                {
                    return new SqlConnection(Configuration.GetValue<String>("ConnectionStrings:EmployeeDBConnectionString"));
                });

                services.AddTransient<IDataLayer, DataLayerSql>();
            }

            services.AddSingleton<BusinessLogic>();
            // Transient - Просто некий функционал, не требует некой затратной инициалиации, не хранит состояний
            //services.AddTransient<IConsoleScreen, ConsoleScreenBase>();
            services.AddTransient<IConsoleScreen, ConsoleScreenWithArrows>();
        }

        private static readonly CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// Точка входа в приложение
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            // Welcome
            Console.WriteLine("Запуск программы ...");

            // Проверка на повторный запуск приложения 
            if (CommonHelper.GetInstanceCheck(ref InstanceMutex, AppDomain.CurrentDomain.FriendlyName))
            {
                Console.WriteLine("Ошибка: Запущена другая копия приложения, повторный запуск копии не воможен. Нажмите любую клавишу ...");
                Console.ReadKey();

                return;
            }

            // В данном приложении не обрабатывается, но можем реалиовать при необходимости
            var cts = new CancellationTokenSource();

            // Собираем DI контейнер
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // awaitable, disposable 
            await using var serviceProvider = serviceCollection.BuildServiceProvider();

            // Блокировка клавиш выключения приложения и событий закрытия приложения
            // Только для Windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Полностью удаляем из меню окна приложения пункт закрытия и блокируем крестик
                // На самом деле, при Alt+F4 система всё равно выгружает выполняемый процесс
                QuitBlocker.BlockCloseMethods();
            }
            // Возникает при нажатии Ctrl+C или Ctrl+Break
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Блокируем стандартное поведение
                Console.WriteLine();
                //Console.WriteLine("Прервано сочетанием клавиш Ctrl+C. Завершение работы...");
                Console.WriteLine("Прерывание работы по Ctrl+C и Ctrl+Break заблокировано");
                Console.WriteLine("Для выхода из приложения выберите пункт меню");
                //cts.Cancel();
            };

            // Поддержка ввода восточных языков, и пр.
            // Также необходим выбор соответствующего шрифта
            if (Configuration.GetValue<bool>("UseUnicode"))
            {
                Console.OutputEncoding = System.Text.Encoding.Unicode;
                Console.InputEncoding = System.Text.Encoding.Unicode;
            }

            try
            {
                Console.WriteLine("Проверка подключения ...");
                var _dataLayer = serviceProvider.GetRequiredService<IDataLayer>();
                await _dataLayer.EnsureConnection();

                // Стартуем
                var businessLogic = serviceProvider.GetRequiredService<BusinessLogic>();
                await businessLogic.RunAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Нажмите любую клавишу ...");
                Console.ReadKey();
            }
            finally
            {
                Console.WriteLine("Освобождаем ресурсы ...");
                // ServiceProvider автоматически вызывает Dispose для всех IDisposable в DI контейнере
                cts.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("Работа завершена ...");
            await Task.Delay(1000);
        }
    }
}
