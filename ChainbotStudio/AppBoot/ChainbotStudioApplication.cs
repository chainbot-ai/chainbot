using Chainbot.Cores.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.Log;
using System.Windows;
using Chainbot.Contracts.App;
using System.Collections.ObjectModel;
using ChainbotStudio.Views;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Config;
using ChainbotStudio.ViewModel;
using Chainbot.Contracts.Classes;

namespace ChainbotStudio.AppBoot
{
    public class ChainbotStudioApplication : StudioApplication
    {
        private IServiceLocator _serviceLocator;

        public ChainbotStudioApplication(IServiceLocator serviceLocator) : base(serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public override void OnMergedDictionariesAdd(Collection<ResourceDictionary> mergedDictionaries)
        {
            ResourceDictionary resourceDictionary = new ResourceDictionary();

            if(GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
            {
                resourceDictionary.Source = new Uri("pack://application:,,,/Resources/ApplicationResources.Dark.xaml");
            }
            else if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Light)
            {
                resourceDictionary.Source = new Uri("pack://application:,,,/Resources/ApplicationResources.Light.xaml");
            }
            

            mergedDictionaries.Add(resourceDictionary);
        }


        public override Window OnSplashWindow()
        {
            return new SplashWindow();
        }
        

        public override Window OnMainWindow()
        {
            return new MainWindow();
        }

        public override void OnException()
        {
            _serviceLocator.ResolveType<MainViewModel>().IsLoading = false;
        }
    }
}
