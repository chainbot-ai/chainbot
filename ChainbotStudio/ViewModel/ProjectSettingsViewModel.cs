using Chainbot.Contracts.Config;
using Chainbot.Contracts.Project;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using System;
using System.IO;
using System.Windows;
using Chainbot.Contracts.Workflow;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProjectSettingsViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window _view;

        private IProjectManagerService _projectManagerService;
        private IWorkflowStateService _workflowStateService;

        private ProjectViewModel _projectViewModel;

        private IRecentProjectsConfigService _recentProjectsConfigService;

        /// <summary>
        /// Initializes a new instance of the ProjectSettingsViewModel class.
        /// </summary>
        public ProjectSettingsViewModel(IProjectManagerService projectManagerService, IRecentProjectsConfigService recentProjectsConfigService
            , IWorkflowStateService workflowStateService, ProjectViewModel projectViewModel)
        {
            _projectManagerService = projectManagerService;
            _recentProjectsConfigService = recentProjectsConfigService;
            _workflowStateService = workflowStateService;

            _projectViewModel = projectViewModel;
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
                    }));
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
            }

            OkCommand.RaiseCanExecuteChanged();
        }




        /// <summary>
        /// The <see cref="ProjectDescription" /> property's name.
        /// </summary>
        public const string ProjectDescriptionPropertyName = "ProjectDescription";

        private string _projectDescriptionProperty = "";

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






        private RelayCommand _okCommand;

        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand
                    ?? (_okCommand = new RelayCommand(
                    () =>
                    {
                        UpdateProjectJson();

                        _projectViewModel.OnProjectSettingsModify(this);

                        _recentProjectsConfigService.Update(_projectManagerService.CurrentProjectConfigFilePath, ProjectName, ProjectDescription);

                        _view.Close();
                    },
                    () => IsProjectNameCorrect && !_workflowStateService.IsRunningOrDebugging));
            }
        }

        private void UpdateProjectJson()
        {
            _projectManagerService.CurrentProjectJsonConfig.name = ProjectName;
            _projectManagerService.CurrentProjectJsonConfig.description = ProjectDescription;

            _projectManagerService.SaveCurrentProjectJson();
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
                        _view.Close();
                    },
                    () => true));
            }
        }


    }
}