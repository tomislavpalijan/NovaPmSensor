using System.Windows;
using NovaPmSensor.ViewModels;

namespace NovaPmSensor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var mw = new ShellMainWindow
            {
                DataContext = new ShellMainViewModel()
            };
            
            MainWindow = mw;

            MainWindow.Show();

            base.OnStartup(e);
        }
    }
}
