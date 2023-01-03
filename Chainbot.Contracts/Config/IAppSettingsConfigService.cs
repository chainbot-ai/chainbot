using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace Chainbot.Contracts.Config
{
    public interface IAppSettingsConfigService
    {
        string GetLastExportDir();
        bool SetLastExportDir(string dir);

        List<string> GetExportDirHistoryList();
        bool AddToExportDirHistoryList(string dir);

        enTheme? CurrentTheme { get; set; }

        enLanguage? CurrentLanguage { get; set; }
    }
}
