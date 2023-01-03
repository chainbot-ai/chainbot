using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowDesignerMainServiceProxy
    {
        KeyedCollection<string, DynamicActivityProperty> GetArguments();

        void Init();

        void CreateArgument(string name, Type type, string val, string description);

        void Save();

        void DeleteArgument(string name);

        void RefreshArgumentsView();
    }
}
