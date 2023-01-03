using Chainbot.Contracts.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using System.Xml;

namespace Chainbot.Cores.Activities
{
    public class ActivityRecentService : IActivityRecentService
    {
        private IPathConfigService _pathConfigService;
        private IConstantConfigService _constantConfigService;

        public ActivityRecentService(IPathConfigService pathConfigService, IConstantConfigService constantConfigService)
        {
            _pathConfigService = pathConfigService;
            _constantConfigService = constantConfigService;
        }

        public void Add(string typeOf)
        {
            XmlDocument doc = new XmlDocument();
            var path = _pathConfigService.RecentActivitiesXml;
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            var activityNodes = rootNode.SelectNodes("Activity");

            while (activityNodes.Count >= _constantConfigService.ActivitiesRecentGroupMaxRecordCount)
            {
                rootNode.RemoveChild(rootNode.LastChild);
                activityNodes = rootNode.SelectNodes("Activity");
            }

            foreach (XmlElement item in activityNodes)
            {
                var typeOfStr = item.GetAttribute("TypeOf");
                if (typeOfStr == typeOf)
                {
                    rootNode.RemoveChild(item);
                    break;
                }
            }

            XmlElement activityElement = doc.CreateElement("Activity");
            activityElement.SetAttribute("TypeOf", typeOf);
            rootNode.PrependChild(activityElement);

            doc.Save(path);
        }

        public List<ActivityGroupOrLeafItem> Query()
        {
            var ret = new List<ActivityGroupOrLeafItem>();

            XmlDocument doc = new XmlDocument();
            doc.Load(_pathConfigService.RecentActivitiesXml);
            var rootNode = doc.DocumentElement;

            var activitiesNodes = rootNode.SelectNodes("Activity");
            foreach (XmlNode activityNode in activitiesNodes)
            {
                var TypeOf = (activityNode as XmlElement).GetAttribute("TypeOf");
                ActivityLeafItem item = new ActivityLeafItem();
                item.TypeOf = TypeOf;
                ret.Add(item);
            }

            return ret;
        }
    }
}
