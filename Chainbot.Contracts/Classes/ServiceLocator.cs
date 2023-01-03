using Chainbot.Contracts.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Classes
{
    public class ServiceLocator
    {
        public static IServiceLocator New()
        {
            return new ServiceLocatorAutofac();
        }
    }
}
