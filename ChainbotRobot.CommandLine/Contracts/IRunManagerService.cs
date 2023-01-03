using Plugins.Shared.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine.Contracts
{
    public interface IRunManagerService
    {
        event EventHandler BeginRunEvent;
        event EventHandler EndRunEvent;

        bool HasException { get; }

        void Init(string name, string version, string xamlPath
            , List<string> activitiesDllLoadFrom, List<string> dependentAssemblies);

        void Run();

        void Stop();

        void LogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails);
    }
}
