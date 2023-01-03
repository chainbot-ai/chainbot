using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Utils;
using ChainbotRobot.Contracts;
using System.ComponentModel;
using System.Windows;
using System.Xml;
using System;
using ChainbotRobot.Librarys;

namespace ChainbotRobot.ViewModel
{
    public class UserPreferencesViewModel : ViewModelBase
    {
        private IServiceLocator _serviceLocator;

        private IRobotPathConfigService _robotPathConfigService;

        private IAutoCloseMessageBoxService _autoCloseMessageBoxService;

        private ICommonService _commonService;

        public Window m_view { get; set; }

        /// <summary>
        /// Initializes a new instance of the UserPreferencesViewModel class.
        /// </summary>
        public UserPreferencesViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _robotPathConfigService = _serviceLocator.ResolveType<IRobotPathConfigService>();

            _autoCloseMessageBoxService = _serviceLocator.ResolveType<IAutoCloseMessageBoxService>();

            _commonService = _serviceLocator.ResolveType<ICommonService>();
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
                        m_view = (Window)p.Source;
                    }));
            }
        }

        public void LoadSettings()
        {
            UpgradeSettings();
            XmlDocument doc = new XmlDocument();
            var path = _robotPathConfigService.AppDataDir + @"\Config\ChainbotRobotSettings.xml";
            doc.Load(path);
            var rootNode = doc.DocumentElement;
            var userSettingsElement = rootNode.SelectSingleNode("UserSettings") as XmlElement;

            var isAutoRunElement = userSettingsElement.SelectSingleNode("IsAutoRun") as XmlElement;
            if (isAutoRunElement.InnerText.ToLower().Trim() == "true")
            {
                SetAutoRun(true);
                this.IsAutoRun = true;
            }
            else
            {
                SetAutoRun(false);
                this.IsAutoRun = false;
            }

            var isAutoOpenMainWindowElement = userSettingsElement.SelectSingleNode("IsAutoOpenMainWindow") as XmlElement;
            if (isAutoOpenMainWindowElement.InnerText.ToLower().Trim() == "true")
            {
                this.IsAutoOpenMainWindow = true;
            }
            else
            {
                this.IsAutoOpenMainWindow = false;
            }


            var isEnableScreenRecorderElement = userSettingsElement.SelectSingleNode("IsEnableScreenRecorder") as XmlElement;
            if (isEnableScreenRecorderElement.InnerText.ToLower().Trim() == "true")
            {
                this.IsEnableScreenRecorder = true;
            }
            else
            {
                this.IsEnableScreenRecorder = false;
            }

            var fpsElement = userSettingsElement.SelectSingleNode("FPS") as XmlElement;
            this.FPS = fpsElement.InnerText.Trim();


            var qualityElement = userSettingsElement.SelectSingleNode("Quality") as XmlElement;
            this.Quality = qualityElement.InnerText.Trim();

            var recordingLimitDaysElement = userSettingsElement.SelectSingleNode("RecordingLimitDays") as XmlElement;
            this.RecordingLimitDays = Convert.ToInt32(recordingLimitDaysElement.InnerText.Trim());


            var isEnableControlServerElement = userSettingsElement.SelectSingleNode("IsEnableControlServer") as XmlElement;
            if (isEnableControlServerElement.InnerText.ToLower().Trim() == "true")
            {
                this.IsEnableControlServer = true;
            }
            else
            {
                this.IsEnableControlServer = false;
            }

            var controlServerUriElement = userSettingsElement.SelectSingleNode("ControlServerUri") as XmlElement;
            this.ControlServerUri = controlServerUriElement.InnerText.Trim();
        }

        private void UpgradeSettings()
        {
            XmlDocument doc = new XmlDocument();
            var path = _robotPathConfigService.AppDataDir + @"\Config\ChainbotRobotSettings.xml";
            doc.Load(path);
            var rootNode = doc.DocumentElement;
            var userSettingsElement = rootNode.SelectSingleNode("UserSettings") as XmlElement;

            var recordingLimitDaysElement = userSettingsElement.SelectSingleNode("RecordingLimitDays") as XmlElement;
            if (recordingLimitDaysElement == null)
            {
                recordingLimitDaysElement = doc.CreateElement("RecordingLimitDays");
                recordingLimitDaysElement.InnerText = "7";

                userSettingsElement.AppendChild(recordingLimitDaysElement);
            }

            doc.Save(path);

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
                        m_view.DragMove();
                    }));
            }
        }



        private RelayCommand<CancelEventArgs> _closingCommand;

        public RelayCommand<CancelEventArgs> ClosingCommand
        {
            get
            {
                return _closingCommand
                    ?? (_closingCommand = new RelayCommand<CancelEventArgs>(
                    e =>
                    {
                        e.Cancel = true;
                        m_view.Hide();
                    }));
            }
        }

        /// <summary>
        /// The <see cref="IsAutoRun" /> property's name.
        /// </summary>
        public const string IsAutoRunPropertyName = "IsAutoRun";

        private bool _isAutoRunProperty = false;

        public bool IsAutoRun
        {
            get
            {
                return _isAutoRunProperty;
            }

            set
            {
                if (_isAutoRunProperty == value)
                {
                    return;
                }

                _isAutoRunProperty = value;
                RaisePropertyChanged(IsAutoRunPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsAutoOpenMainWindow" /> property's name.
        /// </summary>
        public const string IsAutoOpenMainWindowPropertyName = "IsAutoOpenMainWindow";

        private bool _isAutoOpenMainWindowProperty = false;

        public bool IsAutoOpenMainWindow
        {
            get
            {
                return _isAutoOpenMainWindowProperty;
            }

            set
            {
                if (_isAutoOpenMainWindowProperty == value)
                {
                    return;
                }

                _isAutoOpenMainWindowProperty = value;
                RaisePropertyChanged(IsAutoOpenMainWindowPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsEnableScreenRecorder" /> property's name.
        /// </summary>
        public const string IsEnableScreenRecorderPropertyName = "IsEnableScreenRecorder";

        private bool _isEnableScreenRecorderProperty = false;

        public bool IsEnableScreenRecorder
        {
            get
            {
                return _isEnableScreenRecorderProperty;
            }

            set
            {
                if (_isEnableScreenRecorderProperty == value)
                {
                    return;
                }

                _isEnableScreenRecorderProperty = value;
                RaisePropertyChanged(IsEnableScreenRecorderPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="FPS" /> property's name.
        /// </summary>
        public const string FPSPropertyName = "FPS";

        private string _fpsProperty = "0";

        public string FPS
        {
            get
            {
                return _fpsProperty;
            }

            set
            {
                if (_fpsProperty == value)
                {
                    return;
                }

                _fpsProperty = value;
                RaisePropertyChanged(FPSPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Quality" /> property's name.
        /// </summary>
        public const string QualityPropertyName = "Quality";

        private string _qualityProperty = "0";

        public string Quality
        {
            get
            {
                return _qualityProperty;
            }

            set
            {
                if (_qualityProperty == value)
                {
                    return;
                }

                _qualityProperty = value;
                RaisePropertyChanged(QualityPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="RecordingLimitDays" /> property's name.
        /// </summary>
        public const string RecordingLimitDaysPropertyName = "RecordingLimitDays";

        private int _recordingLimitDaysProperty = 7;

        /// <summary>
        /// Sets and gets the RecordingLimitDays property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int RecordingLimitDays
        {
            get
            {
                return _recordingLimitDaysProperty;
            }

            set
            {
                if (_recordingLimitDaysProperty == value)
                {
                    return;
                }

                _recordingLimitDaysProperty = value;
                RaisePropertyChanged(RecordingLimitDaysPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsEnableControlServer" /> property's name.
        /// </summary>
        public const string IsEnableControlServerPropertyName = "IsEnableControlServer";

        private bool _isEnableControlServerProperty = false;

        public bool IsEnableControlServer
        {
            get
            {
                return _isEnableControlServerProperty;
            }

            set
            {
                if (_isEnableControlServerProperty == value)
                {
                    return;
                }

                _isEnableControlServerProperty = value;
                RaisePropertyChanged(IsEnableControlServerPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ControlServerUri" /> property's name.
        /// </summary>
        public const string ControlServerUriPropertyName = "ControlServerUri";

        private string _controlServerUriProperty = "";

        public string ControlServerUri
        {
            get
            {
                var ret = _controlServerUriProperty;
                if (ret.EndsWith("/"))
                {
                    ret = ret.Substring(0, ret.Length - 1);
                }
                return ret;
            }

            set
            {
                if (_controlServerUriProperty == value)
                {
                    return;
                }

                _controlServerUriProperty = value;
                RaisePropertyChanged(ControlServerUriPropertyName);
            }
        }




        private void UpdateSettings()
        {
            XmlDocument doc = new XmlDocument();
            var path = _robotPathConfigService.AppDataDir + @"\Config\ChainbotRobotSettings.xml";
            doc.Load(path);
            var rootNode = doc.DocumentElement;
            var userSettingsElement = rootNode.SelectSingleNode("UserSettings") as XmlElement;

            var isAutoRunElement = userSettingsElement.SelectSingleNode("IsAutoRun") as XmlElement;
            isAutoRunElement.InnerText = IsAutoRun ? "True" : "False";

            var isAutoOpenMainWindowElement = userSettingsElement.SelectSingleNode("IsAutoOpenMainWindow") as XmlElement;
            isAutoOpenMainWindowElement.InnerText = IsAutoOpenMainWindow ? "True" : "False";

            var isEnableScreenRecorderElement = userSettingsElement.SelectSingleNode("IsEnableScreenRecorder") as XmlElement;
            isEnableScreenRecorderElement.InnerText = IsEnableScreenRecorder ? "True" : "False";

            var fpsElement = userSettingsElement.SelectSingleNode("FPS") as XmlElement;
            fpsElement.InnerText = FPS;

            var qualityElement = userSettingsElement.SelectSingleNode("Quality") as XmlElement;
            qualityElement.InnerText = Quality;

            var recordingLimitDaysElement = userSettingsElement.SelectSingleNode("RecordingLimitDays") as XmlElement;
            recordingLimitDaysElement.InnerText = RecordingLimitDays.ToString();

            var isEnableControlServerElement = userSettingsElement.SelectSingleNode("IsEnableControlServer") as XmlElement;
            isEnableControlServerElement.InnerText = IsEnableControlServer ? "True" : "False";

            var controlServerUriElement = userSettingsElement.SelectSingleNode("ControlServerUri") as XmlElement;
            controlServerUriElement.InnerText = ControlServerUri;

            doc.Save(path);

            SetAutoRun(IsAutoRun);
        }

        private void SetAutoRun(bool isAutoRun)
        {
            AutoRunControl.AutoRun(isAutoRun);
        }

        private RelayCommand _resetSettingsCommand;

        public RelayCommand ResetSettingsCommand
        {
            get
            {
                return _resetSettingsCommand
                    ?? (_resetSettingsCommand = new RelayCommand(
                    () =>
                    {
                        var ret = _autoCloseMessageBoxService.Show(m_view, Properties.Resources.AreYouSureToReset, Properties.Resources.ConfirmText, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                        if (ret == MessageBoxResult.OK)
                        {
                            IsAutoRun = false;
                            IsAutoOpenMainWindow = true;

                            IsEnableScreenRecorder = false;
                            FPS = "30";
                            Quality = "50";
                            RecordingLimitDays = 7;

                            IsEnableControlServer = false;
                            ControlServerUri = "";

                            UpdateSettings();
                        }

                    }));
            }
        }



        private RelayCommand _okCommand;

        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand
                    ?? (_okCommand = new RelayCommand(
                    () =>
                    {
                        UpdateSettings();

                        m_view.Close();
                    }));
            }
        }


        private RelayCommand _cancelCommand;

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    () =>
                    {
                        m_view.Close();
                    }));
            }
        }

    }
}