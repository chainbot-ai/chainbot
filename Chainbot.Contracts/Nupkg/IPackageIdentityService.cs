using NuGet.Packaging.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Nupkg
{
    public interface IPackageIdentityService
    {
        Task<List<string>> BuildDependentAssemblies(PackageIdentity identity, ConcurrentDictionary<string, List<string>> cachedInstalledPackagesDict);
    }
}
