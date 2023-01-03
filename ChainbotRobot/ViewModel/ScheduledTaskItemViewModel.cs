using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using Chainbot.Cores.Classes;
using ChainbotRobot.Contracts;
using ChainbotRobot.Database;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChainbotRobot.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ScheduledTaskItemViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ILogService _logService;

        private IServiceLocator _serviceLocator;

        private ScheduledTaskManagementViewModel _scheduledTaskManagementViewModel;
        private ScheduledTaskViewModel _scheduledTaskViewModel;

        private ScheduledTasksDatabase _scheduledTasksDatabase;
        private IScheduledTasksService _scheduledTasksService;

        private MainViewModel _mainViewModel;

        /// <summary>
        /// Initializes a new instance of the ScheduledTaskItemViewModel class.
        /// </summary>
        public ScheduledTaskItemViewModel(IServiceLocator serviceLocator, ILogService logService, ScheduledTasksDatabase scheduledTasksDatabase, IScheduledTasksService scheduledTasksService
            , ScheduledTaskManagementViewModel scheduledTaskManagementViewModel, ScheduledTaskViewModel scheduledTaskViewModel, MainViewModel mainViewModel)
        {
            _serviceLocator = serviceLocator;
            _logService = logService;

            _scheduledTasksDatabase = scheduledTasksDatabase;
            _scheduledTasksService = scheduledTasksService;
            _scheduledTaskManagementViewModel = scheduledTaskManagementViewModel;
            _scheduledTaskViewModel = scheduledTaskViewModel;
            _mainViewModel = mainViewModel;
        }


        public ScheduledTasksTable ScheduledTaskItem { get; set; }


        /// <summary>
        /// The <see cref="Id" /> property's name.
        /// </summary>
        public const string IdPropertyName = "Id";

        private int _idProperty = 0;

        /// <summary>
        /// Sets and gets the Id property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Id
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
        /// The <see cref="Description" /> property's name.
        /// </summary>
        public const string DescriptionPropertyName = "Description";

        private string _descriptionProperty = "";

        /// <summary>
        /// Sets and gets the Description property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Description
        {
            get
            {
                return _descriptionProperty;
            }

            set
            {
                if (_descriptionProperty == value)
                {
                    return;
                }

                _descriptionProperty = value;
                RaisePropertyChanged(DescriptionPropertyName);
            }
        }




        public const string NextRunDescriptionPropertyName = "NextRunDescription";

        private string _nextRunDescriptionProperty = "";

        /// <summary>
        /// Sets and gets the NextRunDescription property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string NextRunDescription
        {
            get
            {
                return _nextRunDescriptionProperty;
            }

            set
            {
                if (_nextRunDescriptionProperty == value)
                {
                    return;
                }

                _nextRunDescriptionProperty = value;
                RaisePropertyChanged(NextRunDescriptionPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsInvalid" /> property's name.
        /// </summary>
        public const string IsInvalidPropertyName = "IsInvalid";

        private bool _isInvalidProperty = false;

        /// <summary>
        /// Sets and gets the IsInvalid property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsInvalid
        {
            get
            {
                return _isInvalidProperty;
            }

            set
            {
                if (_isInvalidProperty == value)
                {
                    return;
                }

                _isInvalidProperty = value;
                RaisePropertyChanged(IsInvalidPropertyName);
            }
        }




        private RelayCommand _mouseLeftButtonUpCommand;

        /// <summary>
        /// Gets the MouseLeftButtonUpCommand.
        /// </summary>
        public RelayCommand MouseLeftButtonUpCommand
        {
            get
            {
                return _mouseLeftButtonUpCommand
                    ?? (_mouseLeftButtonUpCommand = new RelayCommand(
                    () =>
                    {
                        _scheduledTaskViewModel.Reset(true);

                        foreach (var item in _mainViewModel.PackageItems)
                        {
                            var spItem = _serviceLocator.ResolveType<ScheduledPackageItemViewModel>();
                            spItem.Name = item.Name;
                            spItem.Version = item.Version;
                            _scheduledTaskViewModel.ScheduledPackageItems.Add(spItem);
                        }

                        _scheduledTaskViewModel.Init(ScheduledTaskItem);
                        _scheduledTaskManagementViewModel.IsShowScheduledTaskView = true;
                    }));
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
                        var cm = view.FindResource("ScheduledTaskItemContextMenu") as ContextMenu;
                        cm.DataContext = this;
                        cm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                        cm.IsOpen = true;
                    }));
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
                        var info = $"Name: {Name}\r\nDescription: {Description}\r\nNext run time: {NextRunDescription}";
                        Clipboard.SetDataObject(info);
                    }));
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
                        var ret = MessageBox.Show(App.Current.MainWindow, "Are you sure to remove this scheduled task?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (ret == MessageBoxResult.Yes)
                        {
                            _scheduledTasksService.RemoveJob(this.ScheduledTaskItem);
                            _scheduledTasksDatabase.DeleteItem(ScheduledTaskItem);
                            _scheduledTaskManagementViewModel.RefreshScheduledTaskItems();
                        }
                    },
                    () => true));
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




    }
}