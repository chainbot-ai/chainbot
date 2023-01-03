using System.Activities;

namespace Plugins.Shared.Library.Extensions
{
    public interface IWorkflowRuntime
    {
        Activity GetRootActivity();
    }
}