using Plugins.Shared.Library;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Utils;
using Chainbot.Cores.Config;
using Chainbot.Cores.Log;
using Chainbot.Cores.Nupkg;
using Chainbot.Cores.Utils;
using ChainbotRobot.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Cores
{
    public class RemoteLoadDependenciesService : MarshalByRefObject
    {
        private IServiceLocator _serviceLocator = ServiceLocator.New();

        private List<string> _getCurrentActivitiesDllLoadFrom;
        private List<string> _currentDependentAssemblies;

        public RemoteLoadDependenciesService(string projectJsonFile,SharedObject soInstace)
        {
            SharedObject.SetCrossDomainInstance(soInstace);

            _serviceLocator.RegisterTypeSingleton<IConstantConfigService, ConstantConfigService>();
            _serviceLocator.RegisterTypeSingleton<ICommonService, CommonService>();
            _serviceLocator.RegisterTypeSingleton<IPackageControlService, PackageControlService>();
            _serviceLocator.RegisterTypeSingleton<IPackageRepositoryService, PackageRepositoryService>();
            _serviceLocator.RegisterTypeSingleton<IPackageIdentityService, PackageIdentityService>();

            _serviceLocator.RegisterType<ILoadDependenciesService, LoadDependenciesService>();
            _serviceLocator.RegisterTypeSingleton<ILogService, Log4NetService>();

            var serv = _serviceLocator.ResolveType<ILoadDependenciesService>();
            serv.Init(projectJsonFile);
            var t= serv.LoadDependencies();
            t.Wait();

            _getCurrentActivitiesDllLoadFrom = serv.CurrentActivitiesDllLoadFrom;
            _currentDependentAssemblies = serv.CurrentDependentAssemblies;
        }

        public List<string> GetCurrentActivitiesDllLoadFrom()
        {
            return _getCurrentActivitiesDllLoadFrom;
        }

        public List<string> GetCurrentDependentAssemblies()
        {
            return _currentDependentAssemblies;
        }
    }
}
