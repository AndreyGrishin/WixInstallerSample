using System;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Windows;
using InstallerWIX.Model;
using static WIXInstaller.BootstrapperInstaller;

namespace WIXInstaller.Model
{
    public static class ServiceInstaller
    {
        public delegate void InstallError(string message);
        public static event InstallError OnInstallError;

        private static ServiceController controller;

        public static void InstallService()
        {

            string serviceExecuteblePath =  viewModel.InstallFolderPath + $@"\{Settings.ApplicationName}\Service\Service.exe";
            if (!File.Exists(serviceExecuteblePath))
            {
                // Add handler
                return;
            }
            try
            {
                ServiceProcessInstaller ProcesServiceInstaller = new ServiceProcessInstaller();
                ProcesServiceInstaller.Account = ServiceAccount.LocalSystem;
                System.ServiceProcess.ServiceInstaller ServiceInstallerObj = new System.ServiceProcess.ServiceInstaller();
                InstallContext Context = new InstallContext();
                String path = $"/assemblypath={serviceExecuteblePath}";
                String[] cmdline = { path };

                Context = new InstallContext("", cmdline);
                ServiceInstallerObj.Context = Context;
                ServiceInstallerObj.DisplayName = "Application Service";
                ServiceInstallerObj.Description = "Service for Application";
                ServiceInstallerObj.ServiceName = "ApplicationService";
                ServiceInstallerObj.StartType = ServiceStartMode.Automatic;
                ServiceInstallerObj.Parent = ProcesServiceInstaller;

                System.Collections.Specialized.ListDictionary state = new System.Collections.Specialized.ListDictionary();
                ServiceInstallerObj.Install(state);
            }
            catch (Exception e)
            {
                OnInstallError?.Invoke("Error installing service." + e.Message);
            }
        }

        public static void UnistallService()
        {
            string serviceExecuteblePath = viewModel.InstallFolderPath + $@"\{Settings.ApplicationName}\Service\Service.exe";
            if (!File.Exists(serviceExecuteblePath))
            {
                // Add handler
                return;
            }
            try
            {
                System.ServiceProcess.ServiceInstaller ServiceInstallerObj = new System.ServiceProcess.ServiceInstaller();
                InstallContext Context = new InstallContext(null, null);
                ServiceInstallerObj.Context = Context;
                ServiceInstallerObj.ServiceName = "ApplicationService";
                ServiceInstallerObj.Uninstall(null);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                OnInstallError?.Invoke("Error unistalling service." + e.Message);
            }
        }

        public static void StartService()
        {
            try
            {
                controller = new ServiceController
                {
                    ServiceName = "ApplicationService"
                };
                controller.Start();
            }
            catch (Exception exc)
            {
                // Add handler
            }
        }

        public static void StopService()
        {
            var service = new ServiceController("ApplicationService", Environment.MachineName);
            try
            {
                if (service.CanStop)
                {
                    service.Stop();
                }
            }

            finally
            {
                service.Close();
            }
        }
    }
}
