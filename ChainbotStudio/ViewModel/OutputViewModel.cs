using Chainbot.Contracts.App;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Plugins.Shared.Library;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class OutputViewModel : ViewModelBase
    {
        private IServiceLocator _serviceLocator;
        private IDispatcherService _dispatcherService;
        private IProjectManagerService _projectManagerService;

        private ListBox _listBox;

        /// <summary>
        /// Initializes a new instance of the OutputViewModel class.
        /// </summary>
        public OutputViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();
            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();

            _projectManagerService.ProjectCloseEvent += _projectManagerService_ProjectCloseEvent;
        }

        private void _projectManagerService_ProjectCloseEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(()=> {
                ClearAllCommand.Execute(null);
            });
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






        /// <summary>
        /// The <see cref="IsShowTimestamps" /> property's name.
        /// </summary>
        public const string IsShowTimestampsPropertyName = "IsShowTimestamps";

        private bool _isShowTimestampsProperty = false;

        public bool IsShowTimestamps
        {
            get
            {
                return _isShowTimestampsProperty;
            }

            set
            {
                if (_isShowTimestampsProperty == value)
                {
                    return;
                }

                _isShowTimestampsProperty = value;
                RaisePropertyChanged(IsShowTimestampsPropertyName);
            }
        }




        /// <summary>
        /// The <see cref="IsShowError" /> property's name.
        /// </summary>
        public const string IsShowErrorPropertyName = "IsShowError";

        private bool _isShowErrorProperty = true;

        public bool IsShowError
        {
            get
            {
                return _isShowErrorProperty;
            }

            set
            {
                if (_isShowErrorProperty == value)
                {
                    return;
                }

                _isShowErrorProperty = value;
                RaisePropertyChanged(IsShowErrorPropertyName);

                ShowItemsFilter(value, (item) => { return item.IsError; });
            }
        }



        /// <summary>
        /// The <see cref="IsShowWarning" /> property's name.
        /// </summary>
        public const string IsShowWarningPropertyName = "IsShowWarning";

        private bool _isShowWarningProperty = true;

        public bool IsShowWarning
        {
            get
            {
                return _isShowWarningProperty;
            }

            set
            {
                if (_isShowWarningProperty == value)
                {
                    return;
                }

                _isShowWarningProperty = value;
                RaisePropertyChanged(IsShowWarningPropertyName);

                ShowItemsFilter(value, (item) => { return item.IsWarning; });
            }
        }

        /// <summary>
        /// The <see cref="IsShowInformation" /> property's name.
        /// </summary>
        public const string IsShowInformationPropertyName = "IsShowInformation";

        private bool _isShowInformationProperty = true;

        public bool IsShowInformation
        {
            get
            {
                return _isShowInformationProperty;
            }

            set
            {
                if (_isShowInformationProperty == value)
                {
                    return;
                }

                _isShowInformationProperty = value;
                RaisePropertyChanged(IsShowInformationPropertyName);

                ShowItemsFilter(value, (item) => { return item.IsInformation; });
            }
        }

        /// <summary>
        /// The <see cref="IsShowTrace" /> property's name.
        /// </summary>
        public const string IsShowTracePropertyName = "IsShowTrace";

        private bool _isShowTraceProperty = true;

        public bool IsShowTrace
        {
            get
            {
                return _isShowTraceProperty;
            }

            set
            {
                if (_isShowTraceProperty == value)
                {
                    return;
                }

                _isShowTraceProperty = value;
                RaisePropertyChanged(IsShowTracePropertyName);

                ShowItemsFilter(value, (item) => { return item.IsTrace; });
            }
        }



        /// <summary>
        /// The <see cref="ErrorCount" /> property's name.
        /// </summary>
        public const string ErrorCountPropertyName = "ErrorCount";

        private int _errorCountProperty = 0;

        public int ErrorCount
        {
            get
            {
                return _errorCountProperty;
            }

            set
            {
                if (_errorCountProperty == value)
                {
                    return;
                }

                _errorCountProperty = value;
                RaisePropertyChanged(ErrorCountPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="WarningCount" /> property's name.
        /// </summary>
        public const string WarningCountPropertyName = "WarningCount";

        private int _warningCountProperty = 0;

        public int WarningCount
        {
            get
            {
                return _warningCountProperty;
            }

            set
            {
                if (_warningCountProperty == value)
                {
                    return;
                }

                _warningCountProperty = value;
                RaisePropertyChanged(WarningCountPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="InformationCount" /> property's name.
        /// </summary>
        public const string InformationCountPropertyName = "InformationCount";

        private int _informationCountProperty = 0;

        public int InformationCount
        {
            get
            {
                return _informationCountProperty;
            }

            set
            {
                if (_informationCountProperty == value)
                {
                    return;
                }

                _informationCountProperty = value;
                RaisePropertyChanged(InformationCountPropertyName);
            }
        }




        /// <summary>
        /// The <see cref="TraceCount" /> property's name.
        /// </summary>
        public const string TraceCountPropertyName = "TraceCount";

        private int _traceCountProperty = 0;

        public int TraceCount
        {
            get
            {
                return _traceCountProperty;
            }

            set
            {
                if (_traceCountProperty == value)
                {
                    return;
                }

                _traceCountProperty = value;
                RaisePropertyChanged(TraceCountPropertyName);
            }
        }






        private RelayCommand _showTimestampsCommand;


        public RelayCommand ShowTimestampsCommand
        {
            get
            {
                return _showTimestampsCommand
                    ?? (_showTimestampsCommand = new RelayCommand(
                    () =>
                    {
                        IsShowTimestamps = !IsShowTimestamps;

                        foreach (var item in OutputItems)
                        {
                            item.IsShowTimestamps = IsShowTimestamps;
                        }
                    }));
            }
        }




        private RelayCommand _showErrorCommand;

        public RelayCommand ShowErrorCommand
        {
            get
            {
                return _showErrorCommand
                    ?? (_showErrorCommand = new RelayCommand(
                    () =>
                    {
                        IsShowError = !IsShowError;
                    }));
            }
        }



        private RelayCommand _showWarningCommand;

        public RelayCommand ShowWarningCommand
        {
            get
            {
                return _showWarningCommand
                    ?? (_showWarningCommand = new RelayCommand(
                    () =>
                    {
                        IsShowWarning = !IsShowWarning;
                    }));
            }
        }

        private RelayCommand _showInformationCommand;


        public RelayCommand ShowInformationCommand
        {
            get
            {
                return _showInformationCommand
                    ?? (_showInformationCommand = new RelayCommand(
                    () =>
                    {
                        IsShowInformation = !IsShowInformation;
                    }));
            }
        }


        private RelayCommand _showTraceCommand;

        public RelayCommand ShowTraceCommand
        {
            get
            {
                return _showTraceCommand
                    ?? (_showTraceCommand = new RelayCommand(
                    () =>
                    {
                        IsShowTrace = !IsShowTrace;
                    }));
            }
        }



        private RelayCommand _clearAllCommand;

        public RelayCommand ClearAllCommand
        {
            get
            {
                return _clearAllCommand
                    ?? (_clearAllCommand = new RelayCommand(
                    () =>
                    {
                        OutputItems.Clear();

                        ErrorCount = WarningCount = InformationCount = TraceCount = 0;
                    }));
            }
        }


        /// <summary>
        /// The <see cref="OutputItems" /> property's name.
        /// </summary>
        public const string OutputItemsPropertyName = "OutputItems";

        private ObservableCollection<OutputListItemViewModel> _outputItemsProperty = new ObservableCollection<OutputListItemViewModel>();

        public ObservableCollection<OutputListItemViewModel> OutputItems
        {
            get
            {
                return _outputItemsProperty;
            }

            set
            {
                if (_outputItemsProperty == value)
                {
                    return;
                }

                _outputItemsProperty = value;
                RaisePropertyChanged(OutputItemsPropertyName);
            }
        }


        private RelayCommand _copyItemMsgCommand;

        public RelayCommand CopyItemMsgCommand
        {
            get
            {
                return _copyItemMsgCommand
                    ?? (_copyItemMsgCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var item in OutputItems)
                        {
                            if (item.IsSelected)
                            {
                                item.CopyItemMsgCommand.Execute(null);
                                break;
                            }
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

        private void doSearch()
        {
            string searchContent = SearchText ?? "";

            searchContent = searchContent.Trim();

            if (string.IsNullOrEmpty(searchContent))
            {
                foreach (var item in OutputItems)
                {
                    item.IsSearching = false;
                }

                foreach (var item in OutputItems)
                {
                    item.SearchText = searchContent;
                }

                IsSearchResultEmpty = false;

                _listBox.ScrollIntoView(_listBox.SelectedItem);
            }
            else
            {
                foreach (var item in OutputItems)
                {
                    item.IsSearching = true;
                }

                foreach (var item in OutputItems)
                {
                    item.IsMatch = false;
                }


                foreach (var item in OutputItems)
                {
                    item.ApplyCriteria(searchContent);
                }

                IsSearchResultEmpty = true;
                foreach (var item in OutputItems)
                {
                    if (item.IsMatch)
                    {
                        IsSearchResultEmpty = false;
                        break;
                    }
                }

            }
        }


        public void Log(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            if (string.IsNullOrEmpty(msgDetails))
            {
                msgDetails = msg;
            }


            var item = _serviceLocator.ResolveType<OutputListItemViewModel>();

            switch (type)
            {
                case SharedObject.enOutputType.Error:
                    item.IsError = true;
                    ErrorCount++;
                    break;
                case SharedObject.enOutputType.Information:
                    item.IsInformation = true;
                    InformationCount++;
                    break;
                case SharedObject.enOutputType.Warning:
                    item.IsWarning = true;
                    WarningCount++;
                    break;
                case SharedObject.enOutputType.Trace:
                    item.IsTrace = true;
                    TraceCount++;
                    break;
                default:
                    break;
            }

            item.IsShowTimestamps = IsShowTimestamps;
            item.Timestamps = "["+DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")+"]";
            //item.Msg = msg;
            item.MsgDetails = msgDetails;

            if (msg.Contains("@") && msg.Split('@').Length == 3)
            {
                var msgArray = msg.Split('@');
                item.IsCanJump = true;
                item.Msg = msgArray[0];
                item.ActivityIdRef = msgArray[1];
                item.WorkFlowFilePath = msgArray[2];
            }
            else
            {
                item.Msg = msg;
            }

            OutputItems.Add(item);

            doSearch();
        }


        private void ShowItemsFilter(bool isVisible, Func<OutputListItemViewModel, bool> compare)
        {
            foreach (var item in OutputItems)
            {
                if (compare(item))
                {
                    item.IsVisible = isVisible;
                }
            }
        }




    }
}