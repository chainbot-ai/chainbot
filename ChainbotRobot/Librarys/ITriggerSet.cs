using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskScheduler;
namespace ChainbotRobot.Librarys
{
    public interface ITriggerSet
    {
        void Set(ITrigger trigger);
    }
}
