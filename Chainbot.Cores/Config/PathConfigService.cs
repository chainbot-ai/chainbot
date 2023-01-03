using Chainbot.Contracts.App;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Resources.Librarys;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.Utils;
using Plugins.Shared.Library;
using System.IO;

namespace Chainbot.Cores.Config
{
    public class PathConfigService :MarshalByRefServiceBase, IPathConfigService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ILogService _logService;

        private IConstantConfigService _constantConfigService { get; set; }

        private IUpgradeAppSettingsService _upgradeAppSettingsService { get; set; }

        private ICommonService _commonService;

        public string AppDataDir { get; set; }

        public string ProjectsDir { get; set; }

        public string CacheDir { get; set; }

        public string ConfigDir { get; set; }

        public string LogsDir { get; set; }

        public string PackagesDir { get; set; }

        public string PluginsDir { get; set; }

        public string BrowsersDir { get; set; }

        public string ScreenRecorderDir { get; set; }

        public string UdpateDir { get; set; }

        public string ComponentDownloadDir { get; set; }

        public string ProcessDownloadDir { get; set; }

        public string AssembliesCacheDir { get; set; }

        public string ActivitiesConfigCacheDir { get; set; }

        public string ProjectUserConfigXml { get; set; }

        public string CodeSnippetsXml { get; set; }

        public string FavoriteActivitiesXml { get; set; }

        public string RecentActivitiesXml { get; set; }

        public string RecentProjectsXml { get; set; }

        public string ServerSettingsXml { get; set; }

        public string AppSettingsXml { get; set; }

        public string HistoryVersionTxt { get; set; }

        public string HistoryVersionTxt_Ch { get; set; }

        public PathConfigService(IConstantConfigService constantConfigService
            , IUpgradeAppSettingsService upgradeAppSettingsService, ILogService logService, ICommonService commonService
            )
        {
            _constantConfigService = constantConfigService;
            _upgradeAppSettingsService = upgradeAppSettingsService;
            _logService = logService;
            _commonService = commonService;
        }

        public void InitDirs()
        {
            ProjectsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\"+ _constantConfigService.StudioName+@"\Projects";
            if (!System.IO.Directory.Exists(ProjectsDir))
            {
                System.IO.Directory.CreateDirectory(ProjectsDir);
            }

            var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            AppDataDir = localAppData + @"\"+ _constantConfigService.StudioName;

            LogsDir = AppDataDir + @"\Logs";

            PackagesDir = AppDataDir + @"\Packages\Installed";

            if (!System.IO.Directory.Exists(LogsDir))
            {
                System.IO.Directory.CreateDirectory(LogsDir);
            }


            ConfigDir = AppDataDir + @"\Config";

            if (!System.IO.Directory.Exists(ConfigDir))
            {
                System.IO.Directory.CreateDirectory(ConfigDir);
            }

            CodeSnippetsXml = ConfigDir + @"\CodeSnippets.xml";
            if (!System.IO.File.Exists(CodeSnippetsXml))
            {
                byte[] data = ResourcesLoader.CodeSnippets;
                System.IO.File.WriteAllBytes(CodeSnippetsXml, data);
            }

            FavoriteActivitiesXml = ConfigDir + @"\FavoriteActivities.xml";
            if (!System.IO.File.Exists(FavoriteActivitiesXml))
            {
                byte[] data = ResourcesLoader.FavoriteActivities;
                System.IO.File.WriteAllBytes(FavoriteActivitiesXml, data);
            }

            ProjectUserConfigXml = ConfigDir + @"\ProjectUserConfig.xml";
            if (!System.IO.File.Exists(ProjectUserConfigXml))
            {
                byte[] data = ResourcesLoader.ProjectUserConfig;
                System.IO.File.WriteAllBytes(ProjectUserConfigXml, data);
            }

            RecentActivitiesXml = ConfigDir + @"\RecentActivities.xml";
            if (!System.IO.File.Exists(RecentActivitiesXml))
            {
                byte[] data = ResourcesLoader.RecentActivities;
                System.IO.File.WriteAllBytes(RecentActivitiesXml, data);
            }

            RecentProjectsXml = ConfigDir + @"\RecentProjects.xml";
            if (!System.IO.File.Exists(RecentProjectsXml))
            {
                byte[] data = ResourcesLoader.RecentProjects;
                System.IO.File.WriteAllBytes(RecentProjectsXml, data);
            }

            ServerSettingsXml = ConfigDir + @"\ServerSettings.xml";
            if (!System.IO.File.Exists(ServerSettingsXml))
            {
                byte[] data = ResourcesLoader.ServerSettings;
                System.IO.File.WriteAllBytes(ServerSettingsXml, data);
            }

            AppSettingsXml = ConfigDir + @"\AppSettings.xml";
            if (!System.IO.File.Exists(AppSettingsXml))
            {
                byte[] data = ResourcesLoader.AppSettings;
                System.IO.File.WriteAllBytes(AppSettingsXml, data);
            }
            else
            {
                if (_upgradeAppSettingsService.Upgrade())
                {
                    _logService.Debug(string.Format("升级xml配置文件 {0} ……", AppSettingsXml), _logger);
                }
            }

            HistoryVersionTxt = ConfigDir + @"\HistoryVersion.txt";
            if (!File.Exists(HistoryVersionTxt))
            {
                System.IO.File.WriteAllBytes(HistoryVersionTxt, ResourcesLoader.HistoryVersion);
                File.SetAttributes(HistoryVersionTxt, FileAttributes.ReadOnly);
            }

            HistoryVersionTxt_Ch = ConfigDir + @"\历史版本.txt";
            if (!File.Exists(HistoryVersionTxt_Ch))
            {
                System.IO.File.WriteAllBytes(HistoryVersionTxt_Ch, ResourcesLoader.HistoryVersion_Ch);
                File.SetAttributes(HistoryVersionTxt_Ch, FileAttributes.ReadOnly);
            }

            UdpateDir = AppDataDir + @"\Update";
            if (!System.IO.Directory.Exists(UdpateDir))
            {
                System.IO.Directory.CreateDirectory(UdpateDir);
            }

            //ComponentDownloadDir = AppDataDir + @"\Packages\AbilityStore";
            //if (!System.IO.Directory.Exists(ComponentDownloadDir))
            //{
            //    System.IO.Directory.CreateDirectory(ComponentDownloadDir);
            //}

            //ProcessDownloadDir = AppDataDir + @"\Download\Process";
            //if (!System.IO.Directory.Exists(ProcessDownloadDir))
            //{
            //    System.IO.Directory.CreateDirectory(ProcessDownloadDir);
            //}


            PluginsDir = AppDataDir + @"\Plugins";
            if (!System.IO.Directory.Exists(PluginsDir))
            {
                System.IO.Directory.CreateDirectory(PluginsDir);
            }

            BrowsersDir = PluginsDir + @"\Browsers";
            if (!System.IO.Directory.Exists(BrowsersDir))
            {
                System.IO.Directory.CreateDirectory(BrowsersDir);

                var msghostDir = System.IO.Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, "msghost");
                _commonService.DirectoryFilesCopy(msghostDir, BrowsersDir);
            }

            AssembliesCacheDir = AppDataDir + @"\Packages\.cache\AssembliesCache";
            if (!System.IO.Directory.Exists(AssembliesCacheDir))
            {
                System.IO.Directory.CreateDirectory(AssembliesCacheDir);
            }

            ActivitiesConfigCacheDir = AppDataDir + @"\Packages\.cache\ActivitiesConfigCache";
            if (!System.IO.Directory.Exists(ActivitiesConfigCacheDir))
            {
                System.IO.Directory.CreateDirectory(ActivitiesConfigCacheDir);
            }

        }
    }
}
