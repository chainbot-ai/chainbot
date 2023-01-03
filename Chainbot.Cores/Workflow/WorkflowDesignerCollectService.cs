using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowDesignerCollectService : MarshalByRefServiceBase, IWorkflowDesignerCollectService
    {
        private List<IWorkflowDesignerService> _workflowDesignerServiceList = new List<IWorkflowDesignerService>();

        public void Add(IWorkflowDesignerService workflowDesignerService)
        {
            _workflowDesignerServiceList.Add(workflowDesignerService);
        }

        public IWorkflowDesignerService Get(string path)
        {
            foreach (var item in _workflowDesignerServiceList)
            {
                if(item.Path == path)
                {
                    return item;
                }
            }

            return null;
        }

        public void Remove(string path)
        {
            foreach(var item in _workflowDesignerServiceList)
            {
                if(item.Path == path)
                {
                    _workflowDesignerServiceList.Remove(item);
                    break;
                }
            }
        }
    }
}
