using Chainbot.Contracts.App;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using ChainbotStudio.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Input;
using System.Windows.Media;
using System;
using Plugins.Shared.Library;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProjectFileItemViewModel : ProjectBaseItemViewModel
    {
        private IServiceLocator _serviceLocator;

        private DocksViewModel _docksViewModel;

        private IWindowService _windowService;
        private ICommonService _commonService;
        private IWorkflowStateService _workflowStateService;
        private IMessageBoxService _messageBoxService;
        private IProjectManagerService _projectManagerService;

        private ProjectViewModel _projectViewModel;


        public string ActivityFactoryAssemblyQualifiedName { get; set; }


        public string ActivityAssemblyQualifiedName { get; set; }


        public string ActivityDisplayName { get; set; }


        private MainViewModel _mainViewModel;


        /// <summary>
        /// Initializes a new instance of the ProjectFileItemViewModel class.
        /// </summary>
        public ProjectFileItemViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();
            _windowService = _serviceLocator.ResolveType<IWindowService>();

            _projectViewModel = _serviceLocator.ResolveType<ProjectViewModel>();
            _docksViewModel = _serviceLocator.ResolveType<DocksViewModel>();

            _mainViewModel = _serviceLocator.ResolveType<MainViewModel>();
        }



        /// <summary>
        /// The <see cref="Icon" /> property's name.
        /// </summary>
        public const string IconPropertyName = "Icon";

        private ImageSource _iconProperty = null;

        /// <summary>
        /// Sets and gets the Icon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                return _iconProperty;
            }

            set
            {
                if (_iconProperty == value)
                {
                    return;
                }

                _iconProperty = value;
                RaisePropertyChanged(IconPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsMainXamlFile" /> property's name.
        /// </summary>
        public const string IsMainXamlFilePropertyName = "IsMainXamlFile";

        private bool _isMainXamlFileProperty = false;

        /// <summary>
        /// Sets and gets the IsMainXamlFile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMainXamlFile
        {
            get
            {
                return _isMainXamlFileProperty;
            }

            set
            {
                if (_isMainXamlFileProperty == value)
                {
                    return;
                }

                _isMainXamlFileProperty = value;
                RaisePropertyChanged(IsMainXamlFilePropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsXamlFile" /> property's name.
        /// </summary>
        public const string IsXamlFilePropertyName = "IsXamlFile";

        private bool _isXamlFileProperty = false;

        /// <summary>
        /// Sets and gets the IsXamlFile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsXamlFile
        {
            get
            {
                return _isXamlFileProperty;
            }

            set
            {
                if (_isXamlFileProperty == value)
                {
                    return;
                }

                _isXamlFileProperty = value;
                RaisePropertyChanged(IsXamlFilePropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsPythonFile" /> property's name.
        /// </summary>
        public const string IsPythonFilePropertyName = "IsPythonFile";

        private bool _isPythonFileProperty = false;

        /// <summary>
        /// Sets and gets the IsPythonFile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPythonFile
        {
            get
            {
                return _isPythonFileProperty;
            }

            set
            {
                if (_isPythonFileProperty == value)
                {
                    return;
                }

                _isPythonFileProperty = value;
                RaisePropertyChanged(IsPythonFilePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsJavaScriptFile" /> property's name.
        /// </summary>
        public const string IsJavaScriptFilePropertyName = "IsJavaScriptFile";

        private bool _isJavaScriptFileProperty = false;

        /// <summary>
        /// Sets and gets the IsJavaScriptFile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsJavaScriptFile
        {
            get
            {
                return _isJavaScriptFileProperty;
            }

            set
            {
                if (_isJavaScriptFileProperty == value)
                {
                    return;
                }

                _isJavaScriptFileProperty = value;
                RaisePropertyChanged(IsJavaScriptFilePropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsProjectJsonFile" /> property's name.
        /// </summary>
        public const string IsProjectJsonFilePropertyName = "IsProjectJsonFile";

        private bool _isProjectJsonFileProperty = false;

        /// <summary>
        /// Sets and gets the IsProjectJsonFile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsProjectJsonFile
        {
            get
            {
                return _isProjectJsonFileProperty;
            }

            set
            {
                if (_isProjectJsonFileProperty == value)
                {
                    return;
                }

                _isProjectJsonFileProperty = value;
                RaisePropertyChanged(IsProjectJsonFilePropertyName);
            }
        }



        private RelayCommand<MouseButtonEventArgs> _treeNodeMouseRightButtonUpCommand;


        public RelayCommand<MouseButtonEventArgs> TreeNodeMouseRightButtonUpCommand
        {
            get
            {
                return _treeNodeMouseRightButtonUpCommand
                    ?? (_treeNodeMouseRightButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        
                    }));
            }
        }




        private RelayCommand<MouseButtonEventArgs> _treeNodeMouseDoubleClickCommand;


        public RelayCommand<MouseButtonEventArgs> TreeNodeMouseDoubleClickCommand
        {
            get
            {
                return _treeNodeMouseDoubleClickCommand
                    ?? (_treeNodeMouseDoubleClickCommand = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        if (IsXamlFile)
                        {
                            OpenXamlCommand.Execute(null);
                        }
                    }));
            }
        }



        private RelayCommand _openXamlCommand;


        public RelayCommand OpenXamlCommand
        {
            get
            {
                return _openXamlCommand
                    ?? (_openXamlCommand = new RelayCommand(
                    () =>
                    {
                        try
                        {
                            DesignerDocumentViewModel doc;
                            bool isExist = _docksViewModel.IsDocumentExist(Path, out doc);

                            if (!isExist)
                            {
                                _docksViewModel.NewDesignerDocument(Path);
                            }
                            else
                            {
                                doc.IsSelected = true;
                            }
                        }
                        catch (Exception err)
                        {
                            SharedObject.Instance.Output(SharedObject.enOutputType.Error, Chainbot.Resources.Properties.Resources.Message_OpenWorkflowFileError, err);
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_OpenWorkflowFileError);
                        }
                    }));
            }
        }



        private RelayCommand _renameFileCommand;


        public RelayCommand RenameFileCommand
        {
            get
            {
                return _renameFileCommand
                    ?? (_renameFileCommand = new RelayCommand(
                    () =>
                    {
                        var window = new RenameWindow();
                        var vm = window.DataContext as RenameViewModel;

                        vm.Path = Path;
                        vm.Dir = System.IO.Path.GetDirectoryName(Path);
                        vm.SrcName = Name;
                        vm.DstName = _commonService.GetValidFileName(vm.Dir, _commonService.GetFileNameWithoutSuffixFormat(vm.SrcName));
                        vm.IsDirectory = false;
                        vm.IsMain = IsMainXamlFile;

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }



        private RelayCommand _deleteFileCommand;

        public RelayCommand DeleteFileCommand
        {
            get
            {
                return _deleteFileCommand
                    ?? (_deleteFileCommand = new RelayCommand(
                    () =>
                    {
                        if (IsMainXamlFile)
                        {
                            _messageBoxService.ShowWarning(Chainbot.Resources.Properties.Resources.Message_DeleteMainWarn);
                            return;
                        }
                        var ret = _messageBoxService.ShowQuestion(string.Format(Chainbot.Resources.Properties.Resources.Message_DeleteFileQuestion, Path));
                        if (ret)
                        {
                            _commonService.DeleteFile(Path);

                            _projectViewModel.OnDeleteFile(Path);

                            _docksViewModel.OnDeleteFile(Path);
                        }
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }



        private RelayCommand _runCommand;

        public RelayCommand RunCommand
        {
            get
            {
                return _runCommand
                    ?? (_runCommand = new RelayCommand(
                    () =>
                    {
                        OpenXamlCommand.Execute(null);

                        _mainViewModel.RunWorkflowCommand.Execute(null);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }



        private RelayCommand _setAsMainCommand;


        public RelayCommand SetAsMainCommand
        {
            get
            {
                return _setAsMainCommand
                    ?? (_setAsMainCommand = new RelayCommand(
                    () =>
                    {
                        var relativeMainXaml = _commonService.MakeRelativePath(_projectManagerService.CurrentProjectPath, Path);
                        _projectManagerService.CurrentProjectJsonConfig.main = relativeMainXaml;
                        _projectManagerService.SaveCurrentProjectJson();

                        _projectViewModel.Refresh();
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }





    }
}