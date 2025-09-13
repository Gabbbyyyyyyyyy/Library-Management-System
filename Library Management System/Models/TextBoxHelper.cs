using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Library_Management_System.Models
{
    internal static class TextBoxHelper
    {
        // Import WinAPI for placeholder text (cue banner)
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        /// <summary>
        /// Apply a placeholder (cue banner) and optional Enter key handler to a TextBox.
        /// </summary>
        /// <param name="textBox">Target TextBox</param>
        /// <param name="placeholder">Placeholder text to display</param>
        /// <param name="onEnter">Optional Enter key event handler</param>
         // Apply placeholder + optional Enter key handler
        public static void ApplySearchBox(TextBox textBox, string placeholder, KeyEventHandler keyDownHandler = null)
        {
            SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, placeholder);

            if (keyDownHandler != null)
            {
                textBox.KeyDown += keyDownHandler;
            }
        }
    }
}
