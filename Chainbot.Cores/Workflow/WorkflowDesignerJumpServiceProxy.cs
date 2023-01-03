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
    public class WorkflowDesignerJumpServiceProxy : MarshalByRefServiceProxyBase<IWorkflowDesignerJumpService>, IWorkflowDesignerJumpServiceProxy
    {
        public WorkflowDesignerJumpServiceProxy(IAppDomainControllerService appDomainControllerService) : base(appDomainControllerService)
        {

        }

        public string Path
        {
            get
            {
                return InnerService.Path;
            }

            set
            {
                InnerService.Path = value;
            }
        }

        public void FocusActivity(string idRef)
        {
            InnerService.FocusActivity(idRef);
        }

        public void FocusArgument(string argumentName)
        {
            InnerService.FocusArgument(argumentName);
        }

        public void FocusVariable(string variableName, string idRef)
        {
            InnerService.FocusVariable(variableName, idRef);
        }

        public JArray GetAllActivities()
        {
            var str = InnerService.GetAllActivities();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetAllArguments()
        {
            var str = InnerService.GetAllArguments();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public JArray GetAllVariables()
        {
            var str = InnerService.GetAllVariables();

            JArray jArr = (JArray)JsonConvert.DeserializeObject<JArray>(str);
            return jArr;
        }

        public void Init(string path)
        {
            InnerService.Init(path);
        }
    }
}
