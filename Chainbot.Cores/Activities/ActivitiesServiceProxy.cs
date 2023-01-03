using Chainbot.Contracts.Activities;
using Chainbot.Contracts.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.AppDomains;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Plugins.Shared.Library;

namespace Chainbot.Cores.Activities
{
    public class ActivitiesServiceProxy : MarshalByRefServiceProxyBase<IActivitiesService>, IActivitiesServiceProxy
    {

        public ActivitiesServiceProxy(IAppDomainControllerService appDomainControllerService) : base(appDomainControllerService)
        {
            
        }

        public List<string> CustomActivityConfigXmls
        {
            get
            {
                return InnerService.CustomActivityConfigXmls;
            }
        }

        public string GetAssemblyQualifiedName(string typeOf)
        {
            return InnerService.GetAssemblyQualifiedName(typeOf);
        }


        public List<string> Init(List<string> assemblies)
        {
            return InnerService.Init(assemblies);
        }

        public ImageSource GetIcon(string assemblyName, string relativeResPath)
        {
            var iconStream = InnerService.GetIcon(assemblyName, relativeResPath);
            var bf = BitmapFrame.Create(iconStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            return bf;
        }

        public void SetSharedObjectInstance()
        {
            InnerService.SetSharedObjectInstance(SharedObject.Instance);
        }

        public bool IsXamlValid(string xamlPath)
        {
            return InnerService.IsXamlValid(xamlPath);
        }

        public bool IsXamlStringValid(string xamlString, string xamlPath)
        {
            return InnerService.IsXamlStringValid(xamlString, xamlPath);
        }
    }
}
