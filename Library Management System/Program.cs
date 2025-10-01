using System;
using System.Windows.Forms;
using Library_Management_System;
using Library_Management_System.Forms;

namespace LibraryManagementSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ensures DB file + tables exist
            Data.Db.EnsureCreated();

            // Run your startup form
            Application.Run(new PreLoginButtons());
        }
    }
}
