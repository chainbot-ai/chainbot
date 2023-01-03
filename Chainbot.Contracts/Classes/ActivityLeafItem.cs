using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chainbot.Contracts.Classes
{
    public class ActivityLeafItem: ActivityGroupOrLeafItem
    {
        public string Icon { get; set; }
        public string TypeOf { get; set; }
        public string ToolTip { get; set; }
    }
}
