using Chainbot.Contracts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Chainbot.Cores.UI
{
    public class DispatcherService : IDispatcherService
    {
        public void Invoke(Action callback, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            Application.Current.Dispatcher.Invoke(callback, priority);
        }

        public DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            return Application.Current.Dispatcher.InvokeAsync(callback, priority);
        }
    }

}
