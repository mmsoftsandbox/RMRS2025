using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMRS.ConsoleApp
{
    /// <summary>
    /// Класс реалиующий логическую структуру меню.
    /// Не содержит реалиацию отрисовки и навигации.
    /// </summary>
    public class MenuData
    {
        /// <summary>
        /// Пункт меню 
        /// </summary>
        public class MenuItem
        {
            /// <summary>
            /// Идентификатор пункта меню
            /// </summary>
            public int ActionId { get; set; }

            /// <summary>
            /// Строка меню
            /// </summary>
            public string? Prompt { get; set; }

            /// <summary>
            /// Действие-делегат,
            /// Вовращает реультат действия для воможной обработки
            /// </summary>
            public Func<Task<OperationResult?>>? Operation { get; set; }
        }

        /// <summary>
        /// Заголовок меню
        /// </summary>
        public string MenuTitle { get; set; } = string.Empty;

        /// <summary>
        /// Список пунктов меню 
        /// </summary>
        public List<MenuItem> MenuTree { get; set; } = new List<MenuItem>();

        // TO DO Закрыть сеттер
        /// <summary>
        /// Выбранный пункт меню
        /// </summary>
        public int SelectedIndex { get; set; }

        /// <summary>
        /// Начально установленный пункт меню
        /// </summary>
        public int InitiallySelectedIndex { get; set; }

        /// <summary>
        /// Приглашение для ввода
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Сообщение при ошибочном выборе меню
        /// </summary>
        public string WrongPrompt { get; set; } = string.Empty;

        /// <summary>
        /// Подсказка по клавишам
        /// </summary>
        public string Hint { get; set; } = string.Empty;

        /// <summary>
        /// Маркер-указатель активного пункта
        /// </summary>
        public string? SelectedMarker { get; set; }
    }

    /// <summary>
    /// Реультат действия меню, испольуется, например для аналиа выхода из цикла обработки меню
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Флаг выхода из цикла обработки меню
        /// </summary>
        public bool Exit { get; set; }

        /// <summary>
        /// Статус выполнения пункта меню
        /// Здесь реалиован для демо-целей
        /// </summary>
        public bool Success { get; set; }
    }
}
