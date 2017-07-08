using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using static WIXInstaller.BootstrapperInstaller;

namespace InstallerWIX.Model
{
    public static class Settings
    {
        internal const string Manufacturer = "Your manufactorer";

        internal const string ApplicationName = "Your application";

        internal static readonly string ExecutableFile = viewModel.InstallFolderPath + $@"\{ApplicationName}\MyAppication.exe";

        internal static readonly string IconLocation = viewModel.InstallFolderPath + $@"\{ApplicationName}\MyApplication.exe";

        internal const string Version = "1.0.0.0";

        internal const int EstimatedSize = 29912;

        internal const string SupportUrl = "http://smedialink.com/";

        internal static string RequiredSize = "2";

        internal static string ProgramFilesPath => Environment.GetFolderPath(Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles);

        internal static string TempPath => Path.GetTempPath() + $@"{ApplicationName}\";

       

        public static long GetFreeDiscSpace(string discRoot)
        {
            string driveName = Path.GetPathRoot(discRoot);
            DriveInfo drive = new DriveInfo(driveName);
            long size = drive.TotalFreeSpace;
            return (size / 1048000);
        }

    }

}
