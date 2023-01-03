using Newtonsoft.Json.Linq;
using ChainbotRobot.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Contracts
{
    public interface IScheduledTasksService
    {
        ConcurrentDictionary<string, JObject> JobIdNextRunDescDict { get; set; }

        void AddJobs(List<ScheduledTasksTable> items);
        void AddJob(ScheduledTasksTable item);
        void RemoveJob(ScheduledTasksTable item);
        void UpdateJob(ScheduledTasksTable item);
        void ProcessNextRunDTO(DateTimeOffset? nextRunDTO, JObject jobj);
    }
}
