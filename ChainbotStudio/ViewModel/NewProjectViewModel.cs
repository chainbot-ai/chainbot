using Chainbot.Contracts.Config;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Chainbot.Contracts.Nupkg;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NewProjectViewModel : ViewModelBase
    {
        public Window Window { get; set; }

        private bool _isImportNupkgWindow;

        private IDialogService _dialogService;
        private IProjectManagerService _projectManagerService;

        private IProjectUserConfigService _projectUserConfigService;
        private IConstantConfigService _constantConfigService;

        private IRecentProjectsConfigService _recentProjectsConfigService;

        public IPackageImportService _packageImportService;

        private ICommonService _commonService;

        /// <summary>
        /// Initializes a new instance of the NewProjectViewModel class.
        /// </summary>
        public NewProjectViewModel(IDialogService dialogService,IProjectManagerService projectManagerService
            ,IProjectUserConfigService projectUserConfigService,IConstantConfigService constantConfigService
            ,ICommonService commonService, IRecentProjectsConfigService recentProjectsConfigService
            )
        {
            _dialogService = dialogService;
            _projectManagerService = projectManagerService;
            _projectUserConfigService = projectUserConfigService;
            _constantConfigService = constantConfigService;
            _commonService = commonService;
            _recentProjectsConfigService = recentProjectsConfigService;

            _projectUserConfigService.Load();

            ProjectPath = _projectUserConfigService.ProjectCreatePath;
            ProjectName = commonService.GetValidDirectoryName(ProjectPath, _constantConfigService.ProjectCreateName, "{0}", 1);
            
        }

        public void SwitchToImportNupkgWindow(IPackageImportService packageImportService)
        {
            _isImportNupkgWindow = true;

            _packageImportService = packageImportService;

            this.WindowTitle = Chainbot.Resources.Properties.Resources.NewProject_WindowTitle2;
            this.SubTitle = Chainbot.Resources.Properties.Resources.NewProject_SubTitle2;
            this.SubTitleDescription = Chainbot.Resources.Properties.Resources.NewProject_SubTitleDescription2;
            this.ProjectName = _packageImportService.GetId();
            this.ProjectDescription = _packageImportService.GetDescription();
            this.ProjectVersion = _packageImportService.GetVersion();
        }

        /// <summary>
        /// The <see cref="WindowTitle" /> property's name.
        /// </summary>
        public const string WindowTitlePropertyName = "WindowTitle";

        private string _windowTitleProperty = Chainbot.Resources.Properties.Resources.NewProject_WindowTitle1;

        /// <summary>
        /// Sets and gets the WindowTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WindowTitle
        {
            get
            {
                return _windowTitleProperty;
            }

            set
            {
                if (_windowTitleProperty == value)
                {
                    return;
                }

                _windowTitleProperty = value;
                RaisePropertyChanged(WindowTitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SubTitle" /> property's name.
        /// </summary>
        public const string SubTitlePropertyName = "SubTitle";

        private string _subTitleProperty = Chainbot.Resources.Properties.Resources.NewProject_SubTitle1;

        /// <summary>
        /// Sets and gets the SubTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SubTitle
        {
            get
            {
                return _subTitleProperty;
            }

            set
            {
                if (_subTitleProperty == value)
                {
                    return;
                }

                _subTitleProperty = value;
                RaisePropertyChanged(SubTitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SubTitleDescription" /> property's name.
        /// </summary>
        public const string SubTitleDescriptionPropertyName = "SubTitleDescription";

        private string _subTitleDescriptionProperty = Chainbot.Resources.Properties.Resources.NewProject_SubTitleDescription1;

        /// <summary>
        /// Sets and gets the SubTitleDescription property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SubTitleDescription
        {
            get
            {
                return _subTitleDescriptionProperty;
            }

            set
            {
                if (_subTitleDescriptionProperty == value)
                {
                    return;
                }

                _subTitleDescriptionProperty = value;
                RaisePropertyChanged(SubTitleDescriptionPropertyName);
            }
        }

        private RelayCommand<RoutedEventArgs> _projectNameLoadedCommand;

        public RelayCommand<RoutedEventArgs> ProjectNameLoadedCommand
        {
            get
            {
                return _projectNameLoadedCommand
                    ?? (_projectNameLoadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        var textBox = (TextBox)p.Source;
                        textBox.Focus();
                        textBox.SelectAll();
                    }));
            }
        }

        /// <summary>
        /// The <see cref="ProjectVersion" /> property's name.
        /// </summary>
        public const string ProjectVersionPropertyName = "ProjectVersion";

        private string _projectVersionProperty = "";

        public string ProjectVersion
        {
            get
            {
                return _projectVersionProperty;
            }

            set
            {
                if (_projectVersionProperty == value)
                {
                    return;
                }

                _projectVersionProperty = value;
                RaisePropertyChanged(ProjectVersionPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsProjectNameCorrect" /> property's name.
        /// </summary>
        public const string IsProjectNameCorrectPropertyName = "IsProjectNameCorrect";

        private bool _isProjectNameCorrectProperty = false;

        public bool IsProjectNameCorrect
        {
            get
            {
                return _isProjectNameCorrectProperty;
            }

            set
            {
                if (_isProjectNameCorrectProperty == value)
                {
                    return;
                }

                _isProjectNameCorrectProperty = value;
                RaisePropertyChanged(IsProjectNameCorrectPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ProjectName" /> property's name.
        /// </summary>
        public const string ProjectNamePropertyName = "ProjectName";

        private string _projectNameProperty = "";

        public string ProjectName
        {
            get
            {
                return _projectNameProperty;
            }

            set
            {
                if (_projectNameProperty == value)
                {
                    return;
                }

                _projectNameProperty = value;
                RaisePropertyChanged(ProjectNamePropertyName);

                projectNameValidate(value);
            }
        }

        /// <summary>
        /// The <see cref="ProjectNameValidatedWrongTip" /> property's name.
        /// </summary>
        public const string ProjectNameValidatedWrongTipPropertyName = "ProjectNameValidatedWrongTip";

        private string _projectNameValidatedWrongTipProperty = "";

        public string ProjectNameValidatedWrongTip
        {
            get
            {
                return _projectNameValidatedWrongTipProperty;
            }

            set
            {
                if (_projectNameValidatedWrongTipProperty == value)
                {
                    return;
                }

                _projectNameValidatedWrongTipProperty = value;
                RaisePropertyChanged(ProjectNameValidatedWrongTipPropertyName);
            }
        }

        private void projectNameValidate(string value)
        {
            IsProjectNameCorrect = true;
            if (string.IsNullOrEmpty(value))
            {
                IsProjectNameCorrect = false;
                ProjectNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong1;
            }
            else
            {
                if (value.Contains(@"\") || value.Contains(@"/"))
                {
                    IsProjectNameCorrect = false;
                    ProjectNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong2;
                }
                else
                {
                    if (!NuGet.PackageIdValidator.IsValidPackageId(value))
                    {
                        IsProjectNameCorrect = false;
                        ProjectNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong3;
                    }
                    else
                    {
                        System.IO.FileInfo fi = null;
                        try
                        {
                            fi = new System.IO.FileInfo(value);
                        }
                        catch (ArgumentException) { }
                        catch (System.IO.PathTooLongException) { }
                        catch (NotSupportedException) { }
                        if (ReferenceEquals(fi, null))
                        {
                            // file name is not valid
                            IsProjectNameCorrect = false;
                            ProjectNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong2;
                        }
                        else
                        {
                            // file name is valid... May check for existence by calling fi.Exists.
                        }
                    }
                }

                if (Directory.Exists(ProjectPath + @"\" + ProjectName))
                {
                    IsProjectNameCorrect = false;
                    ProjectNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong4;
                }
            }
        }

        /// <summary>
        /// The <see cref="IsProjectPathCorrect" /> property's name.
        /// </summary>
        public const string IsProjectPathCorrectPropertyName = "IsProjectPathCorrect";

        private bool _isProjectPathCorrectProperty = false;

        public bool IsProjectPathCorrect
        {
            get
            {
                return _isProjectPathCorrectProperty;
            }

            set
            {
                if (_isProjectPathCorrectProperty == value)
                {
                    return;
                }

                _isProjectPathCorrectProperty = value;
                RaisePropertyChanged(IsProjectPathCorrectPropertyName);
            }
        }

        public const string ProjectPathPropertyName = "ProjectPath";

        private string _projectPathProperty = "";

        /// <summary>
        /// Sets and gets the ProjectPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProjectPath
        {
            get
            {
                return _projectPathProperty;
            }

            set
            {
                if (_projectPathProperty == value)
                {
                    return;
                }

                _projectPathProperty = value;
                RaisePropertyChanged(ProjectPathPropertyName);

                projectPathValidate(value);
                projectNameValidate(ProjectName);
            }
        }

        private void projectPathValidate(string value)
        {
            IsProjectPathCorrect = true;
            if (string.IsNullOrEmpty(value))
            {
                IsProjectPathCorrect = false;
                ProjectPathValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_LocationValidateWrong1;
            }
            else
            {
                if (!Directory.Exists(value))
                {
                    IsProjectPathCorrect = false;
                    ProjectPathValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_LocationValidateWrong2;
                }
            }
        }

        /// <summary>
        /// The <see cref="ProjectPathValidatedWrongTip" /> property's name.
        /// </summary>
        public const string ProjectPathValidatedWrongTipPropertyName = "ProjectPathValidatedWrongTip";

        private string _projectPathValidatedWrongTipProperty = "";

        public string ProjectPathValidatedWrongTip
        {
            get
            {
                return _projectPathValidatedWrongTipProperty;
            }

            set
            {
                if (_projectPathValidatedWrongTipProperty == value)
                {
                    return;
                }

                _projectPathValidatedWrongTipProperty = value;
                RaisePropertyChanged(ProjectPathValidatedWrongTipPropertyName);
            }
        }

        private RelayCommand _selectProjectPathCommand;

        public RelayCommand SelectProjectPathCommand
        {
            get
            {
                return _selectProjectPathCommand
                    ?? (_selectProjectPathCommand = new RelayCommand(
                    () =>
                    {
                        string dst_dir = "";
                        if (_dialogService.ShowSelectDirDialog(Chainbot.Resources.Properties.Resources.NewProject_SelectLocation, ref dst_dir))
                        {
                            ProjectPath = dst_dir;
                        }
                    }));
            }
        }

        /// <summary>
        /// The <see cref="ProjectDescription" /> property's name.
        /// </summary>
        public const string ProjectDescriptionPropertyName = "ProjectDescription";

        private string _projectDescriptionProperty = Chainbot.Resources.Properties.Resources.NewProject_ProjectDescription;

        public string ProjectDescription
        {
            get
            {
                return _projectDescriptionProperty;
            }

            set
            {
                if (_projectDescriptionProperty == value)
                {
                    return;
                }

                _projectDescriptionProperty = value;
                RaisePropertyChanged(ProjectDescriptionPropertyName);
            }
        }

        private RelayCommand _createProjectCommand;

        public RelayCommand CreateProjectCommand
        {
            get
            {
                return _createProjectCommand
                    ?? (_createProjectCommand = new RelayCommand(
                    () =>
                    {
                        if(!_projectManagerService.CloseCurrentProject())
                        {
                            return;
                        }

                        _projectUserConfigService.ProjectCreatePath = ProjectPath;
                        _projectUserConfigService.Save();

                        Window.Hide();

                        var projectConfigFileAtPath = Path.Combine(ProjectPath, ProjectName);
                        if (_isImportNupkgWindow)
                        {
                            
                            if(!_packageImportService.ExtractToDirectory(projectConfigFileAtPath))
                            {
                                Window.Close();
                                return;
                            }

                            _projectManagerService.UpdateCurrentProjectConfigFilePath(Path.Combine(projectConfigFileAtPath
                                , _constantConfigService.ProjectConfigFileName));

                            _projectManagerService.CurrentProjectJsonConfig.name = ProjectName;
                            _projectManagerService.CurrentProjectJsonConfig.description = ProjectDescription;
                            _projectManagerService.SaveCurrentProjectJson();
                        }
                        else
                        {
                            _projectManagerService.NewProject(ProjectPath, ProjectName, ProjectDescription, ProjectVersion);
                        }

                        _projectManagerService.OpenProject(_projectManagerService.CurrentProjectConfigFilePath);

                        Window.Close();
                    },
                    () => IsProjectNameCorrect && IsProjectPathCorrect));
            }
        }
     
    }
}