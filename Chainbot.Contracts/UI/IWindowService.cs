using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Contracts.UI
{
    public interface IWindowService
    {
        bool IsMinimized { get; }
        bool IsMaximized { get; }
        bool IsNormal { get; }

        bool? ShowDialog(Window window, bool bOwner = true);
        void Show(Window window, bool bOwner = true);

        void ShowMainWindowMinimized();
        void ShowMainWindowMaximized();
        void ShowMainWindowNormal(bool bCheckMinimized = true);
    }
}
