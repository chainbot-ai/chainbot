using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.Services;
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
    public class PublishViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PublishWindow Window { get; set; }

        private IProjectManagerService _projectManagerService;
        private IDialogService _dialogService;
        private IMessageBoxService _messageBoxService;
        private ILogService _logService;
        private IConstantConfigService _constantConfigService;
        private IPackageExportService _packageExportService;
        private IControlServerService _controlServerService;
        private IServerSettingsService _serverSettingsService;


        private string _nupkgLocation;

        private string _robotPackagesLocation;

        /// <summary>
        /// Initializes a new instance of the ExportViewModel class.
        /// </summary>
        public PublishViewModel(IDialogService dialogService
            , IProjectManagerService projectManagerService
            , IMessageBoxService messageBoxService
            , ILogService logService
            , IConstantConfigService constantConfigService
            , IPackageExportService packageExportService
            , IControlServerService controlServerService
            ,IServerSettingsService serverSettingsService
            )
        {
            _dialogService = dialogService;
            _projectManagerService = projectManagerService;
            _messageBoxService = messageBoxService;
            _logService = logService;
            _constantConfigService = constantConfigService;
            _packageExportService = packageExportService;
            _controlServerService = controlServerService;
            _serverSettingsService = serverSettingsService;

            initRobotDefaults();
            initControlServerUrl();
            initVersionInfo();
        }

        private void initControlServerUrl()
        {
            _serverSettingsService.Load();
            ControlServerUrl = _serverSettingsService.ControlServerUrl;

            checkControlServerUrl(ControlServerUrl);
        }

        private void initRobotDefaults()
        {
            var commonApplicationData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
            var packagesDir = commonApplicationData + @"\ChainbotStudio\Packages";
            if (!System.IO.Directory.Exists(packagesDir))
            {
                System.IO.Directory.CreateDirectory(packagesDir);
            }

            _robotPackagesLocation = packagesDir;
        }

        

        private void initVersionInfo()
        {
            CurrentProjectVersion = _projectManagerService.CurrentProjectJsonConfig.projectVersion;

            Version currentVersion = new Version(CurrentProjectVersion);
            Version newVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1);
            NewProjectVersion = newVersion.ToString();
        }

        /// <summary>
        /// The <see cref="IsPublishToControlServer" /> property's name.
        /// </summary>
        public const string IsPublishToControlServerPropertyName = "IsPublishToControlServer";

        private bool _isPublishToControlServer = false;

        /// <summary>
        /// Sets and gets the IsPublishToControlServer property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPublishToControlServer
        {
            get
            {
                return _isPublishToControlServer;
            }

            set
            {
                if (_isPublishToControlServer == value)
                {
                    return;
                }

                _isPublishToControlServer = value;
                RaisePropertyChanged(IsPublishToControlServerPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="ControlServerUrl" /> property's name.
        /// </summary>
        public const string ControlServerUrlPropertyName = "ControlServerUrl";

        private string _controlServerUrlProperty = "";

        /// <summary>
        /// Sets and gets the ControlServerUrl property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ControlServerUrl
        {
            get
            {
                return _controlServerUrlProperty;
            }

            set
            {
                if (_controlServerUrlProperty == value)
                {
                    return;
                }

                _controlServerUrlProperty = value;
                RaisePropertyChanged(ControlServerUrlPropertyName);


                checkControlServerUrl(value);

                
            }
        }

        private void checkControlServerUrl(string value)
        {
            IsControlServerUrlCorrect = !string.IsNullOrWhiteSpace(value);
            if (!IsControlServerUrlCorrect)
            {
                ControlServerUrlValidatedWrongTip = Chainbot.Resources.Properties.Resources.Publish_UrlValidateWrong1;
                return;
            }

            try
            {
                Uri uri = new Uri(value);

                if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    IsControlServerUrlCorrect = true;
                }
                else
                {
                    IsControlServerUrlCorrect = false;
                    ControlServerUrlValidatedWrongTip = Chainbot.Resources.Properties.Resources.Publish_UrlValidateWrong2;
                }
            }
            catch (Exception)
            {
                IsControlServerUrlCorrect = false;
                ControlServerUrlValidatedWrongTip = Chainbot.Resources.Properties.Resources.Publish_UrlValidateWrong3;
            }
        }


        /// <summary>
        /// The <see cref="IsControlServerUrlCorrect" /> property's name.
        /// </summary>
        public const string IsControlServerUrlCorrectPropertyName = "IsControlServerUrlCorrect";

        private bool _isControlServerUrlCorrectProperty = false;

        /// <summary>
        /// Sets and gets the IsControlServerUrlCorrect property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsControlServerUrlCorrect
        {
            get
            {
                return _isControlServerUrlCorrectProperty;
            }

            set
            {
                if (_isControlServerUrlCorrectProperty == value)
                {
                    return;
                }

                _isControlServerUrlCorrectProperty = value;
                RaisePropertyChanged(IsControlServerUrlCorrectPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="ControlServerUrlValidatedWrongTip" /> property's name.
        /// </summary>
        public const string ControlServerUrlValidatedWrongTipPropertyName = "ControlServerUrlValidatedWrongTip";

        private string _controlServerUrlValidatedWrongTipProperty = "";

        /// <summary>
        /// Sets and gets the ControlServerUrlValidatedWrongTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ControlServerUrlValidatedWrongTip
        {
            get
            {
                return _controlServerUrlValidatedWrongTipProperty;
            }

            set
            {
                if (_controlServerUrlValidatedWrongTipProperty == value)
                {
                    return;
                }

                _controlServerUrlValidatedWrongTipProperty = value;
                RaisePropertyChanged(ControlServerUrlValidatedWrongTipPropertyName);
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

                        if (IsPublishToControlServer)
                        {
                            _nupkgLocation = System.IO.Path.GetTempPath();
                        }
                        else
                        {
                            _nupkgLocation = _robotPackagesLocation;
                        }

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

                            var outputPath = _packageExportService.ExportToDir(_nupkgLocation);

                            if (IsPublishToControlServer)
                            {
                                _serverSettingsService.ControlServerUrl = ControlServerUrl;
                                _serverSettingsService.Save();

                                Publish(publishId, publishVersion, ReleaseNotes, outputPath);
                            }
                            else
                            {
                                if (System.IO.File.Exists(outputPath))
                                {
                                    var info = string.Format(Chainbot.Resources.Properties.Resources.Publish_SuccessMessage
                                        , _projectManagerService.CurrentProjectJsonConfig.name, publishVersion, _nupkgLocation);
                                    _messageBoxService.ShowInformation(info);
                                }
                                else
                                {
                                    throw new Exception(Chainbot.Resources.Properties.Resources.Publish_ExceptionMessage1);
                                }
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
                    () => (IsPublishToControlServer && IsControlServerUrlCorrect || IsPublishToControlServer == false) && IsNewProjectVersionCorrect));
            }
        }

        private async void Publish(string projectName, string publishVersion,string publishDescription, string nupkgFilePath)
        {
            bool result = await _controlServerService.Publish(projectName, publishVersion, publishDescription, nupkgFilePath);

            if (System.IO.File.Exists(nupkgFilePath))
            {
                System.IO.File.Delete(nupkgFilePath);
            }

            if (result)
            {
                var info = string.Format(Chainbot.Resources.Properties.Resources.Publish_ErrorMessage
                                   , projectName, NewProjectVersion, _serverSettingsService.ControlServerUrl);
                _messageBoxService.ShowInformation(info);

            }
            else
            {
                var err = string.Format(Chainbot.Resources.Properties.Resources.Publish_ErrorMessage
                                  , projectName, NewProjectVersion, _serverSettingsService.ControlServerUrl);
                _messageBoxService.ShowError(err);
            }
        }

        private void UpdateProjectVersion()
        {
            _projectManagerService.CurrentProjectJsonConfig.projectVersion = NewProjectVersion;
            _projectManagerService.SaveCurrentProjectJson();
        }
    }
}