using System;
using Chainbot.Contracts.Project;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Plugins.Shared.Library;
using Chainbot.Contracts.Log;
using log4net;
using System.ComponentModel;
using Chainbot.Contracts.App;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Activities;
using System.Windows;
using System.IO;
using Chainbot.Cores.Classes;
using Chainbot.Contracts.Config;
using Newtonsoft.Json.Linq;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Utils;
using ChainbotStudio.Views;
using Chainbot.Contracts.Workflow;
using System.Windows.Threading;
using Plugins.Shared.Library.Librarys;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _rpaTitleFlag = Chainbot.Resources.Properties.Resources.Title_Main;

        public MainWindow View;

        private IServiceLocator _serviceLocator;

        private IRecentProjectsConfigService _recentProjectsConfigService;
        private IWorkflowStateService _workflowStateService;

        private IStudioApplication _studioApplication;
        private IProjectManagerService _projectManagerService;
        private ILogService _logService;
        private IDispatcherService _dispatcherService;
        private IConstantConfigService _constantConfigService;
        private IMessageBoxService _messageBoxService;
        private ICommonService _commonService;
        private IWindowService _windowService;
        private IWorkflowBreakpointsServiceProxy _workflowBreakpointsServiceProxy;
        private IPathConfigService _pathConfigService;

        private IAuthorizationService _authorizationService;

        private IWorkflowRunService _currentWorkflowRunService;
        public IWorkflowDebugService CurrentWorkflowDebugService { get; set; }

        private IActivitiesServiceProxy _activitiesServiceProxy;

        private DocksViewModel _docksViewModel;
        private ProjectViewModel _projectViewModel;
        private OutputViewModel _outputViewModel;
        private SearchViewModel _searchViewModel;

        private SearchViewWindow _searchViewWindow;

        public event EventHandler ClosingEvent;


        public enum enSlowStepSpeed
        {
            Off,//Close
            One,//1x
            Two,//2x
            Three,//3x
            Four//4x
        }




        private static DispatcherTimer dispatcherSaveAllTimer = new DispatcherTimer();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _studioApplication = _serviceLocator.ResolveType<IStudioApplication>();
            _constantConfigService = _serviceLocator.ResolveType<IConstantConfigService>();
            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();
            _logService = _serviceLocator.ResolveType<ILogService>();
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();
            _recentProjectsConfigService = _serviceLocator.ResolveType<IRecentProjectsConfigService>();
            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _windowService = _serviceLocator.ResolveType<IWindowService>();
            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _pathConfigService = _serviceLocator.ResolveType<IPathConfigService>();
            _authorizationService = _serviceLocator.ResolveType<IAuthorizationService>();

            _docksViewModel = _serviceLocator.ResolveType<DocksViewModel>();
            _projectViewModel = _serviceLocator.ResolveType<ProjectViewModel>();
            _outputViewModel = _serviceLocator.ResolveType<OutputViewModel>();
            _searchViewModel = _serviceLocator.ResolveType<SearchViewModel>();

            _projectManagerService.ProjectLoadingBeginEvent += _projectManagerService_ProjectLoadingBeginEvent;
            _projectManagerService.ProjectLoadingEndEvent += _projectManagerService_ProjectLoadingEndEvent;
            _projectManagerService.ProjectLoadingExceptionEvent += _projectManagerService_ProjectLoadingExceptionEvent;


            _workflowStateService.BeginRunEvent += _workflowStateService_BeginRunEvent;
            _workflowStateService.EndRunEvent += _workflowStateService_EndRunEvent;

            _workflowStateService.BeginDebugEvent += _workflowStateService_BeginDebugEvent;
            _workflowStateService.EndDebugEvent += _workflowStateService_EndDebugEvent;


            _projectManagerService.ProjectOpenEvent += _projectManagerService_ProjectOpenEvent;
            _projectManagerService.ProjectCloseEvent += _projectManagerService_ProjectCloseEvent;

            _projectManagerService.ProjectPreviewOpenEvent += _projectManagerService_ProjectPreviewOpenEvent;
            _projectManagerService.ProjectPreviewCloseEvent += _projectManagerService_ProjectPreviewCloseEvent;

            SharedObject.Instance.ClearOutputEvent();
            SharedObject.Instance.OutputEvent += Instance_OutputEvent;

            SharedObject.Instance.ClearNotifyEvent();
            SharedObject.Instance.NotifyEvent += Instance_NotifyEvent;

            initAutoSaveAll();
        }



        private void initAutoSaveAll()
        {
            dispatcherSaveAllTimer.Tick -= new EventHandler(dispatcherSaveAllTimer_Tick);
            dispatcherSaveAllTimer.Tick += new EventHandler(dispatcherSaveAllTimer_Tick);

            dispatcherSaveAllTimer.Interval = TimeSpan.FromSeconds(60);//
            dispatcherSaveAllTimer.Start();
        }


        private void dispatcherSaveAllTimer_Tick(object sender, EventArgs e)
        {
            SaveAllCommand.Execute(null);
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
                        View = (MainWindow)p.Source;

                        OnLoaded();
                    }));
            }
        }


        private void OnLoaded()
        {
            ApplicationName = _rpaTitleFlag;
            IsShowHomePage = true;
            IsHomePageCanClose = false;
            View.appMenu.SelectedItem = View.AccountPage;

            _serviceLocator.ResolveType<CheckUpgradeViewModel>().CheckUpgradeCommand.Execute(null);

            ProcessManifestJson();

            ProcessCommandLineInput(_studioApplication.Args);
        }



        private void _workflowStateService_BeginRunEvent(object sender, System.EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                _outputViewModel.ClearAllCommand.Execute(null);

                _windowService.ShowMainWindowMinimized();
            });
        }

        private void _workflowStateService_EndRunEvent(object sender, System.EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                if (_windowService.IsMinimized)
                {
                    _windowService.ShowMainWindowNormal();
                }
            });
        }

        private void _workflowStateService_BeginDebugEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                IsShowDebug = false;
                IsShowContinue = true;

                _outputViewModel.ClearAllCommand.Execute(null);

                _docksViewModel.View.DebugToolWindow.Activate();
            });
        }

        private void _workflowStateService_EndDebugEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                IsShowDebug = true;
                IsShowContinue = false;

                _workflowStateService.IsDebuggingPaused = false;

                _docksViewModel.View.DebugToolWindow.Close();

                if (_docksViewModel.SelectedDocument is DesignerDocumentViewModel)
                {
                    var designerDoc = _docksViewModel.SelectedDocument as DesignerDocumentViewModel;
                    designerDoc.HideCurrentDebugArrow();
                }

            });
        }


        private void _projectManagerService_ProjectPreviewOpenEvent(object sender, EventArgs e)
        {
            _workflowBreakpointsServiceProxy = _serviceLocator.ResolveType<IWorkflowBreakpointsServiceProxy>();
            _workflowBreakpointsServiceProxy.LoadBreakpoints();
        }

        private void _projectManagerService_ProjectPreviewCloseEvent(object sender, CancelEventArgs e)
        {
            _workflowBreakpointsServiceProxy.SaveBreakpoints();
        }




        private void ProcessManifestJson()
        {
            var manifest_json_file_path = System.IO.Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"msghost\manifest.json");
            if (File.Exists(manifest_json_file_path))
            {
                var manifest_json_cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(File.ReadAllText(manifest_json_file_path));
                var current_path = manifest_json_cfg["path"].ToString();
                var correct_path = System.IO.Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"msghost\NativeMessage\chrome\Chainbot.ChainbotNativeMessaging.exe");

                if (current_path.ToLower() != correct_path.ToLower())
                {
                    manifest_json_cfg["path"] = correct_path;

                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(manifest_json_cfg, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(manifest_json_file_path, output);
                }
            }
        }

        private void ProcessCommandLineInput(string[] args)
        {
            if (args.Length > 0)
            {
                var filePath = args[0];

                _logService.Debug(string.Format(Chainbot.Resources.Properties.Resources.Message_CommandLine, filePath), _logger);

                var fileName = System.IO.Path.GetFileName(filePath);

                if (fileName.EqualsIgnoreCase(_constantConfigService.ProjectConfigFileName))
                {
                    _projectManagerService.OpenProject(filePath);
                }
            }
        }


        private void _projectManagerService_ProjectLoadingBeginEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(()=> {
                IsLoading = true;
            });
        }

        private void _projectManagerService_ProjectLoadingEndEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() => {
                IsLoading = false;

                IsShowHomePage = false;
                IsHomePageCanClose = true;
            });
        }


        private void _projectManagerService_ProjectLoadingExceptionEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() => {
                IsLoading = false;
            });
        }

        private void _projectManagerService_ProjectOpenEvent(object sender, System.EventArgs e)
        {           
            var service = sender as IProjectManagerService;

            _dispatcherService.InvokeAsync(() => {

                _activitiesServiceProxy = _projectManagerService.CurrentActivitiesServiceProxy;

                ProjectName = service.CurrentProjectJsonConfig.name;

                IsProjectOpened = true;

                _recentProjectsConfigService.Add(service.CurrentProjectConfigFilePath);

                _docksViewModel.View.DebugToolWindow.Close();
            });

            SharedObject.Instance.ProjectPath = service.CurrentProjectPath;
        }

        private void Instance_OutputEvent(SharedObject.enOutputType type, string msg, string msgDetails = "")
        {
            LogToOutputWindow(type,msg,msgDetails);
        }

        private void Instance_NotifyEvent(string notification, string notificationDetails)
        {
            
        }

        private void _projectManagerService_ProjectCloseEvent(object sender, System.EventArgs e)
        {
            _dispatcherService.InvokeAsync(() => {
                ProjectName = "";

                IsProjectOpened = false;

                IsShowHomePage = true;
                IsHomePageCanClose = false;
            });
        }


        private void LogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            _logService.Info(string.Format("LogToOutputWindow：type={0},msg={1},msgDetails={2}", type.ToString(), msg, msgDetails), _logger);
            _dispatcherService.InvokeAsync(()=> {
                _serviceLocator.ResolveType<OutputViewModel>().Log(type, msg, msgDetails);
            });
        }


        /// <summary>
        /// The <see cref="IsProjectOpened" /> property's name.
        /// </summary>
        public const string IsProjectOpenedPropertyName = "IsProjectOpened";

        private bool _isProjectOpenedProperty = false;

        /// <summary>
        /// Sets and gets the IsProjectOpened property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsProjectOpened
        {
            get
            {
                return _isProjectOpenedProperty;
            }

            set
            {
                if (_isProjectOpenedProperty == value)
                {
                    return;
                }

                _isProjectOpenedProperty = value;
                RaisePropertyChanged(IsProjectOpenedPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsMinimized" /> property's name.
        /// </summary>
        public const string IsMinimizedPropertyName = "IsMinimized";

        private bool _isMinimizedProperty = false;

        /// <summary>
        /// Sets and gets the IsMinimized property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMinimized
        {
            get
            {
                return _isMinimizedProperty;
            }

            set
            {
                if (_isMinimizedProperty == value)
                {
                    return;
                }

                _isMinimizedProperty = value;
                RaisePropertyChanged(IsMinimizedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsShowHomePage" /> property's name.
        /// </summary>
        public const string IsShowHomePagePropertyName = "IsShowHomePage";

        private bool _isShowHomePageProperty = false;

        /// <summary>
        /// Sets and gets the IsShowHomePage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowHomePage
        {
            get
            {
                return _isShowHomePageProperty;
            }

            set
            {
                if (_isShowHomePageProperty == value)
                {
                    return;
                }

                _isShowHomePageProperty = value;
                RaisePropertyChanged(IsShowHomePagePropertyName);


                _docksViewModel.IsAppDomainViewsVisible = !value;
            }
        }



        /// <summary>
        /// The <see cref="IsHomePageCanClose" /> property's name.
        /// </summary>
        public const string IsHomePageCanClosePropertyName = "IsHomePageCanClose";

        private bool _isHomePageCanCloseProperty = false;

        /// <summary>
        /// Sets and gets the IsHomePageCanClose property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsHomePageCanClose
        {
            get
            {
                return _isHomePageCanCloseProperty;
            }

            set
            {
                if (_isHomePageCanCloseProperty == value)
                {
                    return;
                }

                _isHomePageCanCloseProperty = value;
                RaisePropertyChanged(IsHomePageCanClosePropertyName);
            }
        }




        /// <summary>
        /// The <see cref="IsLoading" /> property's name.
        /// </summary>
        public const string IsLoadingPropertyName = "IsLoading";

        private bool _isLoadingProperty = false;

        /// <summary>
        /// Sets and gets the IsLoading property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return _isLoadingProperty;
            }

            set
            {
                if (_isLoadingProperty == value)
                {
                    return;
                }

                _isLoadingProperty = value;
                RaisePropertyChanged(IsLoadingPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ApplicationName" /> property's name.
        /// </summary>
        public const string ApplicationNamePropertyName = "ApplicationName";

        private string _applicationNameProperty = "";

        /// <summary>
        /// Sets and gets the ApplicationName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return _applicationNameProperty;
            }

            set
            {
                if (_applicationNameProperty == value)
                {
                    return;
                }

                _applicationNameProperty = value;
                RaisePropertyChanged(ApplicationNamePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ProjectName" /> property's name.
        /// </summary>
        public const string ProjectNamePropertyName = "ProjectName";

        private string _projectNameProperty = "";

        /// <summary>
        /// Sets and gets the ProjectName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
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

                if(string.IsNullOrEmpty(value))
                {
                    ApplicationName = $"{_rpaTitleFlag}";
                }
                else
                {
                    ApplicationName = $"{_rpaTitleFlag} - {value}";
                }
                
            }
        }


        /// <summary>
        /// The <see cref="IsShowDebug" /> property's name.
        /// </summary>
        public const string IsShowDebugPropertyName = "IsShowDebug";

        private bool _isShowDebugProperty = true;

        /// <summary>
        /// Sets and gets the IsShowDebug property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowDebug
        {
            get
            {
                return _isShowDebugProperty;
            }

            set
            {
                if (_isShowDebugProperty == value)
                {
                    return;
                }

                _isShowDebugProperty = value;
                RaisePropertyChanged(IsShowDebugPropertyName);
            }
        }




        /// <summary>
        /// The <see cref="IsShowContinue" /> property's name.
        /// </summary>
        public const string IsShowContinuePropertyName = "IsShowContinue";

        private bool _isShowContinueProperty = false;

        /// <summary>
        /// Sets and gets the IsShowContinue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowContinue
        {
            get
            {
                return _isShowContinueProperty;
            }

            set
            {
                if (_isShowContinueProperty == value)
                {
                    return;
                }

                _isShowContinueProperty = value;
                RaisePropertyChanged(IsShowContinuePropertyName);
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
                        vm.FilePath = _projectManagerService.CurrentProjectPath;
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
                        vm.FilePath = _projectManagerService.CurrentProjectPath;
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
                        vm.FilePath = _projectManagerService.CurrentProjectPath;
                        vm.FileType = NewXamlFileViewModel.enFileType.StateMachine;

                        _windowService.ShowDialog(window);
                    },
                    () => !_workflowStateService.IsRunningOrDebugging));
            }
        }





        private RelayCommand _runWorkflowCommand;

        /// <summary>
        /// Gets the RunWorkflowCommand.
        /// </summary>
        public RelayCommand RunWorkflowCommand
        {
            get
            {
                return _runWorkflowCommand
                    ?? (_runWorkflowCommand = new RelayCommand(
                    () =>
                    {
                        SaveAllCommand.Execute(null);

                        _currentWorkflowRunService = _serviceLocator.ResolveType<IWorkflowRunService>();
                        _currentWorkflowRunService.Init(_docksViewModel.SelectedDocument.Path
                            , _projectManagerService.CurrentActivitiesDllLoadFrom, _projectManagerService.CurrentDependentAssemblies);

                        _currentWorkflowRunService.Run();

                    },
                    () => _docksViewModel.SelectedDocument is DesignerDocumentViewModel
                    && !string.IsNullOrEmpty(_docksViewModel.SelectedDocument?.Path)
                    && !_workflowStateService.IsRunningOrDebugging));
            }
        }



        private RelayCommand<CancelEventArgs> _closingCommand;

        public RelayCommand<CancelEventArgs> ClosingCommand
        {
            get
            {
                return _closingCommand
                    ?? (_closingCommand = new RelayCommand<CancelEventArgs>(
                    p =>
                    {
                        if(IsLoading)
                        {
                            p.Cancel = true;
                            return;
                        }

                        bool bContinueClose = _docksViewModel.CloseAllQuery();

                        if (!bContinueClose)
                        {
                            p.Cancel = true;
                        }
                        else
                        {
                            StopWorkflowCommand.Execute(null);
                            ClosingEvent?.Invoke(this, EventArgs.Empty);
                        }
                    }));
            }
        }



        private RelayCommand _closedCommand;

        /// <summary>
        /// Gets the ClosedCommand.
        /// </summary>
        public RelayCommand ClosedCommand
        {
            get
            {
                return _closedCommand
                    ?? (_closedCommand = new RelayCommand(
                    () =>
                    {
                        
                    }));
            }
        }





        private RelayCommand _saveCommand;

        /// <summary>
        /// Gets the SaveCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                    () =>
                    {
                        _docksViewModel.SelectedDocument?.Save();
                    },
                    () => _docksViewModel.SelectedDocument?.CanSave() == true));
            }
        }



        private RelayCommand _saveAsCommand;

        /// <summary>
        /// Gets the SaveAsCommand.
        /// </summary>
        public RelayCommand SaveAsCommand
        {
            get
            {
                return _saveAsCommand
                    ?? (_saveAsCommand = new RelayCommand(
                    () =>
                    {
                        var doc = _docksViewModel.SelectedDocument;

                        string userSelPath;
                        bool ret = _commonService.ShowSaveAsFileDialog(out userSelPath, doc.Title, _constantConfigService.XamlFileExtension,
                            Chainbot.Resources.Properties.Resources.FileDialog_Filter1, Chainbot.Resources.Properties.Resources.FileDialog_Title2);

                        if (ret == true)
                        {
                            try
                            {
                                var xamlText = doc.XamlText;
                                File.WriteAllText(userSelPath, xamlText);

                                doc.IsDirty = false;
                                doc.Path = userSelPath;
                                doc.ToolTip = doc.Path;
                                doc.Title = System.IO.Path.GetFileNameWithoutExtension(userSelPath);
                                doc.UpdatePathCrossDomain(doc.Path);

                                if (userSelPath.IsSubPathOf(_projectManagerService.CurrentProjectPath))
                                {
                                    _projectViewModel.RefreshCommand.Execute(null);
                                }
                            }
                            catch (Exception err)
                            {
                                SharedObject.Instance.Output(SharedObject.enOutputType.Error, Chainbot.Resources.Properties.Resources.Message_SaveAsDocumentError, err);
                                _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_SaveAsDocumentError);
                            }
                        }
                    },
                    () => _docksViewModel.SelectedDocument?.CanSave() == true));
            }
        }


        private RelayCommand _saveAllCommand;

        /// <summary>
        /// Gets the SaveAllCommand.
        /// </summary>
        public RelayCommand SaveAllCommand
        {
            get
            {
                return _saveAllCommand
                    ?? (_saveAllCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var doc in _docksViewModel.Documents)
                        {
                            doc.Save();
                        }
                    }));
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
                    },
                    () => IsProjectOpened));
            }
        }





        private RelayCommand _openProjectCommand;

        /// <summary>
        /// Gets the OpenProjectCommand.
        /// </summary>
        public RelayCommand OpenProjectCommand
        {
            get
            {
                return _openProjectCommand
                    ?? (_openProjectCommand = new RelayCommand(
                    () =>
                    {
                        var fileOpened = _commonService.ShowSelectSingleFileDialog(Chainbot.Resources.Properties.Resources.FileDialog_Filter3 + $"|{_constantConfigService.ProjectConfigFileName}|" + Chainbot.Resources.Properties.Resources.FileDialog_Filter2 + "|*.nupkg",
                            Chainbot.Resources.Properties.Resources.FileDialog_Title1);
                        string fileExt = System.IO.Path.GetExtension(fileOpened);
                        if (fileExt.ToLower() == ".nupkg")
                        {
                            var nupkgFilePath = fileOpened;

                            var packageImportService = _serviceLocator.ResolveType<IPackageImportService>();
                            if (packageImportService.Init(nupkgFilePath))
                            {
                                var window = new NewProjectWindow();
                                var vm = window.DataContext as NewProjectViewModel;
                                vm.Window = window;
                                vm.SwitchToImportNupkgWindow(packageImportService);
                                _windowService.ShowDialog(window);
                            }
                            else
                            {
                                _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_NupkgInvalid);
                            }
                        }
                        else if (fileExt.ToLower() == GlobalConstant.ProjectConfigFileExtension)
                        {
                            var projectConfigFilePath = fileOpened;
                            if (_projectManagerService.IsAlreadyOpened(projectConfigFilePath))
                            {
                                return;
                            }

                            if (!string.IsNullOrEmpty(projectConfigFilePath))
                            {
                                if (_projectManagerService.CloseCurrentProject())
                                {
                                    _projectManagerService.OpenProject(projectConfigFilePath);
                                }
                            }
                        }
                    }));
            }
        }






        private RelayCommand _cutCommand;

        /// <summary>
        /// Gets the CutCommand.
        /// </summary>
        public RelayCommand CutCommand
        {
            get
            {
                return _cutCommand
                    ?? (_cutCommand = new RelayCommand(
                    () =>
                    {
                        _docksViewModel.SelectedDocument?.Cut();
                    },
                    () => _docksViewModel.SelectedDocument?.CanCut() == true));
            }
        }


        private RelayCommand _copyCommand;

        /// <summary>
        /// Gets the CopyCommand.
        /// </summary>
        public RelayCommand CopyCommand
        {
            get
            {
                return _copyCommand
                    ?? (_copyCommand = new RelayCommand(
                    () =>
                    {
                        _docksViewModel.SelectedDocument?.Copy();
                    },
                    () => _docksViewModel.SelectedDocument?.CanCopy() == true));
            }
        }


        private RelayCommand _pasteCommand;

        /// <summary>
        /// Gets the PasteCommand.
        /// </summary>
        public RelayCommand PasteCommand
        {
            get
            {
                return _pasteCommand
                    ?? (_pasteCommand = new RelayCommand(
                    () =>
                    {
                        _docksViewModel.SelectedDocument?.Paste();
                    },
                    () => _docksViewModel.SelectedDocument?.CanPaste() == true));
            }
        }


        private RelayCommand _recordingCommand;

        /// <summary>
        /// Gets the RecordingCommand.
        /// </summary>
        public RelayCommand RecordingCommand
        {
            get
            {
                return _recordingCommand
                    ?? (_recordingCommand = new RelayCommand(
                    () =>
                    {
                        _windowService.ShowMainWindowMinimized();

                        var window = new RecordingWindow();
                        _windowService.ShowDialog(window, false);

                        _windowService.ShowMainWindowNormal();
                    },
                    () => _docksViewModel.SelectedDocument is DesignerDocumentViewModel && !_docksViewModel.SelectedDocument.IsReadOnly));
            }
        }



        private RelayCommand _webScraperCommand;

        /// <summary>
        /// Gets the WebScraperCommand.
        /// </summary>
        public RelayCommand WebScraperCommand
        {
            get
            {
                return _webScraperCommand
                    ?? (_webScraperCommand = new RelayCommand(
                    () =>
                    {
                        var window = new DataExtractorWindow();
                        window.Owner = Application.Current.MainWindow;
                        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        window.ShowDialog();
                    },
                    () => _docksViewModel.SelectedDocument is DesignerDocumentViewModel && !_docksViewModel.SelectedDocument.IsReadOnly));
            }
        }



        private RelayCommand _publishCommand;

        /// <summary>
        /// Gets the PublishCommand.
        /// </summary>
        public RelayCommand PublishCommand
        {
            get
            {
                return _publishCommand
                    ?? (_publishCommand = new RelayCommand(
                    () =>
                    {
                        var window = new PublishWindow();
                        var vm = window.DataContext as PublishViewModel;
                        vm.Window = window;
                        _windowService.ShowDialog(window);
                    },
                    () => IsProjectOpened));
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
                        var window = new ExportWindow();
                        var vm = window.DataContext as ExportViewModel;
                        vm.Window = window;
                        _windowService.ShowDialog(window);
                    },
                    () => IsProjectOpened));
            }
        }



        private RelayCommand _stopWorkflowCommand;

        /// <summary>
        /// Gets the StopWorkflowCommand.
        /// </summary>
        public RelayCommand StopWorkflowCommand
        {
            get
            {
                return _stopWorkflowCommand
                    ?? (_stopWorkflowCommand = new RelayCommand(
                    () =>
                    {
                        if (_workflowStateService.IsRunning)
                        {
                            _currentWorkflowRunService.Stop();
                        }

                        if (_workflowStateService.IsDebugging)
                        {
                            if (CurrentWorkflowDebugService == null)
                            {
                                CurrentWorkflowDebugService = _serviceLocator.ResolveType<IWorkflowDebugService>();
                            }

                            CurrentWorkflowDebugService.Stop(_docksViewModel.SelectedDocument.Path);
                        }
                    },
                    () => _workflowStateService.IsRunningOrDebugging));
            }
        }


        private RelayCommand _debugOrContinueWorkflowCommand;

        public RelayCommand DebugOrContinueWorkflowCommand
        {
            get
            {
                return _debugOrContinueWorkflowCommand
                    ?? (_debugOrContinueWorkflowCommand = new RelayCommand(
                    () =>
                    {
                        DebugWorkflowCommand.Execute(null);
                        ContinueWorkflowCommand.Execute(null);
                    },
                    () =>true));
            }
        }


        private RelayCommand _debugWorkflowCommand;

        /// <summary>
        /// Gets the DebugWorkflowCommand.
        /// </summary>
        public RelayCommand DebugWorkflowCommand
        {
            get
            {
                return _debugWorkflowCommand
                    ?? (_debugWorkflowCommand = new RelayCommand(
                    () =>
                    {
                        SaveAllCommand.Execute(null);

                        CurrentWorkflowDebugService = _serviceLocator.ResolveType<IWorkflowDebugService>();

                        var doc = _docksViewModel.SelectedDocument as DesignerDocumentViewModel;
                        CurrentWorkflowDebugService.Init(doc.GetWorkflowDesignerServiceProxy(), _docksViewModel.SelectedDocument.Path
                            , _projectManagerService.CurrentActivitiesDllLoadFrom, _projectManagerService.CurrentDependentAssemblies);

                        CurrentWorkflowDebugService.SetNextOperate(enOperate.Null);
                        CurrentWorkflowDebugService.Debug();
                    },
                    () => _docksViewModel.SelectedDocument is DesignerDocumentViewModel
                    && !string.IsNullOrEmpty(_docksViewModel.SelectedDocument?.Path)
                    && !_workflowStateService.IsRunningOrDebugging));
            }
        }



        private RelayCommand _continueWorkflowCommand;

        /// <summary>
        /// Gets the ContinueWorkflowCommand.
        /// </summary>
        public RelayCommand ContinueWorkflowCommand
        {
            get
            {
                return _continueWorkflowCommand
                    ?? (_continueWorkflowCommand = new RelayCommand(
                    () =>
                    {
                        if (CurrentWorkflowDebugService == null)
                        {
                            CurrentWorkflowDebugService = _serviceLocator.ResolveType<IWorkflowDebugService>();
                        }

                        CurrentWorkflowDebugService.Continue();
                    },
                    () => _workflowStateService.IsDebuggingPaused));
            }
        }



        private RelayCommand _toggleBreakpointCommand;

        /// <summary>
        /// Gets the ToggleBreakpointCommand.
        /// </summary>
        public RelayCommand ToggleBreakpointCommand
        {
            get
            {
                return _toggleBreakpointCommand
                    ?? (_toggleBreakpointCommand = new RelayCommand(
                    () =>
                    {
                        _workflowBreakpointsServiceProxy.ToggleBreakpoint(_docksViewModel.SelectedDocument?.Path);
                    },
                    () => _docksViewModel.SelectedDocument is DesignerDocumentViewModel
                    && !string.IsNullOrEmpty(_docksViewModel.SelectedDocument?.Path)
                    ));
            }
        }


        private RelayCommand _removeAllBreakpointsCommand;

        /// <summary>
        /// Gets the RemoveAllBreakpointsCommand.
        /// </summary>
        public RelayCommand RemoveAllBreakpointsCommand
        {
            get
            {
                return _removeAllBreakpointsCommand
                    ?? (_removeAllBreakpointsCommand = new RelayCommand(
                    () =>
                    {
                        _workflowBreakpointsServiceProxy.RemoveAllBreakpoints();
                    },
                    () => _docksViewModel.SelectedDocument is DesignerDocumentViewModel
                    && !string.IsNullOrEmpty(_docksViewModel.SelectedDocument?.Path)
                    ));
            }
        }



        private RelayCommand _breakCommand;

        /// <summary>
        /// Gets the BreakCommand.
        /// </summary>
        public RelayCommand BreakCommand
        {
            get
            {
                return _breakCommand
                    ?? (_breakCommand = new RelayCommand(
                    () =>
                    {
                        if (CurrentWorkflowDebugService == null)
                        {
                            CurrentWorkflowDebugService = _serviceLocator.ResolveType<IWorkflowDebugService>();
                        }

                        CurrentWorkflowDebugService.Break();
                    },
                    () => _workflowStateService.IsDebugging && !_workflowStateService.IsDebuggingPaused));
            }
        }



        private RelayCommand _stepIntoCommand;

        /// <summary>
        /// Gets the StepIntoCommand.
        /// </summary>
        public RelayCommand StepIntoCommand
        {
            get
            {
                return _stepIntoCommand
                    ?? (_stepIntoCommand = new RelayCommand(
                    () =>
                    {
                        if (!_workflowStateService.IsRunningOrDebugging)
                        {
                            SaveAllCommand.Execute(null);

                            CurrentWorkflowDebugService = _serviceLocator.ResolveType<IWorkflowDebugService>();

                            var doc = _docksViewModel.SelectedDocument as DesignerDocumentViewModel;
                            CurrentWorkflowDebugService.Init(doc.GetWorkflowDesignerServiceProxy(), _docksViewModel.SelectedDocument.Path
                                , _projectManagerService.CurrentActivitiesDllLoadFrom, _projectManagerService.CurrentDependentAssemblies);
                            CurrentWorkflowDebugService.SetNextOperate(enOperate.StepInto);

                            CurrentWorkflowDebugService.Debug();
                        }
                        else
                        {
                            if (CurrentWorkflowDebugService == null)
                            {
                                CurrentWorkflowDebugService = _serviceLocator.ResolveType<IWorkflowDebugService>();
                            }

                            CurrentWorkflowDebugService.Continue(enOperate.StepInto);
                        }
                    },
                    () => !_workflowStateService.IsRunning
                    && _docksViewModel.SelectedDocument is DesignerDocumentViewModel
                    && !string.IsNullOrEmpty(_docksViewModel.SelectedDocument?.Path)
                    && (!_workflowStateService.IsDebugging || (_workflowStateService.IsDebugging && _workflowStateService.IsDebuggingPaused))
                    ));
            }
        }



        private RelayCommand _stepOverCommand;

        /// <summary>
        /// Gets the StepOverCommand.
        /// </summary>
        public RelayCommand StepOverCommand
        {
            get
            {
                return _stepOverCommand
                    ?? (_stepOverCommand = new RelayCommand(
                    () =>
                    {
                        if (!_workflowStateService.IsRunningOrDebugging)
                        {
                            SaveAllCommand.Execute(null);

                            CurrentWorkflowDebugService = _serviceLocator.ResolveType<IWorkflowDebugService>();

                            var doc = _docksViewModel.SelectedDocument as DesignerDocumentViewModel;
                            CurrentWorkflowDebugService.Init(doc.GetWorkflowDesignerServiceProxy(), _docksViewModel.SelectedDocument.Path
                                , _projectManagerService.CurrentActivitiesDllLoadFrom, _projectManagerService.CurrentDependentAssemblies);
                            CurrentWorkflowDebugService.SetNextOperate(enOperate.StepOver);

                            CurrentWorkflowDebugService.Debug();
                        }
                        else
                        {
                            if (CurrentWorkflowDebugService == null)
                            {
                                CurrentWorkflowDebugService = _serviceLocator.ResolveType<IWorkflowDebugService>();
                            }

                            CurrentWorkflowDebugService.Continue(enOperate.StepOver);
                        }
                    },
                    () => !_workflowStateService.IsRunning
                    && _docksViewModel.SelectedDocument is DesignerDocumentViewModel
                    && !string.IsNullOrEmpty(_docksViewModel.SelectedDocument?.Path)
                   && (!_workflowStateService.IsDebugging || (_workflowStateService.IsDebugging && _workflowStateService.IsDebuggingPaused))
                    ));
            }
        }



        /// <summary>
        /// The <see cref="SlowStepSpeed" /> property's name.
        /// </summary>
        public const string SlowStepSpeedPropertyName = "SlowStepSpeed";

        private enSlowStepSpeed _slowStepSpeedProperty = enSlowStepSpeed.Off;

        public enSlowStepSpeed SlowStepSpeed
        {
            get
            {
                return _slowStepSpeedProperty;
            }

            set
            {
                if (_slowStepSpeedProperty == value)
                {
                    return;
                }

                _slowStepSpeedProperty = value;
                RaisePropertyChanged(SlowStepSpeedPropertyName);
            }
        }



        private RelayCommand _slowStepCommand;

        public RelayCommand SlowStepCommand
        {
            get
            {
                return _slowStepCommand
                    ?? (_slowStepCommand = new RelayCommand(
                    () =>
                    {
                        switch (SlowStepSpeed)
                        {
                            case enSlowStepSpeed.Off:
                                SlowStepSpeed = enSlowStepSpeed.One;
                                _workflowStateService.SpeedType = enSpeed.One;
                                break;
                            case enSlowStepSpeed.One:
                                SlowStepSpeed = enSlowStepSpeed.Two;
                                _workflowStateService.SpeedType = enSpeed.Two;
                                break;
                            case enSlowStepSpeed.Two:
                                SlowStepSpeed = enSlowStepSpeed.Three;
                                _workflowStateService.SpeedType = enSpeed.Three;
                                break;
                            case enSlowStepSpeed.Three:
                                SlowStepSpeed = enSlowStepSpeed.Four;
                                _workflowStateService.SpeedType = enSpeed.Four;
                                break;
                            case enSlowStepSpeed.Four:
                                SlowStepSpeed = enSlowStepSpeed.Off;
                                _workflowStateService.SpeedType = enSpeed.Off;
                                break;
                            default:
                                break;
                        }

                        _workflowStateService.RaiseUpdateSlowStepSpeedEvent(_workflowStateService.SpeedType);
                    }));
            }
        }



        private RelayCommand _validateWorkflowCommand;

        public RelayCommand ValidateWorkflowCommand
        {
            get
            {
                return _validateWorkflowCommand
                    ?? (_validateWorkflowCommand = new RelayCommand(
                    () =>
                    {
                        if(_activitiesServiceProxy.IsXamlStringValid(_docksViewModel.SelectedDocument.XamlText, _docksViewModel.SelectedDocument.Path))
                        {
                            _messageBoxService.ShowInformation(Chainbot.Resources.Properties.Resources.Message_WorkflowVaildateSuccess);
                        }
                        else
                        {
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_WorkflowVaildateError);
                        }
                    },
                    () => !string.IsNullOrEmpty(_docksViewModel.SelectedDocument?.Path)
                    ));
            }
        }



        /// <summary>
        /// The <see cref="IsLogActivities" /> property's name.
        /// </summary>
        public const string IsLogActivitiesPropertyName = "IsLogActivities";

        private bool _isLogActivitiesProperty = false;

        public bool IsLogActivities
        {
            get
            {
                return _isLogActivitiesProperty;
            }

            set
            {
                if (_isLogActivitiesProperty == value)
                {
                    return;
                }

                _isLogActivitiesProperty = value;
                RaisePropertyChanged(IsLogActivitiesPropertyName);

                _workflowStateService.IsLogActivities = value;
                _workflowStateService.RaiseUpdateIsLogActivitiesEvent(_workflowStateService.IsLogActivities);
            }
        }



        private RelayCommand _openLogsCommand;

        /// <summary>
        /// Gets the OpenLogsCommand.
        /// </summary>
        public RelayCommand OpenLogsCommand
        {
            get
            {
                return _openLogsCommand
                    ?? (_openLogsCommand = new RelayCommand(
                    () =>
                    {
                        _commonService.LocateDirInExplorer(_pathConfigService.LogsDir);
                    }));
            }
        }


        private RelayCommand _openSearchViewCommand;

        /// <summary>
        /// Gets the OpenSearchViewCommand.
        /// </summary>
        public RelayCommand OpenSearchViewCommand
        {
            get
            {
                return _openSearchViewCommand
                    ?? (_openSearchViewCommand = new RelayCommand(
                    () =>
                    {
                        if (_searchViewWindow == null)
                        {
                            _searchViewWindow = new SearchViewWindow();
                            _searchViewWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                        }

                        _searchViewWindow.Left = View.ActualLeft() + View.ActualWidth / 2 - _searchViewWindow.Width / 2;

                        var height = View.Ribbon.ActualHeight;
                        _searchViewWindow.Top = View.ActualTop() + height + 40;

                        _searchViewWindow.Show();
                        _searchViewWindow.SearchTextBox.Focus();
                        _searchViewModel.StartSearch();
                    },
                    () => IsProjectOpened));
            }
        }

        private RelayCommand _helpCommand;

        /// <summary>
        /// Gets the HelpCommand.
        /// </summary>
        public RelayCommand HelpCommand
        {
            get
            {
                return _helpCommand
                    ?? (_helpCommand = new RelayCommand(
                    () =>
                    {
                        var path = System.IO.Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"Doc\ChainbotStudio-Help.chm");
                        Common.ShellExecute(path);
                    }));
            }
        }


    }
}