using Chainbot.Contracts.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.App
{
    public class ConsoleManager : IConsoleManager
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        public bool Close()
        {
            return FreeConsole();
        }

        public bool Open()
        {
            return AllocConsole();
        }
    }
}
