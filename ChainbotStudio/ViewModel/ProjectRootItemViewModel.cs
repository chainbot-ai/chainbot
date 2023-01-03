using Chainbot.Contracts.App;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using ChainbotStudio.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProjectRootItemViewModel : ProjectBaseItemViewModel
    {
        private IServiceLocator _serviceLocator;

        private IProjectManagerService _projectManagerService;

        private ICommonService _commonService;
        private IWindowService _windowService;
        private IWorkflowStateService _workflowStateService;
        private IConstantConfigService _constantConfigService;

        private ProjectViewModel _projectViewModel;
        private MainViewModel _mainViewModel;

        public string ProjectPath { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the ProjectRootItemViewModel class.
        /// </summary>
        public ProjectRootItemViewModel(IServiceLocator serviceLocator):base(serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _windowService = _serviceLocator.ResolveType<IWindowService>();
            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _constantConfigService = _serviceLocator.ResolveType<IConstantConfigService>();

            _projectViewModel = _serviceLocator.ResolveType<ProjectViewModel>();
            _mainViewModel = _serviceLocator.ResolveType<MainViewModel>();
        }



        /// <summary>
        /// The <see cref="ToolTip" /> property's name.
        /// </summary>
        public const string ToolTipPropertyName = "ToolTip";

        private string _toolTipProperty = "";

        /// <summary>
        /// Sets and gets the ToolTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ToolTip
        {
            get
            {
                return _toolTipProperty;
            }

            set
            {
                if (_toolTipProperty == value)
                {
                    return;
                }

                _toolTipProperty = value;
                RaisePropertyChanged(ToolTipPropertyName);
            }
        }


        

        private RelayCommand _closeProjectCommand;

        /// <summary>
        /// Gets the CloseProjectCommand.
        /// </summary>
        public RelayCommand CloseProjectCommand
        {
            get
            {
                return _closeProjectCommand
                    ?? (_closeProjectCommand = new RelayCommand(
                    () =>
                    {
                        _projectManagerService.CloseCurrentProject();
                    }));
            }
        }



        private RelayCommand _openDirCommand;

        /// <summary>
        /// Gets the OpenDirCommand.
        /// </summary>
        public RelayCommand OpenDirCommand
        {
            get
            {
                return _openDirCommand
                    ?? (_openDirCommand = new RelayCommand(
                    () =>
                    {
                        _commonService.LocateDirInExplorer(ProjectPath);
                    }));
            }
        }



        private RelayCommand _newSequenceCommand;

        /// <summary>
        /// Gets the NewSequenceCommand.
        /// </summary>
        public RelayCommand NewSequenceCommand
        {
            get
            {
                return _newSequenceCommand
                    ?? (_newSequenceCommand = new RelayCommand(
                    () =>
                    {
                        var window = new NewXamlFileWindow();
                        var vm = window.DataContext as NewXamlFileViewModel;

                        vm.ProjectPath = ProjectPath;
                        vm.FilePath = ProjectPath;
                        vm.FileType = NewXamlFileViewModel.enFileType.Sequence;

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }





        private RelayCommand _newFlowchartCommand;

        /// <summary>
        /// Gets the NewFlowchartCommand.
        /// </summary>
        public RelayCommand NewFlowchartCommand
        {
            get
            {
                return _newFlowchartCommand
                    ?? (_newFlowchartCommand = new RelayCommand(
                    () =>
                    {
                        var window = new NewXamlFileWindow();
                        var vm = window.DataContext as NewXamlFileViewModel;

                        vm.ProjectPath = ProjectPath;
                        vm.FilePath = ProjectPath;
                        vm.FileType = NewXamlFileViewModel.enFileType.Flowchart;

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }



        private RelayCommand _newStateMachineCommand;

        /// <summary>
        /// Gets the NewStateMachineCommand.
        /// </summary>
        public RelayCommand NewStateMachineCommand
        {
            get
            {
                return _newStateMachineCommand
                    ?? (_newStateMachineCommand = new RelayCommand(
                    () =>
                    {
                        var window = new NewXamlFileWindow();
                        var vm = window.DataContext as NewXamlFileViewModel;

                        vm.ProjectPath = ProjectPath;
                        vm.FilePath = ProjectPath;
                        vm.FileType = NewXamlFileViewModel.enFileType.StateMachine;

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }








        private RelayCommand _newFolderCommand;

        public RelayCommand NewFolderCommand
        {
            get
            {
                return _newFolderCommand
                    ?? (_newFolderCommand = new RelayCommand(
                    () =>
                    {
                        var window = new NewFolderWindow();

                        var vm = window.DataContext as NewFolderViewModel;
                        vm.Path = ProjectPath;
                        vm.FolderName = _commonService.GetValidDirectoryName(ProjectPath, Chainbot.Resources.Properties.Resources.NewFolder_Title);

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }




        private RelayCommand _openProjectSettingsCommand;

        /// <summary>
        /// Gets the OpenProjectSettingsCommand.
        /// </summary>
        public RelayCommand OpenProjectSettingsCommand
        {
            get
            {
                return _openProjectSettingsCommand
                    ?? (_openProjectSettingsCommand = new RelayCommand(
                    () =>
                    {
                        var window = new ProjectSettingsWindow();

                        var vm = window.DataContext as ProjectSettingsViewModel;
                        vm.ProjectName = _projectManagerService.CurrentProjectJsonConfig.name;
                        vm.ProjectDescription = _projectManagerService.CurrentProjectJsonConfig.description;

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }



        private RelayCommand _importWorkflowCommand;

        /// <summary>
        /// Gets the ImportWorkflowCommand.
        /// </summary>
        public RelayCommand ImportWorkflowCommand
        {
            get
            {
                return _importWorkflowCommand
                    ?? (_importWorkflowCommand = new RelayCommand(
                    () =>
                    {
                        var fileList = _commonService.ShowSelectMultiFileDialog(Chainbot.Resources.Properties.Resources.FileDialog_Filter1 + $"|*{_constantConfigService.XamlFileExtension}", Chainbot.Resources.Properties.Resources.FileDialog_Title3);

                        foreach (var item in fileList)
                        {
                            var sourceFileName = System.IO.Path.GetFileName(item);
                            var sourceFileDir = System.IO.Path.GetDirectoryName(item);
                            var sourcePath = item;
                            var targetPath = Path + @"\" + sourceFileName;
                            if (System.IO.File.Exists(targetPath))
                            {
                                targetPath = Path + @"\" + _commonService.GetValidFileName(Path, sourceFileName, Chainbot.Resources.Properties.Resources.Text_ImportWorkflowSuffix);
                            }

                            System.IO.File.Copy(sourcePath, targetPath, false);
                        }

                        if(fileList.Count>0)
                        {
                            _projectViewModel.Refresh();
                        }
                        
                    }));
            }
        }




        private RelayCommand _exportNupkgCommand;

        /// <summary>
        /// Gets the ExportNupkgCommand.
        /// </summary>
        public RelayCommand ExportNupkgCommand
        {
            get
            {
                return _exportNupkgCommand
                    ?? (_exportNupkgCommand = new RelayCommand(
                    () =>
                    {
                        _mainViewModel.ExportNupkgCommand.Execute(null);
                    }));
            }
        }







    }
}