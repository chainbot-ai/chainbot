using Chainbot.Contracts.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Log
{
    public class DebugLogService : ILogService
    {
        private ConsolePrintService console;
        private DiagnosticsDebugPrintService dbg;
        private Log4NetService log;

        public DebugLogService(ConsolePrintService console, DiagnosticsDebugPrintService dbg, Log4NetService log)
        {
            this.console = console;
            this.dbg = dbg;
            this.log = log;
        }

        public void Debug(object message, object logger = null)
        {
            console.Debug(message);
            dbg.Debug(message);
            log.Debug(message, logger);
        }

        public void Error(object message, object logger = null)
        {
            console.Error(message);
            dbg.Error(message);
            log.Error(message, logger);
        }

        public void Fatal(object message, object logger = null)
        {
            console.Fatal(message);
            dbg.Fatal(message);
            log.Fatal(message, logger);
        }

        public void Info(object message, object logger = null)
        {
            console.Info(message);
            dbg.Info(message);
            log.Info(message, logger);
        }

        public void Warn(object message, object logger = null)
        {
            console.Warn(message);
            dbg.Warn(message);
            log.Warn(message, logger);
        }
    }
}
