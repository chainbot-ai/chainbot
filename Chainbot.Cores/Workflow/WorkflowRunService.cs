using Chainbot.Contracts.Log;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Workflow;
using log4net;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Executor;
using System;
using System.Activities;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.Utils;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowRunService : IWorkflowRunService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;
        private IWorkflowStateService _workflowStateService;
        private IMessageBoxService _messageBoxService;
        private IProjectManagerService _projectManagerService;
        private IDispatcherService _dispatcherService;
        private IWindowService _windowService;

        private string _xamlPath;

        private ChainbotExecutorController _controller = new ChainbotExecutorController();
        private string _guid;

        private List<string> _activitiesDllLoadFrom = new List<string>();
        private List<string> _dependentAssemblies = new List<string>();

        private Stopwatch _workflowExecutorStopwatch = new Stopwatch();

        public WorkflowRunService(ILogService logService,IWorkflowStateService workflowStateService
            ,IMessageBoxService messageBoxService, IProjectManagerService projectManagerService
            , IDispatcherService dispatcherService, IWindowService windowService)
        {
            _logService = logService;
            _workflowStateService = workflowStateService;
            _messageBoxService = messageBoxService;
            _projectManagerService = projectManagerService;
            _dispatcherService = dispatcherService;
            _windowService = windowService;

            _controller.ExceptionEvent += _controller_ExceptionEvent;
            _controller.CompleteEvent += _controller_CompleteEvent;
            _controller.LogEvent += _controller_LogEvent;
            _controller.NotifyEvent += _controller_NotifyEvent;
            _controller.InitStudio(false);
        }

       
        private void _controller_LogEvent(string id, SharedObject.enOutputType type, string msg, string msgDetails)
        {
            SharedObject.Instance.Output(type, msg, msgDetails);
        }

        private void _controller_NotifyEvent(string id, string notification, string notificationDetails)
        {
            SharedObject.Instance.Notify(notification, notificationDetails);
        }

        private void StopwatchRestart()
        {
            _workflowExecutorStopwatch.Restart();
            SharedObject.Instance.Output(SharedObject.enOutputType.Information, string.Format(Chainbot.Resources.Properties.Resources.Message_StartRun, _projectManagerService.CurrentProjectJsonConfig.name));
        }

        private void StopwatchStop()
        {
            _workflowExecutorStopwatch.Stop();

            string elapsedTime = "";
            var elapsedTimeSpan = _workflowExecutorStopwatch.Elapsed.Duration();
            if (_workflowExecutorStopwatch.Elapsed.Days > 0)
            {
                elapsedTime = elapsedTimeSpan.ToString("%d") + " day(s) " + elapsedTimeSpan.ToString(@"hh\:mm\:ss");
            }
            else
            {
                elapsedTime = elapsedTimeSpan.ToString(@"hh\:mm\:ss");
            }

            SharedObject.Instance.Output(SharedObject.enOutputType.Information, string.Format(Chainbot.Resources.Properties.Resources.Message_EndRun, _projectManagerService.CurrentProjectJsonConfig.name, elapsedTime));
        }

        private void _controller_CompleteEvent(string id, bool has_exception)
        {
            _workflowStateService.RaiseEndRunEvent();
            _workflowStateService.RunningOrDebuggingFile = "";

            StopwatchStop();
            
            _logService.Debug(string.Format("End running workflow file {0}", _xamlPath), _logger);
        }

        private void _controller_ExceptionEvent(string id, string title, string msg)
        {
            SharedObject.Instance.Output(SharedObject.enOutputType.Error, Chainbot.Resources.Properties.Resources.Message_ExecError, msg);

            _windowService.ShowMainWindowNormal();

            _dispatcherService.InvokeAsync(()=> {
                _messageBoxService.ShowError(title);
            });
        }

        public void Init(string xamlPath, List<string> activitiesDllLoadFrom, List<string> dependentAssemblies)
        {
            _xamlPath = xamlPath;
            _activitiesDllLoadFrom = activitiesDllLoadFrom;
            _dependentAssemblies = dependentAssemblies;
        }

        public void Run()
        {
            if (_projectManagerService.CurrentActivitiesServiceProxy.IsXamlValid(_xamlPath))
            {
                _logService.Debug(string.Format("Start running workflow file {0} ……", _xamlPath), _logger);

                _workflowStateService.RunningOrDebuggingFile = _xamlPath;
                _workflowStateService.RaiseBeginRunEvent();

                StopwatchRestart();

                var guid = ChainbotExecutorController.Guid();

                _guid = guid;

                var cfg = new ChainbotExecutorStartupConfig();
                cfg.Name = _projectManagerService.CurrentProjectJsonConfig.name;
                cfg.Version = _projectManagerService.CurrentProjectJsonConfig.projectVersion;
                cfg.MainXamlPath = _xamlPath;
                cfg.PipeName = ChainbotExecutorAgent.GetExecutorPipeName(guid);
                cfg.LoadAssemblyFromList = _activitiesDllLoadFrom;
                cfg.AssemblyResolveDllList = _dependentAssemblies;
                cfg.ProjectPath = SharedObject.Instance.ProjectPath;

                _controller.SetStartupConfig(guid, cfg);
                var arg = _controller.Start(guid);

                _logService.Debug($"+++++++++++++++++Start Process ChainbotExecutor.exe+++++++++++++++++ Command line parameters:{arg}", _logger);
            }
            else
            {
                _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_WorkflowVaildateError);
            }
        }

        public void Stop()
        {
            _controller.Stop(_guid);
            _controller_CompleteEvent(_guid, false);
        }

        public void S2CNotify(string notification, string notificationDetails = "")
        {
            _controller.S2CNotify(_guid, notification, notificationDetails);
        }
    }
}
