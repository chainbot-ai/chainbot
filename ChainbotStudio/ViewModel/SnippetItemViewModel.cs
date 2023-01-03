using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Cores.Classes;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SnippetItemViewModel : ViewModelBase
    {
        public string ActivityFactoryAssemblyQualifiedName { get; set; }

        private IContextMenuService _contextMenuService;
        private DocksViewModel _docksViewModel;
        private ICommonService _commonService;
        private IPathConfigService _pathConfigService;
        private IMessageBoxService _messageBoxService;

        /// <summary>
        /// Initializes a new instance of the SnippetItemViewModel class.
        /// </summary>
        public SnippetItemViewModel(IContextMenuService contextMenuService, DocksViewModel docksViewModel, ICommonService commonService
            , IPathConfigService pathConfigService, IMessageBoxService messageBoxService)
        {
            _contextMenuService = contextMenuService;
            _docksViewModel = docksViewModel;
            _commonService = commonService;
            _pathConfigService = pathConfigService;
            _messageBoxService = messageBoxService;
        }




        /// <summary>
        /// The <see cref="Id" /> property's name.
        /// </summary>
        public const string IdPropertyName = "Id";

        private string _idProperty = System.Guid.NewGuid().ToString();

        public string Id
        {
            get
            {
                return _idProperty;
            }

            set
            {
                if (_idProperty == value)
                {
                    return;
                }

                _idProperty = value;
                RaisePropertyChanged(IdPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsExpanded" /> property's name.
        /// </summary>
        public const string IsExpandedPropertyName = "IsExpanded";

        private bool _isExpandedProperty = false;

        public bool IsExpanded
        {
            get
            {
                return _isExpandedProperty;
            }

            set
            {
                if (_isExpandedProperty == value)
                {
                    return;
                }

                _isExpandedProperty = value;
                RaisePropertyChanged(IsExpandedPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="Children" /> property's name.
        /// </summary>
        public const string ChildrenPropertyName = "Children";

        private ObservableCollection<SnippetItemViewModel> _childrenProperty = new ObservableCollection<SnippetItemViewModel>();

        public ObservableCollection<SnippetItemViewModel> Children
        {
            get
            {
                return _childrenProperty;
            }

            set
            {
                if (_childrenProperty == value)
                {
                    return;
                }

                _childrenProperty = value;
                RaisePropertyChanged(ChildrenPropertyName);
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
        /// The <see cref="IsSnippet" /> property's name.
        /// </summary>
        public const string IsSnippetPropertyName = "IsSnippet";

        private bool _isSnippetProperty = false;

        public bool IsSnippet
        {
            get
            {
                return _isSnippetProperty;
            }

            set
            {
                if (_isSnippetProperty == value)
                {
                    return;
                }

                _isSnippetProperty = value;
                RaisePropertyChanged(IsSnippetPropertyName);
            }
        }


        public const string IsUserAddPropertyName = "IsUserAdd";

        private bool _isUserAddProperty = false;

        /// <summary>
        /// Sets and gets the IsUserAdd property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUserAdd
        {
            get
            {
                return _isUserAddProperty;
            }

            set
            {
                if (_isUserAddProperty == value)
                {
                    return;
                }

                _isUserAddProperty = value;
                RaisePropertyChanged(IsUserAddPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="Path" /> property's name.
        /// </summary>
        public const string PathPropertyName = "Path";

        private string _pathProperty = "";

        public string Path
        {
            get
            {
                return _pathProperty;
            }

            set
            {
                if (_pathProperty == value)
                {
                    return;
                }

                _pathProperty = value;
                RaisePropertyChanged(PathPropertyName);
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


        private RelayCommand<MouseButtonEventArgs> _treeNodeMouseRightButtonUpCommand;

        public RelayCommand<MouseButtonEventArgs> TreeNodeMouseRightButtonUpCommand
        {
            get
            {
                return _treeNodeMouseRightButtonUpCommand
                    ?? (_treeNodeMouseRightButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        if(IsUserAdd && !IsSnippet)
                        {
                            _contextMenuService.Show(this, "SnippetItemUserAddContextMenu");
                        }
                        else
                        {
                            _contextMenuService.Show(this, "SnippetItemContextMenu");
                        }
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
                        if (IsSnippet)
                        {
                            OpenSnippetCommand.Execute(null);
                        }

                    }));
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

        public void ApplyCriteria(string criteria, Stack<SnippetItemViewModel> ancestors)
        {
            SearchText = criteria;

            if (IsCriteriaMatched(criteria))
            {
                IsMatch = true;
                IsExpanded = true;

                foreach (var ancestor in ancestors)
                {
                    ancestor.IsMatch = true;
                    ancestor.IsExpanded = true;
                }

                SnippetTreeItemSetAllIsMatch(this, true);
            }

            ancestors.Push(this);
            foreach (var child in Children)
                child.ApplyCriteria(criteria, ancestors);

            ancestors.Pop();
        }

        private bool IsCriteriaMatched(string criteria)
        {
            return string.IsNullOrEmpty(criteria) || Name.ContainsIgnoreCase(criteria);
        }




        private RelayCommand _openSnippetCommand;

        public RelayCommand OpenSnippetCommand
        {
            get
            {
                return _openSnippetCommand
                    ?? (_openSnippetCommand = new RelayCommand(
                    () =>
                    {
                        try
                        {
                            if (IsSnippet)
                            {
                                DesignerDocumentViewModel doc;
                                bool isExist = _docksViewModel.IsDocumentExist(Path, out doc);

                                if (!isExist)
                                {
                                    var newDoc = _docksViewModel.NewDesignerDocument(Path);
                                    newDoc.IsAlwaysReadOnly = true;
                                }
                                else
                                {
                                    doc.IsSelected = true;
                                }
                            }
                            else
                            {
                                _commonService.LocateDirInExplorer(Path);
                            }
                        }
                        catch (Exception err)
                        {
                            SharedObject.Instance.Output(SharedObject.enOutputType.Error, Chainbot.Resources.Properties.Resources.Message_OpenSnippetError, err);
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_OpenSnippetError);
                        } 
                    }));
            }
        }



        private RelayCommand _removeSnippetCommand;

        public RelayCommand RemoveSnippetCommand
        {
            get
            {
                return _removeSnippetCommand
                    ?? (_removeSnippetCommand = new RelayCommand(
                    () =>
                    {
                        XmlDocument doc = new XmlDocument();
                        var path = System.IO.Path.Combine(_pathConfigService.AppDataDir, @"Config\CodeSnippets.xml");
                        doc.Load(path);
                        var rootNode = doc.DocumentElement;

                        var directoryNodes = rootNode.SelectNodes("Directory");
                        foreach (XmlNode dir in directoryNodes)
                        {
                            var id = (dir as XmlElement).GetAttribute("Id");

                            if (Id == id)
                            {
                                rootNode.RemoveChild(dir);
                                break;
                            }
                        }

                        doc.Save(path);

                        Messenger.Default.Send(this, "RemoveSnippet");
                    }));
            }
        }

        
    }
}