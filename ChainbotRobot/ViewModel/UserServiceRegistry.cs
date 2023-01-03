using Chainbot.Contracts.Config;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Utils;
using Chainbot.Cores.Config;
using Chainbot.Cores.Nupkg;
using Chainbot.Cores.Utils;
using ChainbotRobot.Contracts;
using ChainbotRobot.Cores;
using ChainbotRobot.Database;
using ChainbotRobot.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.ViewModel
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
            _serviceLocator.RegisterTypeSingleton<ICommonService, CommonService>();
            _serviceLocator.RegisterTypeSingleton<IConstantConfigService, ConstantConfigService>(); 

            _serviceLocator.RegisterTypeSingleton<IAutoCloseMessageBoxService, AutoCloseMessageBoxService>();
            _serviceLocator.RegisterTypeSingleton<IControlServerService, ControlServerService>();
            _serviceLocator.RegisterTypeSingleton<IRobotPathConfigService, RobotPathConfigService>();
            _serviceLocator.RegisterTypeSingleton<IPackageControlService, PackageControlService>();
            _serviceLocator.RegisterTypeSingleton<IPackageIdentityService, PackageIdentityService>();
            _serviceLocator.RegisterTypeSingleton<IPackageService, PackageService>();
            _serviceLocator.RegisterTypeSingleton<IRunManagerService, RunManagerService>();
            _serviceLocator.RegisterTypeSingleton<ScheduledTasksDatabase>();
            _serviceLocator.RegisterTypeSingleton<IScheduledTasksService, ScheduledTasksService>();


            _serviceLocator.RegisterType<IAboutInfoService, AboutInfoService>();
            _serviceLocator.RegisterType<IFFmpegService, FFmpegService>();
            _serviceLocator.RegisterType<ILoadDependenciesService, LoadDependenciesService>();
        }

        public override void OnRegisterViewModels()
        {
            _serviceLocator.RegisterTypeSingleton<MainViewModel>();
            _serviceLocator.RegisterTypeSingleton<AboutViewModel>();
            _serviceLocator.RegisterTypeSingleton<BrowserViewModel>();
            _serviceLocator.RegisterTypeSingleton<StartupViewModel>();
            _serviceLocator.RegisterTypeSingleton<UserPreferencesViewModel>();

            _serviceLocator.RegisterTypeSingleton<ScheduledTaskManagementViewModel>();
            _serviceLocator.RegisterTypeSingleton<ScheduledTaskViewModel>();

            _serviceLocator.RegisterType<PackageItemViewModel>();
            _serviceLocator.RegisterType<ScheduledTaskItemViewModel>(); 
            _serviceLocator.RegisterType<ScheduledPackageItemViewModel>();
        }

        public override void OnRegisterViews()
        {

        }
    }
}
