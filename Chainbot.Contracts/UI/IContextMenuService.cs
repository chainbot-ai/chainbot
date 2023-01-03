using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chainbot.Contracts.UI
{
    public interface IContextMenuService
    {
        ContextMenu Show(object dataContext,string resKey);
    }
}
