using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.AppDomains;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowBreakpointsServiceProxy : MarshalByRefServiceProxyBase<IWorkflowBreakpointsService>, IWorkflowBreakpointsServiceProxy
    {
        private IWorkflowStateService _workflowStateService;

        public event EventHandler BreakPointsChangedEvent;

        public WorkflowBreakpointsServiceProxy(IAppDomainControllerService appDomainControllerService,IWorkflowStateService workflowStateService) : base(appDomainControllerService)
        {
            _workflowStateService = workflowStateService;
        }

        public void ToggleBreakpoint(string path)
        {
            InnerService.ToggleBreakpoint(path);
            _workflowStateService.RaiseBreakpointsModifyEvent();
        }

        public void RemoveAllBreakpoints()
        {
            InnerService.RemoveAllBreakpoints();

            _workflowStateService.RaiseBreakpointsModifyEvent();
        }

        public void ShowBreakpoints(string path)
        {
            InnerService.ShowBreakpoints(path);
        }

        public void LoadBreakpoints()
        {
            InnerService.LoadBreakpoints();
        }

        public void SaveBreakpoints()
        {
            InnerService.SaveBreakpoints();
        }

        public void DeleteBreakpoint(string path, string activityIdRef)
        {
            InnerService.DeleteBreakpoint(path, activityIdRef);
            _workflowStateService.RaiseBreakpointsModifyEvent();
        }

        public JObject GetBreakpointSetting()
        {
            string breakpointsInfo = InnerService.GetBreakpointSetting();
            return JsonConvert.DeserializeObject<JObject>(breakpointsInfo);
        }

        protected override void OnAfterConnectToInnerService()
        {
            InnerService.BreakPointsChangedEvent += InnerService_BreakPointsChangedEvent;
        }

        private void InnerService_BreakPointsChangedEvent(object sender, EventArgs e)
        {
            BreakPointsChangedEvent?.Invoke(this, e);
        }
    }
}
