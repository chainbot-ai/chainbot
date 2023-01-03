using Chainbot.Contracts.App;
using Chainbot.Contracts.Config;
using Chainbot.Resources.Librarys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chainbot.Cores.App
{
    public class UpgradeAppSettingsService : IUpgradeAppSettingsService
    {
        private IPathConfigService _pathConfigService;
        private IServiceLocator _serviceLocator;

        public UpgradeAppSettingsService(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public bool Upgrade()
        {
            _pathConfigService = _serviceLocator.ResolveType<IPathConfigService>();

            XmlDocument doc = new XmlDocument();
            var path = _pathConfigService.AppSettingsXml;
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            var schemaVersion = rootNode.GetAttribute("schemaVersion");

            XmlDocument docNew = new XmlDocument();

            using (var ms = new MemoryStream(ResourcesLoader.AppSettings))
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
