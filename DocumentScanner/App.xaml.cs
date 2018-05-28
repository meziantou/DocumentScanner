using System.Windows;

namespace DocumentScanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ErrorWindow.RegisterErrorHandler();
        }
    }
}
