using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Librarys;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.UI;
using Chainbot.Cores.Config;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SettingsPageViewModel : ViewModelBase
    {
        private IDialogService _dialogService;

        private IProjectUserConfigService _projectUserConfigService;

        public class ThemeType
        {
            public string Name { get; set; }
            public GlobalConfig.enTheme Type { get; set; }
        }

        public class LanguageType
        {
            public string Name { get; set; }
            public GlobalConfig.enLanguage Type { get; set; }
        }

        public enum enUpdateSettings
        {
            Theme,
            Language,
            PythonPath,
            ProjectsDefaultCreatePath,
            ControlServer,
            AIServer
        }

        private MainViewModel _mainViewModel;

        private IAppSettingsConfigService _appSettingsConfigService;

        private IMessageBoxService _messageBoxService;

        private IServerSettingsService _controlServerConfigService;

        /// <summary>
        /// Initializes a new instance of the SettingsPageViewModel class.
        /// </summary>
        public SettingsPageViewModel(IAppSettingsConfigService appSettingsConfigService, IMessageBoxService messageBoxService
            , MainViewModel mainViewModel,IDialogService dialogService, IProjectUserConfigService projectUserConfigService
            , IServerSettingsService controlServerConfigService)
        {
            _appSettingsConfigService = appSettingsConfigService;
            _messageBoxService = messageBoxService;
            _mainViewModel = mainViewModel;
            _dialogService = dialogService;
            _projectUserConfigService = projectUserConfigService;
            _controlServerConfigService = controlServerConfigService;

            Init();
        }

        private void Init()
        {
            ThemeList.Clear();
            ThemeList.Add(new ThemeType { Name = Chainbot.Resources.Properties.Resources.Listitem_LightTheme, Type = GlobalConfig.enTheme.Light });
            ThemeList.Add(new ThemeType { Name = Chainbot.Resources.Properties.Resources.Listitem_DarkTheme, Type = GlobalConfig.enTheme.Dark });

            LanguageList.Clear();
            LanguageList.Add(new LanguageType { Name = "English", Type = GlobalConfig.enLanguage.English });
            LanguageList.Add(new LanguageType { Name = "中文(简体)", Type = GlobalConfig.enLanguage.Chinese });

            PythonCustomLocation = (Environment.GetEnvironmentVariable("CHAINBOT_PYTHON_PATH", EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable("CHAINBOT_PYTHON_PATH", EnvironmentVariableTarget.Machine));

            _projectUserConfigService.Load();
            ProjectsCustomLocation = _projectUserConfigService.ProjectCreatePath;

            _controlServerConfigService.Load();
            ControlServerCustomLocation = _controlServerConfigService.ControlServerUrl;
            AIServerCustomLocation = _controlServerConfigService.AIServerUrl;
        }

        public const string ThemeListPropertyName = "ThemeList";

        private ObservableCollection<ThemeType> _themeListProperty = new ObservableCollection<ThemeType>();

        /// <summary>
        /// Sets and gets the ThemeList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ThemeType> ThemeList
        {
            get
            {
                return _themeListProperty;
            }

            set
            {
                if (_themeListProperty == value)
                {
                    return;
                }

                _themeListProperty = value;
                RaisePropertyChanged(ThemeListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LanguageList" /> property's name.
        /// </summary>
        public const string LanguageListPropertyName = "LanguageList";

        private ObservableCollection<LanguageType> _languageListProperty = new ObservableCollection<LanguageType>();

        /// <summary>
        /// Sets and gets the LanguageList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<LanguageType>  LanguageList
        {
            get
            {
                return _languageListProperty;
            }

            set
            {
                if (_languageListProperty == value)
                {
                    return;
                }

                _languageListProperty = value;
                RaisePropertyChanged(LanguageListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Theme" /> property's name.
        /// </summary>
        public const string ThemePropertyName = "Theme";

        private GlobalConfig.enTheme _themeProperty = GlobalConfig.CurrentTheme;

        /// <summary>
        /// Sets and gets the Theme property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GlobalConfig.enTheme Theme
        {
            get
            {
                return _themeProperty;
            }

            set
            {
                if (_themeProperty == value)
                {
                    return;
                }

                _themeProperty = value;
                RaisePropertyChanged(ThemePropertyName);

                UpdateSettings(enUpdateSettings.Theme);
            }
        }

        /// <summary>
        /// The <see cref="Language" /> property's name.
        /// </summary>
        public const string LanguagePropertyName = "Language";

        private GlobalConfig.enLanguage _languageProperty = GlobalConfig.CurrentLanguage;

        /// <summary>
        /// Sets and gets the Language property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GlobalConfig.enLanguage Language
        {
            get
            {
                return _languageProperty;
            }

            set
            {
                if (_languageProperty == value)
                {
                    return;
                }

                _languageProperty = value;
                RaisePropertyChanged(LanguagePropertyName);

                UpdateSettings(enUpdateSettings.Language);
            }
        }

        private void UpdateSettings(enUpdateSettings operate)
        {
            Task.Run(() =>
            {
                try
                {
                    switch(operate)
                    {
                        case enUpdateSettings.Theme:
                            {
                                GlobalConfig.CurrentTheme = Theme;
                                _appSettingsConfigService.CurrentTheme = Theme;

                                Common.RunInUI(() => {
                                    if (_messageBoxService.ShowWarningYesNo(Chainbot.Resources.Properties.Resources.Question_Theme, true))
                                    {
                                        _mainViewModel.ClosingEvent += OnRestartEvent;
                                        _mainViewModel.View.Close();
                                        _mainViewModel.ClosingEvent -= OnRestartEvent;
                                    }
                                });
                            }
                            break;
                        case enUpdateSettings.Language:
                            {
                                GlobalConfig.CurrentLanguage = Language;
                                _appSettingsConfigService.CurrentLanguage = Language;

                                Common.RunInUI(() => {
                                    if (_messageBoxService.ShowWarningYesNo(Chainbot.Resources.Properties.Resources.Question_Language, true))
                                    {
                                        _mainViewModel.ClosingEvent += OnRestartEvent;
                                        _mainViewModel.View.Close();
                                        _mainViewModel.ClosingEvent -= OnRestartEvent;
                                    }
                                });
                            }
                            break;
                        case enUpdateSettings.PythonPath:
                            {
                                if (string.IsNullOrEmpty(PythonCustomLocation))
                                {
                                    Environment.SetEnvironmentVariable("CHAINBOT_PYTHON_PATH", null, EnvironmentVariableTarget.User);
                                    Environment.SetEnvironmentVariable("CHAINBOT_PYTHON_PATH", null, EnvironmentVariableTarget.Machine);
                                }
                                else
                                {
                                    Environment.SetEnvironmentVariable("CHAINBOT_PYTHON_PATH", PythonCustomLocation, EnvironmentVariableTarget.User);
                                }
                            }
                            break;
                        case enUpdateSettings.ProjectsDefaultCreatePath:
                            {
                                _projectUserConfigService.ProjectCreatePath = ProjectsCustomLocation;
                                _projectUserConfigService.Save();
                            }
                            break;
                        case enUpdateSettings.ControlServer:
                            {
                                _controlServerConfigService.ControlServerUrl = ControlServerCustomLocation;
                                _controlServerConfigService.Save();
                            }
                            break;
                        case enUpdateSettings.AIServer:
                            {
                                _controlServerConfigService.AIServerUrl = AIServerCustomLocation;
                                _controlServerConfigService.Save();
                            }
                            break;
                    }
                }
                catch (Exception err)
                {
                    Common.RunInUI(() => {
                        _messageBoxService.ShowError(err.ToString());
                    });
                    
                }
                
            });
        }

        private void OnRestartEvent(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, "ChainbotStudio.exe"));
        }

        /// <summary>
        /// The <see cref="ProjectsCustomLocation" /> property's name.
        /// </summary>
        public const string ProjectsCustomLocationPropertyName = "ProjectsCustomLocation";

        private string _projectsCustomLocationProperty = "";

        /// <summary>
        /// Sets and gets the ProjectsCustomLocation property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProjectsCustomLocation
        {
            get
            {
                return _projectsCustomLocationProperty;
            }

            set
            {
                if (_projectsCustomLocationProperty == value)
                {
                    return;
                }

                _projectsCustomLocationProperty = value;
                RaisePropertyChanged(ProjectsCustomLocationPropertyName);


                if (!string.IsNullOrEmpty(value))
                {
                    IsProjectsCustomLocationCorrect = System.IO.Directory.Exists(value);
                    if (!IsProjectsCustomLocationCorrect)
                    {
                        ProjectsCustomLocationValidatedWrongTip = Chainbot.Resources.Properties.Resources.SettingsPage_ValidateWrong1;
                        return;
                    }
                }
                else
                {
                    IsProjectsCustomLocationCorrect = true;
                }

                UpdateSettings(enUpdateSettings.ProjectsDefaultCreatePath);
            }
        }

        private RelayCommand _selectProjectsCustomLocationCommand;

        /// <summary>
        /// Gets the SelectProjectsCustomLocationCommand.
        /// </summary>
        public RelayCommand SelectProjectsCustomLocationCommand
        {
            get
            {
                return _selectProjectsCustomLocationCommand
                    ?? (_selectProjectsCustomLocationCommand = new RelayCommand(
                    () =>
                    {
                        string dst_dir = "";
                        if (_dialogService.ShowSelectDirDialog(Chainbot.Resources.Properties.Resources.SettingsPage_SelectProjectDirectory, ref dst_dir))
                        {
                            ProjectsCustomLocation = dst_dir;
                        }
                    }));
            }
        }


        /// <summary>
        /// The <see cref="IsProjectsCustomLocationCorrect" /> property's name.
        /// </summary>
        public const string IsProjectsCustomLocationCorrectPropertyName = "IsProjectsCustomLocationCorrect";

        private bool _isProjectsCustomLocationCorrectProperty = false;

        /// <summary>
        /// Sets and gets the IsProjectsCustomLocationCorrect property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsProjectsCustomLocationCorrect
        {
            get
            {
                return _isProjectsCustomLocationCorrectProperty;
            }

            set
            {
                if (_isProjectsCustomLocationCorrectProperty == value)
                {
                    return;
                }

                _isProjectsCustomLocationCorrectProperty = value;
                RaisePropertyChanged(IsProjectsCustomLocationCorrectPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ProjectsCustomLocationValidatedWrongTip" /> property's name.
        /// </summary>
        public const string ProjectsCustomLocationValidatedWrongTipPropertyName = "ProjectsCustomLocationValidatedWrongTip";

        private string _projectsCustomLocationValidatedWrongTipProperty = "";

        /// <summary>
        /// Sets and gets the ProjectsCustomLocationValidatedWrongTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProjectsCustomLocationValidatedWrongTip
        {
            get
            {
                return _projectsCustomLocationValidatedWrongTipProperty;
            }

            set
            {
                if (_projectsCustomLocationValidatedWrongTipProperty == value)
                {
                    return;
                }

                _projectsCustomLocationValidatedWrongTipProperty = value;
                RaisePropertyChanged(ProjectsCustomLocationValidatedWrongTipPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsPythonCustomLocationCorrect" /> property's name.
        /// </summary>
        public const string IsPythonCustomLocationCorrectPropertyName = "IsPythonCustomLocationCorrect";

        private bool _isPythonCustomLocationCorrectProperty = true;

        /// <summary>
        /// Sets and gets the IsPythonCustomLocationCorrect property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPythonCustomLocationCorrect
        {
            get
            {
                return _isPythonCustomLocationCorrectProperty;
            }

            set
            {
                if (_isPythonCustomLocationCorrectProperty == value)
                {
                    return;
                }

                _isPythonCustomLocationCorrectProperty = value;
                RaisePropertyChanged(IsPythonCustomLocationCorrectPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="PythonCustomLocationValidatedWrongTip" /> property's name.
        /// </summary>
        public const string PythonCustomLocationValidatedWrongTipPropertyName = "PythonCustomLocationValidatedWrongTip";

        private string _pythonCustomLocationValidatedWrongTipProperty = "";

        /// <summary>
        /// Sets and gets the PythonCustomLocationValidatedWrongTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PythonCustomLocationValidatedWrongTip
        {
            get
            {
                return _pythonCustomLocationValidatedWrongTipProperty;
            }

            set
            {
                if (_pythonCustomLocationValidatedWrongTipProperty == value)
                {
                    return;
                }

                _pythonCustomLocationValidatedWrongTipProperty = value;
                RaisePropertyChanged(PythonCustomLocationValidatedWrongTipPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="PythonCustomLocation" /> property's name.
        /// </summary>
        public const string PythonCustomLocationPropertyName = "PythonCustomLocation";

        private string _pythonCustomLocationProperty = "";

        /// <summary>
        /// Sets and gets the PythonCustomLocation property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PythonCustomLocation
        {
            get
            {
                return _pythonCustomLocationProperty;
            }

            set
            {
                if (_pythonCustomLocationProperty == value)
                {
                    return;
                }

                _pythonCustomLocationProperty = value;
                RaisePropertyChanged(PythonCustomLocationPropertyName);

                if(!string.IsNullOrEmpty(value))
                {
                    IsPythonCustomLocationCorrect = System.IO.Directory.Exists(value);
                    if (!IsPythonCustomLocationCorrect)
                    {
                        PythonCustomLocationValidatedWrongTip = Chainbot.Resources.Properties.Resources.SettingsPage_ValidateWrong1;
                        return;
                    }

                    var python3_dll_file = Path.Combine(value, "python3.dll");
                    if(!File.Exists(python3_dll_file))
                    {
                        IsPythonCustomLocationCorrect = false;
                        PythonCustomLocationValidatedWrongTip = Chainbot.Resources.Properties.Resources.SettingsPage_ValidateWrong2;
                        return;
                    }
                }
                else
                {
                    IsPythonCustomLocationCorrect = true;
                }

                UpdateSettings(enUpdateSettings.PythonPath);
            }
        }

        private RelayCommand _selectPythonCustomLocationCommand;

        /// <summary>
        /// Gets the SelectPythonCustomLocationCommand.
        /// </summary>
        public RelayCommand SelectPythonCustomLocationCommand
        {
            get
            {
                return _selectPythonCustomLocationCommand
                    ?? (_selectPythonCustomLocationCommand = new RelayCommand(
                    () =>
                    {
                        string dst_dir = "";
                        if (_dialogService.ShowSelectDirDialog(Chainbot.Resources.Properties.Resources.SettingsPage_SelectPythonDirectory, ref dst_dir))
                        {
                            PythonCustomLocation = dst_dir;
                        }
                    }));
            }
        }


        private bool controlServerUrlValidate(string value)
        {
            IsControlServerCustomLocationCorrect = true;
            if (!string.IsNullOrEmpty(value))
            {
                if (!value.ToLower().StartsWith("http:") && !value.ToLower().StartsWith("https:"))
                {
                    IsControlServerCustomLocationCorrect = false;
                    ControlServerCustomLocationValidatedWrongTip = Chainbot.Resources.Properties.Resources.SettingsPage_ValidateWrong3;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The <see cref="ControlServerCustomLocation" /> property's name.
        /// </summary>
        public const string ControlServerCustomLocationPropertyName = "ControlServerCustomLocation";

        private string _controlServerCustomLocationProperty = "";

        /// <summary>
        /// Sets and gets the ControlServerCustomLocation property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ControlServerCustomLocation
        {
            get
            {
                return _controlServerCustomLocationProperty;
            }

            set
            {
                if (_controlServerCustomLocationProperty == value)
                {
                    return;
                }

                _controlServerCustomLocationProperty = value;
                RaisePropertyChanged(ControlServerCustomLocationPropertyName);

                if(controlServerUrlValidate(value))
                {
                    UpdateSettings(enUpdateSettings.ControlServer);
                }
            }
        }

        /// <summary>
        /// The <see cref="IsControlServerCustomLocationCorrect" /> property's name.
        /// </summary>
        public const string IsControlServerCustomLocationCorrectPropertyName = "IsControlServerCustomLocationCorrect";

        private bool _isControlServerCustomLocationCorrectProperty = true;

        /// <summary>
        /// Sets and gets the IsControlServerCustomLocationCorrect property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsControlServerCustomLocationCorrect
        {
            get
            {
                return _isControlServerCustomLocationCorrectProperty;
            }

            set
            {
                if (_isControlServerCustomLocationCorrectProperty == value)
                {
                    return;
                }

                _isControlServerCustomLocationCorrectProperty = value;
                RaisePropertyChanged(IsControlServerCustomLocationCorrectPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ControlServerCustomLocationValidatedWrongTip" /> property's name.
        /// </summary>
        public const string ControlServerCustomLocationValidatedWrongTipPropertyName = "ControlServerCustomLocationValidatedWrongTip";

        private string _controlServerCustomLocationValidatedWrongTipProperty = "";

        /// <summary>
        /// Sets and gets the ControlServerCustomLocationValidatedWrongTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ControlServerCustomLocationValidatedWrongTip
        {
            get
            {
                return _controlServerCustomLocationValidatedWrongTipProperty;
            }

            set
            {
                if (_controlServerCustomLocationValidatedWrongTipProperty == value)
                {
                    return;
                }

                _controlServerCustomLocationValidatedWrongTipProperty = value;
                RaisePropertyChanged(ControlServerCustomLocationValidatedWrongTipPropertyName);
            }
        }

        private bool AIServerUrlValidate(string value)
        {
            IsAIServerCustomLocationCorrect = true;
            if (!string.IsNullOrEmpty(value))
            {
                if (!value.ToLower().StartsWith("http:") && !value.ToLower().StartsWith("https:"))
                {
                    IsAIServerCustomLocationCorrect = false;
                    AIServerCustomLocationValidatedWrongTip = Chainbot.Resources.Properties.Resources.SettingsPage_ValidateWrong4;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The <see cref="AIServerCustomLocation" /> property's name.
        /// </summary>
        public const string AIServerCustomLocationPropertyName = "AIServerCustomLocation";

        private string _aiServerCustomLocationProperty = "";

        /// <summary>
        /// Sets and gets the AIServerCustomLocation property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string AIServerCustomLocation
        {
            get
            {
                return _aiServerCustomLocationProperty;
            }

            set
            {
                if (_aiServerCustomLocationProperty == value)
                {
                    return;
                }

                _aiServerCustomLocationProperty = value;
                RaisePropertyChanged(AIServerCustomLocationPropertyName);

                if (AIServerUrlValidate(value))
                {
                    UpdateSettings(enUpdateSettings.AIServer);
                }
            }
        }

        /// <summary>
        /// The <see cref="IsAIServerCustomLocationCorrect" /> property's name.
        /// </summary>
        public const string IsAIServerCustomLocationCorrectPropertyName = "IsAIServerCustomLocationCorrect";

        private bool _isAIServerCustomLocationCorrectProperty = true;

        /// <summary>
        /// Sets and gets the IsAIServerCustomLocationCorrect property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAIServerCustomLocationCorrect
        {
            get
            {
                return _isAIServerCustomLocationCorrectProperty;
            }

            set
            {
                if (_isAIServerCustomLocationCorrectProperty == value)
                {
                    return;
                }

                _isAIServerCustomLocationCorrectProperty = value;
                RaisePropertyChanged(IsAIServerCustomLocationCorrectPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="AIServerCustomLocationValidatedWrongTip" /> property's name.
        /// </summary>
        public const string AIServerCustomLocationValidatedWrongTipPropertyName = "AIServerCustomLocationValidatedWrongTip";

        private string _aiServerCustomLocationValidatedWrongTipProperty = "";

        /// <summary>
        /// Sets and gets the AIServerCustomLocationValidatedWrongTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string AIServerCustomLocationValidatedWrongTip
        {
            get
            {
                return _aiServerCustomLocationValidatedWrongTipProperty;
            }

            set
            {
                if (_aiServerCustomLocationValidatedWrongTipProperty == value)
                {
                    return;
                }

                _aiServerCustomLocationValidatedWrongTipProperty = value;
                RaisePropertyChanged(AIServerCustomLocationValidatedWrongTipPropertyName);
            }
        }
    }
}