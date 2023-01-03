using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.AppDomains
{
    public interface IAppDomainContainerService
    {
        void CreateDomain();
        void UnloadDomain();
        Task CreateHost();

        TService GetHostService<TService>();
    }
}
