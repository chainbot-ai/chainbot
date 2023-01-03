using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowActivityService
    {
        void RemoveTryCatchActivity(string path);
        void SurroundWithTryCatch(string path);

        bool IsActivityDisabled(string path);
        void EnableActivity(string path);
        void DisableActivity(string path);

        void StartRunFromHere(string path);

        bool CheckUnusedVariables(string path);

        bool CheckUnusedArguments(string path);

        string GetUnusedVariables(string path);

        string GetUnusedArguments(string path);

        string GetUnsetOutArgumentActivities(string path);

        string GetAbnormalActivities(string path);

        string GetErrLocationActivities(string path);

        void RemoveUnusedVariables(string path);

        void RemoveUnusedArguments(string path);

        string GetXamlValidInfo(string path);

        string GetUsedVariables(string path);

        string GetUsedArguments(string path);
    }
}
