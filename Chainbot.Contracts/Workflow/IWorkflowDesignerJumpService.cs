using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowDesignerJumpService
    {
        string Path { get; set; }

        void Init(WorkflowDesigner designer);
        void Init(string path);

        string GetAllActivities();
        string GetAllVariables();
        string GetAllArguments();
        void FocusActivity(string idRef);
        void FocusVariable(string variableName, string idRef);
        void FocusArgument(string argumentName);
    }
}
