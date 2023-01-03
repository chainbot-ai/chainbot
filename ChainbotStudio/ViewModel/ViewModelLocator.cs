using ChainbotStudio.AppBoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotStudio.ViewModel
{
    public class ViewModelLocator
    {
        private ServiceRegistry _locator;

        public ViewModelLocator()
        {
            _locator = UserServiceRegistry.Instance;
        }


        public MainViewModel Main
        {
            get
            {
                return _locator.ResolveType<MainViewModel>();
            }
        }

        
        public OutputViewModel Output
        {
            get
            {
                return _locator.ResolveType<OutputViewModel>();
            }
        }


        public DocksViewModel Docks
        {
            get
            {
                return _locator.ResolveType<DocksViewModel>();
            }
        }

        public NewProjectViewModel NewProject
        {
            get
            {
                return _locator.ResolveType<NewProjectViewModel>();
            }
        }

        public SplashViewModel Splash
        {
            get
            {
                return _locator.ResolveType<SplashViewModel>();
            }
        }

        public ProjectViewModel Project
        {
            get
            {
                return _locator.ResolveType<ProjectViewModel>();
            }
        }

        public ActivitiesViewModel Activities
        {
            get
            {
                return _locator.ResolveType<ActivitiesViewModel>();
            }
        }


        public PropertyViewModel Property
        {
            get
            {
                return _locator.ResolveType<PropertyViewModel>();
            }
        }

        public OutlineViewModel Outline
        {
            get
            {
                return _locator.ResolveType<OutlineViewModel>();
            }
        }

        public SnippetsViewModel Snippets
        {
            get
            {
                return _locator.ResolveType<SnippetsViewModel>();
            }
        }
        

        public MessageDetailsViewModel MessageDetails
        {
            get
            {
                return _locator.ResolveType<MessageDetailsViewModel>();
            }
        }

        public RecordingViewModel Recording
        {
            get
            {
                return _locator.ResolveType<RecordingViewModel>();
            }
        }

        public DebugViewModel Debug
        {
            get
            {
                return _locator.ResolveType<DebugViewModel>();
            }
        }

        public PublishViewModel Publish
        {
            get
            {
                return _locator.ResolveType<PublishViewModel>();
            }
        }

        public ExportViewModel Export
        {
            get
            {
                return _locator.ResolveType<ExportViewModel>();
            }
        }

        public DataExtractorViewModel DataExtractor
        {
            get
            {
                return _locator.ResolveType<DataExtractorViewModel>();
            }
        }

        public NewXamlFileViewModel NewXamlFile
        {
            get
            {
                return _locator.ResolveType<NewXamlFileViewModel>();
            }
        }


        public RenameViewModel Rename
        {
            get
            {
                return _locator.ResolveType<RenameViewModel>();
            }
        }

        public NewFolderViewModel NewFolder
        {
            get
            {
                return _locator.ResolveType<NewFolderViewModel>();
            }
        }


        public MessageBoxViewModel MessageBox
        {
            get
            {
                return _locator.ResolveType<MessageBoxViewModel>();
            }
        }

        public ProjectSettingsViewModel ProjectSettings
        {
            get
            {
                return _locator.ResolveType<ProjectSettingsViewModel>();
            }
        }



        public StartPageViewModel StartPage
        {
            get
            {
                return _locator.ResolveType<StartPageViewModel>();
            }
        }

        public ToolsPageViewModel ToolsPage
        {
            get
            {
                return _locator.ResolveType<ToolsPageViewModel>();
            }
        }

        public SettingsPageViewModel SettingsPage
        {
            get
            {
                return _locator.ResolveType<SettingsPageViewModel>();
            }
        }

        public HelpPageViewModel HelpPage
        {
            get
            {
                return _locator.ResolveType<HelpPageViewModel>();
            }
        }

        public CheckUpgradeViewModel CheckUpgrade
        {
            get
            {
                return _locator.ResolveType<CheckUpgradeViewModel>();
            }
        }

        public SearchViewModel Search
        {
            get
            {
                return _locator.ResolveType<SearchViewModel>();
            }
        }

        public AccountPageViewModel AccountPage
        {
            get
            {
                return _locator.ResolveType<AccountPageViewModel>();
            }
        }
    }
}
