using log4net.Config;
using Plugins.Shared.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Classes
{
    public static class Log4NetHelper
    {
        public static void Init()
        {
            string logConfigPath = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, "ChainbotStudio.Log4net.config");
            XmlConfigurator.Configure(new FileStream(logConfigPath, FileMode.Open));
        }
    }
}
