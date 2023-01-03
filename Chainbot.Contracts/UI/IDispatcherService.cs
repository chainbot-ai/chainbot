using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Chainbot.Contracts.UI
{
    public interface IDispatcherService
    {
        void Invoke(Action callback, DispatcherPriority priority = DispatcherPriority.Normal);

        DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority = DispatcherPriority.Normal);
    }
}
