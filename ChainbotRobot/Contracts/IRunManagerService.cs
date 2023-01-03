using Plugins.Shared.Library;
using ChainbotRobot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Contracts
{
    public interface IRunManagerService
    {
        event EventHandler BeginRunEvent;
        event EventHandler EndRunEvent;

        PackageItemViewModel PackageItem { get; }

        bool HasException { get; }

        void Init(PackageItemViewModel packageItem, string xamlPath, List<string> activitiesDllLoadFrom, List<string> dependentAssemblies);
        void UpdatePackageItem(PackageItemViewModel packageItemViewModel);

        void Run();

        void Stop();

        void LogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails);

        void Log(SharedObject.enOutputType type, string msg);

        
    }
}
