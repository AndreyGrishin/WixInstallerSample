using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using InstallerWIX.Model;
using Microsoft.WindowsAPICodePack.Shell.Interop;
using static WIXInstaller.BootstrapperInstaller;

namespace WIXInstaller.Model
{
    public static class Installer
    {
        public delegate void OnInstallComplite();

        public static event OnInstallComplite OnComplite;

        public static void StartInstallation()
        {
            Task.Factory.StartNew(() =>
            {
                if (!Configurator.CreateTempDirectory())
                {
                    MessageBox.Show("Error Installation. Could not create temporary installer folder.");
                    return;
                }

            }).ContinueWith((t) =>
            {
                try
                {
                    CreateApplicationDirectories();
                }
                catch (Exception){ MessageBox.Show("Error Installation. Could not create application directory."); }

            }).ContinueWith((t) =>
            {
                Downloader.DownloadAppParts();
            });
        }

        public static void StartUninstallation()
        {
            DeleteApplicationDirectory();
            DeleteShorcuts();
        }

        public static void Configurate()
        {
            Configurator.CreateStartMenuDirectory();
            Configurator.CreateShortcuts();

            view.Dispatcher.Invoke(() => { OnComplite?.Invoke(); });
        }


        private static void CreateApplicationDirectories()
        {

            string subdirectoryPath = viewModel.InstallFolderPath + $@"\{Settings.ApplicationName}";

            if (Directory.Exists(subdirectoryPath))
                return;

            if (Directory.Exists(viewModel.InstallFolderPath) && !Directory.Exists(subdirectoryPath))
            {
                Directory.CreateDirectory(subdirectoryPath);
            }
            else
            {
                DirectoryInfo mainDirectory = Directory.CreateDirectory(viewModel.InstallFolderPath);
                mainDirectory.CreateSubdirectory(Settings.ApplicationName);
            }
        }

        private static void DeleteApplicationDirectory()
        {
            string applicationPath = Configurator.GetRegistryValue(Settings.ApplicationName, "ApplicationPath");
            if (Directory.Exists(applicationPath))
                Directory.Delete(applicationPath, true);
        }

        private static void DeleteShorcuts()
        {
            Configurator.DeleteStartMenuDirectory();
            Configurator.DeleteDesktopShortcuts();
        }
    }

}
