using System;
using System.Collections.Generic;

namespace RMRS.ConsoleApp
{
    /// <summary>
    /// Интерфейс модели работы с экраном. 
    /// Реализует некий стиль работы с экраном, с меню, а также способом отрисовки данных.
    /// Например, Меню может управляться стрелками или пункты выбираться вводом номера пункта меню.
    /// Так же, вывод на экран может рассматриваться бесконечной лентой выводимых строк, 
    /// или же подразумеваться, как обновляемая область вывода фиксированного размера совпадающего с размером окна консолию 
    /// </summary>
    public interface IConsoleScreen
    {
        /// <summary>
        /// Инициалицация данных меню.
        /// Зареервировано для конфигурирования фреймверков по работе с консольным меню
        /// </summary>
        /// <param name="menuTree"></param>
        /// <returns></returns>
        MenuData InitMenu(List<MenuData.MenuItem> menuTree);

        /// <summary>
        /// Отрисовать меню 
        /// </summary>
        /// <param name="menu"></param>
        public void PrintMenu(MenuData menu);

        /// <summary>
        /// Отрисовать меню и запустить цикл выбора и выполнения операций  
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        Task RunMenuLoop(MenuData menu, CancellationToken ct = default);

        /// <summary>
        /// Отрисовка порции строк при пагинации 
        /// </summary>
        /// <param name="lines">Текст построчно</param>
        void PrintPage(List<string> lines);

        /// <summary>
        /// Отрисовка таблицы 
        /// </summary>
        /// <param name="rows">Построчные данные</param>
        /// <param name="spec">Спецификациия колонок</param>
        public void PrintTable(List<List<object?>> rows, Dictionary<string, (int Width, string Title)> spec);

        /// <summary>
        /// Отобразить сообщение
        /// </summary>
        /// <param name="text"></param>
        void ShowMessage(string? text = default);

        int WindowHeight { get; }

        /// <summary>
        /// Количество строк в отображаемой таблице при пагинации.
        /// </summary>
        int MaxTableRowsCount { get; protected set; }

        /// <summary>
        /// Интерактивно получить от польователя выбор из меню.
        /// В различных фреймверках по работе с консольным меню система навигации может различаться
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        int GetMenuChoice(string prompt);

        /// <summary>
        /// Подсказка по клавишам навигации меню
        /// </summary>
        string HintDefault { get; }

        /// <summary>
        /// Обработчик выхода из меню. По умолчанию.
        /// </summary>
        Func<Task<OperationResult?>> DoMenuExitDefault { get; }
    }
}