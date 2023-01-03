using Chainbot.Contracts.AppDomains;
using Chainbot.Contracts.Classes;
using System;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Activities;
using Chainbot.Cores.Activities;
using System.Windows;
using Chainbot.Contracts.Workflow;
using Chainbot.Cores.Workflow;
using ActiproSoftware.Windows.Themes;
using Chainbot.Cores.Classes;
using Chainbot.Contracts.UI;
using Chainbot.Cores.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Cores.Utils;
using Chainbot.Cores.Log;
using Chainbot.Contracts.Log;
using Chainbot.Cores.ExpressionEditor;
using System.Windows.Threading;
using log4net;
using System.IO;
using log4net.Config;
using Chainbot.Contracts.Config;
using System.Globalization;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace Chainbot.Cores.AppDomains
{
    public class AppDomainServiceHost: MarshalByRefServiceBase, IAppDomainServiceHost
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;
        private IAppSettingsConfigService _appSettingsConfigService;

        private Application _app;
        private IServiceLocator _serviceLocator;
        private IAssemblyResolveService _assemblyResolveService;

        public AppDomainServiceHost()
        {
            Log4NetHelper.Init();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        
        private void ActiproInit()
        {
            ExternalLicenses.Register();

            if (GlobalConfig.CurrentLanguage == enLanguage.Chinese)
                CustomStringHelper.Init();

            if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
            {
                ThemeManager.CurrentTheme = ThemeName.MetroDark.ToString();
            }
            else if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Light)
            {
                ThemeManager.CurrentTheme = ThemeName.MetroLight.ToString();
            }
                
            ThemeManager.AreNativeThemesEnabled = true;
        }

        public void Init()
        {
            _app = new Application();
            _app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _app.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            _serviceLocator = ServiceLocator.New();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                _logService.Error(Chainbot.Resources.Properties.Resources.Message_UnhandledException6 + exception.ToString(), _logger);
            }
        }

        public void AfterInit()
        {
            _appSettingsConfigService = _serviceLocator.ResolveType<IAppSettingsConfigService>();         
            GlobalConfig.CurrentTheme = _appSettingsConfigService.CurrentTheme ?? GlobalConfig.DefaultTheme;
            GlobalConfig.CurrentLanguage = _appSettingsConfigService.CurrentLanguage ?? GlobalConfig.DefaultLanguage;
         
            ActiproInit();

            //ResourceDictionary resourceDictionaryDarkBrushes = new ResourceDictionary();
            //resourceDictionaryDarkBrushes.Source = new Uri("pack://application:,,,/Chainbot.Resources;component/Themes/DarkBrushes.xaml");
            //_app.Resources.MergedDictionaries.Add(resourceDictionaryDarkBrushes);

            ResourceDictionary resourceDictionaryGlobalColors = new ResourceDictionary();
            if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
            {
                resourceDictionaryGlobalColors.Source = new Uri("pack://application:,,,/Chainbot.Resources;component/WorkflowDesigner/GlobalColors.Dark.xaml");
            }
            else if(GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Light)
            {
                resourceDictionaryGlobalColors.Source = new Uri("pack://application:,,,/Chainbot.Resources;component/WorkflowDesigner/GlobalColors.Light.xaml");
            }

            _app.Resources.MergedDictionaries.Add(resourceDictionaryGlobalColors);

            ResourceDictionary pluginsSharedLibraryApplicationResources = new ResourceDictionary();
            pluginsSharedLibraryApplicationResources.Source = new Uri("pack://application:,,,/Plugins.Shared.Library;component/Resources/ApplicationResources.xaml");
            _app.Resources.MergedDictionaries.Add(pluginsSharedLibraryApplicationResources);

            ThemeUtils.AddThemeWindowLoadedEvent();

            if (GlobalConfig.CurrentLanguage == enLanguage.English)
            {
                LanguageUtil.SetLanguage("en");
            }
            else if (GlobalConfig.CurrentLanguage == enLanguage.Chinese)
            {
                LanguageUtil.SetLanguage("zh-CN");
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            _logService.Error(Chainbot.Resources.Properties.Resources.Message_UnhandledException5 + e.Exception.ToString(), _logger);
        }

        public TService GetService<TService>()
        {
            TService tservice = _serviceLocator.ResolveType<TService>();

            if (!(tservice is MarshalByRefServiceBase))
            {
                throw new InvalidOperationException("The service to be retrieved must inherit from MarshalByRefServiceBase");
            }

            return tservice;
        }

        public void RegisterServices()
        {
            _serviceLocator.RegisterTypeSingleton<IAssemblyResolveService, AssemblyResolveService>();
            _serviceLocator.RegisterTypeSingleton<IActivitiesDefaultAttributesService, ActivitiesDefaultAttributesService>();
            _serviceLocator.RegisterTypeSingleton<IWorkflowDesignerCollectService, WorkflowDesignerCollectService>();
            _serviceLocator.RegisterTypeSingleton<IDispatcherService, DispatcherService>();
            _serviceLocator.RegisterTypeSingleton<ICommonService, CommonService>();
            _serviceLocator.RegisterTypeSingleton<IWorkflowBreakpointsService, WorkflowBreakpointsService>();
            _serviceLocator.RegisterTypeSingleton<IWorkflowActivityService, WorkflowActivityService>();
            _serviceLocator.RegisterTypeSingleton<ILogService, Log4NetService>();

            _serviceLocator.RegisterType<IActivitiesService, ActivitiesService>();
            _serviceLocator.RegisterType<IWorkflowDesignerService, WorkflowDesignerService>();
            _serviceLocator.RegisterType<IRecordingService, RecordingService>();
            _serviceLocator.RegisterType<IDataExtractorService, DataExtractorService>();
            _serviceLocator.RegisterType<IWorkflowDesignerJumpService, WorkflowDesignerJumpService>();
        }

        
        public void RegisterCrossDomainInstance<TService>(TService instance) where TService : class
        {
            _serviceLocator.RegisterInstance(instance);
        }

        public void StartAssemblyResolve()
        {
            _assemblyResolveService = _serviceLocator.ResolveType<IAssemblyResolveService>();
            _logService = _serviceLocator.ResolveType<ILogService>();
        }



    }
}
