using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Contracts
{
    public interface IAboutInfoService
    {
        string GetMachineName();

        string GetMachineUserName();

        string GetIp();

        string GetVersion();

        string GetMachineId();
    }
}
