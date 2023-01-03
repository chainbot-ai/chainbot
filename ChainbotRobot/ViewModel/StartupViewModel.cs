using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Utils;
using ChainbotRobot.Contracts;
using ChainbotRobot.Views;
using System;
using System.Windows;
using System.Windows.Threading;

namespace ChainbotRobot.ViewModel
{
    public class StartupViewModel : ViewModelBase
    {
        private IServiceLocator _serviceLocator;

        private IAutoCloseMessageBoxService _autoCloseMessageBoxService;

        private ICommonService _commonService;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private DispatcherTimer refreshProgramStatusTimer { get; set; }

        public Window m_view { get; set; }

        private bool IsQuitAsking = false;

        public MainWindow MainWindow { get; set; }

        public UserPreferencesWindow UserPreferencesWindow { get; set; }

        public AboutWindow AboutWindow { get; set; }


        private MainViewModel _mainViewModel;

        /// <summary>
        /// Initializes a new instance of the StartupViewModel class.
        /// </summary>
        public StartupViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _autoCloseMessageBoxService = _serviceLocator.ResolveType<IAutoCloseMessageBoxService>();


            _mainViewModel = _serviceLocator.ResolveType<MainViewModel>();

            _commonService = _serviceLocator.ResolveType<ICommonService>();

            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Hide();
            }

            if (UserPreferencesWindow == null)
            {
                UserPreferencesWindow = new UserPreferencesWindow();
                UserPreferencesWindow.Hide();
            }

            if (AboutWindow == null)
            {
                AboutWindow = new AboutWindow();
                AboutWindow.Hide();
            }

            App.Current.MainWindow = MainWindow;

            initTimer();
        }

        private void initTimer()
        {
            refreshProgramStatusTimer = new DispatcherTimer();
            refreshProgramStatusTimer.Interval = TimeSpan.FromHours(1);
            refreshProgramStatusTimer.Tick += RefreshProgramStatusTimer_Tick;
            refreshProgramStatusTimer.Start();
            RefreshProgramStatusTimer_Tick(null, null);
        }

        private void RefreshProgramStatusTimer_Tick(object sender, EventArgs e)
        {
            string info = "chainbot";
            RefreshProgramStatus(info);
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
                        m_view = (Window)p.Source;

                        Init();

                        _mainViewModel.InitControlServer();
                    }));
            }
        }

        private void Init()
        {
            ProgramVersion = string.Format("ChainbotRobot-{0}", _commonService.GetProgramVersion());

            var aboutViewModel = AboutWindow.DataContext as AboutViewModel;
            aboutViewModel.LoadAboutInfo();

            var userPreferencesViewModel = UserPreferencesWindow.DataContext as UserPreferencesViewModel;
            userPreferencesViewModel.LoadSettings();

            if (userPreferencesViewModel.IsAutoOpenMainWindow)
            {
                ShowMainWindowCommand.Execute(null);
            }

        }

        public void RefreshProgramStatus(string info)
        {
        }



        private RelayCommand _showMainWindowCommand;

        public RelayCommand ShowMainWindowCommand
        {
            get
            {
                return _showMainWindowCommand
                    ?? (_showMainWindowCommand = new RelayCommand(
                    () =>
                    {
                        MainWindow.Show();
                        MainWindow.Activate();
                    }));
            }
        }

        private RelayCommand _quitMainWindowCommand;

        public RelayCommand QuitMainWindowCommand
        {
            get
            {
                return _quitMainWindowCommand
                    ?? (_quitMainWindowCommand = new RelayCommand(
                    () =>
                    {
                        if (!IsQuitAsking)
                        {
                            IsQuitAsking = true;

                            var ret = _autoCloseMessageBoxService.Show(App.Current.MainWindow, "确定退出吗？", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                            if (ret == MessageBoxResult.Yes)
                            {
                                Application.Current.Shutdown();
                            }

                            IsQuitAsking = false;
                        }

                    }));
            }
        }



        private RelayCommand _userPreferencesCommand;

        public RelayCommand UserPreferencesCommand
        {
            get
            {
                return _userPreferencesCommand
                    ?? (_userPreferencesCommand = new RelayCommand(
                    () =>
                    {
                        _mainViewModel.UserPreferencesCommand.Execute(null);
                    }));
            }
        }


        private RelayCommand _viewLogsCommand;

        public RelayCommand ViewLogsCommand
        {
            get
            {
                return _viewLogsCommand
                    ?? (_viewLogsCommand = new RelayCommand(
                    () =>
                    {
                        _mainViewModel.ViewLogsCommand.Execute(null);
                    }));
            }
        }



        private RelayCommand _viewScreenRecordersCommand;

        public RelayCommand ViewScreenRecordersCommand
        {
            get
            {
                return _viewScreenRecordersCommand
                    ?? (_viewScreenRecordersCommand = new RelayCommand(
                    () =>
                    {
                        _mainViewModel.ViewScreenRecordersCommand.Execute(null);
                    }));
            }
        }




        private RelayCommand _aboutProductCommand;

        public RelayCommand AboutProductCommand
        {
            get
            {
                return _aboutProductCommand
                    ?? (_aboutProductCommand = new RelayCommand(
                    () =>
                    {
                        _mainViewModel.AboutProductCommand.Execute(null);
                    }));
            }
        }



        /// <summary>
        /// The <see cref="ProgramVersion" /> property's name.
        /// </summary>
        public const string ProgramVersionPropertyName = "ProgramVersion";

        private string _programVersionProperty = "";


        public string ProgramVersion
        {
            get
            {
                return _programVersionProperty;
            }

            set
            {
                if (_programVersionProperty == value)
                {
                    return;
                }

                _programVersionProperty = value;
                RaisePropertyChanged(ProgramVersionPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ProgramStatus" /> property's name.
        /// </summary>
        public const string ProgramStatusPropertyName = "ProgramStatus";

        private string _programStatusProperty = "";


        public string ProgramStatus
        {
            get
            {
                return _programStatusProperty;
            }

            set
            {
                if (_programStatusProperty == value)
                {
                    return;
                }

                _programStatusProperty = value;
                RaisePropertyChanged(ProgramStatusPropertyName);
            }
        }








    }
}