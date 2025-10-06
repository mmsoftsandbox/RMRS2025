using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RMRS.ConsoleApp.Helpers
{
    public static class QuitBlocker
    {
        [DllImport("kernel32.dll")]
        private static extern nint GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern nint GetSystemMenu(nint hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool DeleteMenu(nint hMenu, uint uPosition, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(nint hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

        [DllImport("user32.dll")]
        private static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint Msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(nint hWnd, int nIndex);

        private const int GWL_WNDPROC = -4;
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint SC_CLOSE = 0xF060;
        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;

        private static nint originalWndProc;

        private delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

        private static readonly WndProcDelegate wndProcDelegate = new WndProcDelegate(CustomWndProc);

        private static nint CustomWndProc(nint hWnd, uint msg, nint wParam, nint lParam)
        {
            // Блокируем команду закрытия
            if (msg == WM_SYSCOMMAND && wParam.ToInt64() == SC_CLOSE)
            {
                Console.WriteLine();
                Console.WriteLine("Предотвращена попытка закрытия через Alt+F4/крестик");
                return nint.Zero; // Блокируем сообщение
            }

            // Передаем все остальные сообщения стандартному обработчику
            return CallWindowProc(originalWndProc, hWnd, msg, wParam, lParam);
        }

        /// <summary>
        /// Удаляем пункт закрытия и крестик
        /// На самом деле, при Alt+F4 система всё равно выгружает выполняемый процесс
        /// </summary>
        private static void InstallHook()
        {
            nint consoleWindow = GetConsoleWindow();
            originalWndProc = SetWindowLongPtr(consoleWindow, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(wndProcDelegate));
        }

        public static void BlockCloseMethods()
        {
            if (System.Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return;
            }

            nint consoleWindow = GetConsoleWindow();

            if (consoleWindow != nint.Zero)
            {
                nint systemMenu = GetSystemMenu(consoleWindow, false);

                if (systemMenu != nint.Zero)
                {
                    // Полностью удаляем пункт закрытия (Alt+F4 не будет работать)
                    DeleteMenu(systemMenu, SC_CLOSE, MF_BYCOMMAND);

                    // Альтернативно можно просто заблокировать пункт:
                    // EnableMenuItem(systemMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                }

                InstallHook();
            }
        }
    }
}