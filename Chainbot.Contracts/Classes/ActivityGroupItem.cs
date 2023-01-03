using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Classes
{
    public class ActivityGroupItem : ActivityGroupOrLeafItem
    {
        public List<ActivityGroupOrLeafItem> Children = new List<ActivityGroupOrLeafItem>();
    }
}
