using Chainbot.Contracts.Utils;
using ChainbotRobot.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Cores
{
    public class AboutInfoService : IAboutInfoService
    {
        private string machineId;

        private ICommonService _commonService;
        private IAutoCloseMessageBoxService _autoCloseMessageBoxService;

        public AboutInfoService(ICommonService commonService, IAutoCloseMessageBoxService autoCloseMessageBoxService)
        {
            _commonService = commonService;
            _autoCloseMessageBoxService = autoCloseMessageBoxService;

            GetMachineId();
        }

        public string GetIp()
        {
            return _commonService.GetIp();
        }

        public string GetMachineId()
        {
            if (machineId != null)
            {
                return machineId;
            }

            try
            {
                machineId = $"{GetMachineUserName()}@{ _commonService.GetMachineId()}";

                return machineId;
            }
            catch (Exception err)
            {
                _autoCloseMessageBoxService.Show(err.ToString());
                return "";
            }
        }

        public string GetMachineName()
        {
            return Environment.MachineName;
        }

        public string GetMachineUserName()
        {
            return Environment.UserName;
        }

        public string GetVersion()
        {
            return _commonService.GetProgramVersion();
        }
    }
}
