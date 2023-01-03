using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskScheduler;
namespace ChainbotRobot.Librarys
{
    public class TaskSchedulerHelper
    {
        public static void DeleteTask(string taskName)
        {
            TaskSchedulerClass taskScheduler = new TaskSchedulerClass();
            taskScheduler.Connect(null, null, null, null);
            ITaskFolder folder = taskScheduler.GetFolder(@"\");
            folder.DeleteTask(taskName, 0);
        }

        public static IRegisteredTaskCollection GetAllRegisteredTasks()
        {
            TaskSchedulerClass taskScheduler = new TaskSchedulerClass();
            taskScheduler.Connect(null, null, null, null);
            ITaskFolder folder = taskScheduler.GetFolder(@"\");
            IRegisteredTaskCollection registeredTasks = folder.GetTasks(1);
            return registeredTasks;
        }

        public static bool IsExists(string taskName)
        {
            IRegisteredTaskCollection registeredTasks = GetAllRegisteredTasks();
            return registeredTasks.Cast<IRegisteredTask>().Any(task => task.Name.Equals(taskName));
        }

        public static IRegisteredTask CreateTaskScheduler(TaskTriggerOptions options, ITriggerSet triggerSet = null)
        {
            try
            {
                if (IsExists(options.TaskName))
                {
                    DeleteTask(options.TaskName);
                }
 
                TaskSchedulerClass scheduler = new TaskSchedulerClass();
                //pc-name/ip,username,domain,password
                scheduler.Connect(null, null, null, null);

                ITaskDefinition task = scheduler.NewTask(0);
                task.RegistrationInfo.Author = options.Creator; 
                task.RegistrationInfo.Description = options.Description; 
                task.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;

                var trigger = task.Triggers.Create((_TASK_TRIGGER_TYPE2)options.TaskTriggerType);
                trigger.Repetition.Interval = options.Interval;
                trigger.Enabled = true;
                trigger.StartBoundary = options.StartBoundary;
                trigger.EndBoundary = options.EndBoundary;
                triggerSet?.Set(trigger);

                IExecAction action = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Path = options.ActionPath; 
                action.Arguments = options.ActionArg;

                task.Settings.ExecutionTimeLimit = "PT0S";
                task.Settings.DisallowStartIfOnBatteries = false; 
                task.Settings.RunOnlyIfIdle = false;

                ITaskFolder folder = scheduler.GetFolder(@"\");
                IRegisteredTask regTask = folder.RegisterTaskDefinition(options.TaskName, task,
                (int)_TASK_CREATION.TASK_CREATE, null, //user
                null, // password
                _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN);
                return regTask;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
