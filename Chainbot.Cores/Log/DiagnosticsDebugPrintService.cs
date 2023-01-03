using Chainbot.Contracts.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Log
{
    public class DiagnosticsDebugPrintService : ILogService
    {
        public void Debug(object message, object logger = null)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Error(object message, object logger = null)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Fatal(object message, object logger = null)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Info(object message, object logger = null)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Warn(object message, object logger = null)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
