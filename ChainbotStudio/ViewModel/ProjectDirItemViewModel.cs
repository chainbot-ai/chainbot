using Chainbot.Contracts.App;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using ChainbotStudio.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProjectDirItemViewModel : ProjectBaseItemViewModel
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IServiceLocator _serviceLocator;

        private ILogService _logService;
        private ICommonService _commonService;
        private IWorkflowStateService _workflowStateService;
        private IConstantConfigService _constantConfigService;
        private IMessageBoxService _messageBoxService;
        private IProjectManagerService _projectManagerService;
        private IWindowService _windowService;

        private ProjectViewModel _projectViewModel;
        private DocksViewModel _docksViewModel;

        private MainViewModel _mainViewModel;

        public int _removeUnusedScreenshotsCount;
        /// <summary>
        /// Initializes a new instance of the ProjectDirItemViewModel class.
        /// </summary>
        public ProjectDirItemViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _logService = _serviceLocator.ResolveType<ILogService>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _constantConfigService = _serviceLocator.ResolveType<IConstantConfigService>();
            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();
            _windowService = _serviceLocator.ResolveType<IWindowService>();

            _projectViewModel = _serviceLocator.ResolveType<ProjectViewModel>();
            _docksViewModel = _serviceLocator.ResolveType<DocksViewModel>();

            _mainViewModel = _serviceLocator.ResolveType<MainViewModel>();
        }

   

        


        /// <summary>
        /// The <see cref="IsScreenshots" /> property's name.
        /// </summary>
        public const string IsScreenshotsPropertyName = "IsScreenshots";

        private bool _isScreenshotsProperty = false;

        /// <summary>
        /// Sets and gets the IsScreenshots property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsScreenshots
        {
            get
            {
                return _isScreenshotsProperty;
            }

            set
            {
                if (_isScreenshotsProperty == value)
                {
                    return;
                }

                _isScreenshotsProperty = value;
                RaisePropertyChanged(IsScreenshotsPropertyName);
            }
        }





        private RelayCommand _openDirCommand;


        public RelayCommand OpenDirCommand
        {
            get
            {
                return _openDirCommand
                    ?? (_openDirCommand = new RelayCommand(
                    () =>
                    {
                        _commonService.LocateDirInExplorer(Path);
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

                        vm.ProjectPath = _projectManagerService.CurrentProjectPath;
                        vm.FilePath = Path;
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

                        vm.ProjectPath = _projectManagerService.CurrentProjectPath;
                        vm.FilePath = Path;
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

                        vm.ProjectPath = _projectManagerService.CurrentProjectPath;
                        vm.FilePath = Path;
                        vm.FileType = NewXamlFileViewModel.enFileType.StateMachine;

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }




        private RelayCommand _renameDirCommand;


        public RelayCommand RenameDirCommand
        {
            get
            {
                return _renameDirCommand
                    ?? (_renameDirCommand = new RelayCommand(
                    () =>
                    {
                        var window = new RenameWindow();
                        var vm = window.DataContext as RenameViewModel;

                        vm.Path = Path;
                        vm.Dir = System.IO.Path.GetDirectoryName(Path);
                        vm.SrcName = Name;
                        vm.DstName = _commonService.GetValidDirectoryName(vm.Dir, _commonService.GetDirectoryNameWithoutSuffixFormat(vm.SrcName));
                        vm.IsDirectory = true;

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }


        private RelayCommand _deleteDirCommand;

        public RelayCommand DeleteDirCommand
        {
            get
            {
                return _deleteDirCommand
                    ?? (_deleteDirCommand = new RelayCommand(
                    () =>
                    {
                        bool ret = _messageBoxService.ShowQuestion(string.Format(Chainbot.Resources.Properties.Resources.Message_DeleteDirQuestion, Path));

                        if(ret)
                        {
                            _commonService.DeleteDir(Path);

                            _projectViewModel.OnDeleteDir(Path);

                            _docksViewModel.OnDeleteDir(Path);
                        }
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
                        vm.Path = Path;
                        vm.FolderName = _commonService.GetValidDirectoryName(Path, Chainbot.Resources.Properties.Resources.NewFolder_Title);

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }



        private bool CheckScreenshotsImage(object item, object param)
        {
            if (item is FileInfo)
            {
                var fi_img = param as FileInfo;
                var fi_xaml = item as FileInfo;
                if (fi_xaml.Extension.ToLower() == _constantConfigService.XamlFileExtension)
                {
                    if (_commonService.IsStringInFile(fi_xaml.FullName, "\"" + fi_img.Name + "\""))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool EnumScreenshotsImage(object item, object param)
        {
            if (item is FileInfo)
            {
                bool contains = _commonService.DirectoryChildrenForEach(new DirectoryInfo(_projectManagerService.CurrentProjectPath), CheckScreenshotsImage, item);
                if (!contains)
                {
                    var fi = item as FileInfo;
                    try
                    {
                        fi.Delete();

                        _removeUnusedScreenshotsCount++;
                    }
                    catch (Exception err)
                    {
                        _logService.Debug(err, _logger);
                    }

                }
            }

            return false;
        }


        private RelayCommand _removeUnusedScreenshotsCommand;

        /// <summary>
        /// Gets the RemoveUnusedScreenshotsCommand.
        /// </summary>
        public RelayCommand RemoveUnusedScreenshotsCommand
        {
            get
            {
                return _removeUnusedScreenshotsCommand
                    ?? (_removeUnusedScreenshotsCommand = new RelayCommand(
                    () =>
                    {
                        _mainViewModel.SaveAllCommand.Execute(null);

                        _removeUnusedScreenshotsCount = 0;

                        _commonService.DirectoryChildrenForEach(new DirectoryInfo(Path), EnumScreenshotsImage);

                        if (_removeUnusedScreenshotsCount == 0)
                        {
                            _messageBoxService.ShowInformation(Chainbot.Resources.Properties.Resources.Message_RemoveUnusedScreenshot2);
                        }
                        else
                        {
                            _projectViewModel.Refresh();

                            _messageBoxService.ShowInformation(string.Format(Chainbot.Resources.Properties.Resources.Message_RemoveUnusedScreenshot3, _removeUnusedScreenshotsCount));
                        }
                    }));
            }
        }

        
    }
}