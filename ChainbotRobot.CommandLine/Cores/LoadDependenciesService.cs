using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Utils;
using ChainbotRobot.CommandLine.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine.Cores
{
    public class LoadDependenciesService : ILoadDependenciesService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string CacheDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ChainbotStudio";

        private ILogService _logService;

        private IPackageIdentityService _packageIdentityService;

        private ICommonService _commonService;

        public string CurrentProjectJsonFile { get; set; }

        public List<string> CurrentActivitiesDllLoadFrom { get; private set; }
        public List<string> CurrentDependentAssemblies { get; private set; }

        public LoadDependenciesService(ILogService logService, IPackageIdentityService packageIdentityService, ICommonService commonService)
        {
            _logService = logService;
            _packageIdentityService = packageIdentityService;
            _commonService = commonService;
        }

        public void Init(string projectJsonFile)
        {
            CurrentProjectJsonFile = projectJsonFile;
        }

        public async Task LoadDependencies()
        {
            await Task.Run(async () =>
            {
                var json_cfg = ProcessProjectJsonConfig();

                ConcurrentDictionary<string, List<string>> cachedInstalledPackagesDict = new ConcurrentDictionary<string, List<string>>();

                string packagesCacheDir = Path.Combine(CacheDir, @"Packages\.cache\AssembliesCache");
                if (!Directory.Exists(packagesCacheDir))
                {
                    Directory.CreateDirectory(packagesCacheDir);
                }

                var assemblies = new List<string>();
                foreach (JProperty jp in (JToken)json_cfg.dependencies)
                {
                    var ver_range = VersionRange.Parse((string)jp.Value);
                    if (ver_range.IsMinInclusive)
                    {
                        string version = ver_range.MinVersion.ToString();
                        string packageName = jp.Name + "." + version;
                        string packageAssembliesPath = Path.Combine(packagesCacheDir, packageName + ".json");

                        List<string> assembliesList = GetPackageAssembliesCache(packageName, packageAssembliesPath);

                        if (assembliesList != null && assembliesList.Count > 0)
                        {
                            assemblies = assemblies.Union(assembliesList).ToList<string>();
                        }
                        else
                        {
                            var target_ver = NuGetVersion.Parse(ver_range.MinVersion.ToString());
                            var retList = await _packageIdentityService.BuildDependentAssemblies(new PackageIdentity(jp.Name, target_ver), cachedInstalledPackagesDict);
                            assemblies = assemblies.Union(retList).ToList<string>();

                            if (retList.Count > 0)
                            {
                                SavePackageAssembliesCatch(retList, packageName, packageAssembliesPath);
                            }
                        }
                    }
                    else
                    {
                        
                    }
                }

                CurrentActivitiesDllLoadFrom = assemblies;
                CurrentDependentAssemblies = assemblies;
            });

        }

        public ProjectJsonConfig ProcessProjectJsonConfig()
        {
            var json_str = File.ReadAllText(CurrentProjectJsonFile);
            try
            {
                var json_cfg = JsonConvert.DeserializeObject<ProjectJsonConfig>(json_str);
                if (json_cfg.Upgrade())
                {
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(json_cfg, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(CurrentProjectJsonFile, output);

                    json_str = File.ReadAllText(CurrentProjectJsonFile);
                    json_cfg = JsonConvert.DeserializeObject<ProjectJsonConfig>(json_str);
                }

                return json_cfg;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private List<string> GetPackageAssembliesCache(string packageName, string packageAssembliesPath)
        {
            List<string> assembliesList = null;

            try
            {
                if (File.Exists(packageAssembliesPath))
                {
                    JObject jo;
                    using (StreamReader streamReader = new StreamReader(packageAssembliesPath))
                    {
                        jo = JObject.Parse(streamReader.ReadToEnd());
                    }

                    string md5 = jo["md5"]?.ToString();
                    string assemblies = jo["assemblies"]?.ToString();

                    if (_commonService.GetNupkgMd5(packageName + ".nupkg") == md5 && !string.IsNullOrEmpty(assemblies))
                    {
                        List<string> assembliesListTmp = JsonConvert.DeserializeObject<List<string>>(assemblies);

                        assembliesList = new List<string>();
                        foreach (string item in assembliesListTmp)
                        {
                            string absolutePath = Path.Combine(CacheDir, @"Packages\Installed", item);
                            assembliesList.Add(absolutePath);
                        }
                    }
                }

                if (assembliesList != null)
                {
                    foreach (string item in assembliesList)
                    {
                        if (!File.Exists(item))
                        {
                            assembliesList = null;
                            break;
                        }
                    }
                }

                return assembliesList;
            }
            catch
            {
                return null;
            }
        }

        private void SavePackageAssembliesCatch(List<string> packageAssemblies, string packageName, string packageAssembliesPath)
        {
            try
            {
                List<string> assembliesList = new List<string>();
                foreach (string item in packageAssemblies)
                {
                    string relativePath = _commonService.MakeRelativePath(Path.Combine(CacheDir, @"Packages\Installed"), item);
                    assembliesList.Add(relativePath);
                }

                JObject jo = new JObject();
                jo["md5"] = _commonService.GetNupkgMd5(packageName + ".nupkg");
                jo["assemblies"] = JsonConvert.SerializeObject(assembliesList);

                using (StreamWriter streamWriter = new StreamWriter(new FileStream(packageAssembliesPath, FileMode.Create, FileAccess.Write)))
                {
                    streamWriter.Write(jo.ToString(Formatting.Indented, new JsonConverter[0]));
                }
            }
            catch
            {
            }
        }

    }
}
