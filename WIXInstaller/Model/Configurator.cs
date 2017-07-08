using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using InstallerWIX.Model;
using Microsoft.Win32;

using File = System.IO.File;

using static WIXInstaller.BootstrapperInstaller;
using static InstallerWIX.Model.Settings;
namespace WIXInstaller.Model
{
    public static class Configurator
    {

        private static Dictionary<string, string> installShorcutsCollection = new Dictionary<string, string>();

        private static string startMenyAppPath;

        private static string StartMenyAppPath
        {
            get
            {
                if (startMenyAppPath != null)
                    return startMenyAppPath;

                startMenyAppPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                   $@"\Microsoft\Windows\Start Menu\Programs\{Manufacturer}\{ApplicationName}";
                return startMenyAppPath;
            }
        }

        private static string guid;

        private static string GUID
        {
            get
            {
                if (guid != null)
                    return guid;

                guid = "{" + Guid.NewGuid() + "}";
                return guid;
            }

        }

        public static bool CreateTempDirectory()
        {
            try
            {
                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                    return true;
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool DeleteTempDirectory()
        {
            try
            {
                if (Directory.Exists(TempPath))
                {
                    Directory.Delete(TempPath);
                    return true;
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static void CreateStartMenuDirectory()
        {

            if (Directory.Exists(StartMenyAppPath))
                return;

            string appDirPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + $@"\Microsoft\Windows\Start Menu\Programs\{Manufacturer}";
            try
            {
                Directory.CreateDirectory(appDirPath + $@"\{ApplicationName}");
            }
            catch (Exception e)
            {
                // ignored
            }
        }


        public static void CreateShortcuts()
        {

            if (viewModel.CreateDesktopShortcut)
                installShorcutsCollection.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $@"\{ApplicationName}.lnk", ExecutableFile);

            installShorcutsCollection.Add(StartMenyAppPath + $@"\{ApplicationName}.lnk", ExecutableFile);

            foreach (KeyValuePair<string, string> shorcut in installShorcutsCollection)
            {
                CreateShorcut(shorcut.Key, shorcut.Value);
            }
        }

        private static void CreateShorcut(string shortcutPath, string targetPath)
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var shortcut = shell.CreateShortcut(shortcutPath);
                try
                {

                    shortcut.TargetPath = targetPath;
                    shortcut.IconLocation = ExecutableFile;
                    shortcut.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shortcut);
                }
            }
            catch (Exception e)
            {
                // ignored
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }



        internal static void CreateCacheFile()
        {
            try
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Package Cache\" + GUID);

                string directory = Environment.CurrentDirectory;
                string fileName = Process.GetCurrentProcess().MainModule.ModuleName;
                string installerPath = directory + $@"\{fileName}";

                File.Copy(installerPath, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Package Cache\" + GUID + $@"\{fileName}");
            }
            catch (Exception e)
            {

            }
        }

        internal static void DeleteCacheFile()
        {
            try
            {
                var paths = DirSearch();
                foreach (var path in paths)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        DirectoryInfo directoryInfo = fileInfo.Directory;
                        Directory.Delete(directoryInfo?.FullName, true);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                // Ignore
            }
        }

        private static IEnumerable<string> DirSearch()
        {
            return Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Package Cache\", $"{ApplicationName} Installer.exe", SearchOption.AllDirectories);
        }

        public static string GetRegistryValue(string name, string key)
        {
            try
            {
                string displayName;

                // search in: LocalMachine_32
                var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (registryKey != null)
                {
                    foreach (string keyName in registryKey.GetSubKeyNames())
                    {
                        RegistryKey subkey = registryKey.OpenSubKey(keyName);
                        displayName = subkey?.GetValue("DisplayName") as string;
                        if (name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            return subkey.GetValue(key).ToString();
                        }
                    }
                }

                // search in: LocalMachine_64
                registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                if (registryKey != null)
                {
                    foreach (string keyName in registryKey.GetSubKeyNames())
                    {
                        RegistryKey subkey = registryKey.OpenSubKey(keyName);
                        displayName = subkey?.GetValue("DisplayName") as string;
                        if (name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            return subkey.GetValue(key).ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }

            // NOT FOUND
            return null;
        }

        internal static bool IsApplicationInstalled(string name)
        {
            try
            {
                string displayName;

                // search in: LocalMachine_32
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (key != null)
                {
                    foreach (string keyName in key.GetSubKeyNames())
                    {
                        RegistryKey subkey = key.OpenSubKey(keyName, true);
                        displayName = subkey?.GetValue("DisplayName") as string;
                        if (name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                // search in: LocalMachine_64
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                if (key != null)
                {
                    foreach (string keyName in key.GetSubKeyNames())
                    {
                        RegistryKey subkey = key.OpenSubKey(keyName, true);
                        displayName = subkey?.GetValue("DisplayName") as string;
                        if (name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            // NOT FOUND
            return false;
        }


        public static void DeleteStartMenuDirectory()
        {
            string appDirPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + $@"\Microsoft\Windows\Start Menu\Programs\{Manufacturer}";
            try
            {
                if (Directory.Exists(appDirPath))
                    Directory.Delete(appDirPath, true);
            }
            catch (Exception e) { }

        }

        public static void DeleteDesktopShortcuts()
        {
            var shorcuts = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.lnk");
            foreach (var shorcut in shorcuts)
            {
                try
                {
                    Regex regex = new Regex(ApplicationName);
                    if (regex.IsMatch(shorcut))
                        File.Delete(shorcut);
                }
                catch (Exception e) { }
            }

        }

        internal static void ConfigurateRegistry()
        {
            string fileName = Process.GetCurrentProcess().MainModule.ModuleName;
            string cacheFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Package Cache\{GUID}\{fileName}";
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);
            var key = registryKey?.CreateSubKey(GUID);
            if (key != null)
            {
                key.SetValue("ApplicationPath", viewModel.InstallFolderPath);
                key.SetValue("BundleCachePath", $@"{cacheFilePath} ");
                key.SetValue("BundleProviderKey", GUID);
                key.SetValue("BundleVersion", "1.0.0.0");
                key.SetValue("DisplayIcon", $@"{cacheFilePath} ,0");
                key.SetValue("DisplayName", ApplicationName);
                key.SetValue("DisplayVersion", Settings.Version);
                key.SetValue("EstimatedSize", EstimatedSize);
                key.SetValue("Installed", 1);
                key.SetValue("NoElevateOnModify", 1);
                key.SetValue("Publisher", Manufacturer);
                key.SetValue("QuietUninstallString", 1);
                key.SetValue("UninstallString", $"{cacheFilePath} /uninstall");
                key.SetValue("URLInfoAbout", SupportUrl);
            }
        }


        internal static bool ClearRegistry(string name)
        {
            try
            {
                string displayName;

                // search in: LocalMachine_32
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);
                if (key != null)
                {
                    foreach (string keyName in key.GetSubKeyNames())
                    {
                        RegistryKey subkey = key.OpenSubKey(keyName, true);
                        displayName = subkey?.GetValue("DisplayName") as string;
                        if (name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            key.DeleteSubKey(keyName);
                            return true;
                        }
                    }
                }

                // search in: LocalMachine_64
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall", true);
                if (key != null)
                {
                    foreach (string keyName in key.GetSubKeyNames())
                    {
                        RegistryKey subkey = key.OpenSubKey(keyName, true);
                        displayName = subkey?.GetValue("DisplayName") as string;
                        if (name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            key.DeleteSubKey(keyName);
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            // NOT FOUND
            return false;
        }
    }
}
