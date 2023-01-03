using Chainbot.Contracts.Activities;
using Chainbot.Contracts.App;
using Chainbot.Contracts.AppDomains;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.Services;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using Chainbot.Cores.Activities;
using Chainbot.Cores.App;
using Chainbot.Cores.AppDomains;
using Chainbot.Cores.Config;
using Chainbot.Cores.Nupkg;
using Chainbot.Cores.Project;
using Chainbot.Cores.Services;
using Chainbot.Cores.UI;
using Chainbot.Cores.Utils;
using Chainbot.Cores.Workflow;
using ChainbotStudio.AppBoot;
using ChainbotStudio.DragDrop;
using ChainbotStudio.UI;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotStudio.ViewModel
{
    public class UserServiceRegistry: ServiceRegistry
    {
        public static ServiceRegistry Instance;

        public UserServiceRegistry()
        {
            Instance = this;
        }

        public override void OnRegisterServices()
        {
            _serviceLocator.RegisterTypeSingleton<IAuthorizationService, AuthorizationService>();
            _serviceLocator.RegisterTypeSingleton<IContextMenuService, ContextMenuService>();
            _serviceLocator.RegisterTypeSingleton<IWindowService, WindowService>();
            _serviceLocator.RegisterTypeSingleton<IDialogService, DialogService>();
            _serviceLocator.RegisterTypeSingleton<IDispatcherService, DispatcherService>();
            _serviceLocator.RegisterTypeSingleton<IProjectManagerService, ProjectManagerService>();
           
            _serviceLocator.RegisterTypeSingleton<IPathConfigService, PathConfigService>();
            _serviceLocator.RegisterTypeSingleton<IProjectUserConfigService, ProjectUserConfigService>();
            _serviceLocator.RegisterTypeSingleton<IConstantConfigService, ConstantConfigService>();
            _serviceLocator.RegisterTypeSingleton<ICommonService, CommonService>();
            _serviceLocator.RegisterTypeSingleton<IRecentProjectsConfigService, RecentProjectsConfigService>();
            _serviceLocator.RegisterTypeSingleton<IProjectConfigFileService, ProjectConfigFileService>();
            _serviceLocator.RegisterTypeSingleton<IServerSettingsService, ServerSettingsService>();
            _serviceLocator.RegisterTypeSingleton<IAppSettingsConfigService, AppSettingsConfigService>();
            _serviceLocator.RegisterTypeSingleton<IActivitiesChildrenOrderService, ActivitiesChildrenOrderService>();

            _serviceLocator.RegisterTypeSingleton<IUpgradeAppSettingsService, UpgradeAppSettingsService>();
            _serviceLocator.RegisterTypeSingleton<IMessageBoxService, MessageBoxWindowService>();
            //_serviceLocator.RegisterTypeSingleton<IMessageBoxService, MessageBoxService>();
            _serviceLocator.RegisterTypeSingleton<IDirectoryService, DirectoryService>();

            _serviceLocator.RegisterTypeSingleton<IPackageControlService, PackageControlService>();
            _serviceLocator.RegisterTypeSingleton<IPackageRepositoryService, PackageRepositoryService>();
            _serviceLocator.RegisterTypeSingleton<IPackageIdentityService, PackageIdentityService>();

            _serviceLocator.RegisterTypeSingleton<IWorkflowStateService, WorkflowStateService>();

            _serviceLocator.RegisterTypeSingleton<IActivityFavoritesService, ActivityFavoritesService>();
            _serviceLocator.RegisterTypeSingleton<IActivityRecentService, ActivityRecentService>();
            _serviceLocator.RegisterTypeSingleton<IActivityMountService, ActivityMountService>();
            _serviceLocator.RegisterTypeSingleton<ISystemActivityIconService, SystemActivityIconService>();

            _serviceLocator.RegisterTypeSingleton<IAppDomainControllerService, AppDomainControllerService>();
            _serviceLocator.RegisterTypeSingleton<IWorkflowDesignerCollectServiceProxy, WorkflowDesignerCollectServiceProxy>();
            _serviceLocator.RegisterTypeSingleton<IWorkflowBreakpointsServiceProxy, WorkflowBreakpointsServiceProxy>();

            _serviceLocator.RegisterTypeSingleton<IControlServerService, ControlServerService>();

            _serviceLocator.RegisterTypeSingleton<IWorkflowDebugService, WorkflowDebugService>();
            _serviceLocator.RegisterTypeSingleton<IWorkflowRunService, WorkflowRunService>();

            _serviceLocator.RegisterType<IPackageImportService, PackageImportService>();
            _serviceLocator.RegisterType<IPackageExportService, PackageExportService>();
            _serviceLocator.RegisterType<IAppDomainContainerService, AppDomainContainerService>();
            _serviceLocator.RegisterType<IWorkflowDesignerServiceProxy, WorkflowDesignerServiceProxy>();
            _serviceLocator.RegisterType<IActivitiesServiceProxy, ActivitiesServiceProxy>();
            _serviceLocator.RegisterType<IRecordingServiceProxy, RecordingServiceProxy>();
            _serviceLocator.RegisterType<IDataExtractorServiceProxy, DataExtractorServiceProxy>();

            _serviceLocator.RegisterType<IProjectDependenciesService, ProjectDependenciesService>();

            _serviceLocator.RegisterType<IDragSource, ProjectItemDragHandler>();
            _serviceLocator.RegisterType<IDropTarget, ProjectItemDropHandler>();

            _serviceLocator.RegisterType<IWorkflowDesignerJumpServiceProxy, WorkflowDesignerJumpServiceProxy>();
        }

        public override void OnRegisterViewModels()
        {
            _serviceLocator.RegisterTypeSingleton<SplashViewModel>(); 
            _serviceLocator.RegisterTypeSingleton<MainViewModel>();

            _serviceLocator.RegisterTypeSingleton<OutputViewModel>();
            _serviceLocator.RegisterTypeSingleton<DocksViewModel>();
            _serviceLocator.RegisterTypeSingleton<ProjectViewModel>();
            _serviceLocator.RegisterTypeSingleton<ActivitiesViewModel>();
            _serviceLocator.RegisterTypeSingleton<PropertyViewModel>();
            _serviceLocator.RegisterTypeSingleton<OutlineViewModel>();
            _serviceLocator.RegisterTypeSingleton<SnippetsViewModel>();
            _serviceLocator.RegisterTypeSingleton<DebugViewModel>();
            _serviceLocator.RegisterTypeSingleton<CheckUpgradeViewModel>();
            _serviceLocator.RegisterTypeSingleton<SearchViewModel>();

            _serviceLocator.RegisterType<NewProjectViewModel>();
            _serviceLocator.RegisterType<DesignerDocumentViewModel>();
            _serviceLocator.RegisterType<RecentUsedProjectItemViewModel>();
            _serviceLocator.RegisterType<ProjectRootItemViewModel>();
            _serviceLocator.RegisterType<ProjectDirItemViewModel>();
            _serviceLocator.RegisterType<ProjectFileItemViewModel>();
            _serviceLocator.RegisterType<ProjectDependRootItem>();
            _serviceLocator.RegisterType<ProjectDependItem>();
            _serviceLocator.RegisterType<SnippetItemViewModel>();
            _serviceLocator.RegisterType<MessageDetailsViewModel>();
            _serviceLocator.RegisterType<OutputListItemViewModel>();
            _serviceLocator.RegisterType<RecordingViewModel>();
            _serviceLocator.RegisterType<TrackerItemViewModel>();
            _serviceLocator.RegisterType<PublishViewModel>();
            _serviceLocator.RegisterType<ExportViewModel>();
            _serviceLocator.RegisterType<DataExtractorViewModel>();
            _serviceLocator.RegisterType<ActivityGroupItemViewModel>();
            _serviceLocator.RegisterType<ActivityLeafItemViewModel>();
            _serviceLocator.RegisterType<NewXamlFileViewModel>();
            _serviceLocator.RegisterType<RenameViewModel>();
            _serviceLocator.RegisterType<NewFolderViewModel>();
            _serviceLocator.RegisterType<MessageBoxViewModel>();
            _serviceLocator.RegisterType<ProjectSettingsViewModel>();
            _serviceLocator.RegisterType<StartPageViewModel>();
            _serviceLocator.RegisterType<ToolsPageViewModel>();
            _serviceLocator.RegisterType<SettingsPageViewModel>();
            _serviceLocator.RegisterType<HelpPageViewModel>();
            _serviceLocator.RegisterType<SearchItemViewModel>();
            _serviceLocator.RegisterType<AccountPageViewModel>();
        }

        public override void OnRegisterViews()
        {
            
        }
    }
}
