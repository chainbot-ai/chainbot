using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowDesignerJumpServiceProxy
    {
        string Path { get; set; }

        void Init(string path);

        JArray GetAllActivities();
        JArray GetAllVariables();
        JArray GetAllArguments();
        void FocusActivity(string idRef);
        void FocusVariable(string variableName, string idRef);
        void FocusArgument(string argumentName);
    }
}
