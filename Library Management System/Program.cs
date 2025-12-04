 //Minor issue the penaltty in return still counting if admin did not click the confirm button. e.g : the student has 10 pesos penalty then the admin forget to confirm it then in the next day the penalty display of the student is become 20 in the message box appears.
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
            //Application.Run(new PreLoginButtons());// Student Side/Panel
            Application.Run(new PreLoginButtons2());//Admin Side/Panel
        }
    }
}