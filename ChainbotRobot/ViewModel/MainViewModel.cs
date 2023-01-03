using GalaSoft.MvvmLight;
using log4net;
using Chainbot.Contracts.Workflow;
using ChainbotRobot.Contracts;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Threading;
using System;
using System.Threading.Tasks;
using Flurl.Http;
using NuGet;
using Chainbot.Contracts.Utils;
using Plugins.Shared.Library.Librarys;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Linq;
using ChainbotRobot.Views;
using System.Collections.ObjectModel;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using Plugins.Shared.Library;
using System.Threading;
using ChainbotRobot.Cores;
using ChainbotRobot.Database;
using System.IO;

namespace ChainbotRobot.ViewModel
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
        private IServiceLocator _serviceLocator;

        private IRobotPathConfigService _robotPathConfigService;

        private ICommonService _commonService;

        private ILogService _logService;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IRunManagerService _runManagerService { get; set; }


        private Window _view { get; set; }


        private string _programDataPackagesDir { get; set; }

        private string _programDataInstalledPackagesDir { get; set; }

        private IFFmpegService _ffmpegService { get; set; }

        private IPackageService _packageService { get; set; }

        private IControlServerService _controlServerService { get; set; }


        private DispatcherTimer _registerTimer { get; set; }


        private DispatcherTimer _getProcessesTimer { get; set; }


        private DispatcherTimer _getRunProcesssTimer { get; set; }

        private ConcurrentDictionary<string, bool> _packageItemEnableDict = new ConcurrentDictionary<string, bool>();


        private UserPreferencesViewModel _userPreferencesViewModel;


        private StartupViewModel _startupViewModel;

        private IScheduledTasksService _scheduledTasksService;
        private ScheduledTasksDatabase _scheduledTasksDatabase;

        public string ProgramDataPackagesDir
        {
            get
            {
                return _programDataPackagesDir;
            }
        }

        public string ProgramDataInstalledPackagesDir
        {
            get
            {
                return _programDataInstalledPackagesDir;
            }
        }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _runManagerService = _serviceLocator.ResolveType<IRunManagerService>();
            
            _runManagerService.BeginRunEvent += _runManagerService_BeginRunEvent;
            _runManagerService.EndRunEvent += _runManagerService_EndRunEvent;

            Common.RunInUI(() =>
            {
                _controlServerService = _serviceLocator.ResolveType<IControlServerService>();
                _commonService = _serviceLocator.ResolveType<ICommonService>();
                _logService = _serviceLocator.ResolveType<ILogService>();
                _startupViewModel = _serviceLocator.ResolveType<StartupViewModel>();
                _packageService = _serviceLocator.ResolveType<IPackageService>();

                _userPreferencesViewModel = _serviceLocator.ResolveType<UserPreferencesViewModel>();

                _robotPathConfigService = _serviceLocator.ResolveType<IRobotPathConfigService>();
                _robotPathConfigService.InitDirs();

                _programDataPackagesDir = _robotPathConfigService.ProgramDataPackagesDir;
                _programDataInstalledPackagesDir = _robotPathConfigService.ProgramDataInstalledPackagesDir;

                _scheduledTasksService = _serviceLocator.ResolveType<IScheduledTasksService>();
                _scheduledTasksDatabase = _serviceLocator.ResolveType<ScheduledTasksDatabase>();

                _scheduledTasksService.AddJobs(_scheduledTasksDatabase.GetItems());
            });
        }

        private void _runManagerService_BeginRunEvent(object sender, EventArgs e)
        {
            var obj = sender as IRunManagerService;

            SharedObject.Instance.Output(SharedObject.enOutputType.Trace, Properties.Resources.TheProcessStarted);

            if (_ffmpegService != null)
            {
                _ffmpegService.StopCaptureScreen();
                _ffmpegService = null;
            }

            CleanRecordingFiles(_robotPathConfigService.ScreenRecorderDir, _userPreferencesViewModel.RecordingLimitDays);

            if (_userPreferencesViewModel.IsEnableScreenRecorder)
            {
                foreach (var p in System.Diagnostics.Process.GetProcessesByName("ffmpeg"))
                {
                    p.Kill();
                }

                SharedObject.Instance.Output(SharedObject.enOutputType.Trace, Properties.Resources.ScreenRecordingStarted);
                var screenRecorderFilePath = _robotPathConfigService.ScreenRecorderDir + @"\" + obj.PackageItem.Name + @"(" + DateTime.Now.ToString(Properties.Resources.YYYYMMDD) + ").mp4";

                _ffmpegService = _serviceLocator.ResolveType<IFFmpegService>();
                _ffmpegService.Init(screenRecorderFilePath, _userPreferencesViewModel.FPS, _userPreferencesViewModel.Quality);

                Task.Run(() =>
                {
                    _ffmpegService.StartCaptureScreen();
                });

                int wait_count = 0;
                while (!_ffmpegService.IsRunning())
                {
                    wait_count++;
                    Thread.Sleep(300);
                    if (wait_count == 10)
                    {
                        break;
                    }
                }
            }


            Common.RunInUI(() =>
            {
                _view.Hide();

                obj.PackageItem.IsRunning = true;

                IsWorkflowRunning = true;
                WorkflowRunningName = obj.PackageItem.Name;
                WorkflowRunningToolTip = obj.PackageItem.ToolTip;
                WorkflowRunningStatus = Properties.Resources.RunningText;

                RefreshPackageItemRunningStatusByName(WorkflowRunningName, IsWorkflowRunning);
            });
        }


        private void CleanRecordingFiles(string screenRecorderDir, int recordingLimitDays)
        {
            DirectoryInfo dir = new DirectoryInfo(screenRecorderDir);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime < DateTime.Now.AddDays(-recordingLimitDays))
                {
                    _logService.Debug($"Video files<{file.Name}>exceed the retention period ({recordingLimitDays} days) and are cleaned.", _logger);
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception err)
                    {
                        _logService.Warn($"Video file<{file.Name}>cleaning failed. Exception reason:" + err.ToString(), _logger);
                    }
                   
                }
            }
        }

        public void RefreshPackageItemRunningStatusByName(string workflowRunningName, bool isWorkflowRunning)
        {
            foreach(var item in PackageItems)
            {
                if(item.Name == workflowRunningName)
                {
                    item.IsRunning = isWorkflowRunning;
                    break;
                }
            }
        }

        private void _runManagerService_EndRunEvent(object sender, EventArgs e)
        {
            var obj = sender as IRunManagerService;

            SharedObject.Instance.Output(SharedObject.enOutputType.Trace, Properties.Resources.EndProcessRun);

            Task.Run(async () =>
            {
                if (obj.HasException)
                {
                    await _controlServerService.UpdateRunStatus(obj.PackageItem.Name, obj.PackageItem.Version, enProcessStatus.Exception);
                }
                else
                {
                    await _controlServerService.UpdateRunStatus(obj.PackageItem.Name, obj.PackageItem.Version, enProcessStatus.Stop);
                }

            });

            if (_userPreferencesViewModel.IsEnableScreenRecorder)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Trace, Properties.Resources.EndScreenRecording);
                _ffmpegService?.StopCaptureScreen();
                _ffmpegService = null;
            }

            Common.RunInUI(() =>
            {
                _view.Show();
                _view.Activate();

                if(obj.PackageItem != null)
                {
                    obj.PackageItem.IsRunning = false;

                    RefreshPackageItemRunningStatusByName(obj.PackageItem.Name, false);
                }

                foreach (var pkg in PackageItems)
                {
                    pkg.IsRunning = false;
                }

                IsWorkflowRunning = false;
                WorkflowRunningName = "";
                WorkflowRunningStatus = "";
            });
        }

        public void InitControlServer()
        {
            _registerTimer = new DispatcherTimer();
            _registerTimer.Interval = TimeSpan.FromSeconds(60);
            _registerTimer.Tick += _registerTimer_Tick;
            _registerTimer.Start();
            _registerTimer_Tick(null, null);

            _getProcessesTimer = new DispatcherTimer();
            _getProcessesTimer.Interval = TimeSpan.FromSeconds(30);
            _getProcessesTimer.Tick += _getProcessesTimer_Tick;
            _getProcessesTimer.Start();
            _getProcessesTimer_Tick(null, null);

            _getRunProcesssTimer = new DispatcherTimer();
            _getRunProcesssTimer.Interval = TimeSpan.FromSeconds(30);
            _getRunProcesssTimer.Tick += _getRunProcesssTimer_Tick;
            _getRunProcesssTimer.Start();
            _getRunProcesssTimer_Tick(null, null);

        }

        private void _registerTimer_Tick(object sender, EventArgs e)
        {
            _controlServerService.Register();
        }

        private void _getProcessesTimer_Tick(object sender, EventArgs e)
        {
            _getProcessesTimer.Stop();

            Task.Run(async () =>
            {
                var jArr = await _controlServerService.GetProcesses();

                if (jArr != null)
                {
                    bool needRefresh = false;

                    _packageItemEnableDict.Clear();
                    for (int i = 0; i < jArr.Count; i++)
                    {
                        var jObj = jArr[i];
                        var nupkgName = jObj["PROCESSNAME"].ToString();
                        var nupkgVersion = jObj["PROCESSVERSION"].ToString();
                        var nupkgFileName = jObj["NUPKGFILENAME"].ToString();
                        var nupkgUrl = jObj["NUPKGURL"].ToString();

                        _packageItemEnableDict[nupkgName] = true;

                        if (!System.IO.File.Exists(System.IO.Path.Combine(_programDataPackagesDir, nupkgFileName)))
                        {
                            var downloadAndSavePath = await nupkgUrl
                            .WithTimeout(300)
                            .DownloadFileAsync(_programDataPackagesDir, nupkgFileName);
                            needRefresh = true;
                        }

                        var repo = PackageRepositoryFactory.Default.CreateRepository(_programDataPackagesDir);
                        var pkgNameList = repo.FindPackagesById(nupkgName);
                        foreach (var item in pkgNameList)
                        {
                            if (item.Version > new SemanticVersion(nupkgVersion))
                            {
                                var file = _programDataPackagesDir + @"\" + nupkgName + @"." + item.Version.ToString() + ".nupkg";
                                _commonService.DeleteFile(file);
                            }
                        }
                    }

                    if (needRefresh)
                    {
                        Common.RunInUI(() =>
                        {
                            RefreshAllPackages();
                        });
                    }
                }

                _getProcessesTimer.Start();
            });
        }


        private void _getRunProcesssTimer_Tick(object sender, EventArgs e)
        {
            _getRunProcesssTimer.Stop();

            Task.Run(async () =>
            {
                var jObj = await _controlServerService.GetRunProcess();
                if (jObj != null)
                {
                    var processName = jObj["PROCESSNAME"].ToString();
                    var processVersion = jObj["PROCESSVERSION"].ToString();

                    _packageService.Run(processName, processVersion);
                }

                _getRunProcesssTimer.Start();
            });
        }


        public void StopCaptureScreen()
        {
            if (_ffmpegService != null)
            {
                _ffmpegService.StopCaptureScreen();
                _ffmpegService = null;
            }
        }



        public void RefreshAllPackages()
        {
            PackageItems.Clear();

            var repo = PackageRepositoryFactory.Default.CreateRepository(_programDataPackagesDir);
            var pkgList = repo.GetPackages();

            var pkgSet = new SortedSet<string>();
            foreach (var pkg in pkgList)
            {
                pkgSet.Add(pkg.Id);
            }

            Dictionary<string, IPackage> installedPkgDict = new Dictionary<string, IPackage>();

            var packageManager = new PackageManager(repo, _programDataInstalledPackagesDir);
            foreach (IPackage pkg in packageManager.LocalRepository.GetPackages())
            {
                if (!installedPkgDict.ContainsKey(pkg.Id) || pkg.Version > installedPkgDict[pkg.Id].Version)
                {
                    installedPkgDict[pkg.Id] = pkg;
                }
            }

            foreach (var name in pkgSet)
            {
                try
                {
                    var item = _serviceLocator.ResolveType<PackageItemViewModel>();
                    item.Name = name;


                    if (!_userPreferencesViewModel.IsEnableControlServer)
                    {
                        item.IsPackageEnable = true;
                    }
                    else
                    {
                        if (_packageItemEnableDict.ContainsKey(item.Name))
                        {
                            item.IsPackageEnable = _packageItemEnableDict[item.Name];
                        }
                        else
                        {
                            item.IsPackageEnable = false;
                        }
                    }

                    var version = repo.FindPackagesById(name).Max(p => p.Version);
                    item.Version = version.ToString();

                    var pkgNameList = repo.FindPackagesById(name);
                    foreach (var i in pkgNameList)
                    {
                        item._versionList.Add(i.Version.ToString());
                    }

                    bool isNeedUpdate = false;
                    if (installedPkgDict.ContainsKey(item.Name))
                    {
                        var installedVer = installedPkgDict[item.Name].Version;
                        if (version > installedVer || !Directory.Exists(Path.Combine(_programDataInstalledPackagesDir, name + @"." + version)))
                        {
                            isNeedUpdate = true;
                        }
                    }
                    else
                    {
                        isNeedUpdate = true;
                    }
                    item.IsNeedUpdate = isNeedUpdate;

                    var pkg = repo.FindPackage(name, version);
                    item.Package = pkg;
                    var publishedTime = pkg.Published.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                    string toolTip = Properties.Resources.PackageToolTip; 
                    toolTip = toolTip.Replace(@"\r\n", "\r\n");
                    item.ToolTip = string.Format(toolTip, item.Name, item.Version, pkg.ReleaseNotes, pkg.Description, (publishedTime == null ? "unknown" : publishedTime));

                    if (IsWorkflowRunning && item.Name == WorkflowRunningName)
                    {
                        item.IsRunning = true;
                    }

                    PackageItems.Add(item);
                }
                catch (Exception err)
                {
                    _logService.Debug($"Exception occurred in obtaining the information of process<{name}>. There may be a problem with the format of a version corresponding to the process. Exception details:" + err.ToString(), _logger);
                }
            }

            doSearch();
        }

        private RelayCommand<RoutedEventArgs> _loadedCommand;


        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        _view = (Window)p.Source;
                        RefreshAllPackages();
                    }));
            }
        }





        private RelayCommand _MouseLeftButtonDownCommand;


        public RelayCommand MouseLeftButtonDownCommand
        {
            get
            {
                return _MouseLeftButtonDownCommand
                    ?? (_MouseLeftButtonDownCommand = new RelayCommand(
                    () =>
                    {
                        _view.DragMove();
                    }));
            }
        }

        private RelayCommand _activatedCommand;

        public RelayCommand ActivatedCommand
        {
            get
            {
                return _activatedCommand
                    ?? (_activatedCommand = new RelayCommand(
                    () =>
                    {
                        RefreshAllPackages();
                    }));
            }
        }



        private RelayCommand<System.ComponentModel.CancelEventArgs> _closingCommand;

        public RelayCommand<System.ComponentModel.CancelEventArgs> ClosingCommand
        {
            get
            {
                return _closingCommand
                    ?? (_closingCommand = new RelayCommand<System.ComponentModel.CancelEventArgs>(
                    e =>
                    {
                        e.Cancel = true;
                        _view.Hide();
                    }));
            }
        }


        private RelayCommand<DragEventArgs> _dropCommandCommand;

        /// <summary>
        /// Gets the DropCommand.
        /// </summary>
        public RelayCommand<DragEventArgs> DropCommand
        {
            get
            {
                return _dropCommandCommand
                    ?? (_dropCommandCommand = new RelayCommand<DragEventArgs>(
                    e =>
                    {
                        if (e.Data.GetDataPresent(DataFormats.FileDrop))
                        {
                            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                            foreach (string fileFullPath in files)
                            {
                                string extension = System.IO.Path.GetExtension(fileFullPath);
                                if (extension.ToLower() == ".nupkg")
                                {
                                    processImportNupkgFile(fileFullPath);
                                }
                            }
                        }
                    }));
            }
        }


        private bool processImportNupkgFile(string fileFullPath)
        {
            var fileName = System.IO.Path.GetFileName(fileFullPath);

            var dstFileFullPath = _programDataPackagesDir + @"\" + fileName;
            if (System.IO.File.Exists(dstFileFullPath))
            {
                var ret = MessageBox.Show(App.Current.MainWindow, $"The target directory \"{ _programDataPackagesDir}\" has a file \"{ fileName}\" with the same name. Do you want to replace it?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (ret != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            try
            {
                System.IO.File.Copy(fileFullPath, dstFileFullPath, true);
                RefreshCommand.Execute(null);
                MessageBox.Show(App.Current.MainWindow, $"Successfully imported [{fileName}]!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
                MessageBox.Show(App.Current.MainWindow, $"Exception in importing [{fileName}]!", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private RelayCommand _refreshCommand;

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                    () =>
                    {
                        RefreshAllPackages();
                    }));
            }
        }






        private RelayCommand _userPreferencesCommand;

        public RelayCommand UserPreferencesCommand
        {
            get
            {
                return _userPreferencesCommand
                    ?? (_userPreferencesCommand = new RelayCommand(
                    () =>
                    {
                        if (!_startupViewModel.UserPreferencesWindow.IsVisible)
                        {
                            var vm = _startupViewModel.UserPreferencesWindow.DataContext as UserPreferencesViewModel;
                            vm.LoadSettings();

                            _startupViewModel.UserPreferencesWindow.Show();
                        }

                        _startupViewModel.UserPreferencesWindow.Activate();
                    }));
            }
        }


        private RelayCommand _viewLogsCommand;

        public RelayCommand ViewLogsCommand
        {
            get
            {
                return _viewLogsCommand
                    ?? (_viewLogsCommand = new RelayCommand(
                    () =>
                    {
                        _commonService.LocateDirInExplorer(_robotPathConfigService.LogsDir);
                    }));
            }
        }




        private RelayCommand _viewBrowserExtensionsCommand;

        /// <summary>
        /// Gets the ViewBrowserExtensionsCommand.
        /// </summary>
        public RelayCommand ViewBrowserExtensionsCommand
        {
            get
            {
                return _viewBrowserExtensionsCommand
                    ?? (_viewBrowserExtensionsCommand = new RelayCommand(
                    () =>
                    {
                        var window = new BrowserWindow();
                        window.Owner = _view;
                        window.ShowDialog();
                    }));
            }
        }






        private RelayCommand _viewScreenRecordersCommand;

        public RelayCommand ViewScreenRecordersCommand
        {
            get
            {
                return _viewScreenRecordersCommand
                    ?? (_viewScreenRecordersCommand = new RelayCommand(
                    () =>
                    {
                        _commonService.LocateDirInExplorer(_robotPathConfigService.ScreenRecorderDir);
                    }));
            }
        }






        private RelayCommand _aboutProductCommand;

        public RelayCommand AboutProductCommand
        {
            get
            {
                return _aboutProductCommand
                    ?? (_aboutProductCommand = new RelayCommand(
                    () =>
                    {
                        if (!_startupViewModel.AboutWindow.IsVisible)
                        {
                            var vm = _startupViewModel.AboutWindow.DataContext as AboutViewModel;
                            vm.LoadAboutInfo();

                            _startupViewModel.AboutWindow.Show();
                        }

                        _startupViewModel.AboutWindow.Activate();
                    }));
            }
        }



        private RelayCommand _scheduledTaskManagementCommand;

        /// <summary>
        /// Gets the ScheduledTaskManagementCommand.
        /// </summary>
        public RelayCommand ScheduledTaskManagementCommand
        {
            get
            {
                return _scheduledTaskManagementCommand
                    ?? (_scheduledTaskManagementCommand = new RelayCommand(
                    () =>
                    {
                        var window = new ScheduledTaskManagementWindow();
                        var stmVM = _serviceLocator.ResolveType<ScheduledTaskManagementViewModel>();
                        stmVM.RefreshScheduledTaskItems();

                        var stVM = _serviceLocator.ResolveType<ScheduledTaskViewModel>();
                        stVM.Reset(true);

                        foreach (var item in PackageItems)
                        {
                            var spItem = _serviceLocator.ResolveType<ScheduledPackageItemViewModel>();
                            spItem.Name = item.Name;
                            spItem.Version = item.Version;
                            stVM.ScheduledPackageItems.Add(spItem);
                        }

                        window.Owner = _view;
                        window.ShowDialog();
                    }));
            }
        }




        /// <summary>
        /// The <see cref="PackageItems" /> property's name.
        /// </summary>
        public const string PackageItemsPropertyName = "PackageItems";

        private ObservableCollection<PackageItemViewModel> _packageItemsProperty = new ObservableCollection<PackageItemViewModel>();

        public ObservableCollection<PackageItemViewModel> PackageItems
        {
            get
            {
                return _packageItemsProperty;
            }

            set
            {
                if (_packageItemsProperty == value)
                {
                    return;
                }

                _packageItemsProperty = value;
                RaisePropertyChanged(PackageItemsPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsSearchResultEmpty" /> property's name.
        /// </summary>
        public const string IsSearchResultEmptyPropertyName = "IsSearchResultEmpty";

        private bool _isSearchResultEmptyProperty = false;

        public bool IsSearchResultEmpty
        {
            get
            {
                return _isSearchResultEmptyProperty;
            }

            set
            {
                if (_isSearchResultEmptyProperty == value)
                {
                    return;
                }

                _isSearchResultEmptyProperty = value;
                RaisePropertyChanged(IsSearchResultEmptyPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="SearchText" /> property's name.
        /// </summary>
        public const string SearchTextPropertyName = "SearchText";

        private string _searchTextProperty = "";

        public string SearchText
        {
            get
            {
                return _searchTextProperty;
            }

            set
            {
                if (_searchTextProperty == value)
                {
                    return;
                }

                _searchTextProperty = value;
                RaisePropertyChanged(SearchTextPropertyName);

                doSearch();
            }
        }


        private void doSearch()
        {
            var searchContent = SearchText.Trim();
            if (string.IsNullOrEmpty(searchContent))
            {
                foreach (var item in PackageItems)
                {
                    item.IsSearching = false;
                }

                foreach (var item in PackageItems)
                {
                    item.SearchText = searchContent;
                }

                IsSearchResultEmpty = false;
            }
            else
            {
                foreach (var item in PackageItems)
                {
                    item.IsSearching = true;
                }

                foreach (var item in PackageItems)
                {
                    item.IsMatch = false;
                }


                foreach (var item in PackageItems)
                {
                    item.ApplyCriteria(searchContent);
                }

                IsSearchResultEmpty = true;
                foreach (var item in PackageItems)
                {
                    if (item.IsMatch)
                    {
                        IsSearchResultEmpty = false;
                        break;
                    }
                }

            }
        }




        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The <see cref="IsWorkflowRunning" /> property's name.
        /// </summary>
        public const string IsWorkflowRunningPropertyName = "IsWorkflowRunning";

        private bool _isWorkflowRunningProperty = false;

        public bool IsWorkflowRunning
        {
            get
            {
                return _isWorkflowRunningProperty;
            }

            set
            {
                if (_isWorkflowRunningProperty == value)
                {
                    return;
                }

                _isWorkflowRunningProperty = value;
                RaisePropertyChanged(IsWorkflowRunningPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="WorkflowRunningToolTip" /> property's name.
        /// </summary>
        public const string WorkflowRunningToolTipPropertyName = "WorkflowRunningToolTip";

        private string _workflowRunningToolTipProperty = "";

        public string WorkflowRunningToolTip
        {
            get
            {
                return _workflowRunningToolTipProperty;
            }

            set
            {
                if (_workflowRunningToolTipProperty == value)
                {
                    return;
                }

                _workflowRunningToolTipProperty = value;
                RaisePropertyChanged(WorkflowRunningToolTipPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="WorkflowRunningName" /> property's name.
        /// </summary>
        public const string WorkflowRunningNamePropertyName = "WorkflowRunningName";

        private string _workflowRunningNameProperty = "";

        public string WorkflowRunningName
        {
            get
            {
                return _workflowRunningNameProperty;
            }

            set
            {
                if (_workflowRunningNameProperty == value)
                {
                    return;
                }

                _workflowRunningNameProperty = value;
                RaisePropertyChanged(WorkflowRunningNamePropertyName);
            }
        }





        /// <summary>
        /// The <see cref="WorkflowRunningStatus" /> property's name.
        /// </summary>
        public const string WorkflowRunningStatusPropertyName = "WorkflowRunningStatus";

        private string _workflowRunningStatusProperty = "";

        public string WorkflowRunningStatus
        {
            get
            {
                return _workflowRunningStatusProperty;
            }

            set
            {
                if (_workflowRunningStatusProperty == value)
                {
                    return;
                }

                _workflowRunningStatusProperty = value;
                RaisePropertyChanged(WorkflowRunningStatusPropertyName);
            }
        }




        private RelayCommand _stopCommand;

        public RelayCommand StopCommand
        {
            get
            {
                return _stopCommand
                    ?? (_stopCommand = new RelayCommand(
                    () =>
                    {
                        if (_runManagerService != null)
                        {
                            _runManagerService.Stop();
                        }
                    },
                    () => true));
            }
        }


    }
}