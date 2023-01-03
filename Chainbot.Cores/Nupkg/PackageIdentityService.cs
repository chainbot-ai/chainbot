using Chainbot.Contracts.Nupkg;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using Plugins.Shared.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Project;
using System.Collections.Concurrent;

namespace Chainbot.Cores.Nupkg
{
    public class PackageIdentityService : IPackageIdentityService
    {
        private PackageIdentity _identity;

        private IPackageControlService _packageControlService;

        public PackageIdentityService(IPackageControlService packageControlService)
        {
            _packageControlService = packageControlService;
        }

        public async Task<List<string>> BuildDependentAssemblies(PackageIdentity identity, ConcurrentDictionary<string, List<string>> cachedInstalledPackagesDict)
        {
            _identity = identity;

            var list = await GetDependentAssembliesRecord(_identity, cachedInstalledPackagesDict);
            list = list.ConvertAll(f => f.Replace(@"/",@"\"));
            return list;
        }


        private async Task<List<string>> GetDependentAssembliesRecord(PackageIdentity identity, ConcurrentDictionary<string, List<string>> cachedInstalledPackagesDict)
        {
            var retList = new List<string>();

            try
            {
                using (var cacheContext = new SourceCacheContext())
                {
                    var availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
                    await _packageControlService.GetPackageDependencies(identity, cacheContext, availablePackages);

                    var resolverContext = new PackageResolverContext(
                        DependencyBehavior.Lowest,
                        new[] { identity.Id },
                        Enumerable.Empty<string>(),
                        Enumerable.Empty<NuGet.Packaging.PackageReference>(),
                        Enumerable.Empty<PackageIdentity>(),
                        availablePackages,
                        _packageControlService.SourceRepositoryProvider.GetRepositories().Select(s => s.PackageSource),
                        NullLogger.Instance);

                    var resolver = new PackageResolver();
                    var packagesToInstall = resolver.Resolve(resolverContext, CancellationToken.None)
                        .Select(p => availablePackages.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
                    var packagePathResolver = new NuGet.Packaging.PackagePathResolver(_packageControlService.PackagesInstallFolder);
                    var packageExtractionContext = new PackageExtractionContext(_packageControlService.Logger);
                    packageExtractionContext.PackageSaveMode = PackageSaveMode.Defaultv3;
                    var frameworkReducer = new FrameworkReducer();

                    foreach (var packageToInstall in packagesToInstall)
                    {
                        if (cachedInstalledPackagesDict.ContainsKey(packageToInstall.ToString()))
                        {
                            var retPackageCachedList = cachedInstalledPackagesDict[packageToInstall.ToString()];
                            retList = retList.Union(retPackageCachedList).ToList();
                            continue;
                        }

                        // PackageReaderBase packageReader;
                        var installedPath = packagePathResolver.GetInstalledPath(packageToInstall);
                        if (installedPath == null)
                        {
                            var downloadResource = await packageToInstall.Source.GetResourceAsync<DownloadResource>(CancellationToken.None);
                            var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                                packageToInstall,
                                new PackageDownloadContext(cacheContext),
                                NuGet.Configuration.SettingsUtility.GetGlobalPackagesFolder(_packageControlService.Settings),
                                _packageControlService.Logger, CancellationToken.None);

                            await PackageExtractor.ExtractPackageAsync(
                                downloadResult.PackageStream,
                                packagePathResolver,
                                packageExtractionContext,
                                CancellationToken.None);
                        }

                        var retPackageList = new List<string>();
                        InstallPackage(packageToInstall, cachedInstalledPackagesDict, ref retPackageList);

                        retList = retList.Union(retPackageList).ToList();

                        cachedInstalledPackagesDict[packageToInstall.ToString()] = retPackageList;
                    }
                }
            }
            catch (Exception err)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "下载和安装nupkg包过程中出错", err.ToString());
            }

            return retList;
        }

        public bool InstallPackage(PackageIdentity packageToInstall, ConcurrentDictionary<string, List<string>> cachedInstalledPackagesDict, ref List<string> retList)
        {

            var retPackageList = new List<string>();

            if (cachedInstalledPackagesDict.ContainsKey(packageToInstall.ToString()))
            {
                var retPackageCachedList = cachedInstalledPackagesDict[packageToInstall.ToString()];
                retList = retList.Union(retPackageCachedList).ToList();

                return true;
            }

            bool ret = true;

            var packagePathResolver = new NuGet.Packaging.PackagePathResolver(_packageControlService.PackagesInstallFolder);
            var installedPath = packagePathResolver.GetInstalledPath(packageToInstall);

            PackageReaderBase packageReader;
            packageReader = new PackageFolderReader(installedPath);
            var libItems = packageReader.GetLibItems();
            var frameworkReducer = new FrameworkReducer();
            var nearest = frameworkReducer.GetNearest(_packageControlService.NuGetFramework, libItems.Select(x => x.TargetFramework));
            var files = libItems
                .Where(x => x.TargetFramework.Equals(nearest))
                .SelectMany(x => x.Items).ToList();
            foreach (var f in files)
            {
                RecordInstallFile(installedPath, f,ref retPackageList);
            }

            var cont = packageReader.GetContentItems();
            nearest = frameworkReducer.GetNearest(_packageControlService.NuGetFramework, cont.Select(x => x.TargetFramework));
            files = cont
                .Where(x => x.TargetFramework.Equals(nearest))
                .SelectMany(x => x.Items).ToList();
            foreach (var f in files)
            {
                RecordInstallFile(installedPath, f, ref retPackageList);
            }

            try
            {
                var dependencies = packageReader.GetPackageDependencies();
                nearest = frameworkReducer.GetNearest(_packageControlService.NuGetFramework, dependencies.Select(x => x.TargetFramework));
                foreach (var dep in dependencies.Where(x => x.TargetFramework.Equals(nearest)))
                {
                    foreach (var p in dep.Packages)
                    {
                        var local = getLocal(p.Id);
                        InstallPackage(local.Identity, cachedInstalledPackagesDict, ref retPackageList);
                    }
                }
            }
            catch (Exception err)
            {
                ret = false;
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "安装nupkg包出错", err.ToString());
            }

            if (System.IO.Directory.Exists(installedPath + @"\build"))
            {
                if (System.IO.Directory.Exists(installedPath + @"\build\x86"))
                {
                    foreach (var f in System.IO.Directory.GetFiles(installedPath + @"\build\x86"))
                    {
                        var filename = System.IO.Path.GetFileName(f);
                        recordFile(f, ref retPackageList);
                    }
                }
            }

            if (System.IO.Directory.Exists(installedPath + @"\x86"))
            {
                foreach (var f in System.IO.Directory.GetFiles(installedPath + @"\x86"))
                {
                    var filename = System.IO.Path.GetFileName(f);
                    recordFile(f, ref retPackageList);
                }
            }

            cachedInstalledPackagesDict[packageToInstall.ToString()] = retPackageList;

            retList = retList.Union(retPackageList).ToList();

            return ret;
        }

       

        private LocalPackageInfo getLocal(string identity)
        {
            FindLocalPackagesResourceV2 findLocalPackagev2 = new FindLocalPackagesResourceV2(_packageControlService.PackagesInstallFolder);
            var packages = findLocalPackagev2.GetPackages(_packageControlService.Logger, CancellationToken.None).ToList();
            packages = packages.Where(p => p.Identity.Id == identity).ToList();
            LocalPackageInfo res = null;
            foreach (var p in packages)
            {
                if (res == null) res = p;
                if (res.Identity.Version < p.Identity.Version) res = p;
            }

            return res;
        }

       
        private void RecordInstallFile(string installedPath, string f, ref List<string> retList)
        {
            string source = "";
            try
            {
                source = System.IO.Path.Combine(installedPath, f);
                recordFile(source, ref retList);
            }
            catch (Exception ex)
            {

            }
        }


        private void recordFile(string source,ref List<string> retList)
        {
            retList.Add(source);
        }

       
    }
}
