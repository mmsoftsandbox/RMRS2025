using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMRS.ConsoleApp
{
    public class MenuData
    {
        public class MenuItem
        {
            public int ActionId { get; set; }
            public string? Prompt { get; set; }
            public Func<Task<OperationResult?>>? Operation { get; set; }
        }

        public string MenuTitle { get; set; } = string.Empty;

        public List<MenuItem> MenuTree { get; set; } = new List<MenuItem>();

        // TO DO Закрыть сеттер
        public int SelectedIndex { get; set; }

        public int InitiallySelectedIndex { get; set; }

        public string Prompt { get; set; } = string.Empty;

        public string WrongPrompt { get; set; } = string.Empty;

        /// <summary>
        /// Подсказка по клавишам
        /// </summary>
        public string Hint { get; set; } = string.Empty;

        public string? SelectedMarker { get; set; }
    }

    public class OperationResult
    {
        public bool Exit { get; set; }
        public bool Success { get; set; }
    }
}
