using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Log
{
    public interface ILogService
    {
        void Debug(object message, object logger = null);
        void Info(object message, object logger = null);
        void Warn(object message, object logger = null);
        void Error(object message, object logger = null);
        void Fatal(object message, object logger = null);
    }
}
