using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowBreakpointsService
    {
        void LoadBreakpoints();

        void SaveBreakpoints();

        void ShowBreakpoints(string path);

        void ToggleBreakpoint(string path);

        void RemoveAllBreakpoints();

        void DeleteBreakpoint(string path, string activityIdRef);

        string GetBreakpointSetting();

        event EventHandler BreakPointsChangedEvent;

    }
}
