using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Chainbot.Contracts.App;
using System.Collections.ObjectModel;
using System;
using ChainbotRobot.Database;
using Chainbot.Contracts.Log;
using log4net;
using Plugins.Shared.Library.Librarys;
using System.Threading.Tasks;
using ChainbotRobot.Contracts;

namespace ChainbotRobot.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ScheduledTaskManagementViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ILogService _logService;

        private IServiceLocator _serviceLocator;
        private ScheduledTasksDatabase _scheduledTasksDatabase;
        private IPackageService _packageService;
        private IScheduledTasksService _scheduledTasksService;

        /// <summary>
        /// Initializes a new instance of the ScheduledTaskManagementViewModel class.
        /// </summary>
        public ScheduledTaskManagementViewModel(IServiceLocator serviceLocator, ILogService logService, ScheduledTasksDatabase scheduledTasksDatabase
            ,IPackageService packageService, IScheduledTasksService scheduledTasksService)
        {
            _serviceLocator = serviceLocator;

            _logService = logService;
            _scheduledTasksDatabase = scheduledTasksDatabase;
            _packageService = packageService;
            _scheduledTasksService = scheduledTasksService;
        }

        private void Reset()
        {
            SearchText = "";
            ScheduledTaskItems.Clear();
            IsShowScheduledTaskView = false;
        }


        public void RefreshScheduledTaskItems()
        {
            Reset();

            var items = _scheduledTasksDatabase.GetItems();
            foreach(var item in items)
            {
                var itemVM = _serviceLocator.ResolveType<ScheduledTaskItemViewModel>();
                itemVM.ScheduledTaskItem = item;
                itemVM.Id = item.Id;
                itemVM.Name = item.Name;
                itemVM.Description = item.Description;

                var jobj = _scheduledTasksService.JobIdNextRunDescDict[item.Id.ToString()];
                itemVM.NextRunDescription = jobj["msg"].ToString();

                if (jobj["type"].ToString() == "invalid")
                {
                    itemVM.IsInvalid = true;
                }

                if (!_packageService.IsPackageNameExist(itemVM.Name))
                {
                    itemVM.IsInvalid = true;
                    itemVM.NextRunDescription = "Invalid, the corresponding process does not exist.";
                }

                ScheduledTaskItems.Add(itemVM);
            }
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

                doSearch();
            }
        }


        

        /// <summary>
        /// The <see cref="IsShowScheduledTaskView" /> property's name.
        /// </summary>
        public const string IsShowScheduledTaskViewPropertyName = "IsShowScheduledTaskView";

        private bool _isShowScheduledTaskViewProperty = false;

        /// <summary>
        /// Sets and gets the IsShowScheduledTaskView property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowScheduledTaskView
        {
            get
            {
                return _isShowScheduledTaskViewProperty;
            }

            set
            {
                if (_isShowScheduledTaskViewProperty == value)
                {
                    return;
                }

                _isShowScheduledTaskViewProperty = value;
                RaisePropertyChanged(IsShowScheduledTaskViewPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="ScheduledTaskItems" /> property's name.
        /// </summary>
        public const string ScheduledTaskItemsPropertyName = "ScheduledTaskItems";

        private ObservableCollection<ScheduledTaskItemViewModel> _scheduledTaskItemsProperty = new ObservableCollection<ScheduledTaskItemViewModel>();

        /// <summary>
        /// Sets and gets the ScheduledTaskItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ScheduledTaskItemViewModel> ScheduledTaskItems
        {
            get
            {
                return _scheduledTaskItemsProperty;
            }

            set
            {
                if (_scheduledTaskItemsProperty == value)
                {
                    return;
                }

                _scheduledTaskItemsProperty = value;
                RaisePropertyChanged(ScheduledTaskItemsPropertyName);
            }
        }

       


        private RelayCommand _newScheduledTaskCommand;

        /// <summary>
        /// Gets the NewScheduledTaskCommand.
        /// </summary>
        public RelayCommand NewScheduledTaskCommand
        {
            get
            {
                return _newScheduledTaskCommand
                    ?? (_newScheduledTaskCommand = new RelayCommand(
                    () =>
                    {
                        IsShowScheduledTaskView = true;

                        var vm = _serviceLocator.ResolveType<ScheduledTaskViewModel>();
                        vm.Reset(false);
                        vm.IsNew = true;
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
            var searchContent = SearchText.Trim();
            if (string.IsNullOrEmpty(searchContent))
            {
                foreach (var item in ScheduledTaskItems)
                {
                    item.IsSearching = false;
                }

                foreach (var item in ScheduledTaskItems)
                {
                    item.SearchText = searchContent;
                }

                IsSearchResultEmpty = false;
            }
            else
            {
                foreach (var item in ScheduledTaskItems)
                {
                    item.IsSearching = true;
                }

                foreach (var item in ScheduledTaskItems)
                {
                    item.IsMatch = false;
                }


                foreach (var item in ScheduledTaskItems)
                {
                    item.ApplyCriteria(searchContent);
                }

                IsSearchResultEmpty = true;
                foreach (var item in ScheduledTaskItems)
                {
                    if (item.IsMatch)
                    {
                        IsSearchResultEmpty = false;
                        break;
                    }
                }

            }
        }







    }
}