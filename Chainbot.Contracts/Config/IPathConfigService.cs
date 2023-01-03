using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Config
{
    public interface IPathConfigService
    {
        string CacheDir { get; set; }

        string AppDataDir { get; set; }

        string ProjectsDir { get; set; }

        string ConfigDir { get; set; }

        string LogsDir { get; set; }

        string UdpateDir { get; set; }

        string PackagesDir { get; set; }

        string PluginsDir { get; set; }

        string ScreenRecorderDir { get; set; }

        string ComponentDownloadDir { get; set; }

        string ProcessDownloadDir { get; set; }

        string AssembliesCacheDir { get; set; }

        string ActivitiesConfigCacheDir { get; set; }

        string ProjectUserConfigXml { get; set; }

        string CodeSnippetsXml { get; set; }

        string FavoriteActivitiesXml { get; set; }

        string RecentActivitiesXml { get; set; }

        string RecentProjectsXml { get; set; }

        string ServerSettingsXml { get; set; }

        string AppSettingsXml { get; set; }

        string HistoryVersionTxt { get; set; }

        string HistoryVersionTxt_Ch { get; set; }

        void InitDirs();
    }
}
