using Chainbot.Contracts.Config;
using Chainbot.Contracts.Nupkg;
using Chainbot.Cores.Config;
using Chainbot.Cores.Nupkg;
using ChainbotRobot.CommandLine.Contracts;
using ChainbotRobot.CommandLine.Cores;
using ChainbotRobot.CommandLine.Librarys;

namespace ChainbotRobot.CommandLine
{
    public class UserServiceRegistry : ServiceRegistry
    {
        public static ServiceRegistry Instance;

        public UserServiceRegistry()
        {
            Instance = this;
        }

        public override void OnRegisterServices()
        {
            _serviceLocator.RegisterTypeSingleton<IGlobalService, GlobalService>();
            _serviceLocator.RegisterTypeSingleton<IRunManagerService, RunManagerService>();
            _serviceLocator.RegisterTypeSingleton<IProjectService, ProjectService>();
            _serviceLocator.RegisterTypeSingleton<IRegisterService, RegisterService>();
            _serviceLocator.RegisterTypeSingleton<IPackageIdentityService, PackageIdentityService>();
            _serviceLocator.RegisterTypeSingleton<IPackageControlService, PackageControlService>();
            _serviceLocator.RegisterTypeSingleton<IConstantConfigService, ConstantConfigService>();

            _serviceLocator.RegisterType<ILoadDependenciesService, LoadDependenciesService>();
        }

        public override void OnRegisterViewModels()
        {
           
        }

        public override void OnRegisterViews()
        {

        }
    }
}