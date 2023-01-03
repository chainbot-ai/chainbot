using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Executor;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities;
using System.Activities.Presentation.ViewState;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ChainbotExecutor.Librarys
{
    public class WorkflowRunManager : IWorkflowManager
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ConcurrentDictionary<string, string> m_configDict = new ConcurrentDictionary<string, string>();

        private WorkflowApplication m_app;
        private string m_name;
        private string m_version;
        private string m_mainXamlPath;
        private ChainbotExecutorAgent m_agent;

        public WorkflowRunManager(ChainbotExecutorAgent agent,string name, string version, string mainXamlPath)
        {
            this.m_agent = agent;
            this.m_name = name;
            this.m_version = version;
            this.m_mainXamlPath = mainXamlPath;
        }

        public bool HasException { get; set; }


        public static Dictionary<string, object> DeserializeArguments(DynamicActivity workflow, IDictionary<string, object> arguments)
        {
            if (workflow == null || workflow.Properties.Count == 0 || arguments == null)
            {
                return new Dictionary<string, object>();
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> keyValuePair in from a in arguments
                                                                  where a.Value != null
                                                                  select a)
            {
                string key = keyValuePair.Key;
                try
                {
                    if (workflow.Properties.Contains(key))
                    {
                        Type type = workflow.Properties[key].Type.GetGenericArguments()[0];
                        Type type2 = keyValuePair.Value.GetType();
                        if (type2 != type && !type.IsAssignableFrom(type2))
                        {
                            JToken jtoken = JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(keyValuePair.Value));
                            object value = jtoken.ToObject(type);
                            dictionary.Add(key, value);
                        }
                        else
                        {
                            dictionary.Add(key, keyValuePair.Value);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return dictionary;
        }

        public void Run()
        {
            HasException = false;

            Activity workflow = ActivityXamlServices.Load(m_mainXamlPath);

            try
            {
                var result = ActivityValidationServices.Validate(workflow);
                if (result.Errors.Count == 0)
                {
                    Logger.Debug(string.Format("Start running workflow file {0} ……", m_mainXamlPath), logger);

                    if (m_app != null)
                    {
                        m_app.Terminate("");
                    }

                    //"{'inArg':'value','integer':3}"
                    var input_args = GetConfig("input_args");

                    IDictionary<string, object> inputs = new Dictionary<string, object>();
                    if (!string.IsNullOrEmpty(input_args))
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        };

                        inputs = JsonConvert.DeserializeObject<IDictionary<string, object>>(input_args, settings);

                        inputs = DeserializeArguments(workflow as DynamicActivity, inputs);
                    }

                    m_app = new WorkflowApplication(workflow, inputs);
                    m_app.Extensions.Add(new LogToOutputWindowTextWriter());

                    if (workflow is DynamicActivity)
                    {
                        var wr = new WorkflowRuntime();
                        wr.RootActivity = workflow;
                        m_app.Extensions.Add(wr);
                    }

                    m_app.OnUnhandledException = WorkflowApplicationOnUnhandledException;
                    m_app.Completed = WorkflowApplicationExecutionCompleted;
                    m_app.Run();
                }
                else
                {
                    Logger.Debug(string.Format("Workflow verification error, please check the parameter configuration: {0} ……", m_mainXamlPath), logger);
                    m_agent.OnExecutionCompleted(true);
                }
            }
            catch (Exception err)
            {
                Logger.Error(err, logger);
                m_agent.OnExecutionCompleted(true);
            }
            
        }


        public void Stop()
        {
            if (m_app != null)
            {
                try
                {
                    m_app.Terminate("Execution is stopped actively by the user.", new TimeSpan(0, 0, 0, 5));
                }
                catch (Exception err)
                {
                    Logger.Error(err, logger);
                    Environment.Exit(-1);
                }
            }
        }

        private UnhandledExceptionAction WorkflowApplicationOnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
        {
            HasException = true;

            if (e.UnhandledException is CanJumpActivityExcption)
            {
                var ativityExcption = e.UnhandledException.GetBaseException() as CanJumpActivityExcption;
                var idRef = WorkflowViewState.GetIdRef(ativityExcption.Activity);
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, string.Format(Properties.Resources.Message1, ativityExcption.Activity.DisplayName, idRef, ativityExcption.WorkFilePath), ativityExcption.InnerException?.Message);
            }
            else
            {
                var name = e.ExceptionSource.DisplayName;
                var idRef = WorkflowViewState.GetIdRef(e.ExceptionSource);
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, string.Format(Properties.Resources.Message1, name, idRef, m_mainXamlPath), e.UnhandledException.Message);
            }

            return UnhandledExceptionAction.Terminate;
        }

        
        private void WorkflowApplicationExecutionCompleted(WorkflowApplicationCompletedEventArgs obj)
        {
            if (obj.TerminationException != null)
            {
                if (!string.IsNullOrEmpty(obj.TerminationException.Message))
                {
                    HasException = true;

                    m_agent.OnException(obj.TerminationException.Message,obj.TerminationException.ToString());
                }
            }

            m_agent.OnExecutionCompleted(HasException);

            Logger.Debug(string.Format("End running workflow file {0}", m_mainXamlPath), logger);
        }

        public void RedirectLogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            m_agent.RedirectLogToOutputWindow(type,msg, msgDetails);
        }

        public string GetConfig(string key)
        {
            if (m_configDict.ContainsKey(key))
                return m_configDict[key];
            else
                return "";
        }

        public void SetConfig(string key, string val)
        {
            Logger.Debug($"SetConfig,key={key},val={val}", logger);
            m_configDict[key] = val;
        }

        public void OnUnhandledException(string title,Exception err)
        {
            m_agent.OnException(title, err.ToString());
            m_agent.OnExecutionCompleted(true);
        }

        public void RedirectNotification(string notification, string notificationDetails)
        {
            m_agent.RedirectNotification(notification, notificationDetails);
        }
    }
}
