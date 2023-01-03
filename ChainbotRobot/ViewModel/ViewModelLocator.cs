using ChainbotRobot.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.ViewModel
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


        public AboutViewModel About
        {
            get
            {
                return _locator.ResolveType<AboutViewModel>();
            }
        }

        public BrowserViewModel Browser
        {
            get
            {
                return _locator.ResolveType<BrowserViewModel>();
            }
        }



        public StartupViewModel Startup
        {
            get
            {
                return _locator.ResolveType<StartupViewModel>();
            }
        }

        public UserPreferencesViewModel UserPreferences
        {
            get
            {
                return _locator.ResolveType<UserPreferencesViewModel>();
            }
        }


        public ScheduledTaskManagementViewModel ScheduledTaskManagement
        {
            get
            {
                return _locator.ResolveType<ScheduledTaskManagementViewModel>();
            }
        }

        public ScheduledTaskViewModel ScheduledTask
        {
            get
            {
                return _locator.ResolveType<ScheduledTaskViewModel>();
            }
        }


    }
}
