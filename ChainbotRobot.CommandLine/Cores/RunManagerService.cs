using log4net;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Executor;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using ChainbotRobot.CommandLine.Contracts;
using System;
using System.Collections.Generic;

namespace ChainbotRobot.CommandLine.Cores
{
    public class RunManagerService : IRunManagerService
    {
        private IServiceLocator _serviceLocator;

        private string _name;
        private string _version;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _xamlPath { get; set; }

        public bool HasException { get; set; }

        private ChainbotExecutorController _controller = new ChainbotExecutorController();
        private string _guid;

        private List<string> _activitiesDllLoadFrom = new List<string>();
        private List<string> _dependentAssemblies = new List<string>();


        private ILogService _logService;
        private IGlobalService _globalService;

        public event EventHandler BeginRunEvent;
        public event EventHandler EndRunEvent;

        public RunManagerService(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _logService = _serviceLocator.ResolveType<ILogService>();
            _globalService = _serviceLocator.ResolveType<IGlobalService>();

            _controller.ExceptionEvent += _controller_ExceptionEvent;
            _controller.CompleteEvent += _controller_CompleteEvent;
            _controller.LogEvent += _controller_LogEvent;
            _controller.InitRobotCommandLine();
        }

        public void Init(string name, string version,string xamlPath
            , List<string> activitiesDllLoadFrom, List<string> dependentAssemblies)
        {
            _name = name;
            _version = version;

            _xamlPath = xamlPath;

            _activitiesDllLoadFrom = activitiesDllLoadFrom;
            _dependentAssemblies = dependentAssemblies;
        }



        private void _controller_ExceptionEvent(string id, string title, string msg)
        {
            SharedObject.Instance.Output(SharedObject.enOutputType.Error, title, msg);
        }


        private void _controller_CompleteEvent(string id, bool has_exception)
        {
            this.HasException = has_exception;
            EndRunEvent?.Invoke(this, EventArgs.Empty);

            _logService.Debug(string.Format("结束执行工作流文件 {0}", _xamlPath), _logger);

            _globalService.AutoResetEvent.Set();
        }

        private void _controller_LogEvent(string id, SharedObject.enOutputType type, string msg, string msgDetails)
        {
            SharedObject.Instance.Output(type, msg, msgDetails);
        }




        public void Run()
        {
            _logService.Debug(string.Format("开始执行工作流文件 {0} ……", _xamlPath), _logger);
            BeginRunEvent?.Invoke(this, EventArgs.Empty);

            var guid = ChainbotExecutorController.Guid();

            _guid = guid;

            var cfg = new ChainbotExecutorStartupConfig();
            cfg.Name = _name;
            cfg.Version = _version;
            cfg.MainXamlPath = _xamlPath;
            cfg.PipeName = ChainbotExecutorAgent.GetExecutorPipeName(guid);
            cfg.LoadAssemblyFromList = _activitiesDllLoadFrom;
            cfg.AssemblyResolveDllList = _dependentAssemblies;
            cfg.ProjectPath = SharedObject.Instance.ProjectPath;

            cfg.InitialCommandLineJsonConfig["input"] = _globalService.Options.Input;

            _controller.SetStartupConfig(guid, cfg);
            var arg = _controller.Start(guid);
            _logService.Debug($"+++++++++++++++++启动进程 ChainbotExecutor.exe+++++++++++++++++ 命令行参数:{arg}", _logger);
        }


        public void Stop()
        {
            _controller.Stop(_guid);
            _controller_CompleteEvent(_guid, false);
        }



        public void LogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            _logService.Debug(string.Format("活动日志：type={0},msg={1},msgDetails={2}", type, msg, msgDetails), _logger);
        }


    }
}
