using System;
using System.Threading;
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
            using (var mutex = new Mutex(false, "Profile.Id callahanwilliam.com"))
            {
                var isInstanceRunning = !mutex.WaitOne(TimeSpan.Zero);
                if (isInstanceRunning)
                {
                    MessageBox.Show("Only one instance may run at a time!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddExceptionHandlers();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TrayIconController());
                mutex.ReleaseMutex();
            }
        }

        private static void AddExceptionHandlers()
        {
            Application.ThreadException += ErrorHandler.OnThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += ErrorHandler.OnUnhandledException;
        }
    }
}
