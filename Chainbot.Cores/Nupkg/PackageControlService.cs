using Chainbot.Contracts.Config;
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
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Chainbot.Cores.Nupkg
{
    public class PackageControlService: IPackageControlService
    {
        private IConstantConfigService _constantConfigService;
        private ConcurrentDictionary<string, ISet<SourcePackageDependencyInfo>> _availablePackagesDict = new ConcurrentDictionary<string, ISet<SourcePackageDependencyInfo>>();

        public PackageControlService(IConstantConfigService constantConfigService)
        {
            _constantConfigService = constantConfigService;

            if (!System.IO.Directory.Exists(GlobalPackagesFolder))
            {
                System.IO.Directory.CreateDirectory(GlobalPackagesFolder);
            }

            if (!System.IO.Directory.Exists(PackagesInstallFolder))
            {
                System.IO.Directory.CreateDirectory(PackagesInstallFolder);
            }

            //if (!System.IO.Directory.Exists(TargetFolder))
            //{
            //    System.IO.Directory.CreateDirectory(TargetFolder);
            //}
        }


        private ILogger _logger = null;
        public ILogger Logger
        {
            get
            {
                if (_logger == null) _logger = NuGetPackageControllerLogger.Instance;
                return _logger;
            }
        }

        public NuGetFramework _nuGetFramework = null;
        public NuGetFramework NuGetFramework
        {
            get
            {
                if (_nuGetFramework == null) _nuGetFramework = NuGetFramework.ParseFolder("net461");
                return _nuGetFramework;
            }
        }

        public NuGet.Configuration.ISettings _settings = null;
        public NuGet.Configuration.ISettings Settings
        {
            get
            {
                var nugetConfigFile = "Nuget.Default.Config";
                try
                {
                    if (_settings == null) _settings = NuGet.Configuration.Settings.LoadSpecificSettings(SharedObject.Instance.ApplicationCurrentDirectory, nugetConfigFile);
                    return _settings;
                }
                catch (Exception err)
                {
                    SharedObject.Instance.Output(SharedObject.enOutputType.Error, $"读取当前目录下的{nugetConfigFile}配置文件出错", err.ToString());
                    return null;
                }

            }
        }

        public NuGet.Configuration.ISettings _userSettings = null;
        public NuGet.Configuration.ISettings UserSettings
        {
            get
            {
                var nugetConfigFile = "Nuget.User.Config";
                try
                {
                    if (_userSettings == null) _userSettings = NuGet.Configuration.Settings.LoadSpecificSettings(SharedObject.Instance.ApplicationCurrentDirectory, nugetConfigFile);
                    return _userSettings;
                }
                catch (Exception err)
                {
                    SharedObject.Instance.Output(SharedObject.enOutputType.Error, $"读取当前目录下的{nugetConfigFile}配置文件出错", err.ToString());
                    return null;
                }

            }
        }

        public SourceRepositoryProvider SourceRepositoryProvider
        {
            get
            {
                var psp = new PackageSourceProvider(Settings, null, new PackageSourceProvider(UserSettings).LoadPackageSources());
                var _sourceRepositoryProvider = new SourceRepositoryProvider(psp, Repository.Provider.GetCoreV3());

                return _sourceRepositoryProvider;
            }
        }

        private SourceRepositoryProvider _defaultSourceRepositoryProvider = null;
        public SourceRepositoryProvider DefaultSourceRepositoryProvider
        {
            get
            {
                if (_defaultSourceRepositoryProvider == null)
                {
                    var psp = new PackageSourceProvider(Settings);
                    _defaultSourceRepositoryProvider = new SourceRepositoryProvider(psp, Repository.Provider.GetCoreV3());
                }
                return _defaultSourceRepositoryProvider;
            }
        }

        private SourceRepositoryProvider _userDefineSourceRepositoryProvider = null;
        public SourceRepositoryProvider UserDefineSourceRepositoryProvider
        {
            get
            {
                if (_userDefineSourceRepositoryProvider == null)
                {
                    var psp = new PackageSourceProvider(UserSettings);
                    _userDefineSourceRepositoryProvider = new SourceRepositoryProvider(psp, Repository.Provider.GetCoreV3());
                }
                return _userDefineSourceRepositoryProvider;
            }
        }


        public string globalPackagesFolder = null;
        public string GlobalPackagesFolder
        {
            get
            {
                if (string.IsNullOrEmpty(globalPackagesFolder)) globalPackagesFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + $"\\{_constantConfigService.StudioName}\\Packages\\.nuget\\packages";
                return globalPackagesFolder;
            }
        }

        public string _packagesInstallFolder = null;
        public string PackagesInstallFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_packagesInstallFolder)) _packagesInstallFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + $"\\{_constantConfigService.StudioName}\\Packages\\Installed";
                return _packagesInstallFolder;
            }
        }

        //public string _targetFolder = null;
        //public string TargetFolder
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(_targetFolder)) _targetFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + $"\\{_constantConfigService.StudioName}\\Packages\\Target";
        //        return _targetFolder;
        //    }
        //    set
        //    {
        //        _targetFolder = value;
        //    }
        //}


        public async Task<List<IPackageSearchMetadata>> Search(string searchString, bool includePrerelease, string source = "")
        {
            var result = new List<IPackageSearchMetadata>();

            foreach (var sourceRepository in SourceRepositoryProvider.GetRepositories())
            {
                if (!string.IsNullOrEmpty(source))
                {
                    if (sourceRepository.PackageSource.Source.ToLower() != source.ToLower())
                    {
                        continue;
                    }
                }

                var searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>();
                var supportedFramework = new[] { ".NETFramework,Version=v4.6.1" };
                var searchFilter = new SearchFilter(includePrerelease)
                {
                    SupportedFrameworks = supportedFramework,
                    IncludeDelisted = true
                };

                var jsonNugetPackages = await searchResource
                            .SearchAsync(searchString, searchFilter, 0, 50, NullLogger.Instance, CancellationToken.None);

                if (string.IsNullOrEmpty(searchString))
                {
                    foreach (var p in jsonNugetPackages)
                    {
                        var exists = result.Where(x => x.Identity.Id == p.Identity.Id).FirstOrDefault();
                        if (exists == null) result.Add(p);
                    }
                }
                else
                {
                    foreach (var p in jsonNugetPackages.Where(x => x.Title.ToLower().Contains(searchString.ToLower())))
                    {
                        var exists = result.Where(x => x.Identity.Id == p.Identity.Id).FirstOrDefault();
                        if (exists == null) result.Add(p);
                    }
                }

            }

            return result;
        }


        public async Task<List<IPackageSearchMetadata>> SearchPackageVersions(string packageid, bool includePrerelease)
        {
            var ret = new List<IPackageSearchMetadata>();
            foreach (var sourceRepository in SourceRepositoryProvider.GetRepositories())
            {
                var searchResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();

                try
                {
                    var metadataList = await searchResource
                           .GetMetadataAsync(packageid, includePrerelease, true, NullLogger.Instance, CancellationToken.None);

                    if (metadataList.Count() > ret.Count())
                    {
                        ret = metadataList.ToList();
                    }
                }
                catch (Exception err)
                {

                }

            }

            return ret;
        }






        public async Task GetPackageDependencies(PackageIdentity package, SourceCacheContext cacheContext, ISet<SourcePackageDependencyInfo> availablePackages)
        {
            if (_availablePackagesDict.ContainsKey(package.ToString()))
            {
                availablePackages.UnionWith(_availablePackagesDict[package.ToString()]);
                return;
            }

            //if (availablePackages.Contains(package)) return;

            var repositories = SourceRepositoryProvider.GetRepositories();
            foreach (var sourceRepository in repositories)
            {
                try
                {
                    var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
                    var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                        package, NuGetFramework, Logger, CancellationToken.None);
                    if (dependencyInfo == null) continue;
                    availablePackages.Add(dependencyInfo);

                    var availablePackagesDependenciesUnion = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
                    foreach (var dependency in dependencyInfo.Dependencies)
                    {
                        var identity = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);
                        Trace.WriteLine($"###########正在获取{identity.ToString()}.nupkg包的依赖项……");

                        var availablePackagesDependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
                        await GetPackageDependencies(identity, cacheContext, availablePackagesDependencies);
                        availablePackagesDependenciesUnion.UnionWith(availablePackagesDependencies);
                    }

                    availablePackages.UnionWith(availablePackagesDependenciesUnion);

                    break;
                }
                catch (Exception err)
                {

                }

            }

            _availablePackagesDict[package.ToString()] = availablePackages;
        }

        public async Task DownloadAndInstall(PackageIdentity identity)
        {
            try
            {
                using (var cacheContext = new SourceCacheContext())
                {
                    var availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
                    await GetPackageDependencies(identity, cacheContext, availablePackages);

                    var resolverContext = new PackageResolverContext(
                        DependencyBehavior.Lowest,
                        new[] { identity.Id },
                        Enumerable.Empty<string>(),
                        Enumerable.Empty<NuGet.Packaging.PackageReference>(),
                        Enumerable.Empty<PackageIdentity>(),
                        availablePackages,
                        SourceRepositoryProvider.GetRepositories().Select(s => s.PackageSource),
                        NullLogger.Instance);

                    var resolver = new PackageResolver();
                    var packagesToInstall = resolver.Resolve(resolverContext, CancellationToken.None)
                        .Select(p => availablePackages.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
                    var packagePathResolver = new NuGet.Packaging.PackagePathResolver(PackagesInstallFolder);
                    var packageExtractionContext = new PackageExtractionContext(Logger);
                    packageExtractionContext.PackageSaveMode = PackageSaveMode.Defaultv3;
                    var frameworkReducer = new FrameworkReducer();

                    foreach (var packageToInstall in packagesToInstall)
                    {
                        // PackageReaderBase packageReader;
                        var installedPath = packagePathResolver.GetInstalledPath(packageToInstall);
                        if (installedPath == null)
                        {
                            var downloadResource = await packageToInstall.Source.GetResourceAsync<DownloadResource>(CancellationToken.None);
                            var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                                packageToInstall,
                                new PackageDownloadContext(cacheContext),
                                NuGet.Configuration.SettingsUtility.GetGlobalPackagesFolder(Settings),
                                Logger, CancellationToken.None);

                            await PackageExtractor.ExtractPackageAsync(
                                downloadResult.PackageStream,
                                packagePathResolver,
                                packageExtractionContext,
                                CancellationToken.None);
                        }

                        InstallPackage(packageToInstall);
                    }
                }
            }
            catch (Exception err)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "下载和安装nupkg包过程中出错", err.ToString());
            }

        }

        public bool InstallPackage(PackageIdentity identity)
        {
            bool ret = true;

            var packagePathResolver = new NuGet.Packaging.PackagePathResolver(PackagesInstallFolder);
            var installedPath = packagePathResolver.GetInstalledPath(identity);

            PackageReaderBase packageReader;
            packageReader = new PackageFolderReader(installedPath);
            var libItems = packageReader.GetLibItems();
            var frameworkReducer = new FrameworkReducer();
            var nearest = frameworkReducer.GetNearest(NuGetFramework, libItems.Select(x => x.TargetFramework));
            var files = libItems
                .Where(x => x.TargetFramework.Equals(nearest))
                .SelectMany(x => x.Items).ToList();
            foreach (var f in files)
            {
                InstallFile(installedPath, f);
            }

            var cont = packageReader.GetContentItems();
            nearest = frameworkReducer.GetNearest(NuGetFramework, cont.Select(x => x.TargetFramework));
            files = cont
                .Where(x => x.TargetFramework.Equals(nearest))
                .SelectMany(x => x.Items).ToList();
            foreach (var f in files)
            {
                InstallFile(installedPath, f);
            }

            try
            {
                var dependencies = packageReader.GetPackageDependencies();
                nearest = frameworkReducer.GetNearest(NuGetFramework, dependencies.Select(x => x.TargetFramework));
                foreach (var dep in dependencies.Where(x => x.TargetFramework.Equals(nearest)))
                {
                    foreach (var p in dep.Packages)
                    {
                        var local = getLocal(p.Id);
                        InstallPackage(local.Identity);
                    }
                }
            }
            catch (Exception err)
            {
                ret = false;
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "安装nupkg包出错", err.ToString());
            }

            //if (System.IO.Directory.Exists(installedPath + @"\build"))
            //{
            //    if (System.IO.Directory.Exists(installedPath + @"\build\x64"))
            //    {
            //        foreach (var f in System.IO.Directory.GetFiles(installedPath + @"\build\x64"))
            //        {
            //            var filename = System.IO.Path.GetFileName(f);
            //            var target = System.IO.Path.Combine(TargetFolder, filename);
            //            CopyIfNewer(f, target);
            //        }
            //    }
            //}

            return ret;
        }

        public List<Lazy<INuGetResourceProvider>> CreateResourceProviders()
        {
            var result = new List<Lazy<INuGetResourceProvider>>();
            Repository.Provider.GetCoreV3();
            return result;
        }

        private LocalPackageInfo getLocal(string identity)
        {
            FindLocalPackagesResourceV2 findLocalPackagev2 = new FindLocalPackagesResourceV2(PackagesInstallFolder);
            var packages = findLocalPackagev2.GetPackages(Logger, CancellationToken.None).ToList();
            packages = packages.Where(p => p.Identity.Id == identity).ToList();
            LocalPackageInfo res = null;
            foreach (var p in packages)
            {
                if (res == null) res = p;
                if (res.Identity.Version < p.Identity.Version) res = p;
            }
            return res;
        }

        public LocalPackageInfo GetLocalPackageInfo(PackageIdentity identity)
        {
            FindLocalPackagesResourceV2 findLocalPackagev2 = new FindLocalPackagesResourceV2(PackagesInstallFolder);
            var packages = findLocalPackagev2.GetPackages(Logger, CancellationToken.None).ToList();
            packages = packages.Where(p => p.Identity.Id == identity.Id).ToList();
            LocalPackageInfo res = null;
            foreach (var p in packages)
            {
                if (p.Identity.Version == identity.Version)
                {
                    res = p;
                    break;
                }
            }
            return res;
        }

        private void InstallFile(string installedPath, string f)
        {
            string source = "";
            string f2 = "";
            string filename = "";
            string dir = "";
            string target = "";
            try
            {
                source = System.IO.Path.Combine(installedPath, f);

                //var arr = f.Split('/');
                //if (arr[0] == "lib")
                //{
                //    if (arr.Length == 2)
                //    {
                //        f2 = f.Substring(f.IndexOf("/", 3) + 1);
                //    }
                //    else
                //    {
                //        f2 = f.Substring(f.IndexOf("/", 4) + 1);
                //    }
                //}
                //else
                //{
                //    f2 = f.Substring(f.IndexOf("/", 0) + 1);
                //}

                //filename = System.IO.Path.GetFileName(f2);
                //dir = System.IO.Path.GetDirectoryName(f2);
                //target = System.IO.Path.Combine(TargetFolder, dir, filename);
                //if (!System.IO.Directory.Exists(System.IO.Path.Combine(TargetFolder, dir)))
                //{
                //    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(TargetFolder, dir));
                //}
                //CopyIfNewer(source, target);
            }
            catch (Exception err)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "安装nupkg包文件时出错", err.ToString());
            }
        }

        private void CopyIfNewer(string source, string target)
        {
            var infoOld = new System.IO.FileInfo(source);
            var infoNew = new System.IO.FileInfo(target);
            if (infoNew.LastWriteTime != infoOld.LastWriteTime)
            {
                try
                {
                    System.IO.File.Copy(source, target, true);
                    return;
                }
                catch (Exception)
                {

                }
            }
        }

        public NuspecReader GetNuspecReaderInPackagesInstallFolder(PackageIdentity identity)
        {
            var packagePathResolver = new NuGet.Packaging.PackagePathResolver(PackagesInstallFolder);
            var installedPath = packagePathResolver.GetInstalledPath(identity);

            PackageReaderBase packageReader;
            packageReader = new PackageFolderReader(installedPath);

            return packageReader.NuspecReader;
        }






    }
}
