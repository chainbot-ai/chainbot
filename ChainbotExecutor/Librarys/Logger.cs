using log4net;
using System;
using System.Threading.Tasks;

namespace ChainbotExecutor.Librarys
{
    public class Logger
    {
        public static void Debug(object message, ILog logger = null)
        {
#if Chainbot_EXECUTOR_CONSOLE_DEBUG
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
#endif
            if (logger != null)
            {
                logger.Debug(message.ToString());
            }
        }

        public static void Info(object message, ILog logger = null)
        {
#if Chainbot_EXECUTOR_CONSOLE_DEBUG
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
#endif
            if (logger != null)
            {
                logger.Info(message.ToString());
            }
        }


        public static void Warn(object message, ILog logger = null)
        {
#if Chainbot_EXECUTOR_CONSOLE_DEBUG
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
#endif
            if (logger != null)
            {
                logger.Warn(message.ToString());
            }
        }

        public static void Error(object message, ILog logger = null)
        {
#if Chainbot_EXECUTOR_CONSOLE_DEBUG
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
#endif
            if (logger != null)
            {
                logger.Error(message.ToString());
            }
        }


        public static void Fatal(object message, ILog logger = null)
        {
#if Chainbot_EXECUTOR_CONSOLE_DEBUG
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
#endif
            if (logger != null)
            {
                logger.Fatal(message.ToString());
            }
        }
    }
}
