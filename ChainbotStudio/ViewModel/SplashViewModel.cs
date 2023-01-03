using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using System;
using Chainbot.Contracts.Utils;
using System.Threading.Tasks;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using log4net;
using Chainbot.Contracts.UI;
using System.Threading;
using Chainbot.Contracts.Classes;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SplashViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window Window;

        private ILogService _logService;
        private ICommonService _commonService;
        private IPathConfigService _pathConfigService;
        private IDispatcherService _dispatcherService;

        private IAuthorizationService _authorizationService;


        /// <summary>
        /// Initializes a new instance of the SplashViewModel class.
        /// </summary>
        public SplashViewModel(ILogService logService, IDispatcherService dispatcherService,ICommonService commonService
            , IPathConfigService pathConfigService, IAuthorizationService authorizationService)
        {
            _logService = logService;
            _dispatcherService = dispatcherService;
            _commonService = commonService;
            _pathConfigService = pathConfigService;
            _authorizationService = authorizationService;
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
                        Window = (Window)p.Source;

                        Init();
                    }));
            }
        }

        private void Init()
        {
            if (GlobalConfig.CurrentLanguage == enLanguage.English)
            {
                SplashImage = "pack://application:,,,/Chainbot.Resources;Component/Image/Splash/splash_en.png";
            }
            else if (GlobalConfig.CurrentLanguage == enLanguage.Chinese)
            {
                SplashImage = "pack://application:,,,/Chainbot.Resources;Component/Image/Splash/splash.png";
            }

            Version = _commonService.GetProgramVersion();

            Task.Run(()=> {
                _logService.Debug($"Version: {_commonService.GetProgramVersion()}", _logger);
                var computer = Plugins.Shared.Library.Librarys.MyComputerInfo.Instance();
                string computerJson = Newtonsoft.Json.JsonConvert.SerializeObject(computer, Newtonsoft.Json.Formatting.Indented);
                _logService.Debug($"Computer Details: {computerJson}", _logger);

                _dispatcherService.InvokeAsync(()=> {
                    Window.Close();
                });
            });
        }

        /// <summary>
        /// The <see cref="SplashImage" /> property's name.
        /// </summary>
        public const string SplashImagePropertyName = "SplashImage";

        private string _splashImageProperty = "";

        /// <summary>
        /// Sets and gets the SplashImage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SplashImage
        {
            get
            {
                return _splashImageProperty;
            }

            set
            {
                if (_splashImageProperty == value)
                {
                    return;
                }

                _splashImageProperty = value;
                RaisePropertyChanged(SplashImagePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Version" /> property's name.
        /// </summary>
        public const string VersionPropertyName = "Version";

        private string _versionProperty = "";

        public string Version
        {
            get
            {
                return _versionProperty;
            }

            set
            {
                if (_versionProperty == value)
                {
                    return;
                }

                _versionProperty = value;
                RaisePropertyChanged(VersionPropertyName);
            }
        }


    }
}