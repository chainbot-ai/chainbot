using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowDesignerCollectService
    {
        void Add(IWorkflowDesignerService workflowDesignerService);
        void Remove(string path);

        IWorkflowDesignerService Get(string path);
    }
}
