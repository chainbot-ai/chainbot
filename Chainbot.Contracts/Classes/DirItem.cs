using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Classes
{
    public class DirItem : DirOrFileItem
    {
        public List<DirOrFileItem> Children = new List<DirOrFileItem>();
    }
}
