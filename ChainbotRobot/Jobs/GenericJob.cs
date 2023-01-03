using log4net;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library.Librarys;
using Quartz;
using Chainbot.Contracts.Log;
using ChainbotRobot.Contracts;
using ChainbotRobot.Librarys;
using ChainbotRobot.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Jobs
{
    public class GenericJob : IJob
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ILogService _logService;

        private ServiceRegistry _locator = UserServiceRegistry.Instance;

        private IScheduledTasksService _scheduledTasksService;

        private ScheduledTaskManagementViewModel _scheduledTaskManagementViewModel;
        private IPackageService _packageService;

        public GenericJob()
        {
            _scheduledTaskManagementViewModel = _locator.ResolveType<ScheduledTaskManagementViewModel>();
            _scheduledTasksService = _locator.ResolveType<IScheduledTasksService>();
            _packageService = _locator.ResolveType<IPackageService>();
            _logService = _locator.ResolveType<ILogService>();
        }

        public Task Execute(IJobExecutionContext context)
        {
            var jobj = new JObject();
            jobj["type"] = "valid";

            var nextRunDTO = context.NextFireTimeUtc;

            _scheduledTasksService.ProcessNextRunDTO(nextRunDTO,jobj);
            _scheduledTasksService.JobIdNextRunDescDict[context.JobDetail.JobDataMap.GetString("Id")] = jobj;

            Common.RunInUI(() => {
                _scheduledTaskManagementViewModel.RefreshScheduledTaskItems();
            });

            _logService.Debug($"本地定时任务触发，开始执行流程{context.JobDetail.JobDataMap.GetString("Name")}……");
            _packageService.Run(context.JobDetail.JobDataMap.GetString("Name"));

            return Task.FromResult(true);
        }
    }
}
