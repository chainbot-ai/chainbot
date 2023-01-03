using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Log;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Log
{
    public class Log4NetService : MarshalByRefServiceBase, ILogService
    {
        public void Debug(object message, object logger = null)
        {
            if (logger != null)
            {
                (logger as ILog).Debug(message.ToString());
            }
        }

        public void Info(object message, object logger = null)
        {
            if (logger != null)
            {
                (logger as ILog).Info(message.ToString());
            }
        }

        public void Warn(object message, object logger = null)
        {
            if (logger != null)
            {
                (logger as ILog).Warn(message.ToString());
            }
        }

        public void Error(object message, object logger = null)
        {
            if (logger != null)
            {
                (logger as ILog).Error(message.ToString());
            }
        }

        public void Fatal(object message, object logger = null)
        {
            if (logger != null)
            {
                (logger as ILog).Fatal(message.ToString());
            }
        }

        
    }
}
