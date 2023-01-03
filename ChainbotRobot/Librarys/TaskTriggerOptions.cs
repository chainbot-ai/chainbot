using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ChainbotRobot.Librarys
{
    public enum TaskTriggerType
    {
        TASK_TRIGGER_EVENT = 0,
        TASK_TRIGGER_TIME = 1,
        TASK_TRIGGER_DAILY = 2,
        TASK_TRIGGER_WEEKLY = 3,
        TASK_TRIGGER_MONTHLY = 4,
        TASK_TRIGGER_MONTHLYDOW = 5,
        TASK_TRIGGER_IDLE = 6,
        TASK_TRIGGER_REGISTRATION = 7,
        TASK_TRIGGER_BOOT = 8,
        TASK_TRIGGER_LOGON = 9,
        TASK_TRIGGER_SESSION_STATE_CHANGE = 11,
        TASK_TRIGGER_CUSTOM_TRIGGER_01 = 12
    }

    public class TaskTriggerOptions
    {

        public string TaskName { get; set; }

        public string Creator { get; set; }

        public string Description { get; set; }

        public string Interval { get; set; }

        public string StartBoundary { get; set; }

        public string EndBoundary { get; set; }

        public string ActionPath { get; set; }

        public string ActionArg { get; set; }
 
        public TaskTriggerType TaskTriggerType { get; set; }
    }
}
