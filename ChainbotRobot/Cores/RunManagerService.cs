using log4net;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Executor;
using Plugins.Shared.Library.Librarys;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using ChainbotRobot.Contracts;
using ChainbotRobot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChainbotRobot.Cores
{
    public class RunManagerService: IRunManagerService
    {
        private IServiceLocator _serviceLocator;


        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PackageItemViewModel PackageItem { get; set; }


        private string _xamlPath { get; set; }

        public bool HasException { get; set; }

        private ChainbotExecutorController _controller = new ChainbotExecutorController();
        private string _guid;

        private List<string> _activitiesDllLoadFrom = new List<string>();
        private List<string> _dependentAssemblies = new List<string>();


        private ILogService _logService;

        private IControlServerService _controlServerService;

        private IAutoCloseMessageBoxService _autoCloseMessageBoxService;

        public event EventHandler BeginRunEvent;
        public event EventHandler EndRunEvent;

        public RunManagerService(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _logService = _serviceLocator.ResolveType<ILogService>();
            _controlServerService = _serviceLocator.ResolveType<IControlServerService>();
            _autoCloseMessageBoxService = _serviceLocator.ResolveType<IAutoCloseMessageBoxService>();

            _controller.ExceptionEvent += _controller_ExceptionEvent;
            _controller.CompleteEvent += _controller_CompleteEvent;
            _controller.LogEvent += _controller_LogEvent;
            _controller.InitRobot();
        }

        public void Init(PackageItemViewModel packageItem, string xamlPath, List<string> activitiesDllLoadFrom, List<string> dependentAssemblies)
        {
            PackageItem = packageItem;
            _xamlPath = xamlPath;

            _activitiesDllLoadFrom = activitiesDllLoadFrom;
            _dependentAssemblies = dependentAssemblies;
        }

        public void UpdatePackageItem(PackageItemViewModel packageItemViewModel)
        {
            PackageItem = packageItemViewModel;
        }


        private void _controller_ExceptionEvent(string id, string title, string msg)
        {
            Common.RunInUI(() =>
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, title, msg);
                _autoCloseMessageBoxService.Show(App.Current.MainWindow, title, "Prompt", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }


        private void _controller_CompleteEvent(string id, bool has_exception)
        {
            this.HasException = has_exception;
            EndRunEvent?.Invoke(this, EventArgs.Empty);

            _logService.Debug(string.Format("End executing workflow file {0}", _xamlPath), _logger);
        }

        private void _controller_LogEvent(string id, SharedObject.enOutputType type, string msg, string msgDetails)
        {
            Common.RunInUI(() =>
            {
                SharedObject.Instance.Output(type, msg, msgDetails);
            });
        }




        public void Run()
        {
            _logService.Debug(string.Format("Start executing workflow file {0} ……", _xamlPath), _logger);
            BeginRunEvent?.Invoke(this, EventArgs.Empty);

            var guid = ChainbotExecutorController.Guid();

            _guid = guid;

            var cfg = new ChainbotExecutorStartupConfig();
            cfg.Name = PackageItem.Name;
            cfg.Version = PackageItem.Version;
            cfg.MainXamlPath = _xamlPath;
            cfg.PipeName = ChainbotExecutorAgent.GetExecutorPipeName(guid);
            cfg.LoadAssemblyFromList = _activitiesDllLoadFrom;
            cfg.AssemblyResolveDllList = _dependentAssemblies;
            cfg.ProjectPath = SharedObject.Instance.ProjectPath;

            _controller.SetStartupConfig(guid, cfg);
            var arg = _controller.Start(guid);
            _logService.Debug($"+++++++++++++++++Start Process ChainbotExecutor.exe+++++++++++++++++ Command line parameters:{arg}", _logger);
        }

        public void Stop()
        {
            _controller.Stop(_guid);
            _controller_CompleteEvent(_guid, false);
        }


        public void LogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            Log(type, msg);

            _logService.Debug(string.Format(Properties.Resources.ActivityLog, type, msg, msgDetails), _logger);
        }



        public void Log(SharedObject.enOutputType type, string msg)
        {
            Task.Run(async () =>
            {
                const int limit = 150;
                if (msg.Length > limit)
                {
                    msg = msg.Substring(0, limit);
                }

                await _controlServerService.Log(PackageItem.Name, PackageItem.Version, type.ToString(), msg);
            });

        }

        
    }
}
