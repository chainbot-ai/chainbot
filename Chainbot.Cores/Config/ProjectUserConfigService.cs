using Chainbot.Contracts.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chainbot.Cores.Config
{
    public class ProjectUserConfigService : IProjectUserConfigService
    {
        public string ProjectCreatePath { get; set; }

        IPathConfigService _pathConfigService;

        public ProjectUserConfigService(IPathConfigService pathConfigService)
        {
            _pathConfigService = pathConfigService;
        }

        public bool Load()
        {
            XmlDocument doc = new XmlDocument();
            var path = _pathConfigService.ProjectUserConfigXml;
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            var defaultCreatePath = rootNode.GetAttribute("DefaultCreatePath");
            if (string.IsNullOrEmpty(defaultCreatePath) || !Directory.Exists(defaultCreatePath))
            {
                defaultCreatePath = _pathConfigService.ProjectsDir;

                if (!Directory.Exists(defaultCreatePath))
                {
                    Directory.CreateDirectory(defaultCreatePath);
                }
            }

            ProjectCreatePath = defaultCreatePath;

            return true;
        }

        public bool Save()
        {
            XmlDocument doc = new XmlDocument();
            var path = _pathConfigService.ProjectUserConfigXml;
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            rootNode.SetAttribute("DefaultCreatePath", ProjectCreatePath);

            doc.Save(path);

            return true;
        }
    }
}
