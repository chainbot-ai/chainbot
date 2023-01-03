using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.Log;
using log4net;
using System.Threading;
using System.IO;
using NamedPipeWrapper;

namespace Plugins.Shared.Library.Executor
{
    public class ChainbotExecutorController
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly TimeSpan _minMessageAge = TimeSpan.FromMilliseconds(5000);

        private ConcurrentDictionary<string, ChainbotExecutorStartupConfig> _executorStartupConfigs = new ConcurrentDictionary<string, ChainbotExecutorStartupConfig>();
        private string _rpaFlag;

        private NamedPipeServer<string> _commandMessageServer;

        private ConcurrentDictionary<string, string> _clientsDict = new ConcurrentDictionary<string, string>();

        public string _controllerIpcName { get; set; }

        public delegate void CompleteDelegate(string id,bool has_exception);
        public delegate void ExceptionDelegate(string id,string title,string msg);
        public delegate void LogDelegate(string id, SharedObject.enOutputType type, string msg, string msgDetails);
        public delegate void NotifyDelegate(string id, string notification, string notificationDetails);
        public delegate void SetWorkflowDebuggingPausedDelegate(string id, bool is_paused);
        public delegate void HideCurrentLocationDelegate(string id, string workflowFilePath);
        public delegate void ShowCurrentLocationDelegate(string id, string location_id, string workflowFilePath);
        public delegate void ShowLocalsDelegate(string id, string json);

        public event CompleteDelegate CompleteEvent;
        public event ExceptionDelegate ExceptionEvent;
        public event LogDelegate LogEvent;
        public event NotifyDelegate NotifyEvent;

        public event SetWorkflowDebuggingPausedDelegate SetWorkflowDebuggingPausedEvent;
        public event HideCurrentLocationDelegate HideCurrentLocationEvent;
        public event ShowCurrentLocationDelegate ShowCurrentLocationEvent;
        public event ShowLocalsDelegate ShowLocalsEvent;

        public static string GetRobotSharedIpcName()
        {
            return $"ChainbotRobotServices_{Environment.MachineName}@{Environment.UserName}";
        }

        public static string GetStudioSharedIpcName()
        {
            return "ChainbotStudio.exe_"+Guid();
        }


        public bool InitStudio(bool isFromDebug)
        {
            _rpaFlag = "ChainbotStudio";
            if (_commandMessageServer != null)
            {
                _commandMessageServer.Stop();
            }

#if Chainbot_EXECUTOR_IPC_DEBUG
            var flag = isFromDebug ? "DEBUG" : "RUN";
            _controllerIpcName = "ChainbotStudio.exe_" + Guid() + $"#{flag}";
#else
           _controllerIpcName = "ChainbotStudio.exe_" + Guid();
             
#endif
            _commandMessageServer = new NamedPipeServer<string>(_controllerIpcName);
            _commandMessageServer.ClientDisconnected += _commandMessageServer_ClientDisconnected;
            _commandMessageServer.ClientMessage += _commandMessageServer_ClientMessage;
            _commandMessageServer.Start();

            return true;
        }

      
        public bool InitRobot()
        {
            _rpaFlag = "ChainbotRobot";
            if (_commandMessageServer != null)
            {
                _commandMessageServer.Stop();
            }

#if Chainbot_EXECUTOR_IPC_DEBUG
            _controllerIpcName = "ChainbotRobotServices_TEST@TEST";
#else
            _controllerIpcName = $"ChainbotRobotServices_{Environment.MachineName}@{Environment.UserName}";
#endif
            _commandMessageServer = new NamedPipeServer<string>(_controllerIpcName);
            _commandMessageServer.ClientDisconnected += _commandMessageServer_ClientDisconnected;
            _commandMessageServer.ClientMessage += _commandMessageServer_ClientMessage;
            _commandMessageServer.Start();

            return true;
        }



        public bool InitRobotCommandLine()
        {
            _rpaFlag = "ChainbotRobot.CommandLine";

            if (_commandMessageServer != null)
            {
                _commandMessageServer.Stop();
            }

            _controllerIpcName = "ChainbotRobot.CommandLine.exe_" + Guid();

            _commandMessageServer = new NamedPipeServer<string>(_controllerIpcName);
            _commandMessageServer.ClientDisconnected += _commandMessageServer_ClientDisconnected;
            _commandMessageServer.ClientMessage += _commandMessageServer_ClientMessage;
            _commandMessageServer.Start();

            return true;
        }


        private void _commandMessageServer_ClientDisconnected(NamedPipeConnection<string, string> connection)
        {
            foreach (var item in _clientsDict)
            {
                if (item.Value == connection.Name)
                {
                    string val;
                    _clientsDict.TryRemove(item.Key,out val);
                    break;
                }
            }
        }


        private void _commandMessageServer_ClientMessage(NamedPipeConnection<string, string> connection, string message)
        {
            var jobj = (JObject)JsonConvert.DeserializeObject(message);

            _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$receive pipe message=" + message);

            var guid = jobj["id"].ToString();
            switch (jobj["cmd"].ToString())
            {
                case "register":
                    if (_executorStartupConfigs.ContainsKey(guid))
                    {
                        _clientsDict[guid] = connection.Name;

                        var msg = new StartJsonMessage();
                        msg.id = guid;
                        msg.is_in_debugging_state = _executorStartupConfigs[guid].IsInDebuggingState;
                        msg.initial_debug_json_config = _executorStartupConfigs[guid].InitialDebugJsonConfig.ToString();
                        msg.initial_command_line_json_config = _executorStartupConfigs[guid].InitialCommandLineJsonConfig.ToString();
                        msg.json_params = _executorStartupConfigs[guid].JsonParams.ToString();

                        msg.name = _executorStartupConfigs[guid].Name;
                        msg.version = _executorStartupConfigs[guid].Version;
                        msg.main_xaml_path = _executorStartupConfigs[guid].MainXamlPath;
                        msg.load_assembly_from_list = _executorStartupConfigs[guid].LoadAssemblyFromList;
                        msg.assembly_resolve_dll_list = _executorStartupConfigs[guid].AssemblyResolveDllList;
                        msg.project_path = _executorStartupConfigs[guid].ProjectPath;

                        Publish(connection, msg);
                        _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => StartJsonMessage");
                    }
                    break;
                case "exception":
                    ExceptionEvent?.Invoke(guid, jobj["title"].ToString(), jobj["msg"].ToString());
                    break;
                case "complete":
                    CompleteEvent?.Invoke(guid, (bool)jobj["has_exception"]);

                    if (_executorStartupConfigs.ContainsKey(guid))
                    {
                        var msg = new ExitJsonMessage();
                        msg.id = guid;
                        Publish(connection, msg);
                        _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => ExitJsonMessage");
#if !Chainbot_EXECUTOR_IPC_DEBUG
                        ChainbotExecutorStartupConfig cfg;
                        _executorStartupConfigs.TryRemove(guid, out cfg);
#endif
                    }
                    break;
                case "log":
                    var type = (SharedObject.enOutputType)Enum.Parse(typeof(SharedObject.enOutputType), jobj["type"].ToString());
                    LogEvent?.Invoke(guid, type, jobj["msg"].ToString(), jobj["msg_details"].ToString());
                    break;
                case "notification":
                    NotifyEvent?.Invoke(guid, jobj["notification"].ToString(), jobj["notification_details"].ToString());
                    break;
                case "set_workflow_debugging_paused":
                    SetWorkflowDebuggingPausedEvent?.Invoke(guid, (bool)jobj["is_paused"]);
                    break;
                case "show_locals":
                    ShowLocalsEvent?.Invoke(guid, jobj.ToString());
                    break;
                case "hide_current_location":
                    //HideCurrentLocationEvent?.Invoke(guid);
                    HideCurrentLocationEvent?.Invoke(guid, jobj["workflowFilePath"].ToString());
                    break;
                case "show_current_location":
                    //ShowCurrentLocationEvent?.Invoke(guid, jobj["location_id"].ToString());
                    ShowCurrentLocationEvent?.Invoke(guid, jobj["location_id"].ToString(), jobj["workflowFilePath"].ToString());
                    break;
                default:
                    break;
            }
        }



        public static string Guid()
        {
#if Chainbot_EXECUTOR_IPC_DEBUG
            return "BC74C74D-7C8F-4598-9DF4-C118F043DCE8";
#else
            return System.Guid.NewGuid().ToString();
#endif
        }

        public void SetStartupConfig(string guid, ChainbotExecutorStartupConfig cfg)
        {
            _executorStartupConfigs[guid] = cfg;
        }

       
        public ChainbotExecutorStartupConfig GetStartupConfig(string guid)
        {
            return _executorStartupConfigs[guid];
        }

        private NamedPipeConnection<string, string> GetConnectionById(string id)
        {
            foreach (var conn in _commandMessageServer._connections)
            {
                if (conn.Name == _clientsDict[id])
                {
                    return conn;
                }
            }

            return null;
        }

        public void UpdateAgentConfig(string guid, string key, string val)
        {
            try
            {
                if (_executorStartupConfigs.ContainsKey(guid))
                {
                    var pipe = _executorStartupConfigs[guid].PipeName;

                    var connection = GetConnectionById(guid);
                    var msg = new UpdateAgentConfigJsonMessage();
                    msg.key = key;
                    msg.val = val;
                    Publish(connection, msg);

                    _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => UpdateAgentConfigJsonMessage");
                }
            }
            catch (Exception)
            {

                
            }
            
        }

        public string Start(string guid)
        {
            var rpaFlag = _rpaFlag;
            var arguments = $"{guid} {rpaFlag} {_controllerIpcName}";
            using (Process process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory,"ChainbotExecutor.exe"),
                    Arguments = arguments
                }
            })
            {
                process.Start();

                ChildProcessTracker.AddProcess(process);
            }

            return arguments;
        }


        public void Stop(string guid)
        {
            try
            {
                if (_executorStartupConfigs.ContainsKey(guid))
                {
                    var connection = GetConnectionById(guid);
                    var msg = new StopJsonMessage();
                    Publish(connection, msg);
                    _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => StopJsonMessage");
                }
            }
            catch (Exception)
            {

            }
            
        }

        public void Continue(string guid, string opStr)
        {
            try
            {
                if (_executorStartupConfigs.ContainsKey(guid))
                {
                    var connection = GetConnectionById(guid);

                    var msg = new ContinueJsonMessage();
                    msg.next_operate = opStr;
                    Publish(connection, msg);
                    _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => ContinueJsonMessage");
                }
            }
            catch (Exception)
            {

            }
            
        }


        public void S2CNotify(string guid, string notification, string notificationDetails)
        {
            try
            {
                if (_executorStartupConfigs.ContainsKey(guid))
                {
                    var connection = GetConnectionById(guid);

                    var msg = new S2CNotifyJsonMessage();
                    msg.notification = notification;
                    msg.notification_details = notificationDetails;
                    Publish(connection, msg);
                    _logger.Debug("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ => S2CNotifyJsonMessage");
                }
            }
            catch (Exception)
            {

            }
        }



        private void Publish(NamedPipeConnection<string, string> connection, object value)
        {
            connection?.PushMessage(JsonConvert.SerializeObject(value));
        }



    }
}
