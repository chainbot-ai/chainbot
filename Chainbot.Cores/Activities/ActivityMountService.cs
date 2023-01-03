using Chainbot.Contracts.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.Classes;
using System.Xml;
using Chainbot.Resources.Librarys;
using System.IO;

namespace Chainbot.Cores.Activities
{
    public class ActivityMountService : IActivityMountService
    {
        public List<ActivityGroupOrLeafItem> Transform(string activityConfigXml)
        {
            var ret = new List<ActivityGroupOrLeafItem>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(activityConfigXml);
            var rootNode = doc.DocumentElement;

            InitChildren(rootNode, ref ret);

            return ret;
        }

        private void InitChildren(XmlNode node, ref List<ActivityGroupOrLeafItem> children)
        {
            var groupNodes = node.SelectNodes("Group");
            foreach (XmlNode groupNode in groupNodes)
            {
                var name = (groupNode as XmlElement).GetAttribute("Name");
                ActivityGroupItem item = new ActivityGroupItem();
                item.Name = name;

                InitChildren(groupNode, ref item.Children);

                children.Add(item);
            }

            var activitiesNodes = node.SelectNodes("Activity");
            foreach (XmlNode activityNode in activitiesNodes)
            {
                ActivityLeafItem item = new ActivityLeafItem();

                item.Name = (activityNode as XmlElement).GetAttribute("Name");
                item.TypeOf = (activityNode as XmlElement).GetAttribute("TypeOf");
                item.ToolTip = (activityNode as XmlElement).GetAttribute("ToolTip");
                item.Icon = (activityNode as XmlElement).GetAttribute("Icon");

                children.Add(item);
            }
        }


        public List<ActivityGroupOrLeafItem> Mount(List<ActivityGroupOrLeafItem> activitiesCurrent
            , List<ActivityGroupOrLeafItem> activitiesToMount)
        {
            List<ActivityGroupOrLeafItem> ret = new List<ActivityGroupOrLeafItem>();

            Mount(activitiesCurrent, activitiesToMount,ref ret);

            return ret;
        }


        private void Mount(List<ActivityGroupOrLeafItem> activitiesCurrent
            , List<ActivityGroupOrLeafItem> activitiesToMount,ref List<ActivityGroupOrLeafItem> ret)
        {
            foreach (var itemCurrent in activitiesCurrent)
            {
                ret.Add(itemCurrent);
            }

            foreach (var itemMount in activitiesToMount)
            {
                if (itemMount is ActivityLeafItem)
                {
                    ret.Add(itemMount);
                }
            }

            foreach (var itemMount in activitiesToMount)
            {
                if (itemMount is ActivityGroupItem)
                {
                    bool isGroupNameExist = false;

                    ActivityGroupItem itemMountAt = itemMount as ActivityGroupItem;
                    ActivityGroupItem itemCurrentAt = null;

                    foreach (var itemCurrent in activitiesCurrent)
                    {
                        if (itemCurrent is ActivityGroupItem && itemCurrent.Name == itemMount.Name)
                        {
                            isGroupNameExist = true;
                            itemCurrentAt = itemCurrent as ActivityGroupItem;
                            break;
                        }
                    }

                    if (isGroupNameExist)
                    {
                        foreach (var item in ret)
                        {
                            if(item is ActivityGroupItem && item.Name == itemMount.Name)
                            {
                                ret.Remove(item);
                                break;
                            }
                        }

                        ActivityGroupItem itemTemp = new ActivityGroupItem();
                        itemTemp.Name = itemCurrentAt.Name;
                        ret.Add(itemTemp);

                        Mount(itemCurrentAt.Children, itemMountAt.Children, ref itemTemp.Children);
                    }
                    else
                    {
                        ret.Add(itemMount);
                    }
                }
            }
        }
    }
}
