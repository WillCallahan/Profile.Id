using System;
using System.Windows.Forms;

namespace Profile.id
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AddExceptionHandlers();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayIconController());
        }

        private static void AddExceptionHandlers()
        {
            Application.ThreadException += ErrorHandler.OnThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += ErrorHandler.OnUnhandledException;
        }
    }
}
