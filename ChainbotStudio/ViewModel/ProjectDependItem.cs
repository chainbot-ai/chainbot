using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using log4net;

namespace ChainbotStudio.ViewModel
{
    public class ProjectDependItem : ProjectBaseItemViewModel
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IServiceLocator _serviceLocator;

        private ILogService _logService;

        public ProjectDependItem(IServiceLocator serviceLocator) : base(serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _logService = _serviceLocator.ResolveType<ILogService>();
        }

        
    }
}
