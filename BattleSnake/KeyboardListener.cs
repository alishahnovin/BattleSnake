using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleSnake
{
    /// <summary>
    /// Keyboard listener; Quick and dirty 
    /// </summary>
    internal static class Keyboard
    {
        private static IntPtr _WindowHandle = IntPtr.Zero;
        private static DateTime _LastKeyPress = DateTime.MinValue;
        private const int _KeyPressed = 0x8000;

        public static bool IsKeyDown(KeyCode key)
        {
            if (DateTime.Now.Subtract(_LastKeyPress).Milliseconds < 150)
            {
                return false;
            }

            bool isKeyDown = (GetKeyState((int)key) & _KeyPressed) != 0;
            if (isKeyDown)
            {
                if (_WindowHandle == IntPtr.Zero)
                {
                    _WindowHandle = GetConsoleWindow();
                }

                if (_WindowHandle == GetForegroundWindow())
                {
                    _LastKeyPress = DateTime.Now;
                    return true;
                }
            }
            return false;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetKeyState(int key);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        internal static extern IntPtr GetConsoleWindow();
    }

    /// <summary>
    /// Codes representing keyboard keys.
    /// </summary>
    /// <remarks>
    /// Key code documentation:
    /// http://msdn.microsoft.com/en-us/library/dd375731%28v=VS.85%29.aspx
    /// </remarks>
    internal enum KeyCode : int
    {
        Up = 0x26,
        Down = 0x28,
        Escape = 0x1B,
        K = 0x4B,
        M = 0x4D,
        B = 0x42,
        T = 0x54,
        G = 0x47,
        S = 0x53,
        Spacebar = 0x20,
    }
}
