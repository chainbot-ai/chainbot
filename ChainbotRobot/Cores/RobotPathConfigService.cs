using log4net;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using ChainbotRobot.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ChainbotRobot.Cores
{
    public class RobotPathConfigService : IRobotPathConfigService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IConstantConfigService _constantConfigService;
        private ILogService _logService;

        public string AppDataDir { get; set; }

        public string LogsDir { get; set; }

        public string ConfigDir { get; set; }

        public string PackagesDir { get; set; }

        public string ScreenRecorderDir { get; set; }

        public string ProgramDataPackagesDir { get; set; }

        public string ProgramDataInstalledPackagesDir { get; set; }

        public string AppProgramDataDir { get; set; }

        public RobotPathConfigService(IConstantConfigService constantConfigService, ILogService logService)
        {
            _constantConfigService = constantConfigService;
            _logService = logService;
        }

        public void InitDirs()
        {
            var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            AppDataDir = localAppData + @"\" + _constantConfigService.StudioName;

            LogsDir = AppDataDir + @"\Logs";

            if (!System.IO.Directory.Exists(LogsDir))
            {
                System.IO.Directory.CreateDirectory(LogsDir);
            }

            ConfigDir = AppDataDir + @"\Config";

            if (!System.IO.Directory.Exists(ConfigDir))
            {
                System.IO.Directory.CreateDirectory(ConfigDir);
            }

            PackagesDir = AppDataDir + @"\Packages";

            if (!System.IO.Directory.Exists(PackagesDir))
            {
                System.IO.Directory.CreateDirectory(PackagesDir);
            }

            ScreenRecorderDir = AppDataDir + @"\ScreenRecorder";
            if (!System.IO.Directory.Exists(ScreenRecorderDir))
            {
                System.IO.Directory.CreateDirectory(ScreenRecorderDir);
            }


            var commonApplicationData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
            AppProgramDataDir = commonApplicationData + @"\" + _constantConfigService.StudioName;
            ProgramDataPackagesDir = AppProgramDataDir + @"\Packages";
            ProgramDataInstalledPackagesDir = AppProgramDataDir + @"\InstalledPackages";

            if (!System.IO.Directory.Exists(ProgramDataPackagesDir))
            {
                System.IO.Directory.CreateDirectory(ProgramDataPackagesDir);
            }

            if (!System.IO.Directory.Exists(ProgramDataInstalledPackagesDir))
            {
                System.IO.Directory.CreateDirectory(ProgramDataInstalledPackagesDir);
            }




            if (!System.IO.File.Exists(ConfigDir + @"\ChainbotRobotSettings.xml"))
            {
                byte[] data = ChainbotRobot.Properties.Resources.ChainbotRobotSettings;
                System.IO.File.WriteAllBytes(ConfigDir + @"\ChainbotRobotSettings.xml", data);
            }
            else
            {
                if (UpgradeSettings())
                {
                    _logService.Debug(string.Format("Upgrade xml configuration file {0} ……", ConfigDir + @"\ChainbotRobotSettings.xml"), _logger);
                }
            }

        }


        private bool UpgradeSettings()
        {
            XmlDocument doc = new XmlDocument();
            var path = ConfigDir + @"\ChainbotRobotSettings.xml";
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            var schemaVersion = rootNode.GetAttribute("schemaVersion");

            XmlDocument docNew = new XmlDocument();

            using (var ms = new MemoryStream(ChainbotRobot.Properties.Resources.ChainbotRobotSettings))
            {
                ms.Flush();
                ms.Position = 0;
                docNew.Load(ms);
                ms.Close();
            }

            var rootNodeNew = docNew.DocumentElement;
            var schemaVersionNew = rootNodeNew.GetAttribute("schemaVersion");

            var schemaVersionTmp = schemaVersion;

            UpgradeSettings(ref schemaVersionTmp, schemaVersionNew);

            if (schemaVersion == schemaVersionTmp)
            {
                return false;
            }
            else
            {
                schemaVersion = schemaVersionTmp;
                return true;
            }
        }

        private void UpgradeSettings(ref string schemaVersion, string schemaVersionNew)
        {
            if (new Version(schemaVersion) >= new Version(schemaVersionNew))
            {
                return;
            }

            UpgradeSettings(ref schemaVersion, schemaVersionNew);
        }




    }
}
