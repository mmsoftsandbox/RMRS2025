using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RMRS.ConsoleApp.DataModel;
using RMRS.ConsoleApp.Helpers;
using static RMRS.ConsoleApp.Helpers.UserInteractiveHelper;
using static RMRS.ConsoleApp.Helpers.StringHelper;

namespace RMRS.ConsoleApp
{
    public class ConsoleScreenBase : IConsoleScreen
    {
        public virtual MenuData InitMenu(List<MenuData.MenuItem> menuTree)
        {
            throw new NotImplementedException();
        }

        public virtual void PrintMenu(MenuData menu)
        {
            Console.WriteLine();
            ShowMessageHeader(menu.MenuTitle);
            if (string.IsNullOrWhiteSpace(menu.Hint))
            {
                ShowMessageHint(menu.Hint);
            }
            Console.WriteLine();

            if (menu.MenuTree?.Count > 0)
            {
                foreach (var item in menu.MenuTree)
                {
                    Console.WriteLine($"{item.ActionId}. {item.Prompt}");
                }
            }
        }

        public virtual async Task RunMenuLoop(MenuData menu, CancellationToken ct = default)
        {
            if (menu.MenuTree == null)
            {
                Console.WriteLine(menu.MenuTitle);
                Console.WriteLine();
                Console.WriteLine("Нет опций в меню");

                return;
            }

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    // Можем обрабатывать при необходимости
                    //break;
                }

                menu.SelectedIndex = -1;
                PrintMenu(menu);

                Console.WriteLine();
                var option = GetMenuChoice(menu.Prompt);
                option--;

                Console.WriteLine();

                if (option < 0 || menu.MenuTree?.Count < option + 1)
                {
                    Console.WriteLine(menu.WrongPrompt);
                }
                else
                {
                    menu.SelectedIndex = option;
                    try
                    {
                        var action = menu.MenuTree?[option]?.Operation;
                        if (action != null)
                        {
                            Console.WriteLine(); // Отступ перед output опции

                            var _result = await action.Invoke();
                            if (_result?.Exit ?? false)
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        PressAnyKeyOrEsc();
                    }
                }
            }
        }

        public void PrintPage(List<string> lines)
        {
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }
        public void PrintTable(List<List<object?>> rows, Dictionary<string, (int Width, string Title)> spec)
        {
            var lines = new List<string>();
            var lineData = new List<string>();
            var columns = spec.Keys.ToArray();
            foreach (var row in rows)
            {
                lineData = row.Select((x, i) => PadLeft(x, spec[columns[i]].Width))
                .ToList();
                lines.Add(string.Join('|', lineData));
            }

            // Шапка
            lineData = spec
                .Select(x => PadLeft(x.Value.Title, x.Value.Width))
                .ToList();
            lines.Insert(0, string.Join('|', values: lineData));

            PrintPage(lines);
        }

        public virtual void ShowMessage(string? value = default)
        {
            Console.WriteLine(value);
        }

        public virtual void ShowMessageColoured(string? value, ConsoleColor? color = null)
        {
            if (color == null)
            {
                ShowMessage(value);

            }
            else
            {
                var current = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                Console.WriteLine(value);
                Console.ForegroundColor = current;
            }
        }

        public virtual void ShowMessageHeader(string? value = default)
        {
            ShowMessageColoured(value, ConsoleColor.Green);
        }

        public virtual void ShowMessageError(string? value = default)
        {
            ShowMessageColoured(value, ConsoleColor.Red);
        }

        public virtual void ShowMessageWarn(string? value = default)
        {
            ShowMessageColoured(value, ConsoleColor.Yellow);
        }

        public virtual void ShowMessageHint(string? value = default)
        {
            ShowMessageColoured(value, ConsoleColor.Gray);
        }

        public virtual int WindowHeight
        {
            get
            {
                var widht = GetConsoleHeight();
                return widht > 0 ? widht : 40;
            }
        }

        /// <summary>
        /// Количество строк в отображаемой таблице при пагинации.
        /// Уменьшаем на служебные строки:
        /// 4 дополнительные строки: Заголовок + Шапка + пустая + "Press Any Key"
        /// </summary>
        public virtual int MaxTableRowsCount { get => Math.Max(WindowHeight - 4, 1); set => new NotImplementedException(); }

        public virtual int GetMenuChoice(string prompt = ": ")
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var input))
            {
                return input;
            }

            return -1;
        }

        /// <summary>
        /// Подсказка по клавишам навигации меню
        /// </summary>
        public virtual string HintDefault { get; } = "";

        /// <summary>
        /// Обработчик выхода из меню. По умолчанию.
        /// </summary>
        public virtual Func<Task<OperationResult?>> DoMenuExitDefault => async () =>
        {
            Console.WriteLine("...");
            await Task.Delay(1000);
            Console.WriteLine();
            return new OperationResult { Exit = true };
        };
    }
}
