using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities.Presentation;
using Newtonsoft.Json.Linq;
using System.Activities.Debugger;
using Chainbot.Cores.Classes;
using System.Activities.Presentation.Debug;
using log4net;
using Chainbot.Contracts.Log;
using System.Activities;
using System.Activities.Presentation.View;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Config;
using System.Activities.Presentation.ViewState;
using Newtonsoft.Json;
using Plugins.Shared.Library;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowBreakpointsService : MarshalByRefServiceBase, IWorkflowBreakpointsService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;
        private IWorkflowDesignerCollectService _workflowDesignerCollectService;
        private IProjectManagerService _projectManagerService;
        private ICommonService _commonService;
        private IConstantConfigService _constantConfigService;

        private Dictionary<string, JArray> _breakpointsDict = new Dictionary<string, JArray>();

        public event EventHandler BreakPointsChangedEvent;

        public WorkflowBreakpointsService(ILogService logService, IWorkflowDesignerCollectService workflowDesignerCollectService
            ,IProjectManagerService projectManagerService, ICommonService commonService
            , IConstantConfigService constantConfigService
            )
        {
            _logService = logService;
            _workflowDesignerCollectService = workflowDesignerCollectService;
            _projectManagerService = projectManagerService;
            _commonService = commonService;
            _constantConfigService = constantConfigService;
        }


        public void SaveBreakpoints()
        {
            var projectPath = _projectManagerService.CurrentProjectPath;

            if (!string.IsNullOrEmpty(projectPath))
            {
                _logService.Debug(string.Format("Close Project{0}", projectPath), _logger);

                if (_breakpointsDict != null)
                {
                    JObject rootJsonObj = new JObject();
                    JObject projectBreakpointsjsonObj = new JObject();
                    rootJsonObj["ProjectBreakpoints"] = projectBreakpointsjsonObj;

                    JObject valueJsonObj = new JObject();
                    projectBreakpointsjsonObj["Value"] = valueJsonObj;

                    foreach (var item in _breakpointsDict)
                    {
                        JArray jarrayJsonObj = new JArray();
                        valueJsonObj[item.Key] = jarrayJsonObj;

                        foreach (JToken ji in item.Value)
                        {
                            JObject itemJsonObj = new JObject();
                            itemJsonObj["ActivityName"] = (ji as JObject)["ActivityName"].ToString();
                            itemJsonObj["ActivityId"] = (ji as JObject)["ActivityId"].ToString();
                            itemJsonObj["ActivityIdRef"] = (ji as JObject)["ActivityIdRef"].ToString();
                            itemJsonObj["IsValid"] = true;
                            itemJsonObj["IsEnabled"] = (bool)(ji as JObject)["IsEnabled"];
                            jarrayJsonObj.Add(itemJsonObj);
                        }
                    }

                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(rootJsonObj, Newtonsoft.Json.Formatting.Indented);
                    var projectSettingsjson = projectPath + $"\\{_constantConfigService.ProjectLocalDirectoryName}\\{_constantConfigService.ProjectSettingsFileName}";

                    var localDir = projectPath + @"\"+_constantConfigService.ProjectLocalDirectoryName;
                    if (!System.IO.Directory.Exists(localDir))
                    {
                        System.IO.Directory.CreateDirectory(localDir);
                        System.IO.File.SetAttributes(localDir, System.IO.FileAttributes.Hidden);
                    }

                    System.IO.File.WriteAllText(projectSettingsjson, output);
                }

            }
        }

        public void LoadBreakpoints()
        {
            var projectSettingsjson = _projectManagerService.CurrentProjectPath + $"\\{_constantConfigService.ProjectLocalDirectoryName}\\{_constantConfigService.ProjectSettingsFileName}";
            if (System.IO.File.Exists(projectSettingsjson))
            {
                string json = System.IO.File.ReadAllText(projectSettingsjson);
                JObject rootJsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json) as JObject;

                var valueJsonObj = rootJsonObj["ProjectBreakpoints"]["Value"];
                if (valueJsonObj != null)
                {
                    foreach (JProperty jp in valueJsonObj)
                    {
                        _breakpointsDict[jp.Name] = (JArray)jp.Value;
                    }
                }
            }
        }

        public void ShowBreakpoints(string path)
        {
            WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
            var relativeXamlPath = _commonService.MakeRelativePath(_projectManagerService.CurrentProjectPath, path);

            var breakpointsDict = _breakpointsDict;
            if (breakpointsDict.Count > 0)
            {
                if (breakpointsDict.ContainsKey(relativeXamlPath))
                {
                    JArray jarr = (JArray)breakpointsDict[relativeXamlPath].DeepClone();

                    foreach (JToken ji in jarr)
                    {
                        var activityId = ((JObject)ji)["ActivityId"].ToString();
                        var activityIdRef = ((JObject)ji)["ActivityIdRef"].ToString();
                        var IsEnabled = (bool)(((JObject)ji)["IsEnabled"]);

                        SetBreakpoint(path, relativeXamlPath, activityId, activityIdRef, IsEnabled);
                    }
                }
            }
        }

        public void RemoveAllBreakpoints()
        {
            string path;
            WorkflowDesigner workflowDesigner;
            foreach (var item in _breakpointsDict)
            {
                path = System.IO.Path.Combine(SharedObject.Instance.ProjectPath, item.Key);
                workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                if (workflowDesigner != null)
                {
                    workflowDesigner.DebugManagerView.ResetBreakpoints();
                }
            }

            _breakpointsDict.Clear();

            BreakPointsChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void SetBreakpoint(string path, string relativeXamlPath, string activityId, string activityIdRef, bool IsEnabled)
        {
            WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();

            try
            {
                Dictionary<string, SourceLocation> activityIdToSourceLocationMapping = new Dictionary<string, SourceLocation>();
                ChainbotDebuggerService.BuildSourceLocationMappings(workflowDesigner, ref activityIdToSourceLocationMapping);

                if (activityIdToSourceLocationMapping.ContainsKey(activityIdRef))
                {
                    SourceLocation srcLoc = activityIdToSourceLocationMapping[activityIdRef];

                    if (!activityIdToSourceLocationMapping.ContainsKey(activityId) || activityIdToSourceLocationMapping[activityId] != srcLoc)
                    {
                        var newActivityId = activityIdToSourceLocationMapping.Where(a => a.Value == srcLoc && a.Key != activityIdRef).First().Key;
                        if (!string.IsNullOrEmpty(newActivityId))
                        {
                            var jarr = _breakpointsDict[relativeXamlPath];
                            foreach (JToken ji in jarr)
                            {
                                if (((JObject)ji)["ActivityIdRef"].ToString() == activityIdRef)
                                {
                                    ((JObject)ji)["ActivityId"] = newActivityId;
                                    break;
                                }
                            }
                        }
                    }

                    if (IsEnabled)
                    {
                        workflowDesigner.DebugManagerView.InsertBreakpoint(srcLoc, BreakpointTypes.Enabled | BreakpointTypes.Bounded);
                    }
                    else
                    {
                        workflowDesigner.DebugManagerView.DeleteBreakpoint(srcLoc);
                    }
                }
                else
                {
                    RemoveBreakpointLocation(relativeXamlPath, activityIdRef);
                }
            }
            catch (Exception err)
            {
                _logService.Debug(err,_logger);
            }

        }

        private void AddBreakpointLocation(string relativeXamlPath, string activityId, string activityIdRef, string activityName, bool isEnabled)
        {
            RemoveBreakpointLocation(relativeXamlPath, activityIdRef);

            if (!_breakpointsDict.ContainsKey(relativeXamlPath))
            {
                _breakpointsDict.Add(relativeXamlPath, new JArray());
            }

            var jarr = _breakpointsDict[relativeXamlPath];
            JObject jobj = new JObject();
            jobj["ActivityName"] = activityName;
            jobj["ActivityId"] = activityId;
            jobj["ActivityIdRef"] = activityIdRef;
            jobj["IsEnabled"] = isEnabled;
            jarr.Add(jobj);

            BreakPointsChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveBreakpointLocation(string relativeXamlPath, string activityIdRef)
        {
            if (_breakpointsDict.ContainsKey(relativeXamlPath))
            {
                var jarr = _breakpointsDict[relativeXamlPath];

                foreach (JToken ji in jarr)
                {
                    if (((JObject)ji)["ActivityIdRef"].ToString() == activityIdRef)
                    {
                        ji.Remove();
                        break;
                    }
                }

                BreakPointsChangedEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ToggleBreakpoint(string path)
        {
            WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
            var relativeXamlPath = _commonService.MakeRelativePath(_projectManagerService.CurrentProjectPath, path);

            try
            {
                Activity activity = workflowDesigner.Context.Items.GetValue<Selection>().
                        PrimarySelection.GetCurrentValue() as Activity;

                if (activity != null)
                {
                    Dictionary<string, SourceLocation> activityIdToSourceLocationMapping = new Dictionary<string, SourceLocation>();
                    ChainbotDebuggerService.BuildSourceLocationMappings(workflowDesigner, ref activityIdToSourceLocationMapping);

                    string activityIdRef = ChainbotDebuggerService.GetIdRef(activity);

                    if (!activityIdToSourceLocationMapping.ContainsKey(activityIdRef))
                    {
                        return;
                    }
                    SourceLocation srcLoc = activityIdToSourceLocationMapping[activityIdRef];

                    bool bInsertBreakpoint = false;
                    var breakpointLocations = workflowDesigner.DebugManagerView.GetBreakpointLocations();
                    if (breakpointLocations.ContainsKey(srcLoc))
                    {
                        var types = breakpointLocations[srcLoc];
                        if (types != (BreakpointTypes.Enabled | BreakpointTypes.Bounded))
                        {
                            bInsertBreakpoint = true;
                        }
                    }
                    else
                    {
                        bInsertBreakpoint = true;
                    }

                    if (bInsertBreakpoint)
                    {
                        workflowDesigner.DebugManagerView.InsertBreakpoint(srcLoc, BreakpointTypes.Enabled | BreakpointTypes.Bounded);
                        AddBreakpointLocation(relativeXamlPath, activity.Id, activityIdRef, activity.DisplayName, true);
                    }
                    else
                    {
                        workflowDesigner.DebugManagerView.DeleteBreakpoint(srcLoc);
                        RemoveBreakpointLocation(relativeXamlPath, activityIdRef);
                    }
                }
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
            }
        }

        public void DeleteBreakpoint(string path, string activityIdRef)
        {
            WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
           
            try
            {
                if (!string.IsNullOrWhiteSpace(activityIdRef))
                {
                    if (workflowDesigner != null)
                    {
                        Dictionary<string, SourceLocation> activityIdToSourceLocationMapping = new Dictionary<string, SourceLocation>();
                        ChainbotDebuggerService.BuildSourceLocationMappings(workflowDesigner, ref activityIdToSourceLocationMapping);

                        if (activityIdToSourceLocationMapping.ContainsKey(activityIdRef))
                        {
                            SourceLocation srcLoc = activityIdToSourceLocationMapping[activityIdRef];

                            var breakpointLocations = workflowDesigner.DebugManagerView.GetBreakpointLocations();
                            if (breakpointLocations.ContainsKey(srcLoc))
                            {
                                var types = breakpointLocations[srcLoc];
                                if (types == (BreakpointTypes.Enabled | BreakpointTypes.Bounded))
                                {
                                    workflowDesigner.DebugManagerView.DeleteBreakpoint(srcLoc);
                                }
                            }
                        }
                    }

                    var relativeXamlPath = _commonService.MakeRelativePath(_projectManagerService.CurrentProjectPath, path);
                    RemoveBreakpointLocation(relativeXamlPath, activityIdRef);               
                }
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
            }
        }

        public string GetBreakpointSetting()
        {
            string breakpointsInfo =  JsonConvert.SerializeObject(_breakpointsDict);
            return breakpointsInfo;
        }
    }
}
