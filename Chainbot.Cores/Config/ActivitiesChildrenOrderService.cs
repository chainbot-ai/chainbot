using Chainbot.Contracts.Config;
using Chainbot.Resources.Librarys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chainbot.Cores.Config
{
    public class ActivitiesChildrenOrderService : IActivitiesChildrenOrderService
    {
        private bool _hasInit = false;

        private Dictionary<string, int> _orderDict = new Dictionary<string, int>();

        public void Init()
        {
            if(!_hasInit)
            {
                XmlDocument doc = new XmlDocument();

                byte[] encodedString = ResourcesLoader.ActivitiesChildrenOrder;

                using (var ms = new MemoryStream(encodedString))
                {
                    ms.Flush();
                    ms.Position = 0;
                    doc.Load(ms);
                    ms.Close();
                }

                var rootNode = doc.DocumentElement;

                var groupNodes = rootNode.SelectNodes("Group");

                int order = 0;
                foreach (XmlNode groupNode in groupNodes)
                {
                    order++;
                    var name = (groupNode as XmlElement).GetAttribute("Name");
                    _orderDict[name] = order;
                }
            }

            _hasInit = true;
        }

        public int GetOrder(string name,int initOrder)
        {
            if(_orderDict.ContainsKey(name))
            {
                return _orderDict[name];
            }

            return int.MaxValue - 100000 + initOrder;
        }
    }
}
