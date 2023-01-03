using Chainbot.Contracts.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine.Contracts
{
    public interface ILoadDependenciesService
    {
        string CurrentProjectJsonFile { get; }

        void Init(string projectJsonFile);

        Task LoadDependencies();

        ProjectJsonConfig ProcessProjectJsonConfig();

        List<string> CurrentActivitiesDllLoadFrom { get; }

        List<string> CurrentDependentAssemblies { get; }
    }
}
