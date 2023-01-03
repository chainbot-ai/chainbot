using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using log4net;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using Plugins.Shared.Library.Librarys;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Config;

namespace ChainbotStudio.ViewModel
{
    public class CheckUpgradeViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private HttpDownloadFile m_downloader { get; set; }

        private List<string> m_currentVersionUpdateLogList { get; set; } = new List<string>();

        private string m_autoUpgradePackpageVersion { get; set; }
        private string m_autoUpgradePackpageMd5 { get; set; }
        private string m_autoUpgradePackpageUrl { get; set; }
        private List<string> m_latestVersionUpdateLogList { get; set; } = new List<string>();

        private MainViewModel _mainViewModel;
        private IPathConfigService _pathConfigService;
        private IServerSettingsService _serverSettingsService;
        private IDispatcherService _dispatcherService;
        private ICommonService _commonService;
        private ILogService _logService;
        private IMessageBoxService _messageBoxService;

        /// <summary>
        /// Initializes a new instance of the CheckUpgradeViewModel class.
        /// </summary>
        public CheckUpgradeViewModel(MainViewModel mainViewModel,
            IPathConfigService pathConfigService,
            IServerSettingsService serverSettingsService,
            IDispatcherService dispatcherService,
            ICommonService commonService,  
            ILogService logService, 
            IMessageBoxService messageBoxService)
        {
            _mainViewModel = mainViewModel;
            _pathConfigService = pathConfigService;
            _serverSettingsService = serverSettingsService;
            _dispatcherService = dispatcherService;
            _commonService = commonService;
            _logService = logService;
            _messageBoxService = messageBoxService;

            initChainbotUpgradeClientConfig();
            initCurrentVersionInfo();
        }


        private void OnDownloadFinished(HttpDownloadFile obj)
        {
            _dispatcherService.InvokeAsync(() => {
                if(obj.IsDownloadSuccess)
                {
                    var fileMd5 = Common.GetMD5HashFromFile(obj.SaveFilePath);
                    if (m_autoUpgradePackpageMd5.ToLower() == fileMd5.ToLower())
                    {
                        var saveDir = Path.GetDirectoryName(obj.SaveFilePath);

                        var originFileName = Common.GetFileNameFromUrl(obj.Url);

                        var finishedFilePath = saveDir + @"\" + originFileName;

                        if (File.Exists(finishedFilePath))
                        {
                            File.Delete(finishedFilePath);
                        }

                        File.Move(obj.SaveFilePath, finishedFilePath);

                        if (!File.Exists(finishedFilePath))
                        {
                            _logService.Error(string.Format(Chainbot.Resources.Properties.Resources.UpgradePage_ErrMessage2, obj.SaveFilePath, finishedFilePath), logger);

                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.UpgradePage_ErrMessage3);
                        }
                        else
                        {
                            doInstallUpgradePackage(finishedFilePath);
                        }
                    }
                    else
                    {
                        if (File.Exists(obj.SaveFilePath))
                        {
                            File.Delete(obj.SaveFilePath);
                        }

                        _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.UpgradePage_ErrMessage4);
                    }
                }
                else
                {
                    _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.UpgradePage_ErrMessage5);
                }
                
            });
            
        }


        private void OnDownloading(HttpDownloadFile obj)
        {
            DownloadingProgressValue = (int)(100.0* obj.FileDownloadedBytes/obj.FileTotalBytes);
        }


        private void OnRunningChanged(HttpDownloadFile obj)
        {
            _dispatcherService.InvokeAsync(() => {
                if(obj.IsRunning)
                {
                    IsShowProgressBar = true;
                }

                IsDoUpgradeEnable = !obj.IsRunning;
                DoUpgradeCommand.RaiseCanExecuteChanged();
            });
        }

 
        private void doInstallUpgradePackage(string upgradePackageFilePath)
        {
            if (_messageBoxService.ShowQuestion(Chainbot.Resources.Properties.Resources.UpgradePage_QuestionMessage, true))
            {
                _mainViewModel.SaveAllCommand.Execute(null);
                Common.ShellExecute(upgradePackageFilePath);
            }
        }
        
        private void initChainbotUpgradeClientConfig()
        {
            m_currentVersionUpdateLogList.Clear();

            XmlDocument doc = new XmlDocument();

            using (var ms = new MemoryStream(ChainbotStudio.Properties.Resources.ChainbotUpgradeClientConfig))
            {
                ms.Flush();
                ms.Position = 0;
                doc.Load(ms);
                ms.Close();
            }

            var rootNode = doc.DocumentElement; 
            var updateLogElement = rootNode.SelectSingleNode("UpdateLog");
            var items = updateLogElement.SelectNodes("Item");
            foreach(var item in items)
            {
                var text = (item as XmlElement).InnerText;
                m_currentVersionUpdateLogList.Add(text);
            }
        }

        private bool initChainbotUpgradeServerConfig()
        {
            bool ret = true;

            if(string.IsNullOrEmpty(_serverSettingsService.ControlServerUrl))
            {
                return false;
            }

            string rpaUpgradeServerConfigUrl = _serverSettingsService.ControlServerUrl + "/download/ChainbotUpgradeServerConfig.xml";
            var rpaUpgradeServerConfig = HttpRequest.Get(rpaUpgradeServerConfigUrl);

            if(!string.IsNullOrEmpty(rpaUpgradeServerConfig))
            {
                m_latestVersionUpdateLogList.Clear();

                XmlDocument doc = new XmlDocument();

                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rpaUpgradeServerConfig)))
                {
                    ms.Flush();
                    ms.Position = 0;
                    doc.Load(ms);
                    ms.Close();
                }

                var rootNode = doc.DocumentElement;
                var autoUpgradePackpageElement = rootNode.SelectSingleNode("AutoUpgradePackpage") as XmlElement;
                
                m_autoUpgradePackpageVersion = autoUpgradePackpageElement.GetAttribute("Version");
                m_autoUpgradePackpageMd5 = autoUpgradePackpageElement.GetAttribute("Md5");
                m_autoUpgradePackpageUrl = autoUpgradePackpageElement.GetAttribute("Url");

                var updateLogElement = rootNode.SelectSingleNode("UpdateLog");
                var items = updateLogElement.SelectNodes("Item");
                foreach (var item in items)
                {
                    var text = (item as XmlElement).InnerText;
                    m_latestVersionUpdateLogList.Add(text);
                }

                IsCheckUpgradeSuccess = true;
            }
            else
            {
                ret = false;
                IsCheckUpgradeSuccess = false;
                _logService.Error(Chainbot.Resources.Properties.Resources.UpgradePage_ErrMessage6 + rpaUpgradeServerConfigUrl, logger);
            }

            return ret; 
        }

        private void initCurrentVersionInfo()
        {
            CurrentVersionName = "v"+ _commonService.GetProgramVersion();

            CurrentVersionUpdateLog = "";
            foreach (var item in m_currentVersionUpdateLogList)
            {
                CurrentVersionUpdateLog += " ● " + item + Environment.NewLine;
            }
           
        }

        private void initLatestVersionInfo()
        {
            Version currentVersion = new Version(_commonService.GetProgramVersion());
            Version latestVersion = new Version(m_autoUpgradePackpageVersion);

            if(latestVersion > currentVersion)
            {
                IsNeedUpgrade = true;
                LatestVersionName = "v" + m_autoUpgradePackpageVersion;

                LatestVersionUpdateLog = "";
                foreach (var item in m_latestVersionUpdateLogList)
                {
                    LatestVersionUpdateLog += " ● " + item + Environment.NewLine;
                }
            }
            else
            {
                IsNeedUpgrade = false;
            }

        }

        /// <summary>
        /// The <see cref="DownloadingProgressValue" /> property's name.
        /// </summary>
        public const string DownloadingProgressValuePropertyName = "DownloadingProgressValue";

        private int _downloadingProgressValueProperty = 0;

        public int DownloadingProgressValue
        {
            get
            {
                return _downloadingProgressValueProperty;
            }

            set
            {
                if (_downloadingProgressValueProperty == value)
                {
                    return;
                }

                _downloadingProgressValueProperty = value;
                RaisePropertyChanged(DownloadingProgressValuePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsShowProgressBar" /> property's name.
        /// </summary>
        public const string IsShowProgressBarPropertyName = "IsShowProgressBar";

        private bool _isShowProgressBarProperty = false;


        public bool IsShowProgressBar
        {
            get
            {
                return _isShowProgressBarProperty;
            }

            set
            {
                if (_isShowProgressBarProperty == value)
                {
                    return;
                }

                _isShowProgressBarProperty = value;
                RaisePropertyChanged(IsShowProgressBarPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsDoUpgradeEnable" /> property's name.
        /// </summary>
        public const string IsDoUpgradeEnablePropertyName = "IsDoUpgradeEnable";

        private bool _isDoUpgradeEnableProperty = true;

        public bool IsDoUpgradeEnable
        {
            get
            {
                return _isDoUpgradeEnableProperty;
            }

            set
            {
                if (_isDoUpgradeEnableProperty == value)
                {
                    return;
                }

                _isDoUpgradeEnableProperty = value;
                RaisePropertyChanged(IsDoUpgradeEnablePropertyName);
            }
        }

        public const string IsNeedUpgradePropertyName = "IsNeedUpgrade";

        private bool _isNeedUpgradeProperty = false;

        /// <summary>
        /// Sets and gets the IsNeedUpgrade property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNeedUpgrade
        {
            get
            {
                return _isNeedUpgradeProperty;
            }

            set
            {
                if (_isNeedUpgradeProperty == value)
                {
                    return;
                }

                _isNeedUpgradeProperty = value;
                RaisePropertyChanged(IsNeedUpgradePropertyName);

                _mainViewModel.IsNeedUpgrade = value;
            }
        }

        /// <summary>
        /// The <see cref="IsShowCurrentVersionUpdateLog" /> property's name.
        /// </summary>
        public const string IsShowCurrentVersionUpdateLogPropertyName = "IsShowCurrentVersionUpdateLog";

        private bool _isShowCurrentVersionUpdateLogProperty = true;

        public bool IsShowCurrentVersionUpdateLog
        {
            get
            {
                return _isShowCurrentVersionUpdateLogProperty;
            }

            set
            {
                if (_isShowCurrentVersionUpdateLogProperty == value)
                {
                    return;
                }

                _isShowCurrentVersionUpdateLogProperty = value;
                RaisePropertyChanged(IsShowCurrentVersionUpdateLogPropertyName);

                IsShowLatestVersionUpdateLog = !value;
            }
        }

        /// <summary>
        /// The <see cref="IsShowLatestVersionUpdateLog" /> property's name.
        /// </summary>
        public const string IsShowLatestVersionUpdateLogPropertyName = "IsShowLatestVersionUpdateLog";

        private bool _isShowLatestVersionUpdateLogProperty = false;

        public bool IsShowLatestVersionUpdateLog
        {
            get
            {
                return _isShowLatestVersionUpdateLogProperty;
            }

            set
            {
                if (_isShowLatestVersionUpdateLogProperty == value)
                {
                    return;
                }

                _isShowLatestVersionUpdateLogProperty = value;
                RaisePropertyChanged(IsShowLatestVersionUpdateLogPropertyName);

                IsShowCurrentVersionUpdateLog = !value;
            }
        }

        /// <summary>
        /// The <see cref="CurrentVersionName" /> property's name.
        /// </summary>
        public const string CurrentVersionNamePropertyName = "CurrentVersionName";

        private string _currentVersionNameProperty = "";

        public string CurrentVersionName
        {
            get
            {
                return _currentVersionNameProperty;
            }

            set
            {
                if (_currentVersionNameProperty == value)
                {
                    return;
                }

                _currentVersionNameProperty = value;
                RaisePropertyChanged(CurrentVersionNamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LatestVersionName" /> property's name.
        /// </summary>
        public const string LatestVersionNamePropertyName = "LatestVersionName";

        private string _latestVersionNameProperty = "";

        public string LatestVersionName
        {
            get
            {
                return _latestVersionNameProperty;
            }

            set
            {
                if (_latestVersionNameProperty == value)
                {
                    return;
                }

                _latestVersionNameProperty = value;
                RaisePropertyChanged(LatestVersionNamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CurrentVersionUpdateLog" /> property's name.
        /// </summary>
        public const string CurrentVersionUpdateLogPropertyName = "CurrentVersionUpdateLog";

        private string _currentVersionUpdateLogProperty = "";

        public string CurrentVersionUpdateLog
        {
            get
            {
                return _currentVersionUpdateLogProperty;
            }

            set
            {
                if (_currentVersionUpdateLogProperty == value)
                {
                    return;
                }

                _currentVersionUpdateLogProperty = value;
                RaisePropertyChanged(CurrentVersionUpdateLogPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LatestVersionUpdateLog" /> property's name.
        /// </summary>
        public const string LatestVersionUpdateLogPropertyName = "LatestVersionUpdateLog";

        private string _latestVersionUpdateLogProperty = "";

        public string LatestVersionUpdateLog
        {
            get
            {
                return _latestVersionUpdateLogProperty;
            }

            set
            {
                if (_latestVersionUpdateLogProperty == value)
                {
                    return;
                }

                _latestVersionUpdateLogProperty = value;
                RaisePropertyChanged(LatestVersionUpdateLogPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsCheckUpgradeSuccess" /> property's name.
        /// </summary>
        public const string IsCheckUpgradeSuccessPropertyName = "IsCheckUpgradeSuccess";

        private bool _isCheckUpgradeSuccessProperty = true;

        public bool IsCheckUpgradeSuccess
        {
            get
            {
                return _isCheckUpgradeSuccessProperty;
            }

            set
            {
                if (_isCheckUpgradeSuccessProperty == value)
                {
                    return;
                }

                _isCheckUpgradeSuccessProperty = value;
                RaisePropertyChanged(IsCheckUpgradeSuccessPropertyName);
            }
        }

        private RelayCommand _showCurrentVersionUpdateLogCommand;

        public RelayCommand ShowCurrentVersionUpdateLogCommand
        {
            get
            {
                return _showCurrentVersionUpdateLogCommand
                    ?? (_showCurrentVersionUpdateLogCommand = new RelayCommand(
                    () =>
                    {
                        IsShowCurrentVersionUpdateLog = true;
                    }));
            }
        }

        private RelayCommand _showLatestVersionUpdateLogCommand;


        public RelayCommand ShowLatestVersionUpdateLogCommand
        {
            get
            {
                return _showLatestVersionUpdateLogCommand
                    ?? (_showLatestVersionUpdateLogCommand = new RelayCommand(
                    () =>
                    {
                        IsShowLatestVersionUpdateLog = true;
                    }));
            }
        }

        private RelayCommand _doUpgradeCommand;

        public RelayCommand DoUpgradeCommand
        {
            get
            {
                return _doUpgradeCommand
                    ?? (_doUpgradeCommand = new RelayCommand(
                    () =>
                    {
                        bool hasDownload = false;
                        var originFileName = Common.GetFileNameFromUrl(m_autoUpgradePackpageUrl);
                        var path = Path.Combine(_pathConfigService.UdpateDir, originFileName);
                        if(File.Exists(path) && Common.GetMD5HashFromFile(path).ToLower() == m_autoUpgradePackpageMd5.ToLower())
                        {
                            hasDownload = true;
                        }

                        if(hasDownload)
                        {
                            IsShowProgressBar = true;
                            DownloadingProgressValue = 100;

                            doInstallUpgradePackage(path);
                        }
                        else
                        {
                            IsShowProgressBar = false;
                            var thread = new Thread(downloadThread);
                            thread.Start();
                        }
                    },
                    () => IsDoUpgradeEnable));
            }
        }

        private void downloadThread()
        {
            var path = Path.Combine(_pathConfigService.UdpateDir, m_autoUpgradePackpageMd5 + ".exe");

            if(m_downloader != null)
            {
                m_downloader.Stop();

                if(m_downloader.IsRunning)
                {
                    Thread.Sleep(500);
                }
            }

            m_downloader = new HttpDownloadFile();
            m_downloader.OnRunningChanged = OnRunningChanged;
            m_downloader.OnDownloadFinished = OnDownloadFinished;
            m_downloader.OnDownloading = OnDownloading;
            m_downloader.Download(m_autoUpgradePackpageUrl, path);
        }
     
        private RelayCommand _checkUpgradeCommand;

        public RelayCommand CheckUpgradeCommand
        {
            get
            {
                return _checkUpgradeCommand
                    ?? (_checkUpgradeCommand = new RelayCommand(
                    () =>
                    {
                        Task.Run(() =>
                        {
                            if (initChainbotUpgradeServerConfig())
                            {
                                initLatestVersionInfo();

                                if (IsNeedUpgrade)
                                {
                                    IsShowLatestVersionUpdateLog = true;
                                }
                            }
                        });
                       
                    }));
            }
        }

        
    }
}