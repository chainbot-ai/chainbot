using GalaSoft.MvvmLight;
using System.IO;
using Plugins.Shared.Library;
using System.Xml;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using log4net;
using GalaSoft.MvvmLight.Messaging;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.App;
using ChainbotStudio.DragDrop;
using Chainbot.Contracts.Activities;
using Chainbot.Contracts.Classes;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SnippetsViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IServiceLocator _serviceLocator;

        private IDispatcherService _dispatcherService;

        private IPathConfigService _pathConfigService;
        private IConstantConfigService _constantConfigService;
        private IDialogService _dialogService;

        public SnippetItemDragHandler SnippetItemDragHandler { get; set; } = new SnippetItemDragHandler();

        /// <summary>
        /// Initializes a new instance of the SnippetsViewModel class.
        /// </summary>
        public SnippetsViewModel(IServiceLocator serviceLocator, IPathConfigService pathConfigService, IConstantConfigService constantConfigService
            , IDialogService dialogService, IDispatcherService dispatcherService)
        {
            _serviceLocator = serviceLocator;
            _pathConfigService = pathConfigService;
            _constantConfigService = constantConfigService;
            _dialogService = dialogService;
            _dispatcherService = dispatcherService;

            _dispatcherService.InvokeAsync(()=> {
                initSnippets();
            });
            
            Messenger.Default.Register<SnippetItemViewModel>(this, "RemoveSnippet", RemoveSnippet);
        }


        private void RemoveSnippet(SnippetItemViewModel obj)
        {
            initSnippets();
        }

        private void InitGroup(DirectoryInfo di, SnippetItemViewModel parent = null)
        {
            DirectoryInfo[] dis = di.GetDirectories();
            for (int j = 0; j < dis.Length; j++)
            {
                var item = _serviceLocator.ResolveType<SnippetItemViewModel>();
                item.ActivityFactoryAssemblyQualifiedName = ConstantAssemblyQualifiedName.InsertSnippetItemFactory;
                item.Path = dis[j].FullName;
                item.Name = dis[j].Name;

                if (parent != null)
                {
                    parent.Children.Add(item);
                }
                else
                {
                    item.IsExpanded = true;
                    SnippetsItems.Add(item);
                }

                InitGroup(dis[j], item);
            }

            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                var item = _serviceLocator.ResolveType<SnippetItemViewModel>();
                item.ActivityFactoryAssemblyQualifiedName = ConstantAssemblyQualifiedName.InsertSnippetItemFactory;
                item.IsSnippet = true;
                item.Path = fis[i].FullName;
                item.Name = fis[i].Name;

                if (fis[i].Extension.ToLower() == _constantConfigService.XamlFileExtension)
                {
                    if (parent != null)
                    {
                        parent.Children.Add(item);
                    }
                    else
                    {
                        SnippetsItems.Add(item);
                    }
                }

            }

        }


        private void initSnippets()
        {
            SnippetsItems.Clear();
            DirectoryInfo di = new DirectoryInfo(Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory,"Snippets"));
            InitGroup(di);

            InitUserSnippets();
        }


        private void InitUserSnippets()
        {
            XmlDocument doc = new XmlDocument();
            var path = Path.Combine(_pathConfigService.AppDataDir, @"Config\CodeSnippets.xml");
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            var directoryNodes = rootNode.SelectNodes("Directory");
            foreach (XmlNode dir in directoryNodes)
            {
                var dirPath = (dir as XmlElement).GetAttribute("Path");
                var id = (dir as XmlElement).GetAttribute("Id");

                var userItem = _serviceLocator.ResolveType<SnippetItemViewModel>();
                userItem.ActivityFactoryAssemblyQualifiedName = ConstantAssemblyQualifiedName.InsertSnippetItemFactory;
                userItem.IsUserAdd = true;
                userItem.Id = id;
                userItem.Path = dirPath;
                userItem.Name = Path.GetFileName(dirPath);
                userItem.IsExpanded = true;

                SnippetsItems.Add(userItem);

                DirectoryInfo di = new DirectoryInfo(dirPath);
                InitGroup(di, userItem);
            }
        }


        /// <summary>
        /// The <see cref="SnippetsItems" /> property's name.
        /// </summary>
        public const string SnippetsItemsPropertyName = "SnippetsItems";

        private ObservableCollection<SnippetItemViewModel> _snippetsItemsProperty = new ObservableCollection<SnippetItemViewModel>();

        public ObservableCollection<SnippetItemViewModel> SnippetsItems
        {
            get
            {
                return _snippetsItemsProperty;
            }

            set
            {
                if (_snippetsItemsProperty == value)
                {
                    return;
                }

                _snippetsItemsProperty = value;
                RaisePropertyChanged(SnippetsItemsPropertyName);
            }
        }

        private void SnippetTreeItemSetAllIsExpanded(SnippetItemViewModel item, bool IsExpanded)
        {
            item.IsExpanded = IsExpanded;
            foreach (var child in item.Children)
            {
                SnippetTreeItemSetAllIsExpanded(child, IsExpanded);
            }
        }

        private void SnippetTreeItemSetAllIsSearching(SnippetItemViewModel item, bool IsSearching)
        {
            item.IsSearching = IsSearching;
            foreach (var child in item.Children)
            {
                SnippetTreeItemSetAllIsSearching(child, IsSearching);
            }
        }

        private void SnippetTreeItemSetAllIsMatch(SnippetItemViewModel item, bool IsMatch)
        {
            item.IsMatch = IsMatch;
            foreach (var child in item.Children)
            {
                SnippetTreeItemSetAllIsMatch(child, IsMatch);
            }
        }

        private void SnippetTreeItemSetAllSearchText(SnippetItemViewModel item, string SearchText)
        {
            item.SearchText = SearchText;
            foreach (var child in item.Children)
            {
                SnippetTreeItemSetAllSearchText(child, SearchText);
            }
        }


        private RelayCommand _expandAllCommand;

        public RelayCommand ExpandAllCommand
        {
            get
            {
                return _expandAllCommand
                    ?? (_expandAllCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var item in SnippetsItems)
                        {
                            SnippetTreeItemSetAllIsExpanded(item, true);
                        }
                    }));
            }
        }


        private RelayCommand _collapseAllCommand;

        public RelayCommand CollapseAllCommand
        {
            get
            {
                return _collapseAllCommand
                    ?? (_collapseAllCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var item in SnippetsItems)
                        {
                            SnippetTreeItemSetAllIsExpanded(item, false);
                        }
                    }));
            }
        }


        private RelayCommand _refreshCommand;

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                    () =>
                    {
                        initSnippets();
                    }));
            }
        }


        private RelayCommand _addFolderCommand;

        public RelayCommand AddFolderCommand
        {
            get
            {
                return _addFolderCommand
                    ?? (_addFolderCommand = new RelayCommand(
                    () =>
                    {
                        string dst_dir = "";
                        if (_dialogService.ShowSelectDirDialog(Chainbot.Resources.Properties.Resources.SnippetsView_SelectDirectory, ref dst_dir))
                        {
                            XmlDocument doc = new XmlDocument();
                            var path = Path.Combine(_pathConfigService.AppDataDir, @"Config\CodeSnippets.xml");
                            doc.Load(path);
                            var rootNode = doc.DocumentElement;

                            XmlElement dirElement = doc.CreateElement("Directory");
                            dirElement.SetAttribute("Id", System.Guid.NewGuid().ToString());
                            dirElement.SetAttribute("Path", dst_dir);

                            rootNode.AppendChild(dirElement);

                            doc.Save(path);

                            initSnippets();
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

        private void doSearch()
        {
            var searchContent = SearchText ?? "";
            searchContent = searchContent.Trim();

            if (string.IsNullOrEmpty(searchContent))
            {
                foreach (var item in SnippetsItems)
                {
                    SnippetTreeItemSetAllIsSearching(item, false);
                }

                foreach (var item in SnippetsItems)
                {
                    SnippetTreeItemSetAllSearchText(item, "");
                }

                IsSearchResultEmpty = false;
            }
            else
            {
                foreach (var item in SnippetsItems)
                {
                    SnippetTreeItemSetAllIsSearching(item, true);
                }

                foreach (var item in SnippetsItems)
                {
                    SnippetTreeItemSetAllIsMatch(item, false);
                }


                foreach (var item in SnippetsItems)
                {
                    item.ApplyCriteria(searchContent, new Stack<SnippetItemViewModel>());
                }

                IsSearchResultEmpty = true;
                foreach (var item in SnippetsItems)
                {
                    if (item.IsMatch)
                    {
                        IsSearchResultEmpty = false;
                        break;
                    }
                }

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

                doSearch();

            }
        }




    }
}