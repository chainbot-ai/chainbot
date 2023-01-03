using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowBreakpointsServiceProxy
    {
        void LoadBreakpoints();

        void SaveBreakpoints();

        void ShowBreakpoints(string path);

        void ToggleBreakpoint(string path);

        void RemoveAllBreakpoints();

        void DeleteBreakpoint(string path, string activityIdRef);

        JObject GetBreakpointSetting();

        event EventHandler BreakPointsChangedEvent;
    }
}
