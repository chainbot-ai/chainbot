using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowDesignerMainService
    {
        void Init();

        void CreateArgument(string name, Type type, string val, string description);

        void Save();

        void DeleteArgument(string name);

        void RefreshArgumentsView();
    }
}
