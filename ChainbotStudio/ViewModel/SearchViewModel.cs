using Chainbot.Contracts.App;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using ChainbotStudio.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SearchViewModel : ViewModelBase
    {
        public SearchViewWindow _view { get; set; }

        private ListBox _listBox;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private IServiceLocator _serviceLocator;
        private DocksViewModel _docksViewModel;
        private ActivitiesViewModel _activitiesViewModel;
        private ICommonService _commonService;
        private IMessageBoxService _messageBoxService;
        private IConstantConfigService _constantConfigService;
        private IDispatcherService _dispatcherService;

        /// <summary>
        /// Initializes a new instance of the SearchViewModel class.
        /// </summary>
        public SearchViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _docksViewModel = _serviceLocator.ResolveType<DocksViewModel>();
            _activitiesViewModel = _serviceLocator.ResolveType<ActivitiesViewModel>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
            _constantConfigService = _serviceLocator.ResolveType<IConstantConfigService>();
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();
        }

        /// <summary>
        /// The <see cref="SearchText" /> property's name.
        /// </summary>
        public const string SearchTextPropertyName = "SearchText";

        private string _searchTextProperty = "";

        /// <summary>
        /// Sets and gets the SearchText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
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

        /// <summary>
        /// The <see cref="IsCurrentFile" /> property's name.
        /// </summary>
        public const string IsCurrentFilePropertyName = "IsCurrentFile";

        private bool _isCurrentFileProperty = true;

        /// <summary>
        /// Sets and gets the IsCurrentFile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCurrentFile
        {
            get
            {
                return _isCurrentFileProperty;
            }

            set
            {
                if (_isCurrentFileProperty == value)
                {
                    return;
                }

                _isCurrentFileProperty = value;
                RaisePropertyChanged(IsCurrentFilePropertyName);

                StartSearch();
            }
        }

        /// <summary>
        /// The <see cref="IsSearchActivities" /> property's name.
        /// </summary>
        public const string IsSearchActivitiesPropertyName = "IsSearchActivities";

        private bool _isSearchActivitiesProperty = true;

        /// <summary>
        /// Sets and gets the IsSearchActivities property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSearchActivities
        {
            get
            {
                return _isSearchActivitiesProperty;
            }

            set
            {
                if (_isSearchActivitiesProperty == value)
                {
                    return;
                }

                if (!value && !IsSearchVariables && !IsSearchArguments)
                {
                    return;
                }

                _isSearchActivitiesProperty = value;
                RaisePropertyChanged(IsSearchActivitiesPropertyName);

                ShowItemsFilter(value, (item) => { return item.IsActivity; });
            }
        }

        /// <summary>
        /// The <see cref="IsSearchVariables" /> property's name.
        /// </summary>
        public const string IsSearchVariablesPropertyName = "IsSearchVariables";

        private bool _isSearchVariablesProperty = true;

        /// <summary>
        /// Sets and gets the IsSearchVariables property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSearchVariables
        {
            get
            {
                return _isSearchVariablesProperty;
            }

            set
            {
                if (_isSearchVariablesProperty == value)
                {
                    return;
                }

                if (!IsSearchActivities && !value && !IsSearchArguments)
                {
                    return;
                }

                _isSearchVariablesProperty = value;
                RaisePropertyChanged(IsSearchVariablesPropertyName);

                ShowItemsFilter(value, (item) => { return (item.IsVariable || item.IsUsedVariable); });
            }
        }

        /// <summary>
        /// The <see cref="IsSearchArguments" /> property's name.
        /// </summary>
        public const string IsSearchArgumentsPropertyName = "IsSearchArguments";

        private bool _isSearchArgumentsProperty = true;

        /// <summary>
        /// Sets and gets the IsSearchArguments property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSearchArguments
        {
            get
            {
                return _isSearchArgumentsProperty;
            }

            set
            {
                if (_isSearchArgumentsProperty == value)
                {
                    return;
                }

                if (!IsSearchActivities && !IsSearchVariables && !value)
                {
                    return;
                }

                _isSearchArgumentsProperty = value;
                RaisePropertyChanged(IsSearchArgumentsPropertyName);

                ShowItemsFilter(value, (item) => { return (item.IsArgument || item.IsUsedArgument); });
            }
        }

        /// <summary>
        /// The <see cref="SearchItems" /> property's name.
        /// </summary>
        public const string SearchItemsPropertyName = "SearchItems";

        private ObservableCollection<SearchItemViewModel> _searchItemsProperty = new ObservableCollection<SearchItemViewModel>();

        /// <summary>
        /// Sets and gets the SearchItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SearchItemViewModel> SearchItems
        {
            get
            {
                return _searchItemsProperty;
            }

            set
            {
                if (_searchItemsProperty == value)
                {
                    return;
                }

                _searchItemsProperty = value;
                RaisePropertyChanged(SearchItemsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsNotInPut" /> property's name.
        /// </summary>
        public const string IsNotInPutPropertyName = "IsNotInPut";

        private bool _isNotInPutProperty = true;

        /// <summary>
        /// Sets and gets the IsNotInPut property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNotInPut
        {
            get
            {
                return _isNotInPutProperty;
            }

            set
            {
                if (_isNotInPutProperty == value)
                {
                    return;
                }

                _isNotInPutProperty = value;
                RaisePropertyChanged(IsNotInPutPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsSearching" /> property's name.
        /// </summary>
        public const string IsSearchingPropertyName = "IsSearching";

        private bool _isSearchingProperty = false;

        /// <summary>
        /// Sets and gets the IsSearching property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
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
        /// The <see cref="IsSearchResultEmpty" /> property's name.
        /// </summary>
        public const string IsSearchResultEmptyPropertyName = "IsSearchResultEmpty";

        private bool _isSearchResultEmptyProperty = false;

        /// <summary>
        /// Sets and gets the IsSearchResultEmpty property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
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

        public static void HideBoundingBox(object root)
        {
            Control control = root as Control;
            if (control != null)
            {
                control.FocusVisualStyle = null;
            }

            if (root is DependencyObject)
            {
                foreach (object child in LogicalTreeHelper.GetChildren((DependencyObject)root))
                {
                    HideBoundingBox(child);
                }
            }
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
                        _view = (SearchViewWindow)p.Source;
                        HideBoundingBox(_view);
                    }));
            }
        }

        private RelayCommand _searchCommand;

        /// <summary>
        /// Gets the SearchCommand.
        /// </summary>
        public RelayCommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                    () =>
                    {
                        StartSearch();
                    }));
            }
        }

        private RelayCommand _deactivatedCommand;

        /// <summary>
        /// Gets the DeactivatedCommand.
        /// </summary>
        public RelayCommand DeactivatedCommand
        {
            get
            {
                return _deactivatedCommand
                    ?? (_deactivatedCommand = new RelayCommand(
                    () =>
                    {
                        _view.Hide();
                    }));
            }
        }

        private RelayCommand<RoutedEventArgs> _listBoxLoadedCommand;

  
        public RelayCommand<RoutedEventArgs> ListBoxLoadedCommand
        {
            get
            {
                return _listBoxLoadedCommand
                    ?? (_listBoxLoadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        _listBox = (ListBox)p.Source;
                    }));
            }
        }


        private static string wildCardToRegular(string value)
        {
            return "^" + ".*" + Regex.Escape(value).Replace("\\ ", ".*").Replace("\\*", ".*").Replace("\\?", ".") + ".*";
        }

        private bool IsCriteriaMatched(string content, string criteria)
        {
            return Regex.IsMatch(content, wildCardToRegular(criteria), RegexOptions.IgnoreCase);
        }

        private ImageSource GetIconSource(string assemblyQualifiedName)
        {
            var iconSource = _activitiesViewModel.GetIconByAssemblyQualifiedName(assemblyQualifiedName);
            if (iconSource == null)
            {
                iconSource = _commonService.ToImageSource(@"pack://application:,,,/Chainbot.Resources;Component/Image/Search/activity.png");
            }
            return iconSource;
        }

        private void GetAllFlowChartFilePath(string path, ref List<string> cmmlFilePathList)
        {
            DirectoryInfo fdir = new DirectoryInfo(path);

            if (fdir.Name == _constantConfigService.ProjectLocalDirectoryName ||
                fdir.Name == _constantConfigService.ScreenshotsPath)
            {
                return;
            }

            FileInfo[] file = fdir.GetFiles();

            string[] dir = Directory.GetDirectories(path);

            if (file.Length != 0 || dir.Length != 0)
            {
                foreach (FileInfo item in file)
                {
                    if (item.Extension.ToLower() == _constantConfigService.XamlFileExtension)
                    {
                        cmmlFilePathList.Add(item.FullName);
                    }
                }

                foreach (string subdir in dir)
                {
                    GetAllFlowChartFilePath(subdir, ref cmmlFilePathList);
                }
            }
        }

        private void GetSearchRestlt(DesignerDocumentViewModel designDoc, string searchContent, ref JArray activitiesJArray, ref JArray variablesJArray, ref JArray argumentsJArray)
        {
            if (IsSearchActivities)
            {
                var allActivities = designDoc.GetAllActivities();
                foreach (JObject jObj in allActivities)
                {
                    if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                    {
                        activitiesJArray.Add(jObj);
                    }
                }
            }

            if (IsSearchVariables)
            {
                var allVariables = designDoc.GetAllVariables();
                foreach (JObject jObj in allVariables)
                {
                    if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                    {
                        variablesJArray.Add(jObj);
                    }
                }
            }

            if (IsSearchArguments)
            {
                var allArguments = designDoc.GetAllArguments();
                foreach (JObject jObj in allArguments)
                {
                    if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                    {
                        argumentsJArray.Add(jObj);
                    }
                }
            }
        }

        private void GetSearchRestlt(IWorkflowDesignerJumpServiceProxy workflowDesignerJumpServiceProxy, string searchContent, ref JArray activitiesJArray, ref JArray variablesJArray, ref JArray argumentsJArray)
        {
            if (IsSearchActivities)
            {
                var allActivities = workflowDesignerJumpServiceProxy.GetAllActivities();
                foreach (JObject jObj in allActivities)
                {
                    if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                    {
                        activitiesJArray.Add(jObj);
                    }
                }
            }

            if (IsSearchVariables)
            {
                var allVariables = workflowDesignerJumpServiceProxy.GetAllVariables();
                foreach (JObject jObj in allVariables)
                {
                    if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                    {
                        variablesJArray.Add(jObj);
                    }
                }
            }

            if (IsSearchArguments)
            {
                var allArguments = workflowDesignerJumpServiceProxy.GetAllArguments();
                foreach (JObject jObj in allArguments)
                {
                    if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                    {
                        argumentsJArray.Add(jObj);
                    }
                }
            }
        }

        public async void StartSearch()
        {
            await AsyncSearch();
        }

        private async Task AsyncSearch()
        {
            try
            {
                _tokenSource.Cancel();

                SearchItems.Clear();
                IsSearchResultEmpty = false;

                string searchContent = SearchText ?? "";
                searchContent = searchContent.Trim();

                IsNotInPut = false;
                if (searchContent == "")
                {
                    IsNotInPut = true;
                    return;
                }

                _tokenSource = new CancellationTokenSource();
                CancellationToken ct = _tokenSource.Token;

                JArray activitiesJArray = new JArray();
                JArray variablesJArray = new JArray();
                JArray argumentsJArray = new JArray();

                IsSearching = true;

                await Task.Run(() =>
                {
                    try
                    {
                        ct.ThrowIfCancellationRequested();

                        DesignerDocumentViewModel designDoc;
                        foreach (var doc in _docksViewModel.Documents)
                        {
                            ct.ThrowIfCancellationRequested();

                            if (doc is DesignerDocumentViewModel)
                            {
                                designDoc = doc as DesignerDocumentViewModel;

                                if (IsCurrentFile && !designDoc.IsSelected)
                                {
                                    continue;
                                }

                                if (IsSearchActivities)
                                {
                                    var allActivities = designDoc.GetAllActivities();
                                    foreach (JObject jObj in allActivities)
                                    {
                                        ct.ThrowIfCancellationRequested();

                                        if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                                        {
                                            activitiesJArray.Add(jObj);
                                        }
                                    }
                                }

                                if (IsSearchVariables)
                                {
                                    var allVariables = designDoc.GetAllVariables();
                                    if (allVariables.Count > 0)
                                    {
                                        foreach (JObject jObj in allVariables)
                                        {
                                            ct.ThrowIfCancellationRequested();

                                            if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                                            {
                                                variablesJArray.Add(jObj);
                                            }
                                        }

                                        var usedVariables = designDoc.GetUsedVariables();
                                        foreach (JObject jObj in usedVariables)
                                        {
                                            ct.ThrowIfCancellationRequested();

                                            if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                                            {
                                                variablesJArray.Add(jObj);
                                            }
                                        }
                                    }
                                }

                                if (IsSearchArguments)
                                {
                                    var allArguments = designDoc.GetAllArguments();
                                    if (allArguments.Count > 0)
                                    {
                                        foreach (JObject jObj in allArguments)
                                        {
                                            ct.ThrowIfCancellationRequested();

                                            if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                                            {
                                                argumentsJArray.Add(jObj);
                                            }
                                        }

                                        var usedArguments = designDoc.GetUsedArguments();
                                        foreach (JObject jObj in usedArguments)
                                        {
                                            ct.ThrowIfCancellationRequested();

                                            if (IsCriteriaMatched(jObj.Value<string>("DisplayName"), searchContent))
                                            {
                                                argumentsJArray.Add(jObj);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        IsSearching = false;
                    }
                    
                }, ct);

                IsSearching = false;

                SearchItemViewModel item;
                foreach (JObject jObj in activitiesJArray)
                {
                    item = _serviceLocator.ResolveType<SearchItemViewModel>();
                    item.IsActivity = true;
                    item.IdRef = jObj.Value<string>("IdRef");
                    item.IconSource = GetIconSource(jObj.Value<string>("AssemblyQualifiedName"));
                    item.Path = jObj.Value<string>("Path");
                    item.SearchText = searchContent;
                    item.SearchResult = jObj.Value<string>("DisplayName");
                    item.SearchResultDetail = jObj.Value<string>("Location");
                    SearchItems.Add(item);
                }

                foreach (JObject jObj in variablesJArray)
                {
                    item = _serviceLocator.ResolveType<SearchItemViewModel>();
                    item.IdRef = jObj.Value<string>("IdRef");
                    item.Path = jObj.Value<string>("Path");
                    item.SearchText = searchContent;
                    item.SearchResult = jObj.Value<string>("DisplayName");

                    if (!string.IsNullOrEmpty(jObj.Value<string>("ScopeName")))
                    {
                        item.IsVariable = true;
                        item.SearchResultDetail = jObj.Value<string>("Location") + " > " + jObj.Value<string>("ScopeName");
                    }
                    else
                    {
                        item.IsUsedVariable = true;
                        item.SearchResultDetail = jObj.Value<string>("Location");
                    }
                   
                    SearchItems.Add(item);
                }

                foreach (JObject jObj in argumentsJArray)
                {
                    item = _serviceLocator.ResolveType<SearchItemViewModel>();
                    item.Path = jObj.Value<string>("Path");
                    item.SearchText = searchContent;
                    item.SearchResult = jObj.Value<string>("DisplayName");
                    item.SearchResultDetail = jObj.Value<string>("Location");

                    if (!string.IsNullOrEmpty(jObj.Value<string>("IdRef")))
                    {
                        item.IdRef = jObj.Value<string>("IdRef");
                        item.IsUsedArgument = true;
                    }
                    else
                    {
                        item.IsArgument = true;
                    }

                    SearchItems.Add(item);
                }

                if (SearchItems.Count == 0)
                {
                    IsSearchResultEmpty = true;
                }
            }
            catch
            {
            }
        }

        private void ShowItemsFilter(bool isVisible, Func<SearchItemViewModel, bool> compare)
        {
            IsSearchResultEmpty = true;
            foreach (var item in SearchItems)
            {
                if (compare(item))
                {
                    item.IsVisible = isVisible;
                }

                if (item.IsVisible)
                {
                    IsSearchResultEmpty = false;
                }
            }
        }
    }
}