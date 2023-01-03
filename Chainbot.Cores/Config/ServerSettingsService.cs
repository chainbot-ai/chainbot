using Chainbot.Contracts.Classes;
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
    public class ServerSettingsService : MarshalByRefServiceBase, IServerSettingsService
    {
        public string ControlServerUrl { get; set; }

        public string CCLX { get; set; } = "FILE";

        public string AIServerUrl { get; set; } = "http://127.0.0.1:18888";

        public string HelpLinkUrl { get; set; } = "https://chainbot.ai";

        private IPathConfigService _pathConfigService;
        private object LogLock = new object();

        public ServerSettingsService(IPathConfigService pathConfigService)
        {
            _pathConfigService = pathConfigService;
        }


        public void Load()
        {
            XmlDocument doc = new XmlDocument();
            var path = _pathConfigService.ServerSettingsXml;
            doc.Load(path);
            var rootNode = doc.DocumentElement;
            var controlServerElement = rootNode.SelectSingleNode("ControlServer") as XmlElement;
          
            ControlServerUrl = controlServerElement.SelectSingleNode("ControlServerUrl").InnerText;

            if (ControlServerUrl.EndsWith("/"))
            {
                ControlServerUrl = ControlServerUrl.Substring(0, ControlServerUrl.Length - 1);
            }

            if (controlServerElement.SelectSingleNode("CCLX") != null)
            {
                CCLX = controlServerElement.SelectSingleNode("CCLX").InnerText;
            }

            var aiServerElement = rootNode.SelectSingleNode("AIServer") as XmlElement;
            if (aiServerElement?.SelectSingleNode("AIServerUrl") != null)
            {
                AIServerUrl = aiServerElement.SelectSingleNode("AIServerUrl").InnerText;
            }


            if (AIServerUrl.EndsWith("/"))
            {
                AIServerUrl = AIServerUrl.Substring(0, AIServerUrl.Length - 1);
            }


            var helpLinkElement = rootNode.SelectSingleNode("HelpLink") as XmlElement;   
            if (helpLinkElement?.SelectSingleNode("HelpLinkUrl") != null)
            {
                HelpLinkUrl = helpLinkElement.SelectSingleNode("HelpLinkUrl").InnerText;
            }
        }


        public void Save()
        {
            lock (LogLock)
            {
                XmlDocument doc = new XmlDocument();
                var path = _pathConfigService.ServerSettingsXml;
                doc.Load(path);
                var rootNode = doc.DocumentElement;

                if (ControlServerUrl.EndsWith("/"))
                {
                    ControlServerUrl = ControlServerUrl.Substring(0, ControlServerUrl.Length - 1);
                }

                var controlServerElement = rootNode.SelectSingleNode("ControlServer") as XmlElement;
                controlServerElement.SelectSingleNode("ControlServerUrl").InnerText = ControlServerUrl;

                if (AIServerUrl.EndsWith("/"))
                {
                    AIServerUrl = AIServerUrl.Substring(0, AIServerUrl.Length - 1);
                }

                var aiServerElement = rootNode.SelectSingleNode("AIServer") as XmlElement;
                XmlNode aiServerUrlElement;
                if (aiServerElement != null)
                {
                    aiServerUrlElement = aiServerElement.SelectSingleNode("AIServerUrl") as XmlElement;
                    aiServerUrlElement.InnerText = AIServerUrl;
                }
                else
                {
                    aiServerElement = doc.CreateElement("AIServer");
                    aiServerUrlElement = doc.CreateElement("AIServerUrl");
                    aiServerUrlElement.InnerText = AIServerUrl;
                    aiServerElement.AppendChild(aiServerUrlElement);
                    rootNode.AppendChild(aiServerElement);
                }

                doc.Save(path);
            }        
        }
    }
}
