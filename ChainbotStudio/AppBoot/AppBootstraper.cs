using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.UI;
using Chainbot.Cores.Classes;
using Chainbot.Cores.Log;
using ChainbotStudio.ViewModel;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using Chainbot.Contracts.Classes;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace ChainbotStudio.AppBoot
{
    class AppBootstraper
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ServiceRegistry _serviceRegistry = new UserServiceRegistry();
        private static IStudioApplication _app;
        private static ILogService _logService;
        private static IDispatcherService _dispatcher;
        private static IMessageBoxService _messageBoxService;

        public static AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static void Main(string[] args)
        {
            Log4NetHelper.Init();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _serviceRegistry.RegisterServices();
            _app = _serviceRegistry.ResolveType<IStudioApplication>();
            _logService = _serviceRegistry.ResolveType<ILogService>();
            _dispatcher = _serviceRegistry.ResolveType<IDispatcherService>();
            _messageBoxService = _serviceRegistry.ResolveType<IMessageBoxService>();

            _app.Start(args);
            _app.Shutdown();

            Task.Run(()=> {
                Environment.Exit(0);
            });

            _autoResetEvent.WaitOne();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                _app.OnException();

                try
                {
                    var exception = e.ExceptionObject as Exception;
                    if (exception != null)
                    {
                        _logService.Error(Chainbot.Resources.Properties.Resources.Message_UnhandledException5, _logger);
                        _logService.Error(exception, _logger);

                        _messageBoxService.Show(Chainbot.Resources.Properties.Resources.Message_UnhandledException2);
                    }

                    if (exception is System.OutOfMemoryException)
                    {
                        Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    _logService.Fatal(Chainbot.Resources.Properties.Resources.Message_UnhandledException6, _logger);
                    _logService.Fatal(ex, _logger);

                    _messageBoxService.Show(Chainbot.Resources.Properties.Resources.Message_UnhandledException4);

                    Environment.Exit(0);
                }
            });
        }
    }
}
