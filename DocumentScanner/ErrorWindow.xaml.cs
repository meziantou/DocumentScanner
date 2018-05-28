using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DocumentScanner
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }

        public static void RegisterErrorHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;

            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != Application.Current.Dispatcher)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.UnhandledException += DispatcherUnhandledException;
            }
        }

        public static void ShowError(Exception ex, bool canContinue)
        {
            ShowError(ex?.Message, ex?.StackTrace, canContinue);
        }

        public static void ShowError(string message, string stackStace, bool canContinue)
        {

            if (!CanDispatch())
            {
                // TODO log and quit
                return;
            }

            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                // Thread must be STA...
                Thread t = new Thread(o =>
                {
                    ShowWindow(message, stackStace, canContinue);
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                return;
            }

            ShowWindow(message, stackStace, canContinue);
        }

        private static bool CanDispatch()
        {
            var app = Application.Current;
            if (app == null)
                return false;

            return !app.Dispatcher.HasShutdownFinished && !app.Dispatcher.HasShutdownStarted;
        }

        private static void ShowWindow(string message, string stackStace, bool canContinue)
        {
            var window = new ErrorWindow();
            window.TextBlockMessage.Text = message;
            window.TextBlockStackTrace.Text = stackStace;
            window.ShowDialog();
        }

        private static void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError(e.Exception, true);
            e.Handled = true;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ShowError(e.Exception, true);
            e.SetObserved();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            ShowError(ex, !e.IsTerminating);
        }
    }
}
