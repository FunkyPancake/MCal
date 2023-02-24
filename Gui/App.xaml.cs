using System.Windows;

namespace CanUpdaterGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e) {
            
            var mainWindow = new MainWindow();
            
            mainWindow.Show();
        }
    }
}