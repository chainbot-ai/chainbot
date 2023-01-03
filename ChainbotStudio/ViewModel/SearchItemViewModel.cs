using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Plugins.Shared.Library;
using Chainbot.Contracts.UI;
using System;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SearchItemViewModel : ViewModelBase
    {
        private SearchViewModel _searchViewModel;
        private DocksViewModel _docksViewModel;
        private IMessageBoxService _messageBoxService;

        /// <summary>
        /// Initializes a new instance of the SearchItemViewModel class.
        /// </summary>
        public SearchItemViewModel(SearchViewModel searchViewModel, DocksViewModel docksViewModel, IMessageBoxService messageBoxService)
        {
            _searchViewModel = searchViewModel;
            _docksViewModel = docksViewModel;
            _messageBoxService = messageBoxService;
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
        /// The <see cref="IsActivity" /> property's name.
        /// </summary>
        public const string IsActivityPropertyName = "IsActivity";

        private bool _isActivityProperty = false;

        /// <summary>
        /// Sets and gets the IsActivity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsActivity
        {
            get
            {
                return _isActivityProperty;
            }

            set
            {
                if (_isActivityProperty == value)
                {
                    return;
                }

                _isActivityProperty = value;
                RaisePropertyChanged(IsActivityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsVariable" /> property's name.
        /// </summary>
        public const string IsVariablePropertyName = "IsVariable";

        private bool _isVariableProperty = false;

        /// <summary>
        /// Sets and gets the IsVariable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsVariable
        {
            get
            {
                return _isVariableProperty;
            }

            set
            {
                if (_isVariableProperty == value)
                {
                    return;
                }

                _isVariableProperty = value;
                RaisePropertyChanged(IsVariablePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsUsedVariable" /> property's name.
        /// </summary>
        public const string IsUsedVariablePropertyName = "IsUsedVariable";

        private bool _isUsedVariableProperty = false;

        /// <summary>
        /// Sets and gets the IsUsedVariable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUsedVariable
        {
            get
            {
                return _isUsedVariableProperty;
            }

            set
            {
                if (_isUsedVariableProperty == value)
                {
                    return;
                }

                _isUsedVariableProperty = value;
                RaisePropertyChanged(IsUsedVariablePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsArgument" /> property's name.
        /// </summary>
        public const string IsArgumentPropertyName = "IsArgument";

        private bool _isArgumentProperty = false;

        /// <summary>
        /// Sets and gets the IsArgument property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsArgument
        {
            get
            {
                return _isArgumentProperty;
            }

            set
            {
                if (_isArgumentProperty == value)
                {
                    return;
                }

                _isArgumentProperty = value;
                RaisePropertyChanged(IsArgumentPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsUsedArgument" /> property's name.
        /// </summary>
        public const string IsUsedArgumentPropertyName = "IsUsedArgument";

        private bool _myProperty = false;

        /// <summary>
        /// Sets and gets the IsUsedArgument property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUsedArgument
        {
            get
            {
                return _myProperty;
            }

            set
            {
                if (_myProperty == value)
                {
                    return;
                }

                _myProperty = value;
                RaisePropertyChanged(IsUsedArgumentPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IdRef" /> property's name.
        /// </summary>
        public const string IdRefPropertyName = "IdRef";

        private string _idRefProperty = "";

        /// <summary>
        /// Sets and gets the IdRef property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string IdRef
        {
            get
            {
                return _idRefProperty;
            }

            set
            {
                if (_idRefProperty == value)
                {
                    return;
                }

                _idRefProperty = value;
                RaisePropertyChanged(IdRefPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IconSource" /> property's name.
        /// </summary>
        public const string IconSourcePropertyName = "IconSource";

        private ImageSource _iconSourceProperty = null;

        /// <summary>
        /// Sets and gets the IconSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ImageSource IconSource
        {
            get
            {
                return _iconSourceProperty;
            }

            set
            {
                if (_iconSourceProperty == value)
                {
                    return;
                }

                _iconSourceProperty = value;
                RaisePropertyChanged(IconSourcePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Path" /> property's name.
        /// </summary>
        public const string PathPropertyName = "Path";

        private string _pathProperty = "";

        /// <summary>
        /// Sets and gets the Path property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
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
        /// The <see cref="SearchResult" /> property's name.
        /// </summary>
        public const string SearchResultPropertyName = "SearchResult";

        private string _searchResultProperty = "";

        /// <summary>
        /// Sets and gets the SearchResult property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SearchResult
        {
            get
            {
                return _searchResultProperty;
            }

            set
            {
                if (_searchResultProperty == value)
                {
                    return;
                }

                _searchResultProperty = value;
                RaisePropertyChanged(SearchResultPropertyName);
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
        /// The <see cref="SearchResultDetail" /> property's name.
        /// </summary>
        public const string SearchResultDetailPropertyName = "SearchResultDetail";

        private string _searchResultDetailProperty = "";

        /// <summary>
        /// Sets and gets the SearchResultDetail property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SearchResultDetail
        {
            get
            {
                return _searchResultDetailProperty;
            }

            set
            {
                if (_searchResultDetailProperty == value)
                {
                    return;
                }

                _searchResultDetailProperty = value;
                RaisePropertyChanged(SearchResultDetailPropertyName);
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
                        try
                        {
                            _searchViewModel.DeactivatedCommand.Execute(null);

                            DesignerDocumentViewModel doc;
                            bool isExist = _docksViewModel.IsDocumentExist(Path, out doc);

                            if (!isExist)
                            {
                                _docksViewModel.NewDesignerDocument(Path);
                                _docksViewModel.IsDocumentExist(Path, out doc);
                            }
                            else
                            {
                                doc.IsSelected = true;
                            }

                            if (IsActivity || IsUsedVariable || IsUsedArgument)
                            {
                                doc.FocusActivity(IdRef);
                            }
                            else if (IsVariable)
                            {
                                doc.FocusVariable(SearchResult, IdRef);
                            }
                            else if (IsArgument)
                            {
                                doc.FocusArgument(SearchResult);
                            }
                        }
                        catch (Exception err)
                        {
                            SharedObject.Instance.Output(SharedObject.enOutputType.Error, Chainbot.Resources.Properties.Resources.Message_DoubleJumpError, err);
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_DoubleJumpError);
                        }
                    }));
            }
        }
    }
}