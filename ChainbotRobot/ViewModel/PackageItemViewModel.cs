using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Windows.Controls;
using System;
using System.IO;
using NuGet;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using log4net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chainbot.Contracts.Utils;
using Plugins.Shared.Library.Librarys;
using Chainbot.Cores.Classes;
using ChainbotRobot.Contracts;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using ChainbotRobot.Views;
using AppDomainToolkit;
using ChainbotRobot.Cores;

namespace ChainbotRobot.ViewModel
{
    public class PackageItemViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;

        public IPackage Package { get; internal set; }

        public List<string> _versionList { get; set; } = new List<string>();

        private IServiceLocator _serviceLocator;

        private ICommonService _commonService;

        private IAutoCloseMessageBoxService _autoCloseMessageBoxService;

        private MainViewModel _mainViewModel;


        private StartupViewModel _startupViewModel;

        private IRunManagerService _runManagerService;

        public PackageItemViewModel(IServiceLocator serviceLocator, ICommonService commonService, IAutoCloseMessageBoxService autoCloseMessageBoxService
            ,MainViewModel mainViewModel, StartupViewModel startupViewModel, ILogService logService
            , IRunManagerService runManagerService)
        {
            _serviceLocator = serviceLocator;

            _commonService = commonService;

            _autoCloseMessageBoxService = autoCloseMessageBoxService;

            _mainViewModel = mainViewModel;

            _startupViewModel = startupViewModel;

            _logService = logService;

            _runManagerService = runManagerService;
        }

        /// <summary>
        /// The <see cref="IsRunning" /> property's name.
        /// </summary>
        public const string IsRunningPropertyName = "IsRunning";

        private bool _isRunningProperty = false;

        public bool IsRunning
        {
            get
            {
                return _isRunningProperty;
            }

            set
            {
                if (_isRunningProperty == value)
                {
                    return;
                }

                _isRunningProperty = value;
                RaisePropertyChanged(IsRunningPropertyName);

                StartCommand.RaiseCanExecuteChanged();
                UpdateCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string _nameProperty = "";


        public string Name
        {
            get
            {
                return _nameProperty;
            }

            set
            {
                if (_nameProperty == value)
                {
                    return;
                }

                _nameProperty = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Version" /> property's name.
        /// </summary>
        public const string VersionPropertyName = "Version";

        private string _versionProperty = "";

        public string Version
        {
            get
            {
                return _versionProperty;
            }

            set
            {
                if (_versionProperty == value)
                {
                    return;
                }

                _versionProperty = value;
                RaisePropertyChanged(VersionPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsPackageEnable" /> property's name.
        /// </summary>
        public const string IsPackageEnablePropertyName = "IsPackageEnable";

        private bool _isPackageEnableProperty = false;

        /// <summary>
        /// Sets and gets the IsPackageEnable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPackageEnable
        {
            get
            {
                return _isPackageEnableProperty;
            }

            set
            {
                if (_isPackageEnableProperty == value)
                {
                    return;
                }

                _isPackageEnableProperty = value;
                RaisePropertyChanged(IsPackageEnablePropertyName);
            }
        }




        /// <summary>
        /// The <see cref="IsMouseOver" /> property's name.
        /// </summary>
        public const string IsMouseOverPropertyName = "IsMouseOver";

        private bool _isMouseOverProperty = false;

        public bool IsMouseOver
        {
            get
            {
                return _isMouseOverProperty;
            }

            set
            {
                if (_isMouseOverProperty == value)
                {
                    return;
                }

                _isMouseOverProperty = value;
                RaisePropertyChanged(IsMouseOverPropertyName);
            }
        }



        private RelayCommand _mouseEnterCommand;

        public RelayCommand MouseEnterCommand
        {
            get
            {
                return _mouseEnterCommand
                    ?? (_mouseEnterCommand = new RelayCommand(
                    () =>
                    {
                        IsMouseOver = true;
                    }));
            }
        }


        private RelayCommand _mouseLeaveCommand;

        public RelayCommand MouseLeaveCommand
        {
            get
            {
                return _mouseLeaveCommand
                    ?? (_mouseLeaveCommand = new RelayCommand(
                    () =>
                    {
                        IsMouseOver = false;
                    }));
            }
        }


        /// <summary>
        /// The <see cref="IsNeedUpdate" /> property's name.
        /// </summary>
        public const string IsNeedUpdatePropertyName = "IsNeedUpdate";

        private bool _isNeedUpdateProperty = false;


        public bool IsNeedUpdate
        {
            get
            {
                return _isNeedUpdateProperty;
            }

            set
            {
                if (_isNeedUpdateProperty == value)
                {
                    return;
                }

                _isNeedUpdateProperty = value;
                RaisePropertyChanged(IsNeedUpdatePropertyName);
            }
        }


        private RelayCommand _copyItemInfoCommand;

        public RelayCommand CopyItemInfoCommand
        {
            get
            {
                return _copyItemInfoCommand
                    ?? (_copyItemInfoCommand = new RelayCommand(
                    () =>
                    {
                        Clipboard.SetDataObject(ToolTip);
                    }));
            }
        }


        private RelayCommand _locateItemCommand;

        public RelayCommand LocateItemCommand
        {
            get
            {
                return _locateItemCommand
                    ?? (_locateItemCommand = new RelayCommand(
                    () =>
                    {
                        var file = _mainViewModel.ProgramDataPackagesDir + @"\" + Name + @"." + Version+".nupkg";
                        _commonService.LocateFileInExplorer(file);
                    }));
            }
        }


        void DeleteNuPkgsFile(bool bRefresh = true)
        {
            foreach (var ver in _versionList)
            {
                var file = _mainViewModel.ProgramDataPackagesDir + @"\" + Name + @"." + ver + ".nupkg";
                _commonService.DeleteFile(file);
            }

            if(bRefresh)
            {
                Common.RunInUI(() =>
                {
                    _mainViewModel.RefreshCommand.Execute(null);
                });
            }
           
        }


        private RelayCommand _removeItemCommand;

        public RelayCommand RemoveItemCommand
        {
            get
            {
                return _removeItemCommand
                    ?? (_removeItemCommand = new RelayCommand(
                    () =>
                    {
                        var ret = _autoCloseMessageBoxService.Show(App.Current.MainWindow, Properties.Resources.AreYouSureToRemoveThePackage, Properties.Resources.ConfirmText, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (ret == MessageBoxResult.Yes)
                        {
                            if (this.Package != null)
                            {
                                var repo = PackageRepositoryFactory.Default.CreateRepository(_mainViewModel.ProgramDataPackagesDir);
                                var packageManager = new PackageManager(repo, _mainViewModel.ProgramDataInstalledPackagesDir);

                                packageManager.PackageUninstalled += (sender, eventArgs) =>
                                {
                                    if (!packageManager.LocalRepository.Exists(this.Name))
                                    {
                                        DeleteNuPkgsFile();
                                    }
                                };

                                if (packageManager.LocalRepository.Exists(this.Name))
                                {
                                    while (packageManager.LocalRepository.Exists(this.Name))
                                    {
                                        packageManager.UninstallPackage(this.Name);
                                    }

                                }
                                else
                                {
                                    DeleteNuPkgsFile();
                                }

                                
                            }
                        }
                    },
                    () => !IsRunning));
            }
        }

        private RelayCommand _mouseRightButtonUpCommand;

        public RelayCommand MouseRightButtonUpCommand
        {
            get
            {
                return _mouseRightButtonUpCommand
                    ?? (_mouseRightButtonUpCommand = new RelayCommand(
                    () =>
                    {
                        var view = App.Current.MainWindow;
                        var cm = view.FindResource("PackageItemContextMenu") as ContextMenu;
                        cm.DataContext = this;
                        cm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                        cm.IsOpen = true;
                    }));
            }
        }


    

        private RelayCommand _mouseDoubleClickCommand;

        public RelayCommand MouseDoubleClickCommand
        {
            get
            {
                return _mouseDoubleClickCommand
                    ?? (_mouseDoubleClickCommand = new RelayCommand(
                    () =>
                    {
                        if(IsPackageEnable)
                        {
                            updateOrStart();
                        }
                    }));
            }
        }

        private void updateOrStart()
        {
            if(IsNeedUpdate)
            {
                UpdateCommand.Execute(null);
            }
            else
            {
                StartCommand.Execute(null);
            }
        }



        /// <summary>
        /// The <see cref="IsVisible" /> property's name.
        /// </summary>
        public const string IsVisiblePropertyName = "IsVisible";

        private bool _isVisibleProperty = true;

        public bool IsVisible
        {
            get
            {
                return _isVisibleProperty;
            }

            set
            {
                if (_isVisibleProperty == value)
                {
                    return;
                }

                _isVisibleProperty = value;
                RaisePropertyChanged(IsVisiblePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsSelected" /> property's name.
        /// </summary>
        public const string IsSelectedPropertyName = "IsSelected";

        private bool _isSelectedProperty = false;

        public bool IsSelected
        {
            get
            {
                return _isSelectedProperty;
            }

            set
            {
                if (_isSelectedProperty == value)
                {
                    return;
                }

                _isSelectedProperty = value;
                RaisePropertyChanged(IsSelectedPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="ToolTip" /> property's name.
        /// </summary>
        public const string ToolTipPropertyName = "ToolTip";

        private string _toolTipProperty = null;

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




        /// <summary>
        /// The <see cref="IsSearching" /> property's name.
        /// </summary>
        public const string IsSearchingPropertyName = "IsSearching";

        private bool _isSearchingProperty = false;

        public bool IsSearching
        {
            get
            {
                return _isSearchingProperty;
            }

            set
            {
                if (_isSearchingProperty == value)
                {
                    return;
                }

                _isSearchingProperty = value;
                RaisePropertyChanged(IsSearchingPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SearchText" /> property's name.
        /// </summary>
        public const string SearchTextPropertyName = "SearchText";

        private string _searchTextProperty = "";

        public string SearchText
        {
            get
            {
                return _searchTextProperty;
            }

            set
            {
                if (_searchTextProperty == value)
                {
                    return;
                }

                _searchTextProperty = value;
                RaisePropertyChanged(SearchTextPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsMatch" /> property's name.
        /// </summary>
        public const string IsMatchPropertyName = "IsMatch";

        private bool _isMatchProperty = false;

        public bool IsMatch
        {
            get
            {
                return _isMatchProperty;
            }

            set
            {
                if (_isMatchProperty == value)
                {
                    return;
                }

                _isMatchProperty = value;
                RaisePropertyChanged(IsMatchPropertyName);
            }
        }


        public void ApplyCriteria(string criteria)
        {
            SearchText = criteria;

            if (IsCriteriaMatched(criteria))
            {
                IsMatch = true;

            }
        }

        private bool IsCriteriaMatched(string criteria)
        {
            return string.IsNullOrEmpty(criteria) || Name.ContainsIgnoreCase(criteria);
        }








        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        private RelayCommand _startCommand;

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand
                    ?? (_startCommand = new RelayCommand(
                    () =>
                    {
                        _runManagerService.UpdatePackageItem(this);
                        Log(SharedObject.enOutputType.Trace, Properties.Resources.ProcessStart);

                        if(_mainViewModel.IsWorkflowRunning)
                        {
                            Log(SharedObject.enOutputType.Warning, Properties.Resources.WorkflowAlreadyRunning);
                            //_autoCloseMessageBoxService.Show(App.Current.MainWindow, ResxIF.GetString("WorkflowAlreadyRunning"), ResxIF.GetString("PromptText"), MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        if (!File.Exists(Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, "ChainbotExecutor.exe")))
                        {
                            Log(SharedObject.enOutputType.Error, Properties.Resources.CannotFindChainbotExecutor);
                            _autoCloseMessageBoxService.Show(App.Current.MainWindow, Properties.Resources.CannotFindChainbotExecutor, Properties.Resources.PromptText, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var projectDir = _mainViewModel.ProgramDataInstalledPackagesDir + @"\" + Name + @"." + Version+ @"\lib\net45";
                        var projectJsonFile = projectDir + @"\project.json";
                        if (System.IO.File.Exists(projectJsonFile))
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Common.RunInUI(()=> {
                                        this.IsRunning = true;
                                        _mainViewModel.IsWorkflowRunning = true;
                                        _mainViewModel.WorkflowRunningName = Name;
                                        _mainViewModel.WorkflowRunningToolTip = ToolTip;
                                        _mainViewModel.WorkflowRunningStatus = Properties.Resources.LoadingProjectDependencies; 
                                        _mainViewModel.RefreshPackageItemRunningStatusByName(Name, true);
                                    });

                                    SharedObject.Instance.ProjectPath = projectDir;
                                    SharedObject.Instance.ClearOutputEvent();
                                    SharedObject.Instance.OutputEvent += Instance_OutputEvent;

                                    //var serv = _serviceLocator.ResolveType<ILoadDependenciesService>();
                                    //serv.Init(projectJsonFile);
                                    //await serv.LoadDependencies();

                                    var context = AppDomainContext.Create();

                                    List<string> currentActivitiesDllLoadFrom = null;
                                    List<string> currentDependentAssemblies = null;
                                    using (var remoteLoadDependenciesService = Remote<RemoteLoadDependenciesService>.CreateProxy(context.Domain, projectJsonFile,SharedObject.Instance))
                                    {
                                        currentActivitiesDllLoadFrom = remoteLoadDependenciesService.RemoteObject.GetCurrentActivitiesDllLoadFrom();
                                        currentDependentAssemblies = remoteLoadDependenciesService.RemoteObject.GetCurrentDependentAssemblies();
                                    }

                                    string json = System.IO.File.ReadAllText(projectJsonFile);
                                    JObject jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
                                    var relativeMainXaml = jsonObj["main"].ToString();
                                    var absoluteMainXaml = System.IO.Path.Combine(projectDir, relativeMainXaml);

                                    if (System.IO.File.Exists(absoluteMainXaml))
                                    {
                                        RunWorkflow(projectDir, absoluteMainXaml,currentActivitiesDllLoadFrom,currentDependentAssemblies);
                                    }
                                    else
                                    {
                                        Common.RunInUI(()=> {
                                            _autoCloseMessageBoxService.Show(App.Current.MainWindow, Properties.Resources.CannotFindMainXamlFileError, Properties.Resources.PromptText, MessageBoxButton.OK, MessageBoxImage.Error);
                                        });
                                    }
                                }
                                catch (Exception err)
                                {
                                    _logService.Error(err,_logger);

                                    Common.RunInUI(()=> {
                                        this.IsRunning = false;
                                        string errMsg = string.Format("{0}\nThe reason is:{1}", Properties.Resources.ProjectStartError, err.Message);
                                        Log(SharedObject.enOutputType.Warning, errMsg);
                                        _autoCloseMessageBoxService.Show(App.Current.MainWindow, errMsg, Properties.Resources.PromptText, MessageBoxButton.OK, MessageBoxImage.Error);
                                    });
                                }
                            });
                        }
                        else
                        {
                            Log(SharedObject.enOutputType.Error, Properties.Resources.CannotFindProjectJsonFile + "：" + projectJsonFile);
                            _autoCloseMessageBoxService.Show(App.Current.MainWindow, Properties.Resources.CannotFindProjectJsonFile, Properties.Resources.PromptText, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        
                    },
                    () => !IsRunning));
            }
        }


        private void RunWorkflow(string projectDir, string absoluteMainXaml,List<string> activitiesDllLoadFrom, List<string> dependentAssemblies)
        {
            System.GC.Collect();

            _runManagerService.Init(this,absoluteMainXaml, activitiesDllLoadFrom, dependentAssemblies);
            _runManagerService.Run();
        }

        private void Instance_OutputEvent(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            _runManagerService.LogToOutputWindow(type, msg, msgDetails);
        }

        private void Log(SharedObject.enOutputType type, string msg)
        {
            _runManagerService.Log(type,msg);
        }

        private RelayCommand _updateCommand;

        public RelayCommand UpdateCommand
        {
            get
            {
                return _updateCommand
                    ?? (_updateCommand = new RelayCommand(
                    () =>
                    {
                        if(this.Package != null)
                        {
                            var repo = PackageRepositoryFactory.Default.CreateRepository(_mainViewModel.ProgramDataPackagesDir);
                            var packageManager = new PackageManager(repo, _mainViewModel.ProgramDataInstalledPackagesDir);

                            packageManager.PackageInstalled += (sender, eventArgs) =>
                            {
                                _mainViewModel.RefreshCommand.Execute(null);
                            };

                            packageManager.InstallPackage(this.Package, true, true, true);
                        }
                    },
                    () => !IsRunning));
            }
        }










    }
}