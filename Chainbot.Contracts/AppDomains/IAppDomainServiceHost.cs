using Chainbot.Contracts.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.AppDomains
{
    public interface IAppDomainServiceHost
    {
        TService GetService<TService>();

        void RegisterCrossDomainInstance<TService>(TService instance) where TService : class;

        void RegisterServices();

        void StartAssemblyResolve();
    }
}
