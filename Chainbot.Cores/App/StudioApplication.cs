using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using log4net;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Chainbot.Contracts.UI;
using Plugins.Shared.Library.UiAutomation;
using GalaSoft.MvvmLight.Threading;
using Chainbot.Cores.ExpressionEditor;
using ActiproSoftware.Windows.Themes;
using Chainbot.Cores.Classes;
using System.Reflection;
using System.IO;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace Chainbot.Cores.App
{
    public abstract class StudioApplication : IStudioApplication
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IServiceLocator _serviceLocator;

        private Application _app;
        private ILogService _logService;

        private IConsoleManager _console;
        private IDispatcherService _dispatcherService;
        private IMessageBoxService _messageBoxService;
        private IPathConfigService _pathConfigService;
        private IAppSettingsConfigService _appSettingsConfigService;

        private IConstantConfigService _constantConfigService;

        private string[] _args;

        public string[] Args
        {
            get
            {
                return _args;
            }
        }

        public StudioApplication(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _logService = _serviceLocator.ResolveType<ILogService>();
            _console = _serviceLocator.ResolveType<IConsoleManager>();
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();
            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
            _constantConfigService = _serviceLocator.ResolveType<IConstantConfigService>();
            _pathConfigService = _serviceLocator.ResolveType<IPathConfigService>();
            _appSettingsConfigService = _serviceLocator.ResolveType<IAppSettingsConfigService>();

            Init();
         }

        private void Init()
        {
            _pathConfigService.InitDirs();
            InitThemeAndLanguage();
            FileAssociationInit();
            ActiproInit();
        }

        private void InitThemeAndLanguage()
        {
            GlobalConfig.CurrentTheme = _appSettingsConfigService.CurrentTheme ?? GlobalConfig.DefaultTheme;
            GlobalConfig.CurrentLanguage = _appSettingsConfigService.CurrentLanguage ?? GlobalConfig.DefaultLanguage;
        }

        private void FileAssociationInit()
        {
            try
            {
            FileAssociationHelper.AssociateFileExtension(_constantConfigService.XamlFileExtension);
            FileAssociationHelper.AssociateFileExtension(_constantConfigService.ProjectConfigFileExtension);
        }
            catch (Exception e)
            {
                _logService.Error("文件后缀绑定失败，可能权限不足，请检查是否为管理员权限运行！错误信息："+ e, _logger);
            }
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
                ThemeManager.CurrentTheme = ThemeName.MetroLightPurple.ToString();
            }
            ThemeManager.AreNativeThemesEnabled = true;
        }

        public void Start(string[] args)
        {
            PreProcessCommandLineInput(args);
            
#if DEBUG
            _console.Open();
#endif
            _args = args;

            _logService.Debug("Application Start", _logger);

            _app = InitApp();

            OnStart();

            var window = OnLoginWindow();
            window?.ShowDialog();

            if(!IsLoginSuccessful(window))
            {
                return;
            }

            window = OnSplashWindow();
            window?.ShowDialog();

            window = OnMainWindow();
            window?.ShowDialog();
        }

        public virtual bool IsLoginSuccessful(Window window)
        {
            return true;
        }

        private void PreProcessCommandLineInput(string[] args)
        {
            if (args.Length > 0)
            {
                var filePath = args[0];

                var fileName = Path.GetFileName(filePath);
                if (!fileName.EqualsIgnoreCase(_constantConfigService.ProjectConfigFileName))
                {
                    Environment.Exit(0);
                }
            }
        }

        public virtual void OnStart()
        {
            DispatcherHelper.Initialize();

            UiElement.Init();

            if (GlobalConfig.CurrentLanguage == enLanguage.English)
            {
                LanguageUtil.SetLanguage("en-US");
            }
            else if (GlobalConfig.CurrentLanguage == enLanguage.Chinese)
            {
                LanguageUtil.SetLanguage("zh-CN");
            }
        }

        private Application InitApp()
        {
            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            OnMergedDictionariesAdd(app.Resources.MergedDictionaries);
            return app;
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            _dispatcherService.InvokeAsync(() =>
            {
                OnException();

                try
                {
                    _logService.Error(Chainbot.Resources.Properties.Resources.Message_UnhandledException1, _logger);
                    _logService.Error(e.Exception, _logger);

                    var comException = e.Exception as System.Runtime.InteropServices.COMException;

                    if (comException != null && comException.ErrorCode == -2147221040)
                    {
                        return;
                    }


                    if (e.Exception is TargetInvocationException)
                    {
                        var tiExp = e.Exception as TargetInvocationException;
                        if (tiExp.InnerException is FileNotFoundException)
                        {
                            var fileExp = tiExp.InnerException as FileNotFoundException;
                            if (fileExp.FileName.ContainsIgnoreCase("Microsoft.VisualStudio"))
                            {
                                return;
                            }

                            if (fileExp.FileName.ContainsIgnoreCase("office"))
                            {
                                return;
                            }
                        }

                    }

                    if (e.Exception is System.IO.FileLoadException)
                    {
                        return;
                    }

                    _messageBoxService.Show(Chainbot.Resources.Properties.Resources.Message_UnhandledException2);

                    if (e.Exception is System.OutOfMemoryException)
                    {
                        Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    _logService.Fatal(Chainbot.Resources.Properties.Resources.Message_UnhandledException3, _logger);
                    _logService.Fatal(ex, _logger);
                    _messageBoxService.Show(Chainbot.Resources.Properties.Resources.Message_UnhandledException4);

                    Environment.Exit(0);
                }
            });
        }

        public void Shutdown()
        {
            _logService.Debug("Application End", _logger);

#if DEBUG
            _console.Close();
#endif
        }

        public virtual Window OnLoginWindow()
        {
            return null;
        }

        public virtual Window OnSplashWindow()
        {
            return null;
        }

        public abstract Window OnMainWindow();


        public virtual void OnMergedDictionariesAdd(Collection<ResourceDictionary> mergedDictionaries)
        {
            
        }

        public abstract void OnException();

    }
}
