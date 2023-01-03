using Chainbot.Contracts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Chainbot.Cores.UI
{
    public class ContextMenuService : IContextMenuService
    {
        public ContextMenu Show(object dataContext, string resKey)
        {
            var view = Application.Current.MainWindow;
            var cm = view.FindResource(resKey) as ContextMenu;
            cm.DataContext = dataContext;
            cm.Placement = PlacementMode.MousePoint;
            cm.IsOpen = true;
            return cm;
        }
    }
}
