using log4net;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Cores.Utils
{
    public class AuthorizationService: IAuthorizationService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IPathConfigService _pathConfigService;
        private ILogService _logService;

        public AuthorizationService(IPathConfigService pathConfigService, ILogService logService)
        {
            _pathConfigService = pathConfigService;
            _logService = logService;
        }


        

    }
}
