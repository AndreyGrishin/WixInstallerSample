using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using InstallerWIX.Model;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WIXInstaller.Model;
using WIXInstaller.VIew.Pages;

using static WIXInstaller.BootstrapperInstaller;

namespace WIXInstaller.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public BootstrapperApplication Bootstrapper { get; private set; }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(BootstrapperInstaller bootstrapperInstaller)
        {
          //   IsThinking = false;
            Bootstrapper = bootstrapperInstaller;
           // Bootstrapper.ApplyComplete += OnApplyComplete;
           // Bootstrapper.DetectPackageComplete += OnDetectPackageComplete;
          //  Bootstrapper.PlanComplete += OnPlanComplete;
            Downloader.OnDownloadAppPartsComplite += Installer.Configurate;
            Downloader.OnSizeAppPartsUpdate += DownloaderOnSizeAppPartsUpdate;
            Installer.OnComplite += InstallerOnComplite;
        }


        #region Properties
        private Frame MainFrame => view?.MainFrame;
        private FinishPage finishPage;
        private InstallPage installPage;
        private SelectPathPage selectPathPage;

        private NavigationService service;

        #region object

        private object currentPage = new MainPage();
        public object CurrentPage
        {
            get { return currentPage; }
            set
            {
                currentPage = value;
                RaisePropertyChanged("CurrentPage");
            }
        }

        #endregion // object

        #region bool


        private bool createDesktopShortcut = true;
        public bool CreateDesktopShortcut
        {
            get { return createDesktopShortcut; }
            set
            {
                createDesktopShortcut = value;
                RaisePropertyChanged("CreateDesktopShortcut");
            }
        }

        private bool installEnabled;
        public bool InstallEnabled
        {
            get { return installEnabled; }
            set
            {
                installEnabled = value;
                RaisePropertyChanged("InstallEnabled");
            }
        }

        private bool uninstallEnabled;
        public bool UninstallEnabled
        {
            get { return uninstallEnabled; }
            set
            {
                uninstallEnabled = value;
                RaisePropertyChanged("UninstallEnabled");
            }
        }

        private bool isLaunchApplication = true;
        public bool IsLaunchApplication
        {
            get { return isLaunchApplication; }
            set
            {
                isLaunchApplication = value;
                RaisePropertyChanged("IsLaunchApplication");
            }
        }

        private bool isThinking;
        public bool IsThinking
        {
            get { return isThinking; }
            set
            {
                isThinking = value;
                RaisePropertyChanged("IsThinking");
            }
        }

        #endregion // bool

        #region string

        public string RequiredSize
        {
            get { return Settings.RequiredSize; }
        }


        private string installCompleted;
        public string InstallCompleted
        {
            get
            {
                return installCompleted;
            }
            set
            {
                installCompleted = value;
                RaisePropertyChanged("InstallCompleted");
            }
        }
        private string installFolderPath;
        public string InstallFolderPath
        {
            get
            {
                if (!string.IsNullOrEmpty(installFolderPath))
                    return installFolderPath;
                else
                    return $@"{Settings.ProgramFilesPath}\{Settings.Manufacturer}";

            }
            set
            {
                installFolderPath = value;
                RaisePropertyChanged("InstallFolderPath");
            }
        }

        private string freeDiscSpace;
        public string FreeDiscSpace
        {
            get { return Settings.GetFreeDiscSpace(InstallFolderPath).ToString(); }
            set
            {
                freeDiscSpace = value;
                RaisePropertyChanged("FreeDiscSpace");
            }
        }


        #endregion // string

        #region Quantitative

        private long installProgressValue;
        public long InstallProgressValue
        {
            get { return installProgressValue; }
            set
            {
                installProgressValue = value;
                RaisePropertyChanged("InstallProgressValue");
            }
        }



        private long maxValAppPartsSize = long.MaxValue;
        public long MaxValAppPartsSize
        {
            get { return maxValAppPartsSize; }
            set
            {
                maxValAppPartsSize = value;
                RaisePropertyChanged("MaxValAppPartsSize");
            }
        }

        #endregion // Quantitative

        #region Enums

        

        private Visibility launchAppVisibility = Visibility.Visible;
        public Visibility LaunchAppVisibility
        {
            get { return launchAppVisibility; }
            set
            {
                launchAppVisibility = value;
                RaisePropertyChanged("LaunchAppVisibility");
            }
        }
        #endregion

        #endregion //Properties

        #region RelayCommands

        private RelayCommand installCommand;
        public RelayCommand InstallCommand
        {
            get
            {
                if (installCommand == null)
                    installCommand = new RelayCommand(() =>
                    {
                        MainFrame.Navigate(selectPathPage);
                    }, () => InstallEnabled == true);

                return installCommand;
            }
        }

        private RelayCommand uninstallCommand;
        public RelayCommand UninstallCommand
        {
            get
            {
                if (uninstallCommand == null)

                    uninstallCommand = new RelayCommand(() =>
                    {
                        Installer.StartUninstallation();
                        UnistallCacheAndRegistry();
                        InstallEnabled = true;
                        InstallCommand.RaiseCanExecuteChanged();
                        UninstallEnabled = false;

                    }, () => UninstallEnabled == true);


                return uninstallCommand;
            }
        }

        private RelayCommand selectPathCommand;
        public RelayCommand SelectPathCommand
        {
            get
            {
                if (selectPathCommand == null)
                    selectPathCommand = new RelayCommand(() =>
                    {
                        var dialog = new CommonOpenFileDialog()
                        {
                            IsFolderPicker = true
                        };
                        try
                        {
                            CommonFileDialogResult result = dialog.ShowDialog();
                            InstallFolderPath = dialog.FileName + $@"\{Settings.Manufacturer}";
                            RaisePropertyChanged("FreeDiscSpace");
                        }
                        catch (Exception e)
                        {
                            // The dialog has closed. To do nothing 
                        }

                    });

                return selectPathCommand;
            }
        }



        private RelayCommand settingsToNextPageCommand;
        public RelayCommand SettingsToNextPageCommand
        {
            get
            {
                if (settingsToNextPageCommand == null)
                    settingsToNextPageCommand = new RelayCommand(() =>
                    {


                    }, () =>
                    {
                        if (Convert.ToInt32(RequiredSize) > Convert.ToInt32(FreeDiscSpace))
                            return false;
                        else
                            return true;
                    });

                return settingsToNextPageCommand;
            }
        }

        private RelayCommand navigateBackCommand;
        public RelayCommand NavigateBackCommand
        {
            get
            {
                if (navigateBackCommand == null)
                    navigateBackCommand = new RelayCommand(() =>
                    {
                        service.GoBack();
                    });

                return navigateBackCommand;
            }
        }

        private RelayCommand startInstallCommand;
        public RelayCommand StartInstallCommand
        {
            get
            {
                if (startInstallCommand == null)
                    startInstallCommand = new RelayCommand(() =>
                    {
                        MainFrame.Navigate(installPage);
                        StartInstall();
                    });

                return startInstallCommand;
            }
        }

        private RelayCommand finishInstallationCommand;
        public RelayCommand FinishInstallationCommand
        {
            get
            {
                if (finishInstallationCommand == null)
                    finishInstallationCommand = new RelayCommand(() =>
                    {
                        if (IsLaunchApplication)
                            Launcher.LaunchExecutableFile(Settings.ExecutableFile, string.Empty);

                        BootstrapperDispatcher.InvokeShutdown();
                    });

                return finishInstallationCommand;
            }
        }
        #endregion //RelayCommands

        #region Methods

        private void InstallerOnComplite()
        {
            InstallCacheAndRegistry();
        }
        private void DownloaderOnSizeAppPartsUpdate(long size)
        {
            MaxValAppPartsSize = size;
        }

        private void DownloaderOnDownloadDatabasesComplite()
        {
            MainFrame.Navigate(finishPage);
        }


        private void StartInstall()
        {
            Installer.StartInstallation();
        }

        //private void InstallMSIExecute()
        //{
        //    IsThinking = true;
        //    Bootstrapper.Engine.Plan(LaunchAction.Install);

        //}

        //private void UninstallMSIExecute()
        //{
        //    IsThinking = true;
        //    Bootstrapper.Engine.Plan(LaunchAction.Uninstall);
        //}


        private void InstallCacheAndRegistry()
        {
            Configurator.CreateCacheFile();
            Configurator.ConfigurateRegistry();
            OnInstallComplete();
        }

        private void UnistallCacheAndRegistry()
        {
            Configurator.DeleteCacheFile();
            Configurator.ClearRegistry(Settings.ApplicationName);
            OnUnistallComplete();
        }


        private void OnInstallComplete()
        {
            InstallCompleted = "Installation complete!";

            view.Dispatcher.Invoke(() => { MainFrame.Navigate(finishPage); });
        }

        private void OnUnistallComplete()
        {
            InstallCompleted = "Uninstalling completed!";
            LaunchAppVisibility = Visibility.Collapsed;
            IsLaunchApplication = false;
            view.Dispatcher.Invoke(() => { MainFrame.Navigate(finishPage); });

        }

        /// <summary>
        /// Method that gets invoked when the Bootstrapper ApplyComplete event is fired.
        /// This is called after a bundle installation has completed. Make sure we updated the view.
        /// </summary>
        //private void OnApplyComplete(object sender, ApplyCompleteEventArgs e)
        //{
        //    IsThinking = false;
        //    InstallCompleted = "Installation completed";
        //    view.Dispatcher.Invoke(() =>
        //    {
        //        MainFrame.Navigate(finishPage);
        //    });
        //}



        /// <summary>
        /// Method that gets invoked when the Bootstrapper DetectPackageComplete event is fired.
        /// Checks the PackageId and sets the installation scenario. The PackageId is the ID
        /// specified in one of the package elements (msipackage, exepackage, msppackage,
        /// msupackage) in the WiX bundle.
        /// </summary>
        //private void OnDetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        //{

        //    if (e.PackageId == "InstallationPackageId")
        //    {

        //        if (e.State == PackageState.Absent)
        //            InstallEnabled = true;

        //        else if (e.State == PackageState.Present)
        //        {
        //            UninstallEnabled = true;
        //            if (args.Length != 0 && args == "/IsCacheFile")
        //                return;

        //            Launcher.CheckInstalledInstance();
        //        }
        //    }

        //}

        /// <summary>
        /// Method that gets invoked when the Bootstrapper PlanComplete event is fired.
        /// If the planning was successful, it instructs the Bootstrapper Engine to 
        /// install the packages.
        /// </summary>
        private void OnPlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (e.Status >= 0)
                Bootstrapper.Engine.Apply(IntPtr.Zero);
        }

        private void DetectInstalledPackage()
        {
            if (Configurator.IsApplicationInstalled(Settings.ApplicationName))
            {
                UninstallEnabled = true;
                view.Dispatcher.Invoke(() =>
                {
                    UninstallCommand?.RaiseCanExecuteChanged();
                });
            }
            else
            {
                InstallEnabled = true;
                view.Dispatcher.Invoke(() =>
                {
                    InstallCommand?.RaiseCanExecuteChanged();
                });

            }
        }

        #endregion // Methods

        #region Events

        public void WindowLoaded()
        {
            DetectInstalledPackage();
            service = MainFrame.NavigationService;
            finishPage = new FinishPage();
            installPage = new InstallPage();
            selectPathPage = new SelectPathPage();
        }



        public void DragWindow()
        {
            view.DragMove();
        }

        #endregion // Events
    }
}