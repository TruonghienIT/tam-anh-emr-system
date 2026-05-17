using DotNetEnv;
using System.Configuration;
using System.Data;
using System.Windows;
using TamAnh_EMR_System.View;
using QuestPDF.Infrastructure;

namespace TamAnh_EMR_System
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DotNetEnv.Env.Load();

            base.OnStartup(e);

            var loginView = new LoginView();
            loginView.Show();
        }
        public App()
        {
            QuestPDF.Settings.License =
                LicenseType.Community;
        }
    }

}
