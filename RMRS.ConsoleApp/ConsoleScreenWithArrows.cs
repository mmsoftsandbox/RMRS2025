using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RMRS.ConsoleApp.Helpers;
using static RMRS.ConsoleApp.Helpers.UserInteractiveHelper;

namespace RMRS.ConsoleApp
{
    /// <summary>
    /// Модел работы с экраном. 
    /// Реализует Меню с управлением стрелками
    /// </summary>
    public class ConsoleScreenWithArrows : ConsoleScreenBase
    {
        private const string _selectedMarker = ">>> ";

        public override void PrintMenu(MenuData menu)
        {
            
            var selectedMarker = menu.SelectedMarker ?? _selectedMarker;
            var indentString = new string(' ', selectedMarker.Length);


            Console.Clear();

            ShowMessageHeader(menu.MenuTitle);
            if (!string.IsNullOrWhiteSpace(menu.Hint))
            {
                ShowMessageHint(menu.Hint);
            }
            Console.WriteLine();

            foreach (var item in menu.MenuTree)
            {
                if (item == menu.MenuTree[menu.SelectedIndex])
                {
                    Console.Write(selectedMarker);
                }
                else
                {
                    Console.Write(indentString);
                }

                Console.WriteLine(item.Prompt);
            }
        }

        public override async Task RunMenuLoop(MenuData menu, CancellationToken ct = default)
        {
            if (menu.MenuTree == null)
            {
                Console.WriteLine(menu.MenuTitle);
                Console.WriteLine();
                Console.WriteLine("Нет опций в меню");

                return;
            }

            ConsoleKeyInfo keyinfo;
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    // Можем обрабатывать при необходимости
                    //break;
                }

                PrintMenu(menu);
                keyinfo = Console.ReadKey();

                // Handle each key input (down arrow will write the menu again with a different selected item)
                if (keyinfo.Key == ConsoleKey.DownArrow)
                {
                    if (menu.SelectedIndex + 1 < menu.MenuTree?.Count)
                    {
                        menu.SelectedIndex++;
                        continue;
                    }
                }
                if (keyinfo.Key == ConsoleKey.UpArrow)
                {
                    if (menu.SelectedIndex - 1 >= 0)
                    {
                        menu.SelectedIndex--;
                        continue;
                    }
                }

                // Handle different action for the option
                if (keyinfo.Key == ConsoleKey.Enter)
                {
                    try
                    {
                        var action = menu.MenuTree?[menu.SelectedIndex]?.Operation;
                        // Обнуляем не здесь, а после ввполнения операции или вначале while(),
                        // т.к. menu.SelectedIndex испольуется в замыкании в делегате Operation()
                        //menu.SelectedIndex = 0;

                        if (action != null)
                        {
                            Console.WriteLine(); // Отступ перед output опции

                            var result = await action.Invoke();
                            menu.SelectedIndex = 0;
                            if (result?.Exit ?? false)
                            {
                                return;
                            }
                        }
                        menu.SelectedIndex = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        PressAnyKeyOrEsc();
                    }
                }
            }
        }

        /// <summary>
        /// Подсказка по клавишам навигации меню
        /// </summary>
        public override string HintDefault { get; } = "Для навигации используйте стрелки Вверх и Вниз, а для выбора <Enter>";

        /// <summary>
        /// Отрисовка таблицы. 
        /// Значения в строках перечислены через запятую, без выравнивания по ширине
        /// </summary>
        /// <param name="rows">Построчные данные</param>
        /// <param name="spec">Спецификациия колонок</param>
        public override void PrintTable(List<List<object?>> rows, Dictionary<string, (int Width, string Title)> spec)
        {
            var lines = new List<string>();
            var lineData = new List<string>();
            var columns = spec.Keys.ToArray();
            foreach (var row in rows)
            {
                lineData = row.Select(x => x == null ? "N/A" : x?.ToString()?.Trim() ?? string.Empty).ToList();
                lines.Add(string.Join(", ", lineData));
            }

            // Шапка
            lineData = spec
                .Select(x => x.Value.Title).ToList();
            lines.Insert(0, string.Join(", ", values: lineData));

            PrintPage(lines);
        }

    }
}