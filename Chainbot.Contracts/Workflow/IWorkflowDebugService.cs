using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Workflow
{
    public enum enSpeed
    {
        Off,//close
        One,//1x
        Two,//2x
        Three,//3x
        Four//4x
    }

    public enum enOperate
    {
        Null,
        StepInto,
        StepOver,
        Continue,
        Break,
        Stop,
    }

    public interface IWorkflowDebugService
    {
        void Init(IWorkflowDesignerServiceProxy workflowDesignerServiceProxy, string xamlPath, List<string> activitiesDllLoadFrom, List<string> dependentAssemblies);

        void Debug();
        void Stop();
        void Stop(string workflowFilePath);

        void S2CNotify(string notification, string notificationDetails = "");
        

        void SetSpeed(enSpeed speedType);
        void Break();
        void Continue(enOperate operate = enOperate.Continue);
        void SetNextOperate(enOperate operate);
    }
}
