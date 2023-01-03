using ChainbotRobot.CommandLine.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine.Cores
{
    public class RegisterService: IRegisterService
    {
        public string _localChainbotStudioDir { get; private set; }

        public RegisterService()
        {
            var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            _localChainbotStudioDir = localAppData + @"\ChainbotStudio";
        }
    }
}
