using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chainbot.Contracts.Activities
{
    public interface ISystemActivityIconService
    {
        ImageSource GetIcon(string typeOf);
    }
}
