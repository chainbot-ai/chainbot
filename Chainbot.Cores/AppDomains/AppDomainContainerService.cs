using Chainbot.Contracts.App;
using Chainbot.Contracts.AppDomains;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Workflow;
using log4net;
using Plugins.Shared.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.AppDomains
{
    public class AppDomainContainerService : IAppDomainContainerService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IServiceLocator _serviceLocator;

        private string _appDomainName = "WorkflowDomain[" + Guid.NewGuid().ToString() + "]";
        private AppDomain _appDomain;
        private AppDomainServiceHost _appDomainServiceHost;
        private IMessageBoxService _messageBoxService;
        private ILogService _logService;

        public AppDomainContainerService(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
            _logService = _serviceLocator.ResolveType<ILogService>();
        }

        public void CreateDomain()
        {
            string baseDirectory = SharedObject.Instance.ApplicationCurrentDirectory;
            AppDomainSetup info = new AppDomainSetup
            {
                LoaderOptimization = LoaderOptimization.MultiDomain,
                ApplicationBase = baseDirectory,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
            };

            _appDomain = AppDomain.CreateDomain(_appDomainName, AppDomain.CurrentDomain.Evidence, info, AppDomain.CurrentDomain.PermissionSet
                , Array.Empty<StrongName>());
        }

        public void UnloadDomain()
        {
            try
            {
                AppDomain.Unload(_appDomain);
            }
            catch (Exception err)
            {
                _logService.Error(err, _logger);
            }
            
        }

        private async Task<T> Create<T>()
        {
            return await Task.Run(() =>
            {
                Type typeFromHandle = typeof(T);
                if (!typeof(MarshalByRefServiceBase).IsAssignableFrom(typeFromHandle))
                {
                    throw new InvalidOperationException(string.Format("{0} must inherit from MarshalByRefServiceBase", typeFromHandle));
                }
                T result = (T)this._appDomain.CreateInstanceAndUnwrap(typeFromHandle.Assembly.FullName, typeFromHandle.FullName);
                return result;
            });
        }


        public void RegisterCrossDomainInstance()
        {
            _appDomainServiceHost.RegisterCrossDomainInstance(_serviceLocator.ResolveType<IConstantConfigService>());
            _appDomainServiceHost.RegisterCrossDomainInstance(_serviceLocator.ResolveType<IPathConfigService>());
            _appDomainServiceHost.RegisterCrossDomainInstance(_serviceLocator.ResolveType<IProjectManagerService>());
            _appDomainServiceHost.RegisterCrossDomainInstance(_serviceLocator.ResolveType<IWorkflowStateService>());
            _appDomainServiceHost.RegisterCrossDomainInstance(_serviceLocator.ResolveType<IAppSettingsConfigService>());
            _appDomainServiceHost.RegisterCrossDomainInstance(_serviceLocator.ResolveType<IServerSettingsService>());
        }

        public async Task CreateHost()
        {
            _appDomainServiceHost = await Create<AppDomainServiceHost>();

            _appDomainServiceHost.Init();
            RegisterCrossDomainInstance();

            _appDomainServiceHost.RegisterServices();

            _appDomainServiceHost.StartAssemblyResolve();

            _appDomainServiceHost.AfterInit();
        }

        public TService GetHostService<TService>()
        {
            return _appDomainServiceHost.GetService<TService>();
        }
    }
}
