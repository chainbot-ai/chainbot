using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.Utils;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using Chainbot.Contracts.Config;
using System.Windows.Media.Imaging;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Activities;
using ChainbotStudio.DragDrop;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using Chainbot.Cores.Classes;
using Chainbot.Contracts.UI;
using GongSolutions.Wpf.DragDrop;
using NuGet.Versioning;
using Newtonsoft.Json.Linq;
using System.IO;
using Plugins.Shared.Library;
using System.Windows.Media;
using System.Windows;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProjectViewModel : ViewModelBase
    {
        private IServiceLocator _serviceLocator;
        private IDirectoryService _directoryService;
        private IProjectManagerService _projectManagerService;
        private IConstantConfigService _constantConfigService;
        private IActivitiesServiceProxy _activitiesServiceProxy;
        private ICommonService _commonService;
        private IDispatcherService _dispatcherService;
        private IMessageBoxService _messageBoxService;

        public IDragSource ProjectItemDragHandler { get; set; }

        public IDropTarget ProjectItemDropHandler { get; set; }

        private ProjectRootItemViewModel _projectRootVM;
        private ProjectDirItemViewModel _projectScreenshotsItemVM;

        public Dictionary<string, bool> IsExpandedDict = new Dictionary<string, bool>();


        private Dictionary<string, ProjectBaseItemViewModel> ItemPathDict = new Dictionary<string, ProjectBaseItemViewModel>();

        /// <summary>
        /// Initializes a new instance of the ProjectViewModel class.
        /// </summary>
        public ProjectViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _directoryService = _serviceLocator.ResolveType<IDirectoryService>();
            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();
            _constantConfigService = _serviceLocator.ResolveType<IConstantConfigService>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();
            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();

            ProjectItemDragHandler = _serviceLocator.ResolveType<IDragSource>();
            ProjectItemDropHandler = _serviceLocator.ResolveType<IDropTarget>();

            _projectManagerService.ProjectOpenEvent += _projectManagerService_ProjectOpenEvent;
            _projectManagerService.ProjectCloseEvent += _projectManagerService_ProjectCloseEvent;


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


        private void Init(bool bOpenMainXaml = true)
        {
            ProjectItems.Clear();
            ItemPathDict.Clear();
            var fileOrDirItems = _directoryService.Query(_projectManagerService.CurrentProjectPath
                , (DirOrFileItem item) => (
                !(
                item is DirItem && item.Name == _constantConfigService.ProjectLocalDirectoryName 
                || (item is FileItem && !new List<string>() { _constantConfigService.XamlFileExtension, ".py", ".js" }.Contains((item as FileItem).Extension.ToLower()))) 
                ));

            var projectRoot = _serviceLocator.ResolveType<ProjectRootItemViewModel>();
            _projectRootVM = projectRoot;
            projectRoot.IsExpanded = true;
            projectRoot.Name = _projectManagerService.CurrentProjectJsonConfig.name;
            projectRoot.ProjectPath = _projectManagerService.CurrentProjectPath;
            projectRoot.Path = _projectManagerService.CurrentProjectPath;
            projectRoot.ToolTip = _projectManagerService.CurrentProjectPath;

            var dependRootItem = InitDependenciesItems();
            projectRoot.Children.Add(dependRootItem);

            ProjectFileItemViewModel mainXamlFileItem;
            TransformChildren(fileOrDirItems, projectRoot.Children, out mainXamlFileItem);

            ProjectItems.Add(projectRoot);

            if(bOpenMainXaml)
            {
                mainXamlFileItem?.OpenXamlCommand.Execute(null);
            }

            Directory.SetCurrentDirectory(SharedObject.Instance.ProjectPath);
        }

        private ProjectDependRootItem InitDependenciesItems()
        {
            var dependRootItem = _serviceLocator.ResolveType<ProjectDependRootItem>();
            dependRootItem.Name = Chainbot.Resources.Properties.Resources.Name_Dependencies;
   
            dependRootItem.IsExpanded = true;

            var json_cfg = _projectManagerService.CurrentProjectJsonConfig;

            foreach (JProperty jp in (JToken)json_cfg.dependencies)
            {
                var ver_range = VersionRange.Parse((string)jp.Value);

                string ver_desc = "";

                if (ver_range.MinVersion == ver_range.MaxVersion)
                {
                    ver_desc = $" = {ver_range.MinVersion}";
                }
                else
                {
                    if (ver_range.MinVersion != null && ver_range.IsMinInclusive)
                    {
                        ver_desc = $" >= {ver_range.MinVersion}";
                    }
                }

                var desc = jp.Name + ver_desc;

                var dependItem = _serviceLocator.ResolveType<ProjectDependItem>();
                dependItem.Name = desc;
                dependRootItem.Children.Add(dependItem);
            }

            return dependRootItem;
        }

        private void _projectManagerService_ProjectOpenEvent(object sender, System.EventArgs e)
        {
            _dispatcherService.InvokeAsync(()=> {
                IsProjectOpened = true;

                var service = sender as IProjectManagerService;

                _activitiesServiceProxy = service.CurrentActivitiesServiceProxy;

                Init();
            });
        }

        private void _projectManagerService_ProjectCloseEvent(object sender, System.EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                IsProjectOpened = false;
                IsSearchResultEmpty = false;
                
                ProjectItems.Clear();
                IsExpandedDict.Clear();
                ItemPathDict.Clear();
                SearchText = "";
            });
        }

        public void Refresh()
        {
            Init(false);
            StartSearch();
        }

        //private DrawingImage Convert(string svgFile)
        //{
        //    WpfDrawingSettings _wpfSettings = new WpfDrawingSettings();
        //    _wpfSettings.IncludeRuntime = false;
        //    _wpfSettings.TextAsGeometry = false;

        //    var converter = new DrawingSvgConverter(_wpfSettings);
        //    var iconStream = System.Windows.Application.GetResourceStream(new Uri($"{svgFile}", UriKind.Absolute)).Stream;
        //    DrawingGroup drawingGroup = converter.Read(iconStream);
        //    DrawingImage drawingImage = new DrawingImage(drawingGroup);

        //    return drawingImage;
        //}

        private void TransformChildren(List<DirOrFileItem> children1, ObservableCollection<ProjectBaseItemViewModel> children2,out ProjectFileItemViewModel mainXamlFileItem)
        {
            mainXamlFileItem = null;

            foreach (var dirOrFileItem in children1)
            {
                if (dirOrFileItem is DirItem)
                {
                    var dirItem = dirOrFileItem as DirItem;
                    var item = _serviceLocator.ResolveType<ProjectDirItemViewModel>();
                    item.Name = dirItem.Name;
                    item.Path = dirItem.Path;
                    item.IsScreenshots = dirItem.Name.ToLower() == _constantConfigService.ScreenshotsPath.ToLower();
                    if(item.IsScreenshots)
                    {
                        _projectScreenshotsItemVM = item;
                    }
                    TransformChildren(dirItem.Children, item.Children,out mainXamlFileItem);
                    children2.Add(item);
                    ItemPathDict[item.Path] = item;
                }
                else if (dirOrFileItem is FileItem)
                {
                    var fileItem = dirOrFileItem as FileItem;
                    var item = _serviceLocator.ResolveType<ProjectFileItemViewModel>();
                    item.Name = fileItem.Name;
                    item.Path = fileItem.Path;

                    if (fileItem.Extension.ToLower() == _constantConfigService.XamlFileExtension.ToLower())
                    {
                        item.Icon = Application.Current.TryFindResource("XamlDrawingImage") as DrawingImage;
                        item.IsXamlFile = true;

                        item.ActivityFactoryAssemblyQualifiedName = _activitiesServiceProxy.GetAssemblyQualifiedName(ConstantAssemblyQualifiedName.InvokeWorkflowFileFactory);

                        if(_projectManagerService.ActivitiesTypeOfDict.ContainsKey(ConstantAssemblyQualifiedName.InvokeWorkflowFileActivity))
                        {
                            item.ActivityDisplayName = _projectManagerService.ActivitiesTypeOfDict[ConstantAssemblyQualifiedName.InvokeWorkflowFileActivity].Name;
                            item.ActivityAssemblyQualifiedName = _activitiesServiceProxy.GetAssemblyQualifiedName(ConstantAssemblyQualifiedName.InvokeWorkflowFileActivity);
                        }
                    }else if(fileItem.Name.ToLower() == _constantConfigService.ProjectConfigFileName.ToLower())
                    {
                        item.IsProjectJsonFile = true;
                    }
                    else
                    {
                        if(fileItem.Extension.ToLower() == ".py")
                        {
                            item.IsPythonFile = true;

                            item.ActivityFactoryAssemblyQualifiedName = _activitiesServiceProxy.GetAssemblyQualifiedName(ConstantAssemblyQualifiedName.InvokePythonFileFactory);

                            if (_projectManagerService.ActivitiesTypeOfDict.ContainsKey(ConstantAssemblyQualifiedName.InvokePythonFileActivity))
                            {
                                item.ActivityDisplayName = _projectManagerService.ActivitiesTypeOfDict[ConstantAssemblyQualifiedName.InvokePythonFileActivity].Name;
                                item.ActivityAssemblyQualifiedName = _activitiesServiceProxy.GetAssemblyQualifiedName(ConstantAssemblyQualifiedName.InvokePythonFileActivity);
                            }
                        }else if(fileItem.Extension.ToLower() == ".js")
                        {
                            item.IsJavaScriptFile = true;

                            item.ActivityFactoryAssemblyQualifiedName = _activitiesServiceProxy.GetAssemblyQualifiedName(ConstantAssemblyQualifiedName.InjectJavaScriptFileFactory);

                            if (_projectManagerService.ActivitiesTypeOfDict.ContainsKey(ConstantAssemblyQualifiedName.InjectJavaScriptFileActivity))
                            {
                                item.ActivityDisplayName = _projectManagerService.ActivitiesTypeOfDict[ConstantAssemblyQualifiedName.InjectJavaScriptFileActivity].Name;
                                item.ActivityAssemblyQualifiedName = _activitiesServiceProxy.GetAssemblyQualifiedName(ConstantAssemblyQualifiedName.InjectJavaScriptFileActivity);
                            }
                        }

                        item.Icon = fileItem.AssociatedIcon;
                    }

                    if (item.Path == _projectManagerService.CurrentProjectMainXamlFileAbsolutePath)
                    {
                        item.Icon = Application.Current.TryFindResource("MainDrawingImage") as DrawingImage;
                        item.IsMainXamlFile = true;
                        mainXamlFileItem = item;
                    }

                    children2.Add(item);
                    ItemPathDict[item.Path] = item;
                }
            }
        }

        
        public void OnMoveDir(string srcPath, string dstPath)
        {
            Refresh();
        }

        public void OnProjectSettingsModify(ProjectSettingsViewModel projectSettingsViewModel)
        {
            Refresh();
        }


        public void OnDeleteDir(string path)
        {
            Refresh();
        }

        public void OnDeleteFile(string path)
        {
            Refresh();
        }


        public void OnRename(RenameViewModel renameViewModel)
        {
            if(renameViewModel.IsMain)
            {
                var relativeMainXaml = _commonService.MakeRelativePath(_projectManagerService.CurrentProjectPath, renameViewModel.Dir + @"\" + renameViewModel.DstName);
                _projectManagerService.CurrentProjectJsonConfig.main = relativeMainXaml;
                _projectManagerService.SaveCurrentProjectJson();
            }

            var tempIsExpandedDict = new Dictionary<string, bool>();
            foreach (var item in IsExpandedDict)
            {
                if (item.Key.ContainsIgnoreCase(renameViewModel.Path))
                {
                    var newValue = item.Value;

                    var newKey = "";
                    if(item.Key.EqualsIgnoreCase(renameViewModel.Path))
                    {
                        newKey = item.Key.Replace(renameViewModel.Path, renameViewModel.NewPath);
                    }
                    else
                    {
                        newKey = item.Key.Replace(renameViewModel.Path + @"\", renameViewModel.NewPath + @"\");
                    }

                    tempIsExpandedDict[newKey] = newValue;
                }
                else
                {
                    tempIsExpandedDict[item.Key] = item.Value;
                }
            }

            IsExpandedDict = tempIsExpandedDict;

            Refresh();
        }

       


        /// <summary>
        /// The <see cref="ProjectItems" /> property's name.
        /// </summary>
        public const string ProjectItemsPropertyName = "ProjectItems";

        private ObservableCollection<ProjectRootItemViewModel> _projectItemsProperty = new ObservableCollection<ProjectRootItemViewModel>();


        public ObservableCollection<ProjectRootItemViewModel> ProjectItems
        {
            get
            {
                return _projectItemsProperty;
            }

            set
            {
                if (_projectItemsProperty == value)
                {
                    return;
                }

                _projectItemsProperty = value;
                RaisePropertyChanged(ProjectItemsPropertyName);
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
                        _projectRootVM.OpenDirCommand.Execute(null);
                    }));
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
                        _projectRootVM.OpenProjectSettingsCommand.Execute(null);
                    }));
            }
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
                        _serviceLocator.ResolveType<MainViewModel>().SaveAllCommand.Execute(null);

                        RefreshCommand.Execute(null);

                        if(_projectScreenshotsItemVM == null)
                        {
                            _messageBoxService.ShowInformation(Chainbot.Resources.Properties.Resources.Message_RemoveUnusedScreenshot1);
                        }
                        else
                        {
                            _projectScreenshotsItemVM.RemoveUnusedScreenshotsCommand.Execute(null);
                        }
                        
                    }));
            }
        }


        private void ProjectTreeItemSetAllIsSearching(ProjectBaseItemViewModel item, bool IsSearching)
        {
            item.IsSearching = IsSearching;
            foreach (var child in item.Children)
            {
                ProjectTreeItemSetAllIsSearching(child, IsSearching);
            }
        }

        private void ProjectTreeItemSetAllIsMatch(ProjectBaseItemViewModel item, bool IsMatch)
        {
            item.IsMatch = IsMatch;
            foreach (var child in item.Children)
            {
                ProjectTreeItemSetAllIsMatch(child, IsMatch);
            }
        }

        private void ProjectTreeItemSetAllSearchText(ProjectBaseItemViewModel item, string SearchText)
        {
            item.SearchText = SearchText;
            foreach (var child in item.Children)
            {
                ProjectTreeItemSetAllSearchText(child, SearchText);
            }
        }



        /// <summary>
        /// The <see cref="IsSearchResultEmpty" /> property's name.
        /// </summary>
        public const string IsSearchResultEmptyPropertyName = "IsSearchResultEmpty";

        private bool _isSearchResultEmptyProperty = false;

        public bool IsSearchResultEmpty
        {
            get
            {
                return _isSearchResultEmptyProperty;
            }

            set
            {
                if (_isSearchResultEmptyProperty == value)
                {
                    return;
                }

                _isSearchResultEmptyProperty = value;
                RaisePropertyChanged(IsSearchResultEmptyPropertyName);
            }
        }


        private async Task doSearch()
        {
            await Task.Run(() =>
            {
                string searchContent = SearchText ?? "";

                searchContent = searchContent.Trim();

                if (string.IsNullOrEmpty(searchContent))
                {
                    foreach (var item in ProjectItems)
                    {
                        ProjectTreeItemSetAllIsSearching(item, false);
                    }

                    foreach (var item in ProjectItems)
                    {
                        ProjectTreeItemSetAllSearchText(item, "");
                    }

                    IsSearchResultEmpty = false;
                }
                else
                {
                    foreach (var item in ProjectItems)
                    {
                        ProjectTreeItemSetAllIsSearching(item, true);
                    }

                    foreach (var item in ProjectItems)
                    {
                        ProjectTreeItemSetAllIsMatch(item, false);
                    }


                    foreach (var item in ProjectItems)
                    {
                        item.ApplyCriteria(searchContent, new Stack<ProjectBaseItemViewModel>());
                    }

                    IsSearchResultEmpty = true;
                    foreach (var item in ProjectItems)
                    {
                        if (item.IsMatch)
                        {
                            IsSearchResultEmpty = false;
                            break;
                        }
                    }

                }
            });
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

                StartSearch();

            }
        }

        public ProjectBaseItemViewModel GetProjectItemByFullPath(string xamlFilePath)
        {
            if(ItemPathDict.ContainsKey(xamlFilePath))
            {
                return ItemPathDict[xamlFilePath];
            }

            return null;
        }

        private async void StartSearch()
        {
            await doSearch();
        }



        private RelayCommand _refreshCommand;

        /// <summary>
        /// Gets the RefreshCommand.
        /// </summary>
        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                    () =>
                    {
                        Refresh();
                    }));
            }
        }


        private void ProjectTreeItemSetAllIsExpanded(ProjectBaseItemViewModel item, bool IsExpanded)
        {
            item.IsExpanded = IsExpanded;
            foreach (var child in item.Children)
            {
                ProjectTreeItemSetAllIsExpanded(child, IsExpanded);
            }
        }

        private RelayCommand _expandAllCommand;

        /// <summary>
        /// Gets the ExpandAllCommand.
        /// </summary>
        public RelayCommand ExpandAllCommand
        {
            get
            {
                return _expandAllCommand
                    ?? (_expandAllCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var item in ProjectItems)
                        {
                            ProjectTreeItemSetAllIsExpanded(item, true);
                        }
                    }));
            }
        }



        private RelayCommand _collapseAllCommand;

        /// <summary>
        /// Gets the CollapseAllCommand.
        /// </summary>
        public RelayCommand CollapseAllCommand
        {
            get
            {
                return _collapseAllCommand
                    ?? (_collapseAllCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var item in ProjectItems)
                        {
                            ProjectTreeItemSetAllIsExpanded(item, false);
                        }
                    }));
            }
        }



    }
}