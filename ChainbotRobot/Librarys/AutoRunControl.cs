using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChainbotRobot.Librarys
{
    public static class AutoRunControl
    {
        private const string TaskSchedulerName = "ChainbotRobotAutoRun";

        public static void AutoRun(bool isAutoRun)
        {
            try
            {
                if (HasUAC())
                {
                    TaskSchedulerStart(isAutoRun);
                }
                else
                {
                    RegeditStart(isAutoRun);
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private static bool HasUAC()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            int versionMajor = osInfo.Version.Major;
            return versionMajor >= 6;
        }

        private static void RegeditStart(bool isAutoRun)
        {
            string filefullpath = Application.ExecutablePath;
            string appName = Path.GetFileNameWithoutExtension(filefullpath);
            string regPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey _rlocal = Registry.LocalMachine.OpenSubKey(regPath, true);
            if (_rlocal == null) _rlocal = Registry.LocalMachine.CreateSubKey(regPath);
            if (isAutoRun)
            {
                _rlocal.SetValue(appName, string.Format(@"""{0}""", filefullpath));
            }
            else
            {
                _rlocal.DeleteValue(appName, false);
            }
            _rlocal.Close();
        }

        private static void TaskSchedulerStart(bool isAutoRun)
        {
            if (isAutoRun)
            {
                var options = new TaskTriggerOptions
                {
                    TaskName = TaskSchedulerName,
                    Creator = "ChainbotRobot",
                    Description = "Start up and automatically run the Chainbot Robot.",
                    ActionPath = Application.ExecutablePath,
                    TaskTriggerType = TaskTriggerType.TASK_TRIGGER_LOGON
                };
                TaskSchedulerHelper.CreateTaskScheduler(options);
            }
            else
            {
                if (TaskSchedulerHelper.IsExists(TaskSchedulerName))
                    TaskSchedulerHelper.DeleteTask(TaskSchedulerName);
            }
        }
    }
}
