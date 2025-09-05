using System;
using System.Windows.Forms;
using Library_Management_System;

namespace LibraryManagementSystem
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ✅ Ensure DB and tables are created
            Data.Db.EnsureCreated();

            // Run your Main form
            Application.Run(new MainForm());
        }
    }
}
