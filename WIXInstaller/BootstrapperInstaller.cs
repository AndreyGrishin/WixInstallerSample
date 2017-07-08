using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Diagnostics;
using System.Windows.Threading;
using WIXInstaller.Model;
using WIXInstaller.ViewModel;

namespace WIXInstaller
{
    public class BootstrapperInstaller : BootstrapperApplication
    {
        public static Dispatcher BootstrapperDispatcher { get; private set; }
        public static MainViewModel viewModel;

        public static MainWindow view;

        public static string Args = string.Empty;

        // entry point for our custom UI
        protected override void Run()
        {
            //Debugger.Launch();

            if (Command.GetCommandLineArgs().Length > 0)
                Args = Command.GetCommandLineArgs()[0];

            var envArgs = Environment.GetCommandLineArgs();
            if (envArgs.Length == 5)
                Args = envArgs[envArgs.Length - 1];

            Engine.Log(LogLevel.Verbose, "Launching");

            BootstrapperDispatcher = Dispatcher.CurrentDispatcher;

            if (!Launcher.IsRunAsAdmin())
                Launcher.RunAsAdministrator();


            viewModel = new MainViewModel(this);
            viewModel.Bootstrapper.Engine.Detect();
            view = new MainWindow { DataContext = viewModel };
            view.Closed += (sender, e) => BootstrapperDispatcher.InvokeShutdown();
            view.Show();

            Dispatcher.Run();

            Engine.Quit(0);
        }
    }
}
