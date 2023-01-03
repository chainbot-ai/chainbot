using ChainbotRobot.CommandLine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine.Cores
{
    public class GlobalService : IGlobalService
    {
        public AutoResetEvent AutoResetEvent { get; set; } = new AutoResetEvent(false);

        public Options Options { get; set; } = new Options();
    }
}
