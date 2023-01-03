using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.AppDomains;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowDesignerCollectServiceProxy : MarshalByRefServiceProxyBase<IWorkflowDesignerCollectService>, IWorkflowDesignerCollectServiceProxy
    {
        public WorkflowDesignerCollectServiceProxy(IAppDomainControllerService appDomainControllerService) : base(appDomainControllerService)
        {
        }

        public void Remove(string path)
        {
            InnerService.Remove(path);
        }
    }
}
