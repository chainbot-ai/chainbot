using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Chainbot.Contracts.Nupkg
{
    public interface IPackageControlService
    {
        ILogger Logger { get; }

        NuGetFramework NuGetFramework { get; }

        NuGet.Configuration.ISettings Settings { get; }

        NuGet.Configuration.ISettings UserSettings { get; }

        SourceRepositoryProvider SourceRepositoryProvider { get; }

        SourceRepositoryProvider DefaultSourceRepositoryProvider { get; }

        SourceRepositoryProvider UserDefineSourceRepositoryProvider { get; }

        string GlobalPackagesFolder { get; }

        string PackagesInstallFolder { get; }

        //string TargetFolder { get; set; }

        Task<List<IPackageSearchMetadata>> Search(string searchString, bool includePrerelease, string source = "");

        Task<List<IPackageSearchMetadata>> SearchPackageVersions(string packageid, bool includePrerelease);

        Task GetPackageDependencies(PackageIdentity package, SourceCacheContext cacheContext, ISet<SourcePackageDependencyInfo> availablePackages);

        Task DownloadAndInstall(PackageIdentity identity);

        bool InstallPackage(PackageIdentity identity);

        List<Lazy<INuGetResourceProvider>> CreateResourceProviders();

        LocalPackageInfo GetLocalPackageInfo(PackageIdentity identity);

        NuspecReader GetNuspecReaderInPackagesInstallFolder(PackageIdentity identity);
    }
}
