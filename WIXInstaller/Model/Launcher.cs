using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using InstallerWIX.Model;
using Microsoft.Win32;

namespace WIXInstaller.Model
{
    public static class Launcher
    {
        public static void CheckInstalledInstance()
        {
            string cachePathApplication = Configurator.GetRegistryValue(Settings.ApplicationName, "BundleCachePath");


            if (cachePathApplication == null)
                return;

            LaunchExecutableFile(cachePathApplication, "/IsCacheFile");

            Environment.Exit(0);
            BootstrapperInstaller.BootstrapperDispatcher.InvokeShutdown();

        }


        public static void RunAsAdministrator()
        {
            string directory = Environment.CurrentDirectory;
            string fileName = Process.GetCurrentProcess().MainModule.ModuleName;
            string path = directory + $@"\{fileName}";
            // Launch itself as administrator
            ProcessStartInfo proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = path,
                Verb = "runas"
            };

            try
            {
                Process.Start(proc);
            }
            catch (Exception e)
            {
                // The user refused the elevation.
                Environment.Exit(0);
            }
            Environment.Exit(0); // Quit itself

        }

        public static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void LaunchExecutableFile(string appPath, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo(appPath)
            {
                UseShellExecute = false,
                Verb = "runas",
                Arguments = arguments
            };

            try
            {
                Process.Start(info);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }




    }
}
