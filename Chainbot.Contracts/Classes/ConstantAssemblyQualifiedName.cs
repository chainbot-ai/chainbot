using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Classes
{
    public class ConstantAssemblyQualifiedName
    {
        public const string InvokeWorkflowFileActivity = "Chainbot.Core.Activities.Workflow.InvokeWorkflowFileActivity,Chainbot.Core.Activities";
        public static readonly string InvokeWorkflowFileFactory = typeof(InvokeWorkflowFileFactory).AssemblyQualifiedName;

        public const string InvokePythonFileActivity = "Chainbot.Script.Activities.Python.InvokePythonFileActivity,Chainbot.Script.Activities";
        public static readonly string InvokePythonFileFactory = typeof(InvokePythonFileFactory).AssemblyQualifiedName;

        public const string InjectJavaScriptFileActivity = "Chainbot.UIAutomation.Activities.Browser.InjectJavaScriptFileActivity,Chainbot.UIAutomation.Activities";
        public static readonly string InjectJavaScriptFileFactory = typeof(InjectJavaScriptFileFactory).AssemblyQualifiedName;

        public static readonly string InsertSnippetItemFactory = typeof(InsertSnippetItemFactory).AssemblyQualifiedName;
    }
}
