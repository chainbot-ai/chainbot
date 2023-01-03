using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ChainbotRobot.Contracts
{
    public interface IAutoCloseMessageBoxService
    {
        MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon
            , MessageBoxResult defaultResult = MessageBoxResult.Yes);

        MessageBoxResult Show(string messageBoxText);
    }
}
