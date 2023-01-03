using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using ChainbotStudio.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using Plugins.Shared.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ExportViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Window Window { get; set; }

        private IProjectManagerService _projectManagerService;
        private IDialogService _dialogService;
        private IAppSettingsConfigService _appSettingsConfigService;
        private IMessageBoxService _messageBoxService;
        private ILogService _logService;
        private IConstantConfigService _constantConfigService;
        private IPackageExportService _packageExportService;

        /// <summary>
        /// Initializes a new instance of the ExportViewModel class.
        /// </summary>
        public ExportViewModel(IDialogService dialogService
            , IProjectManagerService projectManagerService
            , IAppSettingsConfigService appSettingsConfigService
            , IMessageBoxService messageBoxService
            , ILogService logService
            , IConstantConfigService constantConfigService
            ,IPackageExportService packageExportService
            )
        {
            _dialogService = dialogService;
            _projectManagerService = projectManagerService;
            _appSettingsConfigService = appSettingsConfigService;
            _messageBoxService = messageBoxService;
            _logService = logService;
            _constantConfigService = constantConfigService;
            _packageExportService = packageExportService;

            InitHistory();
            InitVersionInfo();
        }

        private void InitHistory()
        {
            CustomLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var lastExportDir = _appSettingsConfigService.GetLastExportDir();
            if (System.IO.Directory.Exists(lastExportDir))
            {
                CustomLocation = lastExportDir;
            }else
            {
                CustomLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            var exportDirHistoryList = _appSettingsConfigService.GetExportDirHistoryList();
            if(exportDirHistoryList != null)
            {
                foreach(var item in exportDirHistoryList)
                {
                    CustomLocations.Add(item);
                }
            }
        }

        private void InitVersionInfo()
        {
            CurrentProjectVersion = _projectManagerService.CurrentProjectJsonConfig.projectVersion;

            Version currentVersion = new Version(CurrentProjectVersion);
            Version newVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1);
            NewProjectVersion = newVersion.ToString();
        }

        /// <summary>
        /// The <see cref="CustomLocation" /> property's name.
        /// </summary>
        public const string CustomLocationPropertyName = "CustomLocation";

        private string _customLocationProperty = "";

        public string CustomLocation
        {
            get
            {
                return _customLocationProperty;
            }

            set
            {
                if (_customLocationProperty == value)
                {
                    return;
                }

                _customLocationProperty = value;
                RaisePropertyChanged(CustomLocationPropertyName);

                IsCustomLocationCorrect = System.IO.Directory.Exists(value);
                if (!IsCustomLocationCorrect)
                {
                    CustomLocationValidatedWrongTip = Chainbot.Resources.Properties.Resources.Export_UrlValidateWrong;
                }
            }
        }

        /// <summary>
        /// The <see cref="CustomLocations" /> property's name.
        /// </summary>
        public const string CustomLocationsPropertyName = "CustomLocations";

        private ObservableCollection<string> _customLocationsProperty = new ObservableCollection<string>();

        public ObservableCollection<string> CustomLocations
        {
            get
            {
                return _customLocationsProperty;
            }

            set
            {
                if (_customLocationsProperty == value)
                {
                    return;
                }

                _customLocationsProperty = value;
                RaisePropertyChanged(CustomLocationsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsCustomLocationCorrect" /> property's name.
        /// </summary>
        public const string IsCustomLocationCorrectPropertyName = "IsCustomLocationCorrect";

        private bool _isCustomLocationCorrectProperty = false;

        public bool IsCustomLocationCorrect
        {
            get
            {
                return _isCustomLocationCorrectProperty;
            }

            set
            {
                if (_isCustomLocationCorrectProperty == value)
                {
                    return;
                }

                _isCustomLocationCorrectProperty = value;
                RaisePropertyChanged(IsCustomLocationCorrectPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CustomLocationValidatedWrongTip" /> property's name.
        /// </summary>
        public const string CustomLocationValidatedWrongTipPropertyName = "CustomLocationValidatedWrongTip";

        private string _customLocationValidatedWrongTipProperty = "";

        public string CustomLocationValidatedWrongTip
        {
            get
            {
                return _customLocationValidatedWrongTipProperty;
            }

            set
            {
                if (_customLocationValidatedWrongTipProperty == value)
                {
                    return;
                }

                _customLocationValidatedWrongTipProperty = value;
                RaisePropertyChanged(CustomLocationValidatedWrongTipPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ReleaseNotes" /> property's name.
        /// </summary>
        public const string ReleaseNotesPropertyName = "ReleaseNotes";

        private string _releaseNotesProperty = "";

        public string ReleaseNotes
        {
            get
            {
                return _releaseNotesProperty;
            }

            set
            {
                if (_releaseNotesProperty == value)
                {
                    return;
                }

                _releaseNotesProperty = value;
                RaisePropertyChanged(ReleaseNotesPropertyName);
            }
        }

        private RelayCommand _browserFolderCommand;

        public RelayCommand BrowserFolderCommand
        {
            get
            {
                return _browserFolderCommand
                    ?? (_browserFolderCommand = new RelayCommand(
                    () =>
                    {
                        string dst_dir = "";
                        if (_dialogService.ShowSelectDirDialog(Chainbot.Resources.Properties.Resources.Export_SelectDir, ref dst_dir))
                        {
                            CustomLocation = dst_dir;
                        }
                    }));
            }
        }

        /// <summary>
        /// The <see cref="CurrentProjectVersion" /> property's name.
        /// </summary>
        public const string CurrentProjectVersionPropertyName = "CurrentProjectVersion";

        private string _currentProjectVersionProperty = "";

        public string CurrentProjectVersion
        {
            get
            {
                return _currentProjectVersionProperty;
            }

            set
            {
                if (_currentProjectVersionProperty == value)
                {
                    return;
                }

                _currentProjectVersionProperty = value;
                RaisePropertyChanged(CurrentProjectVersionPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="NewProjectVersion" /> property's name.
        /// </summary>
        public const string NewProjectVersionPropertyName = "NewProjectVersion";

        private string _newProjectVersionProperty = "";

        public string NewProjectVersion
        {
            get
            {
                return _newProjectVersionProperty;
            }

            set
            {
                if (_newProjectVersionProperty == value)
                {
                    return;
                }

                _newProjectVersionProperty = value;
                RaisePropertyChanged(NewProjectVersionPropertyName);

                IsNewProjectVersionCorrect = !string.IsNullOrWhiteSpace(value);
                if (!IsNewProjectVersionCorrect)
                {
                    NewProjectVersionValidatedWrongTip = Chainbot.Resources.Properties.Resources.Publish_VersionValidateWrong1;
                    return;
                }

                try
                {
                    var ver = new Version(value);
                    if (ver.Major >= 0 && ver.Minor >= 0 && ver.Build >= 0 && ver.Revision < 0)
                    {
                        IsNewProjectVersionCorrect = true;
                    }
                    else
                    {
                        IsNewProjectVersionCorrect = false;
                        NewProjectVersionValidatedWrongTip = Chainbot.Resources.Properties.Resources.Publish_VersionValidateWrong2;
                    }
                }
                catch (Exception)
                {
                    IsNewProjectVersionCorrect = false;
                    NewProjectVersionValidatedWrongTip = Chainbot.Resources.Properties.Resources.Publish_VersionValidateWrong3;
                }

            }
        }

        /// <summary>
        /// The <see cref="NewProjectVersionValidatedWrongTip" /> property's name.
        /// </summary>
        public const string NewProjectVersionValidatedWrongTipPropertyName = "NewProjectVersionValidatedWrongTip";

        private string _newProjectVersionValidatedWrongTipProperty = "";

        public string NewProjectVersionValidatedWrongTip
        {
            get
            {
                return _newProjectVersionValidatedWrongTipProperty;
            }

            set
            {
                if (_newProjectVersionValidatedWrongTipProperty == value)
                {
                    return;
                }

                _newProjectVersionValidatedWrongTipProperty = value;
                RaisePropertyChanged(NewProjectVersionValidatedWrongTipPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsNewProjectVersionCorrect" /> property's name.
        /// </summary>
        public const string IsNewProjectVersionCorrectPropertyName = "IsNewProjectVersionCorrect";

        private bool _isNewProjectVersionCorrectProperty = false;

        public bool IsNewProjectVersionCorrect
        {
            get
            {
                return _isNewProjectVersionCorrectProperty;
            }

            set
            {
                if (_isNewProjectVersionCorrectProperty == value)
                {
                    return;
                }

                _isNewProjectVersionCorrectProperty = value;
                RaisePropertyChanged(IsNewProjectVersionCorrectPropertyName);

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
                        Window.Close();
                    }));
            }
        }

        private RelayCommand _okCommand;

        /// <summary>
        /// Gets the OkCommand.
        /// </summary>
        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand
                    ?? (_okCommand = new RelayCommand(
                    () =>
                    {
                        Window.Hide();

                        var nupkgLocation = CustomLocation;

                        try
                        {
                            var publishAuthors = Environment.UserName;
                            var publishVersion = NewProjectVersion;
                            var projectPath = _projectManagerService.CurrentProjectPath;
                            var publishId = _projectManagerService.CurrentProjectJsonConfig.name;
                            var publishDesc = string.IsNullOrWhiteSpace(_projectManagerService.CurrentProjectJsonConfig.description) ? "N/A" : _projectManagerService.CurrentProjectJsonConfig.description;

                            var dependenciesList = new List<NugetPackageItem>();
                            foreach (JProperty jp in (JToken)_projectManagerService.CurrentProjectJsonConfig.dependencies)
                            {
                                dependenciesList.Add(new NugetPackageItem(jp.Name, (string)jp.Value));
                            }


                            UpdateProjectVersion();

                            _packageExportService.Init(publishId, publishVersion, publishDesc, publishAuthors, publishAuthors, ReleaseNotes);
                            _packageExportService.WithDependencies(dependenciesList);
                            _packageExportService.WithFiles(projectPath, $"{_constantConfigService.ProjectLocalDirectoryName}/**");

                            var outputPath = _packageExportService.ExportToDir(nupkgLocation);

                            _appSettingsConfigService.AddToExportDirHistoryList(nupkgLocation);

                            _appSettingsConfigService.SetLastExportDir(nupkgLocation);

                            if (System.IO.File.Exists(outputPath))
                            {
                                var info = string.Format(Chainbot.Resources.Properties.Resources.Export_SuccessMessage
                                    , _projectManagerService.CurrentProjectJsonConfig.name, publishVersion, nupkgLocation);
                                _messageBoxService.ShowInformation(info);
                            }
                            else
                            {
                                throw new Exception(Chainbot.Resources.Properties.Resources.Publish_ExceptionMessage1);
                            }

                           
                        }
                        catch (Exception err)
                        {
                            SharedObject.Instance.Output(SharedObject.enOutputType.Error, Chainbot.Resources.Properties.Resources.Publish_ExceptionMessage2, err);
                            _logService.Debug(err, _logger);
                            _messageBoxService.ShowWarning(Chainbot.Resources.Properties.Resources.Publish_ExceptionMessage2);
                        }

                        Window.Close();
                    },
                    () => IsCustomLocationCorrect && IsNewProjectVersionCorrect));
            }
        }

        private void UpdateProjectVersion()
        {
            _projectManagerService.CurrentProjectJsonConfig.projectVersion = NewProjectVersion;
            _projectManagerService.SaveCurrentProjectJson();
        }
    }
}