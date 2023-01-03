using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Contracts
{
    public interface IPackageService
    {
        void Run(string name, string version);
        void Run(string name);

        bool IsPackageNameExist(string name);
    }
}
