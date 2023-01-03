using Chainbot.Contracts.App;
using Chainbot.Contracts.AppDomains;
using Chainbot.Cores.AppDomains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.AppDomains
{
    public class AppDomainControllerService : IAppDomainControllerService
    {
        public event EventHandler ChildAppDomainCreated;
        public event EventHandler ChildAppDomainUnloading;

        private IServiceLocator _serviceLocator;
        private IAppDomainContainerService  _appDomainContainerService;

        public AppDomainControllerService(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public async Task CreateAppDomain()
        {
            if (_appDomainContainerService != null)
            {
                throw new Exception(Chainbot.Resources.Properties.Resources.Message_UnhandledException7);
            }

            _appDomainContainerService = _serviceLocator.ResolveType<IAppDomainContainerService>();

            _appDomainContainerService.CreateDomain();
            await _appDomainContainerService.CreateHost();

            ChildAppDomainCreated?.Invoke(this, EventArgs.Empty);
        }

        public void UnloadAppDomain()
        {
            ChildAppDomainUnloading?.Invoke(this, EventArgs.Empty);

            _appDomainContainerService.UnloadDomain();
            _appDomainContainerService = null;

            
        }

        public TService GetHostService<TService>()
        {
            return _appDomainContainerService.GetHostService<TService>();
        }
    }
}
