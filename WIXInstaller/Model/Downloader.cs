using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static InstallerWIX.Model.Settings;
using static WIXInstaller.BootstrapperInstaller;

namespace WIXInstaller.Model
{
    public static class Downloader
    {
        public delegate void OnDownloadComplite();

        public delegate void OnSizeUpdate(long size);

        public static event OnDownloadComplite OnDownloadAppPartsComplite;

        public static event OnSizeUpdate OnSizeAppPartsUpdate;

        private static readonly Dictionary<string, string> ApplicationParts = new Dictionary<string, string>
        {
            { "https://raw.githubusercontent.com/AndreyGrishin/Files/master/MyAppication.zip", "MyAppication.zip"}
        };

        private static WebClient _client;

        public static async void DownloadAppParts()
        {
            long size = ApplicationParts.Sum(part => GetFileSizeAsync(part.Key).Result);

            SetSizeProgressBar(size);

            _client = new WebClient();
            _client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
            foreach (var appPair in ApplicationParts)
            {
                bytesIn = 0;
                try
                {
                    await _client.DownloadFileTaskAsync(new Uri(appPair.Key), TempPath + appPair.Value);
                }
                catch (WebException e)
                {
                    // Add event handlers
                }
                ExtractFiles(TempPath + appPair.Value, viewModel.InstallFolderPath + $@"\{ApplicationName}");
            }

            OnDownloadAppPartsComplite?.Invoke();
        }
        private static async Task<long> GetFileSizeAsync(string url)
        {
            long size = 0;
            WebRequest req = WebRequest.Create(url);
            req.Timeout = 6000;
            req.Method = "HEAD";
            using (WebResponse resp = await req.GetResponseAsync().ConfigureAwait(false))
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long contentLength))
                {
                    size += contentLength;
                }
            }
            return size;
        }


        private static void ExtractFiles(string zipPath, string extractPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            catch (Exception e)
            {
                // Add event handlers
            }
        }

        private static long bytesIn; // Bytes received. Used to calculate the current size of the received bytes
        private static void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            long downloadBytes = downloadProgressChangedEventArgs.BytesReceived - bytesIn;
            bytesIn = downloadProgressChangedEventArgs.BytesReceived;
            viewModel.InstallProgressValue += downloadBytes;
        }


        private static void SetSizeProgressBar(long size)
        {
            OnSizeAppPartsUpdate?.Invoke(size);
        }
    }
}
