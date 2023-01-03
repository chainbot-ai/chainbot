using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowRunService
    {
        void Init(string xamlPath, List<string> activitiesDllLoadFrom, List<string> dependentAssemblies);

        void Run();
        void Stop();

        void S2CNotify(string notification, string notificationDetails = "");
    }
}
