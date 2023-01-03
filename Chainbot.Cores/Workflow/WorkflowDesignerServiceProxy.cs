using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.AppDomains;
using System.Windows;
using System.AddIn.Pipeline;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowDesignerServiceProxy : MarshalByRefServiceProxyBase<IWorkflowDesignerService>, IWorkflowDesignerServiceProxy
    {
        public string XamlText
        {
            get
            {
                return InnerService.XamlText;
            }
        }

        public event EventHandler ModelChangedEvent;
        public event EventHandler CanExecuteChanged;
        public event EventHandler<string> ModelAddedEvent;

        public WorkflowDesignerServiceProxy(IAppDomainControllerService appDomainControllerService) : base(appDomainControllerService)
        {

        }

        public void Init(string path)
        {
            InnerService.Init(path);
        }

        public void UpdatePath(string path)
        {
            InnerService.UpdatePath(path);
        }

        public FrameworkElement GetDesignerView()
        {
            return FrameworkElementAdapters.ContractToViewAdapter(InnerService.GetDesignerView());
        }

        public FrameworkElement GetPropertyView()
        {
            return FrameworkElementAdapters.ContractToViewAdapter(InnerService.GetPropertyView());
        }

        public FrameworkElement GetOutlineView()
        {
            return FrameworkElementAdapters.ContractToViewAdapter(InnerService.GetOutlineView());
        }

        protected override void OnAfterConnectToInnerService()
        {
            InnerService.ModelChangedEvent += InnerService_ModelChangedEvent;
            InnerService.CanExecuteChanged += InnerService_CanExecuteChanged;
            InnerService.ModelAddedEvent += InnerService_ModelAddedEvent;
        }

        private void InnerService_ModelAddedEvent(object sender, string e)
        {
            ModelAddedEvent?.Invoke(this, e);
        }

        private void InnerService_CanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(this,e);
        }

        private void InnerService_ModelChangedEvent(object sender, EventArgs e)
        {
            ModelChangedEvent?.Invoke(this, e);
        }

        public void Save()
        {
            InnerService.Save();
        }

        public void FlushDesigner()
        {
            InnerService.FlushDesigner();
        }

        public bool CanUndo()
        {
            return InnerService.CanUndo();
        }

        public bool CanRedo()
        {
            return InnerService.CanRedo();
        }

        public bool CanCut()
        {
            return InnerService.CanCut();
        }

        public bool CanCopy()
        {
            return InnerService.CanCopy();
        }

        public bool CanPaste()
        {
            return InnerService.CanPaste();
        }

        public bool CanDelete()
        {
            return InnerService.CanDelete();
        }


        public void Undo()
        {
            InnerService.Undo();
        }

        public void Redo()
        {
            InnerService.Redo();
        }


        public void Cut()
        {
            InnerService.Cut();
        }

        public void Copy()
        {
            InnerService.Copy();
        }

        public void Paste()
        {
            InnerService.Paste();
        }

        public void Delete()
        {
            InnerService.Delete();
        }

        public void ShowCurrentLocation(string locationId)
        {
            InnerService.ShowCurrentLocation(locationId);
        }

        public void ShowCurrentLocation(string locationId, string workflowFilePath)
        {
            InnerService.ShowCurrentLocation(locationId, workflowFilePath);
        }

        public void HideCurrentLocation()
        {
            InnerService.HideCurrentLocation();
        }

        public void HideCurrentLocation(string workflowFilePath)
        {
            InnerService.HideCurrentLocation(workflowFilePath);
        }

        public string GetActivityIdJsonArray()
        {
            return InnerService.GetActivityIdJsonArray();
        }

        public string GetBreakpointIdJsonArray()
        {
            return InnerService.GetBreakpointIdJsonArray();
        }

        public string GetTrackerVars()
        {
            return InnerService.GetTrackerVars();
        }

        public void ShowBreakpoints()
        {
            InnerService.ShowBreakpoints();
        }

        public void SetReadOnly(bool isReadOnly)
        {
            InnerService.SetReadOnly(isReadOnly);
        }

        public void RefreshArgumentsView()
        {
            InnerService.RefreshArgumentsView();
        }

        public void UpdateCurrentSelecteddDesigner()
        {
            InnerService.UpdateCurrentSelecteddDesigner();
        }

        public JArray GetAllActivities()
        {
            var str = InnerService.GetAllActivities();
            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetAllVariables()
        {
            var str = InnerService.GetAllVariables();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetAllArguments()
        {
            var str = InnerService.GetAllArguments();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public void FocusActivity(string idRef)
        {
            InnerService.FocusActivity(idRef);
        }

        public void FocusVariable(string variableName, string idRef)
        {
            InnerService.FocusVariable(variableName,idRef);
        }

        public void FocusArgument(string argumentName)
        {
            InnerService.FocusArgument(argumentName);
        }

        public bool CheckUnusedVariables()
        {
            return InnerService.CheckUnusedVariables();
        }

        public bool CheckUnusedArguments()
        {
            return InnerService.CheckUnusedArguments();
        }

        public JArray GetUnusedVariables()
        {
            var str = InnerService.GetUnusedVariables();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetUnusedArguments()
        {
            var str = InnerService.GetUnusedArguments();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetUsedVariables()
        {
            var str = InnerService.GetUsedVariables();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetUsedArguments()
        {
            var str = InnerService.GetUsedArguments();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetUnsetOutArgumentActivities()
        {
            var str = InnerService.GetUnsetOutArgumentActivities();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetAbnormalActivities()
        {
            var str = InnerService.GetAbnormalActivities();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetErrLocationActivities()
        {
            var str = InnerService.GetErrLocationActivities();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public void RemoveUnusedVariables()
        {
            InnerService.RemoveUnusedVariables();
        }

        public void RemoveUnusedArguments()
        {
            InnerService.RemoveUnusedArguments();
        }

        public JArray GetXamlValidInfo()
        {
            var str = InnerService.GetXamlValidInfo();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public void InsertActivity(string name, string assemblyQualifiedName)
        {
            InnerService.InsertActivity(name,assemblyQualifiedName);
        }

        public void ResetZoom()
        {
            InnerService.ResetZoom();
        }

        public void ZoomIn()
        {
            InnerService.ZoomIn();
        }

        public void ZoomOut()
        {
            InnerService.ZoomOut();
        }

        public void ToggleMiniMap()
        {
            InnerService.ToggleMiniMap();
        }

        public void FitToScreen()
        {
            InnerService.FitToScreen();
        }

        public void ExpandAll()
        {
            InnerService.ExpandAll();
        }

        public void CollapseAl()
        {
            InnerService.CollapseAl();
        }

        public void ToggleVariableDesigner()
        {
            InnerService.ToggleVariableDesigner();
        }

        public void ToggleArgumentDesigne()
        {
            InnerService.ToggleArgumentDesigne();
        }

        public void ToggleImportsDesigner()
        {
            InnerService.ToggleImportsDesigner();
        }
    }
}
