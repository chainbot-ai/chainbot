using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine.Contracts
{
    public interface IGlobalService
    {
        AutoResetEvent AutoResetEvent { get; set; }

        Options Options { get; set; }
    }
}
