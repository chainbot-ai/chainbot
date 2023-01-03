using Chainbot.Contracts.Activities;
using Chainbot.Contracts.App;
using Chainbot.Contracts.AppDomains;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using Chainbot.Resources.Librarys;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Plugins.Shared.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Chainbot.Cores.Project
{
    public class ProjectManagerService : MarshalByRefServiceBase, IProjectManagerService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IServiceLocator _serviceLocator;

        private ILogService _logService;

        private IMessageBoxService _messageBoxService;
        private ICommonService _commonService;
        private IConstantConfigService _constantConfigService;
        private IProjectConfigFileService _projectConfigFileService;
        private IPackageRepositoryService _packageRepositoryService;
        private IAppDomainControllerService _appDomainControllerService;
        private IPackageIdentityService _packageIdentityService;
        private IDispatcherService _dispatcherService;
        private IPathConfigService _pathConfigService;

        private IActivityMountService _activityMountService;

        private IWorkflowStateService _workflowStateService;

        public event EventHandler ProjectLoadingBeginEvent;
        public event EventHandler ProjectLoadingEndEvent;
        public event EventHandler ProjectLoadingExceptionEvent;

        public event EventHandler ProjectPreviewOpenEvent;
        public event EventHandler ProjectOpenEvent;

        public event EventHandler<CancelEventArgs> ProjectPreviewCloseEvent;
        public event EventHandler ProjectCloseEvent;

        public ProjectJsonConfig CurrentProjectJsonConfig { get; private set; }

        public string CurrentProjectConfigFilePath { get; private set; }

        public string CurrentProjectPath { get; private set; }

        public List<string> AllActivityConfigXmls { get; private set; }

        public List<ActivityGroupOrLeafItem> Activities{ get; private set; }

        public Dictionary<string, ActivityGroupOrLeafItem> ActivitiesTypeOfDict { get; private set; } = new Dictionary<string, ActivityGroupOrLeafItem>();

        public IActivitiesServiceProxy CurrentActivitiesServiceProxy { get; private set; }

        public ConcurrentDictionary<string, List<string>> PackageAssemblies { get; private set; } = new ConcurrentDictionary<string, List<string>>();

        public string CurrentProjectMainXamlFileAbsolutePath
        {
            get
            {
                return CurrentProjectPath + @"\" + CurrentProjectJsonConfig.main;
            }
        }


        public List<string> CurrentActivitiesDllLoadFrom { get; private set; }

        public List<string> CurrentDependentAssemblies { get; private set; }

        public ProjectManagerService(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _logService = _serviceLocator.ResolveType<ILogService>();
            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _constantConfigService = _serviceLocator.ResolveType<IConstantConfigService>();
            _projectConfigFileService = _serviceLocator.ResolveType<IProjectConfigFileService>();
            _packageRepositoryService = _serviceLocator.ResolveType<IPackageRepositoryService>();
            _packageIdentityService = _serviceLocator.ResolveType<IPackageIdentityService>();
            _activityMountService = _serviceLocator.ResolveType<IActivityMountService>();
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();
            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _pathConfigService = _serviceLocator.ResolveType<IPathConfigService>();

            _appDomainControllerService = _serviceLocator.ResolveType<IAppDomainControllerService>();
        }


        private void InitProjectJson(string projectsPath, string projectName,string projectDescription,string projectVersion)
        {
            var config = new ProjectJsonConfig();
            config.Init();
            config.studioVersion = _commonService.GetProgramVersion();
            config.name = projectName;
            config.description = projectDescription;
            config.main = _constantConfigService.MainXamlFileName;

            if(!string.IsNullOrEmpty(projectVersion))
            {
                config.projectVersion = projectVersion;
            }

            _packageRepositoryService.Init(SharedObject.Instance.ApplicationCurrentDirectory + @"\Packages");
            var list = _packageRepositoryService.GetMatchedPackagesByIdAndMaxVersion(_constantConfigService.ProjectDefaultDependentPackagesMatchRegex);

            foreach(var item in list)
            {
                config.dependencies[item.Id] = $"[{item.Version}]";
            }

            config.projectType = "Workflow";

            CurrentProjectJsonConfig = config;

            string json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);

            CurrentProjectConfigFilePath = projectsPath + @"\" + projectName + @"\" + _constantConfigService.ProjectConfigFileName;
            using (FileStream fs = new FileStream(CurrentProjectConfigFilePath, FileMode.Create))
            { 
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }

        }

        private void InitMainXaml(string projectsPath, string projectName)
        {
            byte[] data = ResourcesLoader.Main;
            FileStream fileStream = new FileStream(projectsPath + @"\" + projectName + @"\"+ _constantConfigService.MainXamlFileName, FileMode.CreateNew);
            fileStream.Write(data, 0, (int)(data.Length));
            fileStream.Close();
        }


        public bool NewProject(string projectsPath, string projectName, string projectDescription,string projectVersion)
        {
            var projectDir = projectsPath + @"\" + projectName;

            CurrentProjectPath = projectDir;

            try
            {
                Directory.CreateDirectory(projectDir);
            }
            catch (Exception e)
            {
                _logService.Error(e,_logger);

                _messageBoxService.ShowError("创建项目目录失败，请检查！");
                return false;
            }

            InitProjectJson(projectsPath, projectName, projectDescription, projectVersion);

            InitMainXaml(projectsPath, projectName);

            return true;
        }

        public bool SaveCurrentProjectJson()
        {
            try
            {
                string json = JsonConvert.SerializeObject(CurrentProjectJsonConfig, Newtonsoft.Json.Formatting.Indented);
                using (FileStream fs = new FileStream(CurrentProjectConfigFilePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(json);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool CloseCurrentProject()
        {
            if(string.IsNullOrEmpty(CurrentProjectPath))
            {
                return true;
            }

            if(_workflowStateService.IsRunningOrDebugging)
            {
                _messageBoxService.ShowInformation("请停止当前正在运行或调试的项目再关闭");
                return false;
            }

            CancelEventArgs cancelEventArgs = new CancelEventArgs();
            ProjectPreviewCloseEvent?.Invoke(this, cancelEventArgs);
            if(cancelEventArgs.Cancel)
            {
                return false;
            }

            ProjectCloseEvent?.Invoke(this, EventArgs.Empty);

            CurrentProjectPath = CurrentProjectConfigFilePath = "";
            CurrentProjectJsonConfig = null;
            ActivitiesTypeOfDict.Clear();

            _appDomainControllerService.UnloadAppDomain();

            return true;
        }


        public async void ReopenCurrentProject()
        {
            var currentProjectConfigFilePath = CurrentProjectConfigFilePath;
            CloseCurrentProject();
            await OpenProject(currentProjectConfigFilePath);
        }

        public bool IsProjectExists(string projectConfigFilePath)
        {
            if (!File.Exists(projectConfigFilePath))
            {
                return false;
            }

            return true;
        }

        public async Task OpenProject(string projectConfigFilePath)
        {
            try
            {
                ProjectLoadingBeginEvent?.Invoke(this, EventArgs.Empty);

                await Task.Run(async () =>
                {
                    CurrentProjectConfigFilePath = projectConfigFilePath;
                    CurrentProjectPath = Path.GetDirectoryName(projectConfigFilePath);

                    _projectConfigFileService.Load(projectConfigFilePath);
                    CurrentProjectJsonConfig = _projectConfigFileService.ProjectJsonConfig;

                    await _dispatcherService.InvokeAsync(async () =>
                    {
                        await _appDomainControllerService.CreateAppDomain();

                        await Task.Run(async () =>
                        {
                            ProjectPreviewOpenEvent?.Invoke(this, EventArgs.Empty);

                            await InitDependencies();
                
                            ProjectOpenEvent?.Invoke(this, EventArgs.Empty);

                            ProjectLoadingEndEvent?.Invoke(this, EventArgs.Empty);
                        });
                    });

                });
            }
            catch (Exception e)
            {
                _logService.Error(e, _logger);
                _messageBoxService.ShowError("打开项目时发生异常，请检查！");

                ProjectLoadingExceptionEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task InitDependencies()
        {
            await Task.Run(async () =>
            {
                ConcurrentDictionary<string, List<string>> cachedInstalledPackagesDict = new ConcurrentDictionary<string, List<string>>();

                var assemblies = new List<string>();
                foreach (JProperty jp in (JToken)this.CurrentProjectJsonConfig.dependencies)
                {
                    var ver_range = VersionRange.Parse((string)jp.Value);
                    if (ver_range.IsMinInclusive)
                    {
                        string version = ver_range.MinVersion.ToString();
                        string packageName = jp.Name + "." + version;
                        string packageAssembliesPath = Path.Combine(_pathConfigService.AssembliesCacheDir, packageName + ".json");

                        List<string> assembliesList = GetPackageAssembliesCache(packageName, packageAssembliesPath);

                        if (assembliesList != null && assembliesList.Count > 0)
                        {
                            assemblies = assemblies.Union(assembliesList).ToList<string>();
                        }
                        else
                        {
                            var target_ver = NuGetVersion.Parse(version);
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

                CurrentActivitiesServiceProxy = _serviceLocator.ResolveType<IActivitiesServiceProxy>();
                CurrentActivitiesServiceProxy.SetSharedObjectInstance();

                CurrentActivitiesDllLoadFrom = CurrentActivitiesServiceProxy.Init(assemblies);
                CurrentDependentAssemblies = assemblies;

                AllActivityConfigXmls = CurrentActivitiesServiceProxy.CustomActivityConfigXmls;

                var systemActivities = Encoding.UTF8.GetString(ResourcesLoader.SystemActivities);
                AllActivityConfigXmls.Insert(0, systemActivities);

                List<ActivityGroupOrLeafItem> activitiesCurrent = new List<ActivityGroupOrLeafItem>();
                foreach (var activityConfigXml in AllActivityConfigXmls)
                {
                    var activitiesToMount = _activityMountService.Transform(activityConfigXml);

                    activitiesCurrent = _activityMountService.Mount(activitiesCurrent, activitiesToMount);
                }

                Activities = activitiesCurrent;

                InitActivitiesTypeOfDict(Activities);
            });
        }

        private List<string> GetPackageAssembliesCache(string packageName, string packageAssembliesPath)
        {
            List<string> assembliesList = null;

            try
            {
                if (PackageAssemblies.ContainsKey(packageName))
                {
                    assembliesList = PackageAssemblies[packageName];
                }
                else
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
                                string absolutePath = Path.Combine(_pathConfigService.PackagesDir, item);
                                assembliesList.Add(absolutePath);
                            }

                            PackageAssemblies[packageName] = assembliesList;
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

                            List<string> outAssembliesList;
                            PackageAssemblies.TryRemove(packageName, out outAssembliesList);
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
                PackageAssemblies[packageName] = packageAssemblies;

                List<string> assembliesList = new List<string>();
                foreach (string item in packageAssemblies)
                {
                    string relativePath = _commonService.MakeRelativePath(_pathConfigService.PackagesDir, item);
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

        private void InitActivitiesTypeOfDict(List<ActivityGroupOrLeafItem> list)
        {
            foreach (var item in list)
            {
                if (item is ActivityGroupItem)
                {
                    var group = item as ActivityGroupItem;
                    InitActivitiesTypeOfDict(group.Children);
                }
                else
                {
                    var leaf = item as ActivityLeafItem;
                    ActivitiesTypeOfDict[leaf.TypeOf] = leaf;
                }
            }
        }

        public bool IsAlreadyOpened(string projectConfigFilePath)
        {
            return CurrentProjectConfigFilePath == projectConfigFilePath;
        }

        public void UpdateCurrentProjectConfigFilePath(string projectConfigFilePath)
        {
            CurrentProjectConfigFilePath = projectConfigFilePath;
            CurrentProjectPath = Path.GetDirectoryName(projectConfigFilePath);

            _projectConfigFileService.Load(projectConfigFilePath);
            CurrentProjectJsonConfig = _projectConfigFileService.ProjectJsonConfig;
        }

        public string GetCurrentProjectDependencyVersionById(string id)
        {
            var ret = "";

            foreach (JProperty jp in (JToken)CurrentProjectJsonConfig.dependencies)
            {
                if(jp.Name == id)
                {
                    var ver_range = VersionRange.Parse((string)jp.Value);
                    if (ver_range.IsMinInclusive)
                    {
                        ret = ver_range.MinVersion.ToString();
                    }
                    break;
                }
            }

            return ret;
        }
    }
}
