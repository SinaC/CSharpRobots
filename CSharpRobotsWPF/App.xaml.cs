using System.Configuration;
using System.Windows;
using Common;

namespace CSharpRobotsWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //
            ExecuteOnUIThread.Initialize();

            //
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], "robots.log");
        }
    }
}
