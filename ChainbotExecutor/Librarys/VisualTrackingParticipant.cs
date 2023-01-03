using log4net;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Executor;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;

namespace ChainbotExecutor.Librarys
{
    public class VisualTrackingParticipant : TrackingParticipant
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private WorkflowDebuggerManager workflowDebuggerManager;

       
        public ManualResetEvent m_slowStepEvent = new ManualResetEvent(false);

        public ActivityScheduledRecord m_lastDebugActivityScheduledRecord { get; set; }

       // public ActivityStateRecord m_lastActivityStateRecord { get; set; }
        public ActivityArgsVars m_lastActivityArgsVars;

        public string WorkflowFilePath;

        public Dictionary<string, string> m_activityIdParentMap = new Dictionary<string, string>();//child id => parent id

        public VisualTrackingParticipant(WorkflowDebuggerManager workflowDebuggerManager)
        {
            this.workflowDebuggerManager = workflowDebuggerManager;
            this.WorkflowFilePath = Common.MakeRelativePath(SharedObject.Instance.ProjectPath, workflowDebuggerManager.m_mainXamlPath);
            SharedObject.Instance.VisualTrackingParticipant = this;
        }


        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            OnTrackingRecordReceived(record, timeout);
        }

        private bool MeetingBreakpoint(ActivityInfo child)
        {
            //return workflowDebuggerManager.IsMeetingBreakpoint(child.Id);
            return workflowDebuggerManager.IsMeetingBreakpoint(child.Id, this.WorkflowFilePath);
        }

        private bool activityCurrentOrParentIdExists(string id1, string id2)
        {
            if (id1 == id2)
            {
                return true;
            }

            if (m_activityIdParentMap.ContainsKey(id1))
            {
                return activityCurrentOrParentIdExists(m_activityIdParentMap[id1], id2);
            }

            return false;
        }

        private void doWaitThings(string id)
        {
            showCurrentLocation(id);
            showLocals();
        }

        private void processSlowStep(string id)
        {
            var speed_ms = 0;
            try
            {
                speed_ms = Convert.ToInt32(workflowDebuggerManager.GetConfig("slow_step_speed_ms"));
            }
            catch(Exception)
            {

            }
            
            if (speed_ms > 0)
            {
                doWaitThings(id);
                m_slowStepEvent.WaitOne(speed_ms);
                m_slowStepEvent.Reset();
            }
        }

        private void processWait(string id)
        {
            bool isPaused = !m_slowStepEvent.WaitOne(0);

            workflowDebuggerManager.SetWorkflowDebuggingPaused(isPaused);

            if (isPaused)
            {
                doWaitThings(id);
            }

            m_slowStepEvent.WaitOne();
            m_slowStepEvent.Reset();

            workflowDebuggerManager.SetWorkflowDebuggingPaused(false);

            hideCurrentLocation();
        }

        private void showLocals()
        {
            workflowDebuggerManager.ShowLocals(m_lastActivityArgsVars);
        }

        private void hideCurrentLocation()
        {
            var nextOperateStr = this.workflowDebuggerManager.GetConfig("next_operate");
            if (nextOperateStr == "Stop")
            {
                return;
            }

            //workflowDebuggerManager.HideCurrentLocation();
            workflowDebuggerManager.HideCurrentLocation(this.WorkflowFilePath);
        }

        private void showCurrentLocation(string id)
        {
            var nextOperateStr = this.workflowDebuggerManager.GetConfig("next_operate");
            if (nextOperateStr == "Stop")
            {
                return;
            }

            //workflowDebuggerManager.ShowCurrentLocation(id);
            workflowDebuggerManager.ShowCurrentLocation(id, this.WorkflowFilePath);
        }

        protected void OnTrackingRecordReceived(TrackingRecord record, TimeSpan timeout)
        {
            //Logger.Debug("OnTrackingRecordReceived=>"+ record.ToString(), logger);

            try
            {
                var nextOperateStr = this.workflowDebuggerManager.GetConfig("next_operate");
                var is_log_activities = this.workflowDebuggerManager.GetConfig("is_log_activities") == "true";

                //Logger.Debug("next_operate => " + nextOperateStr, logger);

                if (record is WorkflowInstanceRecord)
                {

                }
                else if (record is ActivityScheduledRecord)
                {
                    var activityScheduledRecord = record as ActivityScheduledRecord;

                    //if (activityScheduledRecord.Child != null && this.workflowDebuggerManager.ActivityIdContains(activityScheduledRecord.Child.Id))
                    if (activityScheduledRecord.Child != null && activityScheduledRecord.Activity != null)
                    {
                        m_activityIdParentMap[activityScheduledRecord.Child.Id] = activityScheduledRecord.Activity.Id;

                        if (MeetingBreakpoint(activityScheduledRecord.Child))
                        {
                            m_slowStepEvent.Reset();
                            processWait(activityScheduledRecord.Child.Id);
                            m_lastDebugActivityScheduledRecord = activityScheduledRecord;
                        }
                        else
                        {
                            if (nextOperateStr == "Null"
                            || nextOperateStr == "Continue"
                            )
                            {
                                processSlowStep(activityScheduledRecord.Child.Id);
                            }
                            else if (nextOperateStr == "Break")
                            {
                                m_slowStepEvent.Reset();
                                processWait(activityScheduledRecord.Child.Id);
                                m_lastDebugActivityScheduledRecord = activityScheduledRecord;
                            }
                            else if (nextOperateStr == "StepInto")
                            {
                                m_slowStepEvent.Reset();
                                processWait(activityScheduledRecord.Child.Id);
                                m_lastDebugActivityScheduledRecord = activityScheduledRecord;
                            }
                            else if (nextOperateStr == "StepOver")
                            {
                                if (m_lastDebugActivityScheduledRecord != null)
                                {
                                    if (activityCurrentOrParentIdExists(activityScheduledRecord.Activity.Id, m_lastDebugActivityScheduledRecord.Child.Id))
                                    {
                                        workflowDebuggerManager.SetWorkflowDebuggingPaused(false);
                                    }
                                    else
                                    {
                                        m_slowStepEvent.Reset();
                                        processWait(activityScheduledRecord.Child.Id);
                                        m_lastDebugActivityScheduledRecord = activityScheduledRecord;
                                    }
                                }
                                else
                                {
                                    m_slowStepEvent.Reset();
                                    processWait(activityScheduledRecord.Child.Id);
                                    m_lastDebugActivityScheduledRecord = activityScheduledRecord;
                                }

                            }
                        }
                    }


                }
                else if (record is ActivityStateRecord)
                {
                    var activityStateRecord = record as ActivityStateRecord;

                    if (activityStateRecord.State == ActivityStates.Closed
                         && (activityStateRecord.Activity.TypeName == "System.Activities.Statements.Sequence"
                             || activityStateRecord.Activity.TypeName == "System.Activities.Statements.Flowchart"
                            )
                        && (nextOperateStr == "Null"
                            || nextOperateStr == "Continue"
                            )
                        )
                    {
                        processSlowStep(activityStateRecord.Activity.Id);
                    }

                    if (activityStateRecord.State == ActivityStates.Closed
                        && (
                        activityStateRecord.Activity.TypeName == "System.Activities.Statements.Sequence"
                        || activityStateRecord.Activity.TypeName == "System.Activities.Statements.Flowchart"
                        )
                        && (
                        nextOperateStr == "StepInto"
                        || nextOperateStr == "StepOver"
                             )
                        )
                    {
                        if (!activityCurrentOrParentIdExists(activityStateRecord.Activity.Id, m_lastDebugActivityScheduledRecord.Child.Id))
                        {
                            m_slowStepEvent.Reset();
                            processWait(activityStateRecord.Activity.Id);
                        }
                    }

                    if (is_log_activities)
                    {
                        var name = activityStateRecord.Activity.Name;

                        dynamic activityObj = new ReflectionObject(activityStateRecord.Activity);
                        var activity = activityObj.Activity;

                        if (activityStateRecord.Activity.TypeName == "System.Activities.DynamicActivity")
                        {
                            name = (activity as DynamicActivity).Name;
                        }
                        else
                        {
                            name = activity.DisplayName;
                        }

                        SharedObject.Instance.Output(SharedObject.enOutputType.Trace, string.Format("{0} {1}", name, activityStateRecord.State));
                    }

                    //m_lastActivityStateRecord = activityStateRecord;
                    m_lastActivityArgsVars = new ActivityArgsVars(activityStateRecord);

                    if (activityStateRecord.Variables.Keys.Count > 0 && SharedObject.Instance.IsBeginDebugChildWorkflow)
                    {
                        Collection<string> variables = null;

                        foreach (var item in this.TrackingProfile.Queries)
                        {
                            if (item is ActivityStateQuery)
                            {
                                var query = item as ActivityStateQuery;

                                variables = query.Variables;
                                break;
                            }
                        }

                        foreach (var item in activityStateRecord.Variables.Keys)
                        {
                            if (!variables.Contains(item))
                                variables.Add(item);
                        }

                        SharedObject.Instance.IsBeginDebugChildWorkflow = false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Debug(string.Format("Tracker exception: {0}", e), logger);
            }     
        }
    }
}