using Chainbot.Contracts.Activities;
using Chainbot.Contracts.App;
using Chainbot.Contracts.AppDomains;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Project;
using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using ChainbotStudio.DragDrop;
using Chainbot.Contracts.Classes;
using System.Activities.Presentation;
using Chainbot.Cores.Classes;
using GalaSoft.MvvmLight.CommandWpf;
using Chainbot.Contracts.UI;
using System.Threading;
using log4net;
using Chainbot.Contracts.Log;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ActivitiesViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ILogService _logService;

        private IServiceLocator _serviceLocator;

        private IProjectManagerService _projectManagerService;

        private IActivitiesChildrenOrderService _activitiesChildrenOrderService;

        private IActivityFavoritesService _activityFavoritesService;
        private IActivityRecentService _activityRecentService;
        private IActivityMountService _activityMountService;
        private ISystemActivityIconService _systemActivityIconService;
        private IConstantConfigService _constantConfigService;
        private IDispatcherService _dispatcherService;

        private IActivitiesServiceProxy _activitiesServiceProxy;

        public ActivityItemDragHandler ActivityItemDragHandler { get; set; } = new ActivityItemDragHandler();

        private Dictionary<string, ActivityLeafItemViewModel> ActivityLeafItemTypeOfDict = new Dictionary<string, ActivityLeafItemViewModel>();
        private Dictionary<string, ActivityLeafItemViewModel> AssemblyQualifiedNameDict = new Dictionary<string, ActivityLeafItemViewModel>();

        public ActivityGroupItemViewModel FavoritesGroupItem { get; private set; }
        public ActivityGroupItemViewModel RecentGroupItem { get; private set; }
        public ActivityGroupItemViewModel TemplateGroupItem { get; private set; }
        public ActivityGroupItemViewModel ActivitiesGroupItem { get; private set; }


        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the ActivitiesViewModel class.
        /// </summary>
        public ActivitiesViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();
            _logService = _serviceLocator.ResolveType<ILogService>();

            _activityFavoritesService = _serviceLocator.ResolveType<IActivityFavoritesService>();
            _activityRecentService = _serviceLocator.ResolveType<IActivityRecentService>();
            _activityMountService = _serviceLocator.ResolveType<IActivityMountService>();
            _systemActivityIconService = _serviceLocator.ResolveType<ISystemActivityIconService>();
            _constantConfigService = _serviceLocator.ResolveType<IConstantConfigService>();
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();

            _activitiesChildrenOrderService = _serviceLocator.ResolveType<IActivitiesChildrenOrderService>();

            _projectManagerService.ProjectOpenEvent += _projectManagerService_ProjectOpenEvent;
            _projectManagerService.ProjectCloseEvent += _projectManagerService_ProjectCloseEvent;
        }

       
        private void _projectManagerService_ProjectOpenEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(()=> {
                IsProjectOpened = true;

                _activitiesServiceProxy = _projectManagerService.CurrentActivitiesServiceProxy;

                var activityGroup = ActivitiesItemsAppendActivity();
                var activities = _projectManagerService.Activities;
                try
                {
                    ActivitiesItemsAppendActivities(activityGroup.Children, activities);

                    _activitiesChildrenOrderService.Init();
                    ActivityGroupChildrenSort(activityGroup);
                }
                catch (Exception err)
                {
                    _logService.Error(err, _logger);
                }
                

                var favoritesList = _activityFavoritesService.Query();
                ActivitiesItemsAppendFavorites(favoritesList);

                var recentList = _activityRecentService.Query();
                ActivitiesItemsAppendRecent(recentList);

                //var templateList = new List<ActivityGroupOrLeafItem>();
                //ActivitiesItemsAppendTemplate(templateList);

                ActivitiesItemsSortByGroupType();
                FavoritesGroupItemSort();
            });
            
        }

        private void ActivityGroupChildrenSort(ActivityGroupItemViewModel activityGroup)
        {
            var children = activityGroup.Children;

            children.Sort((x, y) => _activitiesChildrenOrderService.GetOrder(x.Name,x.InitOrder).CompareTo(_activitiesChildrenOrderService.GetOrder(y.Name, y.InitOrder)));

            foreach(var child in children)
            {
                if(child is ActivityGroupItemViewModel)
                {
                    ActivityGroupChildrenSort(child as ActivityGroupItemViewModel);
                }
            }
        }


        private void FavoritesGroupItemSort()
        {
            FavoritesGroupItem.Children.Sort((x, y) => x.Name.CompareTo(y.Name));
        }

        private void ActivitiesItemsSortByGroupType()
        {
            ActivityItems.Sort((x, y) => x.GroupType.CompareTo(y.GroupType));
        }


        public ImageSource GetIconByAssemblyQualifiedName(string assemblyQualifiedName)
        {
            if(AssemblyQualifiedNameDict.ContainsKey(assemblyQualifiedName))
            {
                var item = AssemblyQualifiedNameDict[assemblyQualifiedName];

                return item.IconSource;
            }

            return null;
        }

        private void _projectManagerService_ProjectCloseEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(()=> {
                IsProjectOpened = false;
                IsSearchResultEmpty = false;
                ActivityItems.Clear();
                SearchText = "";
                ActivityLeafItemTypeOfDict.Clear();
                AssemblyQualifiedNameDict.Clear();
            });
        }


        private void GroupChildrenRemove(ActivityGroupItemViewModel group,string typeOf)
        {
            foreach (var item in group.Children)
            {
                var leafItemVM = item as ActivityLeafItemViewModel;
                if (leafItemVM.TypeOf == typeOf)
                {
                    group.Children.Remove(item);
                    break;
                }
            }
        }

        private void GroupChildrenLimit(ActivityGroupItemViewModel group, int maxCount)
        {
            while (group.Children.Count >= maxCount)
            {
                group.Children.Remove(group.Children.LastOrDefault());
            }
        }

        private void GroupChildrenAddFront(ActivityGroupItemViewModel group, string typeOf)
        {
            if (ActivityLeafItemTypeOfDict.ContainsKey(typeOf))
            {
                group.Children.Insert(0, ActivityLeafItemTypeOfDict[typeOf]);
            }
        }

        public void AddToRecent(string typeOf)
        {
            _activityRecentService.Add(typeOf);

            GroupChildrenRemove(RecentGroupItem, typeOf);

            GroupChildrenLimit(RecentGroupItem, _constantConfigService.ActivitiesRecentGroupMaxRecordCount);
            GroupChildrenAddFront(RecentGroupItem, typeOf);           
        }

        public void RemoveFromFavorites(ActivityLeafItemViewModel activityLeafItemViewModel)
        {
            activityLeafItemViewModel.IsInFavorites = false;
            _activityFavoritesService.Remove(activityLeafItemViewModel.TypeOf);

            GroupChildrenRemove(FavoritesGroupItem, activityLeafItemViewModel.TypeOf);
        }

        public void AddToFavorites(ActivityLeafItemViewModel activityLeafItemViewModel)
        {
            activityLeafItemViewModel.IsInFavorites = true;
            _activityFavoritesService.Add(activityLeafItemViewModel.TypeOf);

            GroupChildrenRemove(FavoritesGroupItem, activityLeafItemViewModel.TypeOf);
            GroupChildrenAddFront(FavoritesGroupItem, activityLeafItemViewModel.TypeOf);

            FavoritesGroupItemSort();
        }

        private void ActivitiesItemsAppendFavorites(List<ActivityGroupOrLeafItem> favoritesList)
        {
            var groupItem = _serviceLocator.ResolveType<ActivityGroupItemViewModel>();
            ActivityItems.Add(groupItem);
            FavoritesGroupItem = groupItem;

            groupItem.Name = Chainbot.Resources.Properties.Resources.Activities_FacoritesName;
            groupItem.ToolTip = Chainbot.Resources.Properties.Resources.Activities_FacoritesToolTip;
            groupItem.GroupType = ActivityBaseItemViewModel.enGroupType.Favorites;
            groupItem.Icon = new BitmapImage(new Uri("pack://application:,,,/Chainbot.Resources;Component/Image/Activities/favorites.png"));
            groupItem.IsExpanded = false;

            FavoritesOrRecentGroupAppendItems(groupItem, favoritesList,true);
        }


        private void ActivitiesItemsAppendRecent(List<ActivityGroupOrLeafItem> recentList)
        {
            var groupItem = _serviceLocator.ResolveType<ActivityGroupItemViewModel>();
            ActivityItems.Add(groupItem);
            RecentGroupItem = groupItem;

            groupItem.Name = Chainbot.Resources.Properties.Resources.Activities_RecentName;
            groupItem.ToolTip = Chainbot.Resources.Properties.Resources.Activities_RecentToolTip;
            groupItem.GroupType = ActivityBaseItemViewModel.enGroupType.Recent;
            groupItem.Icon = new BitmapImage(new Uri("pack://application:,,,/Chainbot.Resources;Component/Image/Activities/recent.png"));
            groupItem.IsExpanded = false;

            FavoritesOrRecentGroupAppendItems(groupItem, recentList,false);
        }


        private ActivityGroupItemViewModel ActivitiesItemsAppendActivity()
        {
            var groupItem = _serviceLocator.ResolveType<ActivityGroupItemViewModel>();
            ActivityItems.Add(groupItem);
            ActivitiesGroupItem = groupItem;

            groupItem.Name = Chainbot.Resources.Properties.Resources.Activities_ListName;
            groupItem.GroupType = ActivityBaseItemViewModel.enGroupType.Activities;
            groupItem.Icon = Application.Current.TryFindResource("ActivityListDrawingImage") as DrawingImage;
            groupItem.IsExpanded = true;

            return groupItem;
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

        private void FavoritesOrRecentGroupAppendItems(ActivityGroupItemViewModel groupItem, List<ActivityGroupOrLeafItem> items,bool isInFavorites)
        {
            foreach (var favorOrRecentItem in items)
            {
                var leafItem = favorOrRecentItem as ActivityLeafItem;

                if (ActivityLeafItemTypeOfDict.ContainsKey(leafItem.TypeOf))
                {
                    var leafItemVM = ActivityLeafItemTypeOfDict[leafItem.TypeOf];
                    if(isInFavorites)
                    {
                        leafItemVM.IsInFavorites = true;
                    }
                    
                    groupItem.Children.Add(leafItemVM);
                }
            }
        }




        private void ActivitiesItemsAppendActivities(ObservableCollection<ActivityBaseItemViewModel> vmList,List<ActivityGroupOrLeafItem> list)
        {
            int initOrder = 0;
            foreach(var groupOrLeafItem in list)
            {
                initOrder++;
                if (groupOrLeafItem is ActivityGroupItem)
                {
                    var groupItem = groupOrLeafItem as ActivityGroupItem;

                    var groupItemVM = _serviceLocator.ResolveType<ActivityGroupItemViewModel>();
                    groupItemVM.InitOrder = initOrder;
                    vmList.Add(groupItemVM);

                    groupItemVM.Name = groupItem.Name;

                    ActivitiesItemsAppendActivities(groupItemVM.Children, groupItem.Children);
                }
                else
                {
                    var leafItem = groupOrLeafItem as ActivityLeafItem;

                    var leafItemVM = _serviceLocator.ResolveType<ActivityLeafItemViewModel>();
                    leafItemVM.InitOrder = initOrder;
                    vmList.Add(leafItemVM);

                    leafItemVM.Name = leafItem.Name;
                    leafItemVM.TypeOf = leafItem.TypeOf;
                    leafItemVM.AssemblyQualifiedName = _activitiesServiceProxy.GetAssemblyQualifiedName(leafItem.TypeOf);
                    leafItemVM.ToolTip = leafItem.ToolTip;


                    ActivityLeafItemTypeOfDict[leafItemVM.TypeOf] = leafItemVM;
                    AssemblyQualifiedNameDict[leafItemVM.AssemblyQualifiedName] = leafItemVM;


                    if (_projectManagerService.ActivitiesTypeOfDict.ContainsKey(leafItemVM.TypeOf))
                    {
                        var item = _projectManagerService.ActivitiesTypeOfDict[leafItemVM.TypeOf] as ActivityLeafItem;
                        
                        if (!string.IsNullOrEmpty(item.Icon))
                        {
                            string[] sArray = item.TypeOf.Split(',');
                            if (sArray.Length > 1)
                            {
                                leafItemVM.IconSource = _activitiesServiceProxy.GetIcon(sArray[1], item.Icon);
                            }
                        }
                        else
                        {
                            leafItemVM.IconSource = _systemActivityIconService.GetIcon(item.TypeOf);
                        }
                    }


                }
            }
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
        /// The <see cref="ActivityItems" /> property's name.
        /// </summary>
        public const string ActivityItemsPropertyName = "ActivityItems";

        private ObservableCollection<ActivityBaseItemViewModel> _activityItemsProperty = new ObservableCollection<ActivityBaseItemViewModel>();

        /// <summary>
        /// Sets and gets the ActivityItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ActivityBaseItemViewModel> ActivityItems
        {
            get
            {
                return _activityItemsProperty;
            }

            set
            {
                if (_activityItemsProperty == value)
                {
                    return;
                }

                _activityItemsProperty = value;
                RaisePropertyChanged(ActivityItemsPropertyName);
            }
        }






        private void ActivityTreeItemSetAllIsSearching(ActivityBaseItemViewModel item, bool IsSearching)
        {
            item.IsSearching = IsSearching;
            foreach (var child in item.Children)
            {
                ActivityTreeItemSetAllIsSearching(child, IsSearching);
            }
        }

        private void ActivityTreeItemSetAllIsMatch(ActivityBaseItemViewModel item, bool IsMatch)
        {
            item.IsMatch = IsMatch;
            foreach (var child in item.Children)
            {
                ActivityTreeItemSetAllIsMatch(child, IsMatch);
            }
        }


        private void ActivityTreeItemSetAllSearchText(ActivityBaseItemViewModel item, string SearchText)
        {
            item.SearchText = SearchText;
            foreach (var child in item.Children)
            {
                ActivityTreeItemSetAllSearchText(child, SearchText);
            }
        }

        private void ActivityTreeItemSetAllIsExpanded(ActivityBaseItemViewModel item, bool IsExpanded)
        {
            item.IsExpanded = IsExpanded;
            foreach (var child in item.Children)
            {
                ActivityTreeItemSetAllIsExpanded(child, IsExpanded);
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
                        foreach (var item in ActivityItems)
                        {
                            ActivityTreeItemSetAllIsExpanded(item, true);
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
                        foreach (var item in ActivityItems)
                        {
                            ActivityTreeItemSetAllIsExpanded(item, false);
                        }
                    }));
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
            try
            {
                _tokenSource.Cancel();

                _tokenSource = new CancellationTokenSource();
                CancellationToken ct = _tokenSource.Token;

                await Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    string searchContent = SearchText ?? "";

                    searchContent = searchContent.Trim();

                    if (string.IsNullOrEmpty(searchContent))
                    {
                        foreach (var item in ActivityItems)
                        {
                            ct.ThrowIfCancellationRequested();

                            ActivityTreeItemSetAllIsSearching(item, false);
                        }

                        foreach (var item in ActivityItems)
                        {
                            ct.ThrowIfCancellationRequested();

                            ActivityTreeItemSetAllSearchText(item, "");
                        }

                        IsSearchResultEmpty = false;
                    }
                    else
                    {
                        foreach (var item in ActivityItems)
                        {
                            ct.ThrowIfCancellationRequested();

                            ActivityTreeItemSetAllIsSearching(item, true);
                        }

                        foreach (var item in ActivityItems)
                        {
                            ct.ThrowIfCancellationRequested();

                            ActivityTreeItemSetAllIsMatch(item, false);
                        }


                        foreach (var item in ActivityItems)
                        {
                            ct.ThrowIfCancellationRequested();

                            item.ApplyCriteria(searchContent, new Stack<ActivityBaseItemViewModel>());
                        }

                        IsSearchResultEmpty = true;
                        foreach (var item in ActivityItems)
                        {
                            ct.ThrowIfCancellationRequested();

                            if (item.IsMatch)
                            {
                                IsSearchResultEmpty = false;
                                break;
                            }
                        }
                    }
                }, ct);
            }
            catch (OperationCanceledException e)
            {

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

                StartSearch();

            }
        }

        

        private async void StartSearch()
        {
            await doSearch();
        }
    }
}