using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Activities.Tracking;
using log4net;
using System.Threading;
using Python.Runtime;
using NamedPipeWrapper;

namespace Plugins.Shared.Library.Executor
{
    public class ChainbotExecutorAgent
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly TimeSpan _minMessageAge = TimeSpan.FromMilliseconds(5000);

        public string _guid;

        private NamedPipeClient<string> _client;

        private string _target { get; set; }

        public delegate void StartDelegate(bool is_in_debugging_state, JObject debug_json_cfg, JObject command_line_json_cfg
            , string name, string version, string mainXamlPath, List<string> loadAssemblyFromList, List<string> assemblyResolveDllList, string projectPath, JObject json_params);
        public delegate void StopDelegate();
        public delegate void ExitDelegate();
        public delegate void UpdateAgentConfigDelegate(string id, string key, string val);
        public delegate void ContinueDelegate(string id, string next_operate);
        public delegate void S2CNotificationDelegate(string id, string notification, string notification_details);

        public event StartDelegate StartEvent;
        public event StopDelegate StopEvent;
        public event ContinueDelegate ContinueEvent;
        public event ExitDelegate ExitEvent;
        public event UpdateAgentConfigDelegate UpdateAgentConfigEvent;
        public event S2CNotificationDelegate S2CNotificationEvent;

        public bool Init(string rpaFlag,string controllerIpcName)
        {
            _target = rpaFlag;
             _client = new NamedPipeClient<string>(controllerIpcName);
            _client.Disconnected += _client_Disconnected;

            _client.ServerMessage += _client_ServerMessage;
            _client.Start();
            _client.WaitForConnection();

            return true;
        }

        public static string GetExecutorPipeName(string guid)
        {
            return $"Executor_{guid}";
        }

        public void Register(string guid)
        {
            this._guid = guid;

            var msg = new RegisterJsonMessage();
            msg.id = guid;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => RegisterJsonMessage");
        }

        private void _client_ServerMessage(NamedPipeConnection<string, string> connection, string message)
        {
             var jobj = (JObject)JsonConvert.DeserializeObject(message);

            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$receive pipe server message:" + message);

            switch (jobj["cmd"].ToString())
            {
                case "start":
                    var sjm = JsonConvert.DeserializeObject<StartJsonMessage>(message);
                    var debug_json_cfg = (JObject)JsonConvert.DeserializeObject(sjm.initial_debug_json_config);
                    var json_params = (JObject)JsonConvert.DeserializeObject(sjm.json_params);
                    var command_line_json_cfg = (JObject)JsonConvert.DeserializeObject(sjm.initial_command_line_json_config);
                    StartEvent?.Invoke(sjm.is_in_debugging_state, debug_json_cfg, command_line_json_cfg, sjm.name, sjm.version
                        , sjm.main_xaml_path, sjm.load_assembly_from_list, sjm.assembly_resolve_dll_list, sjm.project_path, json_params);
                    break;
                case "stop":
                    StopEvent?.Invoke();
                    break;
                case "continue":
                    ContinueEvent?.Invoke(_guid, jobj["next_operate"].ToString());
                    break;
                case "exit":
                    ExitEvent?.Invoke();
                    break;
                case "update_agent_config":
                    UpdateAgentConfigEvent?.Invoke(_guid, jobj["key"].ToString(), jobj["val"].ToString());
                    break;
                case "s2c_notification":
                    S2CNotificationEvent?.Invoke(_guid, jobj["notification"].ToString(), jobj["notification_details"].ToString());
                    break;
                default:
                    break;
            }
        }

        private void _client_Disconnected(NamedPipeConnection<string, string> connection)
        {
            Environment.Exit(0);
        }

        public void OnExecutionCompleted(bool hasException)
        {
            var msg = new CompleteJsonMessage();
            msg.id = _guid;
            msg.has_exception = hasException;
            Thread.Sleep(1000);
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => CompleteJsonMessage");
        }

        public void OnException(string title,string exceptionMsg)
        {
            var msg = new ExceptionJsonMessage();
            msg.id = _guid;
            msg.title = title;
            msg.msg = exceptionMsg;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => ExceptionJsonMessage");
        }

        public void RedirectLogToOutputWindow(SharedObject.enOutputType type, string msgStr, string msgDetails)
        {
            var msg = new LogJsonMessage();
            msg.id = _guid;
            msg.type = type.ToString();
            msg.msg = msgStr;
            msg.msg_details = msgDetails;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => LogJsonMessage");
        }

        public void RedirectNotification(string notification, string notificationDetails)
        {
            var msg = new NotificationJsonMessage();
            msg.id = _guid;
            msg.notification = notification;
            msg.notification_details = notificationDetails;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => NotificationJsonMessage");
        }


        public void SetWorkflowDebuggingPaused(bool isPaused)
        {
            var msg = new SetWorkflowDebuggingPausedJsonMessage();
            msg.id = _guid;
            msg.is_paused = isPaused;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => SetWorkflowDebuggingPausedJsonMessage");
        }

        public void ShowLocals(ActivityArgsVars activityArgsVars)
        {
            var msg = new ShowLocalsJsonMessage();
            msg.id = _guid;

            foreach(var item in activityArgsVars.Variables)
            {
                msg.Variables[item.Key] = item.Value;
            }

            foreach (var item in activityArgsVars.Arguments)
            {
                msg.Arguments[item.Key] = item.Value;
            }

            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => ShowLocalsJsonMessage");
        }

        public void HideCurrentLocation()
        {
            var msg = new HideCurrentLocationJsonMessage();
            msg.id = _guid;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => HideCurrentLocationJsonMessage");
        }

        public void HideCurrentLocation(string workflowFilePath)
        {
            var msg = new HideCurrentLocationJsonMessage();
            msg.id = _guid;
            msg.workflowFilePath = workflowFilePath;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => HideCurrentLocationJsonMessage");
        }

        public void ShowCurrentLocation(string locationId)
        {
            var msg = new ShowCurrentLocationJsonMessage();
            msg.id = _guid;
            msg.location_id = locationId;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => ShowCurrentLocationJsonMessage");
        }

        public void ShowCurrentLocation(string locationId, string workflowFilePath)
        {
            var msg = new ShowCurrentLocationJsonMessage();
            msg.id = _guid;
            msg.workflowFilePath = workflowFilePath;
            msg.location_id = locationId;
            Publish(msg);
            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => ShowCurrentLocationJsonMessage");
        }

        private void Publish(object value)
        {
            _client.PushMessage(JsonConvert.SerializeObject(value));
        }

    }
}
