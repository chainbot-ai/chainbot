using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using ChainbotRobot.Contracts;
using ChainbotRobot.Database;
using ChainbotRobot.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Chainbot.Contracts.Classes;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace ChainbotRobot.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ScheduledTaskViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ILogService _logService;

        private IServiceLocator _serviceLocator;

        private ScheduledTaskManagementViewModel _scheduledTaskManagementViewModel;

        private ScheduledTasksDatabase _scheduledTasksDatabase;

        private IScheduledTasksService _scheduledTasksService;

        private string FrequencyType;

        private string MonthType;

        private string MonthValue;

        [Flags]
        public enum WeekEnumWithFlags
        {
            Null = 0x0,
            WholeWeek = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday,

            Monday = 0x01,
            Tuesday = 0x02,
            Wednesday = 0x04,
            Thursday = 0x08,
            Friday = 0x10,
            Saturday = 0x20,
            Sunday = 0x40,
        }

        public Type WeekEnumWithFlagsType { get; set; }

        private ScheduledTaskView _view;


        private ScheduledTasksTable _scheduledTaskItem;

        /// <summary>
        /// Initializes a new instance of the ScheduledTaskViewModel class.
        /// </summary>
        public ScheduledTaskViewModel(IServiceLocator serviceLocator, ILogService logService, ScheduledTasksDatabase scheduledTasksDatabase, IScheduledTasksService scheduledTasksService
            , ScheduledTaskManagementViewModel scheduledTaskManagementViewModel)
        {
            _serviceLocator = serviceLocator;
            _logService = logService;
            _scheduledTasksService = scheduledTasksService;

            _scheduledTasksDatabase = scheduledTasksDatabase;
            _scheduledTaskManagementViewModel = scheduledTaskManagementViewModel;

            this.WeekEnumWithFlagsType = typeof(WeekEnumWithFlags);
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
                        _view = (ScheduledTaskView)p.Source;
                    }));
            }
        }

        public void Reset(bool isClearScheduledPackageItems)
        {
            WeekEnumWithFlagsCurrentValue = WeekEnumWithFlags.Null;

            IsNew = false;

            IsShowDailyFrequency = IsShowMonthlyFrequency = IsShowWeeklyFrequency = IsShowMonthlyFrequencyAndDay = false;

            BeginDate = DateTime.Today;
            RunTime = DateTime.Now;

            EveryNum = 1;

            DayOfMonthNum = 1;

            if (_view != null)
            {
                _view._selectedPackageItemAutoCompleteBox.Text = "";
                _view._selectedFrequencyComboBox.SelectedIndex = 0;
                _view._monthPositionComboBox.SelectedIndex = 0;
                _view._dayOfMonthInfoComboBox.SelectedIndex = 0;
            }

            if(isClearScheduledPackageItems)
            {
                ScheduledPackageItems.Clear();
            }
        }


        public void Init(ScheduledTasksTable scheduledTaskItem)
        {
            IsNew = false;
            _scheduledTaskItem = scheduledTaskItem;

            switch(_scheduledTaskItem.FrequencyType)
            {
                case "Once":
                    SelectedFrequencIndex = 0;
                    break;
                case "Daily":
                    SelectedFrequencIndex = 1;
                    break;
                case "Weekly":
                    SelectedFrequencIndex = 2;
                    break;
                case "Monthly":
                    SelectedFrequencIndex = 3;
                    InitMonthTypeAndValue();
                    break;
            }

            EveryNum = _scheduledTaskItem.EveryNum;

            WeekEnumWithFlagsCurrentValue = (WeekEnumWithFlags)_scheduledTaskItem.WeekDays;

            RunTime = DateTime.ParseExact(_scheduledTaskItem.RunTime, "HH:mm", CultureInfo.CurrentCulture);
            BeginDate = DateTime.ParseExact(_scheduledTaskItem.BeginDate, "yyyy-MM-dd", CultureInfo.CurrentCulture);

            if (_view != null)
            {
                _view._selectedPackageItemAutoCompleteBox.Text = _scheduledTaskItem.Name;
            }
        }


        private void InitMonthTypeAndValue()
        {
            switch (_scheduledTaskItem.MonthType)
            {
                case "Day":
                    MonthPositionSelectedIndex = 0;
                    DayOfMonthNum = Convert.ToInt32(_scheduledTaskItem.MonthValue);
                    return;
                case "First":
                    MonthPositionSelectedIndex = 1;
                    break;
                case "Second":
                    MonthPositionSelectedIndex = 2;
                    break;
                case "Third":
                    MonthPositionSelectedIndex = 3;
                    break;
                case "Fourth":
                    MonthPositionSelectedIndex = 4;
                    break;
                case "Last":
                    MonthPositionSelectedIndex = 5;
                    break;
            }

            switch (_scheduledTaskItem.MonthValue)
            {
                case "Day":
                    DayOfMonthInfoSelectedIndex = 0;
                    return;
                case "Weekday":
                    DayOfMonthInfoSelectedIndex = 1;
                    return;
                case "WeekendDay":
                    DayOfMonthInfoSelectedIndex = 2;
                    return;
                case "Monday":
                    DayOfMonthInfoSelectedIndex = 3;
                    return;
                case "Tuesday":
                    DayOfMonthInfoSelectedIndex = 4;
                    return;
                case "Wednesday":
                    DayOfMonthInfoSelectedIndex = 5;
                    return;
                case "Thursday":
                    DayOfMonthInfoSelectedIndex = 6;
                    return;
                case "Friday":
                    DayOfMonthInfoSelectedIndex = 7;
                    return;
                case "Saturday":
                    DayOfMonthInfoSelectedIndex = 8;
                    return;
                case "Sunday":
                    DayOfMonthInfoSelectedIndex = 9;
                    return;
            }

        }


        /// <summary>
        /// The <see cref="WeekEnumWithFlagsCurrentValue" /> property's name.
        /// </summary>
        public const string WeekEnumWithFlagsCurrentValuePropertyName = "WeekEnumWithFlagsCurrentValue";

        private WeekEnumWithFlags _weekEnumWithFlagsCurrentValueProperty = WeekEnumWithFlags.Null;

        /// <summary>
        /// Sets and gets the WeekEnumWithFlagsCurrentValue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public WeekEnumWithFlags WeekEnumWithFlagsCurrentValue
        {
            get
            {
                return _weekEnumWithFlagsCurrentValueProperty;
            }

            set
            {
                OnWeekEnumWithFlagsCurrentValueChanged(value);
                if (_weekEnumWithFlagsCurrentValueProperty == value)
                {
                    return;
                }

                _weekEnumWithFlagsCurrentValueProperty = value;
                RaisePropertyChanged(WeekEnumWithFlagsCurrentValuePropertyName);
            }
        }

        private void OnWeekEnumWithFlagsCurrentValueChanged(WeekEnumWithFlags value)
        {
            if(_view != null)
            {
                if (value == WeekEnumWithFlags.Null)
                {
                    _view._weekEnumWithFlagsTextBlock.Foreground = Brushes.Red;
                }
                else
                {
                    _view._weekEnumWithFlagsTextBlock.Foreground = Brushes.Black;
                }
            }
        }


        /// <summary>
        /// The <see cref="IsNew" /> property's name.
        /// </summary>
        public const string IsNewPropertyName = "IsNew";

        private bool _isNewProperty = false;

        /// <summary>
        /// Sets and gets the IsNew property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNew
        {
            get
            {
                return _isNewProperty;
            }

            set
            {
                if (_isNewProperty == value)
                {
                    return;
                }

                _isNewProperty = value;
                RaisePropertyChanged(IsNewPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="ScheduledPackageItems" /> property's name.
        /// </summary>
        public const string ScheduledPackageItemsPropertyName = "ScheduledPackageItems";

        private ObservableCollection<ScheduledPackageItemViewModel> _scheduledPackageItemsProperty = new ObservableCollection<ScheduledPackageItemViewModel>();

        /// <summary>
        /// Sets and gets the ScheduledPackageItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ScheduledPackageItemViewModel> ScheduledPackageItems
        {
            get
            {
                return _scheduledPackageItemsProperty;
            }

            set
            {
                if (_scheduledPackageItemsProperty == value)
                {
                    return;
                }

                _scheduledPackageItemsProperty = value;
                RaisePropertyChanged(ScheduledPackageItemsPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsShowDailyFrequency" /> property's name.
        /// </summary>
        public const string IsShowDailyFrequencyPropertyName = "IsShowDailyFrequency";

        private bool _isShowDailyFrequencyProperty = false;


        /// <summary>
        /// Sets and gets the IsShowDailyFrequency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowDailyFrequency
        {
            get
            {
                return _isShowDailyFrequencyProperty;
            }

            set
            {
                if (_isShowDailyFrequencyProperty == value)
                {
                    return;
                }

                _isShowDailyFrequencyProperty = value;
                RaisePropertyChanged(IsShowDailyFrequencyPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsShowWeeklyFrequency" /> property's name.
        /// </summary>
        public const string IsShowWeeklyFrequencyPropertyName = "IsShowWeeklyFrequency";

        private bool _isShowWeeklyFrequencyProperty = false;

        /// <summary>
        /// Sets and gets the IsShowWeeklyFrequency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowWeeklyFrequency
        {
            get
            {
                return _isShowWeeklyFrequencyProperty;
            }

            set
            {
                if (_isShowWeeklyFrequencyProperty == value)
                {
                    return;
                }

                _isShowWeeklyFrequencyProperty = value;
                RaisePropertyChanged(IsShowWeeklyFrequencyPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsShowMonthlyFrequency" /> property's name.
        /// </summary>
        public const string IsShowMonthlyFrequencyPropertyName = "IsShowMonthlyFrequency";

        private bool _isShowMonthlyFrequencyProperty = false;

        /// <summary>
        /// Sets and gets the IsShowMonthlyFrequency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowMonthlyFrequency
        {
            get
            {
                return _isShowMonthlyFrequencyProperty;
            }

            set
            {
                if (_isShowMonthlyFrequencyProperty == value)
                {
                    return;
                }

                _isShowMonthlyFrequencyProperty = value;
                RaisePropertyChanged(IsShowMonthlyFrequencyPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsShowMonthlyFrequencyAndDay" /> property's name.
        /// </summary>
        public const string IsShowMonthlyFrequencyAndDayPropertyName = "IsShowMonthlyFrequencyAndDay";

        private bool _isShowMonthlyFrequencyAndDayProperty = false;

        /// <summary>
        /// Sets and gets the IsShowMonthlyFrequencyAndDay property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowMonthlyFrequencyAndDay
        {
            get
            {
                return _isShowMonthlyFrequencyAndDayProperty;
            }

            set
            {
                if (_isShowMonthlyFrequencyAndDayProperty == value)
                {
                    return;
                }

                _isShowMonthlyFrequencyAndDayProperty = value;
                RaisePropertyChanged(IsShowMonthlyFrequencyAndDayPropertyName);
            }
        }




        /// <summary>
        /// The <see cref="BeginDate" /> property's name.
        /// </summary>
        public const string BeginDatePropertyName = "BeginDate";

        private DateTime _beginDateProperty = DateTime.Today;

        /// <summary>
        /// Sets and gets the BeginDate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DateTime BeginDate
        {
            get
            {
                return _beginDateProperty;
            }

            set
            {
                if (_beginDateProperty == value)
                {
                    return;
                }

                _beginDateProperty = value;
                RaisePropertyChanged(BeginDatePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="RunTime" /> property's name.
        /// </summary>
        public const string RunTimePropertyName = "RunTime";

        private DateTime _runTimeProperty = DateTime.Now;

        /// <summary>
        /// Sets and gets the RunTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DateTime RunTime
        {
            get
            {
                return _runTimeProperty;
            }

            set
            {
                if (_runTimeProperty == value)
                {
                    return;
                }

                _runTimeProperty = value;
                RaisePropertyChanged(RunTimePropertyName);
            }
        }




        


        /// <summary>
        /// The <see cref="EveryNum" /> property's name.
        /// </summary>
        public const string EveryNumPropertyName = "EveryNum";

        private int _everyNumProperty = 1;

        /// <summary>
        /// Sets and gets the EveryNum property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int EveryNum
        {
            get
            {
                return _everyNumProperty;
            }

            set
            {
                if (_everyNumProperty == value)
                {
                    return;
                }

                _everyNumProperty = value;
                RaisePropertyChanged(EveryNumPropertyName);
            }
        }
        


        /// <summary>
        /// The <see cref="DayOfMonthNum" /> property's name.
        /// </summary>
        public const string DayOfMonthNumPropertyName = "DayOfMonthNum";

        private int _dayOfMonthNumProperty = 1;

        /// <summary>
        /// Sets and gets the DayOfMonthNum property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int DayOfMonthNum
        {
            get
            {
                return _dayOfMonthNumProperty;
            }

            set
            {
                if (_dayOfMonthNumProperty == value)
                {
                    return;
                }

                _dayOfMonthNumProperty = value;
                RaisePropertyChanged(DayOfMonthNumPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="SelectedFrequencIndex" /> property's name.
        /// </summary>
        public const string SelectedFrequencIndexPropertyName = "SelectedFrequencIndex";

        private int _selectedFrequencIndexProperty = 0;

        /// <summary>
        /// Sets and gets the SelectedFrequencIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SelectedFrequencIndex
        {
            get
            {
                return _selectedFrequencIndexProperty;
            }

            set
            {
                OnSelectedFrequencyIndexChanged(value);

                if (_selectedFrequencIndexProperty == value)
                {
                    return;
                }

                _selectedFrequencIndexProperty = value;
                RaisePropertyChanged(SelectedFrequencIndexPropertyName);
            }
        }
        

        private void OnSelectedFrequencyIndexChanged(int idx)
        {
            IsShowDailyFrequency = IsShowWeeklyFrequency = IsShowMonthlyFrequency = false;
            EveryNum = 1;

            MonthPositionSelectedIndex = 0;
            DayOfMonthInfoSelectedIndex = 0;

            WeekEnumWithFlagsCurrentValue = WeekEnumWithFlags.Null;

            switch (idx)
            {
                case 0:
                    FrequencyType = "Once";
                    break;
                case 1:
                    FrequencyType = "Daily";
                    IsShowDailyFrequency = true;
                    break;
                case 2:
                    FrequencyType = "Weekly";
                    IsShowWeeklyFrequency = true;
                    break;
                case 3:
                    FrequencyType = "Monthly";
                    IsShowMonthlyFrequency = true;
                    break;
            }
        }


        /// <summary>
        /// The <see cref="MonthPositionSelectedIndex" /> property's name.
        /// </summary>
        public const string MonthPositionSelectedIndexPropertyName = "MonthPositionSelectedIndex";

        private int _monthPositionSelectedIndexProperty = 0;

        /// <summary>
        /// Sets and gets the MonthPositionSelectedIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MonthPositionSelectedIndex
        {
            get
            {
                return _monthPositionSelectedIndexProperty;
            }

            set
            {
                OnMonthPositionSelectedIndexChanged(value);

                if (_monthPositionSelectedIndexProperty == value)
                {
                    return;
                }

                _monthPositionSelectedIndexProperty = value;
                RaisePropertyChanged(MonthPositionSelectedIndexPropertyName);              
            }
        }

        private void OnMonthPositionSelectedIndexChanged(int idx)
        {
            IsShowMonthlyFrequencyAndDay = false;
            switch (idx)
            {
                case 0:
                    MonthType = "Day";
                    IsShowMonthlyFrequencyAndDay = true;
                    break;
                case 1:
                    MonthType = "First";
                    break;
                case 2:
                    MonthType = "Second";
                    break;
                case 3:
                    MonthType = "Third";
                    break;
                case 4:
                    MonthType = "Fourth";
                    break;
                case 5:
                    MonthType = "Last";
                    break;
            }
        }


        /// <summary>
        /// The <see cref="DayOfMonthInfoSelectedIndex" /> property's name.
        /// </summary>
        public const string DayOfMonthInfoSelectedIndexPropertyName = "DayOfMonthInfoSelectedIndex";

        private int _dayOfMonthInfoSelectedIndexProperty = 0;

        /// <summary>
        /// Sets and gets the DayOfMonthInfoSelectedIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int DayOfMonthInfoSelectedIndex
        {
            get
            {
                return _dayOfMonthInfoSelectedIndexProperty;
            }

            set
            {
                OnDayOfMonthInfoSelectedIndexChanged(value);

                if (_dayOfMonthInfoSelectedIndexProperty == value)
                {
                    return;
                }

                _dayOfMonthInfoSelectedIndexProperty = value;
                RaisePropertyChanged(DayOfMonthInfoSelectedIndexPropertyName);
            }
        }

        private void OnDayOfMonthInfoSelectedIndexChanged(int idx)
        {
            switch (idx)
            {
                case 0:
                    MonthValue = "Day";
                    break;
                case 1:
                    MonthValue = "Weekday";
                    break;
                case 2:
                    MonthValue = "WeekendDay";
                    break;
                case 3:
                    MonthValue = "Monday";
                    break;
                case 4:
                    MonthValue = "Tuesday";
                    break;
                case 5:
                    MonthValue = "Wednesday";
                    break;
                case 6:
                    MonthValue = "Thursday";
                    break;
                case 7:
                    MonthValue = "Friday";
                    break;
                case 8:
                    MonthValue = "Saturday";
                    break;
                case 9:
                    MonthValue = "Sunday";
                    break;
            }
        }


        /// <summary>
        /// The <see cref="TaskDescription" /> property's name.
        /// </summary>
        public const string TaskDescriptionPropertyName = "TaskDescription";

        private string _taskDescriptionProperty = "";

        /// <summary>
        /// Sets and gets the TaskDescription property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TaskDescription
        {
            get
            {
                return _taskDescriptionProperty;
            }

            set
            {
                if (_taskDescriptionProperty == value)
                {
                    return;
                }

                _taskDescriptionProperty = value;
                RaisePropertyChanged(TaskDescriptionPropertyName);
            }
        }



        private ScheduledTasksTable MakeScheduledTaskItemInfo(int id = 0)
        {
            var item = new ScheduledTasksTable();
            item.Id = id;
            item.Name = _view._selectedPackageItemAutoCompleteBox.Text;
            item.BeginDate = BeginDate.ToString("yyyy-MM-dd");
            item.RunTime = RunTime.ToString("HH:mm");
            item.FrequencyType = FrequencyType;
            item.EveryNum = EveryNum;
            item.WeekDays = (int)WeekEnumWithFlagsCurrentValue;
            item.MonthType = MonthType;


            if (IsShowMonthlyFrequencyAndDay && MonthType == "Day")
            {
                item.MonthValue = DayOfMonthNum.ToString();
            }
            else
            {
                item.MonthValue = MonthValue;
            }

            

            return item;
        }


        private RelayCommand _saveCommand;

        /// <summary>
        /// Gets the SaveCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                    () =>
                    {
                        try
                        {
                            if(IsNew)
                            {
                                var item = MakeScheduledTaskItemInfo();
                                item.Description = TaskDescription;

                                _scheduledTasksDatabase.InsertItem(item);
                                _scheduledTasksService.AddJob(item);
                            }
                            else
                            {
                                var item = MakeScheduledTaskItemInfo(_scheduledTaskItem.Id);
                                item.Description = TaskDescription;
                                _scheduledTasksDatabase.UpdateItem(item);
                                _scheduledTasksService.UpdateJob(item);
                            }
                            
                        }
                        catch (Exception err)
                        {
                            var info = $"Exception occurred when saving the scheduled task. Exception details:" + err.ToString();

                            _logService.Debug(info, _logger);
                            MessageBox.Show(info, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        _scheduledTaskManagementViewModel.IsShowScheduledTaskView = false;
                        _scheduledTaskManagementViewModel.RefreshScheduledTaskItems();
                    },
                    CanSave));
            }
        }

        private bool CanSave()
        {
            TaskDescription = "";

            if (string.IsNullOrEmpty(_view?._selectedPackageItemAutoCompleteBox.Text))
            {
                return false;
            }

            if(IsShowWeeklyFrequency && _weekEnumWithFlagsCurrentValueProperty == WeekEnumWithFlags.Null)
            {
                return false;
            }

            MakeTaskDescription();
            
            return true;
        }


        private string makeMonthDesc(string monthType,string monthValue)
        {
            var desc = "";
            if (monthType == "Day")
            {
                desc = $"{monthValue}th";
            }
            else
            {
                var prefix = monthType;
                var postfix = monthValue;

                if (GlobalConfig.CurrentLanguage == enLanguage.Chinese)
                {
                    prefix = prefix.Replace("First", "第一");
                    prefix = prefix.Replace("Second", "第二");
                    prefix = prefix.Replace("Third", "第三");
                    prefix = prefix.Replace("Fourth", "第四");
                    prefix = prefix.Replace("Last", "最后一");

                    postfix = postfix.Replace("WeekendDay", "个周末");
                    postfix = postfix.Replace("Day", "天");
                    postfix = postfix.Replace("Weekday", "个工作日");
                    postfix = postfix.Replace("Monday", "个星期一");
                    postfix = postfix.Replace("Tuesday", "个星期二");
                    postfix = postfix.Replace("Wednesday", "个星期三");
                    postfix = postfix.Replace("Thursday", "个星期四");
                    postfix = postfix.Replace("Friday", "个星期五");
                    postfix = postfix.Replace("Saturday", "个星期六");
                    postfix = postfix.Replace("Sunday", "个星期天");
                }       

                desc = $"{prefix}{postfix}";
            }

            return desc;
        }

        private void MakeTaskDescription()
        {
            ScheduledTasksTable item = MakeScheduledTaskItemInfo();
            switch(item.FrequencyType)
            {
                case "Once":
                    TaskDescription = $"The process \"{ item.Name}\" will be executed once on the day of {item.BeginDate} {item.RunTime}.";
                    break;
                case "Daily":
                    TaskDescription = $"The process \"{ item.Name}\" will be executed on {item.BeginDate} {item.RunTime}, and it will be executed again every {item.EveryNum} days at the same time of the day.";
                    break;
                case "Weekly":
                    TaskDescription = $"The process \"{ item.Name}\" will be executed on {item.BeginDate} {item.RunTime} (whether it can be executed on that day needs to be judged whether the specified conditions are met), and the \"{ WeekEnumWithFlagsCurrentValue}\" will be executed again every {item.EveryNum} week at the same time on that day.";
                    break;
                case "Monthly":
                    var desc = makeMonthDesc(item.MonthType,item.MonthValue);
                    TaskDescription = $"The process \"{ item.Name}\" will be executed on {item.BeginDate} {item.RunTime} (whether the process can be executed on that day needs to be judged whether the specified conditions are met), and it will be executed again at the same time of the day every [{desc}] month of {item.EveryNum}";
                    break;
            }
        }

        private RelayCommand _cancelCommand;

        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    () =>
                    {
                        var stmVM = _serviceLocator.ResolveType<ScheduledTaskManagementViewModel>();
                        stmVM.IsShowScheduledTaskView = false;
                    },
                    () => true));
            }
        }




    }
}