using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ChainbotRobot.Contracts
{
    public enum enProcessStatus
    {
        Stop,
        Start,
        Exception
    }

    public interface IControlServerService
    {
        void Register();

        Task<JArray> GetProcesses();

        Task<JObject> GetRunProcess();

        Task UpdateRunStatus(string projectName, string projectVersion, enProcessStatus status);

        Task Log(string projectName, string projectVersion, string level, string msg);
    }
}
