using Chainbot.Contracts.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.Classes;
using System.Xml;
using Chainbot.Contracts.Config;

namespace Chainbot.Cores.Activities
{
    public class ActivityFavoritesService : IActivityFavoritesService
    {
        private IPathConfigService _pathConfigService;

        public ActivityFavoritesService(IPathConfigService pathConfigService)
        {
            _pathConfigService = pathConfigService;
        }

        public void Add(string typeOf)
        {
            var xmlPath = _pathConfigService.FavoriteActivitiesXml;
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            var rootNode = doc.DocumentElement;

            var activitiesNodes = rootNode.SelectNodes("Activity");
            foreach (XmlNode activityNode in activitiesNodes)
            {
                var typeOfStr = (activityNode as XmlElement).GetAttribute("TypeOf");
                if(typeOfStr == typeOf)
                {
                    return;
                }
            }

            XmlElement activityElement = doc.CreateElement("Activity");
            activityElement.SetAttribute("TypeOf", typeOf);
            rootNode.PrependChild(activityElement);

            doc.Save(xmlPath);
        }

        public void Remove(string typeOf)
        {
            var xmlPath = _pathConfigService.FavoriteActivitiesXml;
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            var rootNode = doc.DocumentElement;

            var activitiesNodes = rootNode.SelectNodes("Activity");
            foreach (XmlNode activityNode in activitiesNodes)
            {
                var typeOfStr = (activityNode as XmlElement).GetAttribute("TypeOf");
                if (typeOfStr == typeOf)
                {
                    rootNode.RemoveChild(activityNode);
                    break;
                }
            }

            doc.Save(xmlPath);
        }

        public List<ActivityGroupOrLeafItem> Query()
        {
            var ret = new List<ActivityGroupOrLeafItem>();

            XmlDocument doc = new XmlDocument();
            doc.Load(_pathConfigService.FavoriteActivitiesXml);
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
