using log4net;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.UiAutomation;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Utils;
using Chainbot.Cores.ExpressionEditor;
using ChainbotRobot.Contracts;
using ChainbotRobot.Librarys;
using ChainbotRobot.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Chainbot.Contracts.Classes;
using static Chainbot.Contracts.Classes.GlobalConfig;
using System.IO;
using System.Xml;

namespace ChainbotRobot
{
    public partial class App : Application
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IConsoleManager _consoleManager;
        private ILogService _logService;
        private ICommonService _commonService;
        private IAutoCloseMessageBoxService _autoCloseMessageBoxService;

        private Mutex instanceMutex = null;

        private static ServiceRegistry _serviceRegistry = new UserServiceRegistry();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Init();
        }

        
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            UnInit();
        }


        private void Init()
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            ExternalLicenses.Register();

            _serviceRegistry.RegisterServices();

            _consoleManager = _serviceRegistry.ResolveType<IConsoleManager>();
            _logService = _serviceRegistry.ResolveType<ILogService>();
            _commonService = _serviceRegistry.ResolveType<ICommonService>();
            _autoCloseMessageBoxService = _serviceRegistry.ResolveType<IAutoCloseMessageBoxService>();

            bool createdNew = false;
            instanceMutex = new Mutex(true, "{1D9C8216-8931-449B-B18F-741F019C60CA}", out createdNew);
            if (createdNew)
            {
#if DEBUG
                _consoleManager.Open();
#endif

                UiElement.Init();
            }
            else
            {
                Environment.Exit(0);
            }


            SetLanguage();
        }

        private void SetLanguage()
        {
            enLanguage currentLanguage = GlobalConfig.DefaultLanguage;
            try
            {
                string localApplicationDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string appSettingsPath = Path.Combine(localApplicationDataDir, @"ChainbotStudio\Config\AppSettings.xml");
                XmlDocument doc = new XmlDocument();
                doc.Load(appSettingsPath);
                var rootNode = doc.DocumentElement;

                var languageElement = rootNode.SelectSingleNode("Language") as XmlElement;
                if (languageElement != null)
                {
                    var currentLanguageElement = languageElement.SelectSingleNode("CurrentLanguage") as XmlElement;
                    currentLanguage = (enLanguage)Enum.Parse(typeof(enLanguage), currentLanguageElement.InnerText, true);
                }

            }
            catch (Exception)
            {
            }

            GlobalConfig.CurrentLanguage = currentLanguage;

            if (GlobalConfig.CurrentLanguage == enLanguage.English)
            {
                LanguageUtil.SetLanguage("en-US");
            }
            else if (GlobalConfig.CurrentLanguage == enLanguage.Chinese)
            {
                LanguageUtil.SetLanguage("zh-CN");
            }
        }

        private void UnInit()        {
            var mainViewModel = _serviceRegistry.ResolveType<MainViewModel>();
            mainViewModel?.StopCaptureScreen();

#if DEBUG
            _consoleManager.Close();
#endif
        }


        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                _logService.Error("UI线程全局异常", _logger);
                _logService.Error(e.Exception, _logger);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                _logService.Fatal("不可恢复的UI线程全局异常", _logger);
                _logService.Fatal(ex, _logger);
            }

            Common.RunInUI(() =>
            {
                _autoCloseMessageBoxService.Show(App.Current.MainWindow, e.Exception.ToString(), "异常提示", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    _logService.Error("非UI线程全局异常", _logger);
                    _logService.Error(exception, _logger);
                }
            }
            catch (Exception ex)
            {
                _logService.Fatal("不可恢复的非UI线程全局异常", _logger);
                _logService.Fatal(ex, _logger);
            }

            Common.RunInUI(() =>
            {
                _autoCloseMessageBoxService.Show(App.Current.MainWindow, (e.ExceptionObject as Exception).ToString(), "异常提示", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }


    }
}
