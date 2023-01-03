using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Executor;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.ViewState;
using System.Activities.Statements;
using System.Activities.Tracking;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotExecutor.Librarys
{
    public class WorkflowDebuggerManager: IWorkflowManager
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ConcurrentDictionary<string, string> m_configDict = new ConcurrentDictionary<string, string>();

        private WorkflowApplication m_app;
        private string m_name;
        private string m_version;
        public string m_mainXamlPath;
        private ChainbotExecutorAgent m_agent;


        private VisualTrackingParticipant m_simTracker;

        public WorkflowDebuggerManager(ChainbotExecutorAgent agent, string name, string version, string mainXamlPath)
        {
            this.m_agent = agent;
            this.m_name = name;
            this.m_version = version;
            this.m_mainXamlPath = mainXamlPath;
        }

        public bool HasException { get; set; }


        public void Continue()
        {
            m_simTracker.m_slowStepEvent.Set();
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
                    Logger.Debug(string.Format("Start debugging workflow file {0} ……", m_mainXamlPath), logger);

                    if (m_app != null)
                    {
                        m_app.Terminate("");
                    }

                    m_app = new WorkflowApplication(workflow);

                    m_simTracker = generateTracker();
                    m_app.Extensions.Add(m_simTracker);

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


        public bool IsMeetingBreakpoint(string id)
        {
            var jarr_str = GetConfig("breakpoint_id_json_array");
            if (jarr_str != "")
            {
                JArray jarr = JArray.Parse(jarr_str);

                foreach (JToken val in jarr)
                {
                    if ((string)val == id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsMeetingBreakpoint(string id, string xamlPath)
        {
            var jarr_str = GetConfig("breakpoint_id_json_array");
            if (jarr_str != "")
            {
                Dictionary<string, JArray> breakpointsDict = JsonConvert.DeserializeObject<Dictionary<string, JArray>>(jarr_str);
                if (breakpointsDict.ContainsKey(xamlPath))
                {
                    JArray jarr = (JArray)breakpointsDict[xamlPath];

                    foreach (JToken ji in jarr)
                    {
                        var activityId = ((JObject)ji)["ActivityId"].ToString();
                        if (activityId == id)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void SetWorkflowDebuggingPaused(bool isPaused)
        {
            m_agent.SetWorkflowDebuggingPaused(isPaused);
        }

        public void ShowLocals(ActivityArgsVars activityStateRecord)
        {
            m_agent.ShowLocals(activityStateRecord);
        }

        public void HideCurrentLocation()
        {
            m_agent.HideCurrentLocation();
        }

        public void HideCurrentLocation(string workflowFilePath)
        {
            m_agent.HideCurrentLocation(workflowFilePath);
        }

        public void ShowCurrentLocation(string id)
        {
            m_agent.ShowCurrentLocation(id);
        }

        public void ShowCurrentLocation(string id, string workflowFilePath)
        {
            m_agent.ShowCurrentLocation(id, workflowFilePath);
        }

        public bool ActivityIdContains(string id)
        {
            var jarr_str = GetConfig("activity_id_json_array");
            if (jarr_str != "")
            {
                JArray jarr = JArray.Parse(jarr_str);
                foreach (JToken val in jarr)
                {
                    if((string)val == id)
                    {
                        return true;
                    }
                }
            }

            return false;
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

                    m_agent.OnException(obj.TerminationException.Message, obj.TerminationException.ToString());
                }
            }

            m_agent.OnExecutionCompleted(HasException);

            Logger.Debug(string.Format("End debugging workflow file {0}", m_mainXamlPath), logger);
        }

        public void RedirectLogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            m_agent.RedirectLogToOutputWindow(type, msg, msgDetails);
        }

        public void RedirectNotification(string notification, string notificationDetails)
        {
            m_agent.RedirectNotification(notification, notificationDetails);
        }

        private VisualTrackingParticipant generateTracker()
        {
            const String all = "*";

            VisualTrackingParticipant simTracker = new VisualTrackingParticipant(this)
            {
                TrackingProfile = new TrackingProfile()
                {
                    Name = "CustomTrackingProfile",
                    Queries =
                        {
                         new CustomTrackingQuery()
                            {
                                Name = all,
                                ActivityName = all
                            },
                            new WorkflowInstanceQuery()
                            {
                                // Limit workflow instance tracking records for started and completed workflow states
                                States = { WorkflowInstanceStates.Started, WorkflowInstanceStates.Completed },
                            },

                             new ActivityStateQuery()
                            {
                                // Subscribe for track records from all activities for all states
                                ActivityName = all,
                                States = { all },

                                // Extract workflow variables and arguments as a part of the activity tracking record
                                // VariableName = "*" allows for extraction of all variables in the scope
                                // of the activity
                                Variables =
                                {
                                     all
                                },

                                Arguments =
                                {
                                    all
                                },
                            },

                             new ActivityScheduledQuery()
                            {
                                ActivityName = all,
                                ChildActivityName = all
                            },
                        }
                }
            };

            trackerVarsAdd(simTracker);

            return simTracker;
        }

        private void trackerVarsAdd(VisualTrackingParticipant simTracker)
        {
            Collection<string> variables = null;

            foreach (var item in simTracker.TrackingProfile.Queries)
            {
                if (item is ActivityStateQuery)
                {
                    var query = item as ActivityStateQuery;

                    variables = query.Variables;
                    break;
                }
            }

            var jarr_str = GetConfig("tracker_vars");
            if (jarr_str != "")
            {
                JArray jarr = JArray.Parse(jarr_str);

                foreach (JToken val in jarr)
                {
                    variables.Add((string)val);
                }
            }

        }

        public void OnUnhandledException(string title, Exception err)
        {
            m_agent.OnException(title, err.ToString());
            m_agent.OnExecutionCompleted(true);
        }

        
    }
}
