using Chainbot.Contracts.UI;
using ChainbotStudio.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class OutputListItemViewModel : ViewModelBase
    {
        private IWindowService _windowService;
        private IContextMenuService _contextMenuService;
        private IMessageBoxService _messageBoxService;
        private DocksViewModel _docksViewModel;

        /// <summary>
        /// Initializes a new instance of the OutputListItemViewModel class.
        /// </summary>
        public OutputListItemViewModel(IWindowService windowService,
            IContextMenuService contextMenuService,
            IMessageBoxService messageBoxService,
            DocksViewModel docksViewModel)
        {
            _windowService = windowService;
            _contextMenuService = contextMenuService;
            _messageBoxService = messageBoxService;
            _docksViewModel = docksViewModel;
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

                updateToolTip();
            }
        }

        private void updateToolTip()
        {
            if (IsShowTimestamps)
            {
                ToolTip = string.Format(Chainbot.Resources.Properties.Resources.ToolTip_OutPut1, Timestamps, Msg);
            }
            else
            {
                ToolTip = string.Format(Chainbot.Resources.Properties.Resources.ToolTip_OutPut2, Msg);
            }
        }



        /// <summary>
        /// The <see cref="Timestamps" /> property's name.
        /// </summary>
        public const string TimestampsPropertyName = "Timestamps";

        private string _timestampsProperty = "";


        public string Timestamps
        {
            get
            {
                return _timestampsProperty;
            }

            set
            {
                if (_timestampsProperty == value)
                {
                    return;
                }

                _timestampsProperty = value;
                RaisePropertyChanged(TimestampsPropertyName);
            }
        }





        /// <summary>
        /// The <see cref="IsError" /> property's name.
        /// </summary>
        public const string IsErrorPropertyName = "IsError";

        private bool _isErrorProperty = false;


        public bool IsError
        {
            get
            {
                return _isErrorProperty;
            }

            set
            {
                if (_isErrorProperty == value)
                {
                    return;
                }

                _isErrorProperty = value;
                RaisePropertyChanged(IsErrorPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsWarning" /> property's name.
        /// </summary>
        public const string IsWarningPropertyName = "IsWarning";

        private bool _isWarningProperty = false;

        public bool IsWarning
        {
            get
            {
                return _isWarningProperty;
            }

            set
            {
                if (_isWarningProperty == value)
                {
                    return;
                }

                _isWarningProperty = value;
                RaisePropertyChanged(IsWarningPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsInformation" /> property's name.
        /// </summary>
        public const string IsInformationPropertyName = "IsInformation";

        private bool _isInformationProperty = false;

        public bool IsInformation
        {
            get
            {
                return _isInformationProperty;
            }

            set
            {
                if (_isInformationProperty == value)
                {
                    return;
                }

                _isInformationProperty = value;
                RaisePropertyChanged(IsInformationPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsTrace" /> property's name.
        /// </summary>
        public const string IsTracePropertyName = "IsTrace";

        private bool _isTraceProperty = false;

        public bool IsTrace
        {
            get
            {
                return _isTraceProperty;
            }

            set
            {
                if (_isTraceProperty == value)
                {
                    return;
                }

                _isTraceProperty = value;
                RaisePropertyChanged(IsTracePropertyName);
            }
        }




        /// <summary>
        /// The <see cref="Msg" /> property's name.
        /// </summary>
        public const string MsgPropertyName = "Msg";

        private string _msgProperty = "";


        public string Msg
        {
            get
            {
                return _msgProperty;
            }

            set
            {
                if (_msgProperty == value)
                {
                    return;
                }

                _msgProperty = value;
                RaisePropertyChanged(MsgPropertyName);

                updateToolTip();


            }
        }


        /// <summary>
        /// The <see cref="MsgDetails" /> property's name.
        /// </summary>
        public const string MsgDetailsPropertyName = "MsgDetails";

        private string _msgDetailsProperty = "";

        public string MsgDetails
        {
            get
            {
                return _msgDetailsProperty;
            }

            set
            {
                if (_msgDetailsProperty == value)
                {
                    return;
                }

                _msgDetailsProperty = value;
                RaisePropertyChanged(MsgDetailsPropertyName);
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
        /// The <see cref="ActivityIdRef" /> property's name.
        /// </summary>
        public const string ActivityIdRefPropertyName = "ActivityIdRef";

        private string _activityIdRefProperty = null;

        public string ActivityIdRef
        {
            get
            {
                return _activityIdRefProperty;
            }

            set
            {
                if (_activityIdRefProperty == value)
                {
                    return;
                }

                _activityIdRefProperty = value;
                RaisePropertyChanged(ActivityIdRefPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="WorkFlowFilePath" /> property's name.
        /// </summary>
        public const string WorkFlowFilePathPropertyName = "WorkFlowFilePath";

        private string _workFlowFilePathProperty = null;

        public string WorkFlowFilePath
        {
            get
            {
                return _workFlowFilePathProperty;
            }

            set
            {
                if (_workFlowFilePathProperty == value)
                {
                    return;
                }

                _workFlowFilePathProperty = value;
                RaisePropertyChanged(WorkFlowFilePathPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsCanJump" /> property's name.
        /// </summary>
        public const string IsCanJumpPropertyName = "IsCanJump";

        private bool _isCanJumpProperty = false;

        /// <summary>
        /// Sets and gets the IsCanJump property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCanJump
        {
            get
            {
                return _isCanJumpProperty;
            }

            set
            {
                if (_isCanJumpProperty == value)
                {
                    return;
                }

                _isCanJumpProperty = value;
                RaisePropertyChanged(IsCanJumpPropertyName);
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

        private static string wildCardToRegular(string value)
        {
            return ".*" + Regex.Escape(value).Replace("\\ ", ".*") + ".*";
        }

        private bool IsCriteriaMatched(string criteria)
        {
            return string.IsNullOrEmpty(criteria) || Regex.IsMatch(Msg, wildCardToRegular(criteria), RegexOptions.IgnoreCase);
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
                        _contextMenuService.Show(this, "OutputItemContextMenu");
                    }));
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
                        Clipboard.SetDataObject(Msg);
                    }));
            }
        }

        public void JumpErrorActivity()
        {
            try
            {
                DesignerDocumentViewModel doc;
                bool isExist = _docksViewModel.IsDocumentExist(WorkFlowFilePath, out doc);

                if (!isExist)
                {
                    _docksViewModel.NewDesignerDocument(WorkFlowFilePath);
                    _docksViewModel.IsDocumentExist(WorkFlowFilePath, out doc);
                }
                else
                {
                    doc.IsSelected = true;
                }

                doc.FocusActivity(ActivityIdRef);
            }
            catch (Exception err)
            {
                _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_JumpError + err.Message);
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
                        if (IsCanJump)
                        {
                            JumpErrorActivity();
                        }
                        else
                        {
                            ViewItemMsgDetailCommand.Execute(null);
                        }
                    }));
            }
        }

        private RelayCommand _viewItemMsgDetailCommand;

        public RelayCommand ViewItemMsgDetailCommand
        {
            get
            {
                return _viewItemMsgDetailCommand
                    ?? (_viewItemMsgDetailCommand = new RelayCommand(
                    () =>
                    {
                        var window = new MessageDetailsWindow();
                        
                        var vm = window.DataContext as MessageDetailsViewModel;
                        vm.WindowTitle = Chainbot.Resources.Properties.Resources.MessageDetails_Title2;
                        vm.MsgDetails = MsgDetails;
                        _windowService.ShowDialog(window);
                    }));
            }
        }



    }
}