using ChainbotRobot.Contracts;
using System;
using System.Collections.Generic;
using ChainbotRobot.Database;
using Chainbot.Contracts.App;
using Quartz;
using Quartz.Impl;
using ChainbotRobot.Jobs;
using System.Globalization;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using static ChainbotRobot.ViewModel.ScheduledTaskViewModel;
using ChainbotRobot.Calendars;
using Chainbot.Contracts.Log;
using log4net;
using System.Windows;

namespace ChainbotRobot.Cores
{
    public class ScheduledTasksService : IScheduledTasksService
    {
        private ILogService _logService;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IServiceLocator _serviceLocator;

        public ConcurrentDictionary<string, JObject> JobIdNextRunDescDict { get; set; } = new ConcurrentDictionary<string, JObject>();

        private ISchedulerFactory _factory;
        private IScheduler _scheduler;

        public ScheduledTasksService(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _logService = _serviceLocator.ResolveType<ILogService>();

            Init();
        }

        private async void Init()
        {
            _factory = new StdSchedulerFactory();
            _scheduler = await _factory.GetScheduler();
            await _scheduler.Start();
        }

        public void AddJobs(List<ScheduledTasksTable> items)
        {
            foreach (var item in items)
            {
                AddJob(item);
            }
        }

        public async void AddJob(ScheduledTasksTable item)
        {
            IJobDetail job = JobBuilder.Create<GenericJob>()
                .WithIdentity("JobId#" + item.Id.ToString(), "JobGroup")
                .UsingJobData("Id", item.Id.ToString())
                .UsingJobData("Name", item.Name)
                .UsingJobData("BeginDate", item.BeginDate)
                .UsingJobData("RunTime", item.RunTime)
                .UsingJobData("FrequencyType", item.FrequencyType)
                .UsingJobData("EveryNum", item.EveryNum)
                .UsingJobData("WeekDays", item.WeekDays)
                .UsingJobData("MonthType", item.MonthType)
                .UsingJobData("MonthValue", item.MonthValue)
                .Build();

            ITrigger trigger = null;

            var beginDateTime = DateTime.ParseExact(item.BeginDate + " " + item.RunTime, "yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);

            var jobj = new JObject();
            jobj["type"] = "valid";

            EveryWeekNumCalendar everyWeekNumCalendar = null;
            DayOfWeekOrderCalendar dayOfWeekOrderCalendar = null;

            switch (item.FrequencyType)
            {
                case "Once":
                    if (beginDateTime >= DateTime.Now)
                    {
                        trigger = TriggerBuilder.Create()
                           .WithIdentity("TriggerId#" + item.Id.ToString(), "JobGroup")
                           .StartAt(beginDateTime)
                           .Build();
                    }
                    else
                    {
                        jobj["type"] = "invalid";
                        jobj["msg"] = "Invalid, the process is no longer running";
                    }
                    break;
                case "Daily":
                    trigger = TriggerBuilder.Create()
                           .WithIdentity("TriggerId#" + item.Id.ToString(), "JobGroup")
                           .StartAt(beginDateTime)
                           .WithSchedule(
                                CalendarIntervalScheduleBuilder.Create()
                                .WithIntervalInDays(item.EveryNum)
                                .WithMisfireHandlingInstructionDoNothing()
                                )
                           .Build();
                    break;
                case "Weekly":
                    var list = new List<DayOfWeek>();

                    if ((item.WeekDays & (int)WeekEnumWithFlags.Monday) > 0)
                    {
                        list.Add(DayOfWeek.Monday);
                    }
                    if ((item.WeekDays & (int)WeekEnumWithFlags.Tuesday) > 0)
                    {
                        list.Add(DayOfWeek.Tuesday);
                    }
                    if ((item.WeekDays & (int)WeekEnumWithFlags.Wednesday) > 0)
                    {
                        list.Add(DayOfWeek.Wednesday);
                    }
                    if ((item.WeekDays & (int)WeekEnumWithFlags.Thursday) > 0)
                    {
                        list.Add(DayOfWeek.Thursday);
                    }
                    if ((item.WeekDays & (int)WeekEnumWithFlags.Friday) > 0)
                    {
                        list.Add(DayOfWeek.Friday);
                    }
                    if ((item.WeekDays & (int)WeekEnumWithFlags.Saturday) > 0)
                    {
                        list.Add(DayOfWeek.Saturday);
                    }
                    if ((item.WeekDays & (int)WeekEnumWithFlags.Sunday) > 0)
                    {
                        list.Add(DayOfWeek.Sunday);
                    }

                    everyWeekNumCalendar = new EveryWeekNumCalendar(item.EveryNum, beginDateTime);
                    await _scheduler.AddCalendar("EveryWeekNumCalendar#" + item.Id.ToString(), everyWeekNumCalendar, true, true);

                    trigger = TriggerBuilder.Create()
                          .WithIdentity("TriggerId#" + item.Id.ToString(), "JobGroup")
                          .StartAt(beginDateTime)
                          .WithSchedule(
                                CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(beginDateTime.Hour, beginDateTime.Minute, list.ToArray())
                                .WithMisfireHandlingInstructionDoNothing()
                                )
                          .ModifiedByCalendar("EveryWeekNumCalendar#" + item.Id.ToString())
                          .Build();
                    break;
                case "Monthly":
                    if (item.MonthType == "Day")
                    {
                        trigger = TriggerBuilder.Create()
                          .WithIdentity("TriggerId#" + item.Id.ToString(), "JobGroup")
                          .StartAt(beginDateTime)
                          .WithSchedule(
                                CronScheduleBuilder.CronSchedule($"0 {beginDateTime.Minute} {beginDateTime.Hour} {item.MonthValue} {beginDateTime.Month}/{item.EveryNum} ? *")//cron表达式
                                .WithMisfireHandlingInstructionDoNothing()
                                )
                          .Build();
                    }
                    else
                    {
                        if (item.MonthValue == "Day")
                        {
                            string dayNumStr = "";
                            if (item.MonthType == "First")
                            {
                                dayNumStr = "1";
                            }
                            else if (item.MonthType == "Second")
                            {
                                dayNumStr = "2";
                            }
                            else if (item.MonthType == "Third")
                            {
                                dayNumStr = "3";
                            }
                            else if (item.MonthType == "Fourth")
                            {
                                dayNumStr = "4";
                            }
                            else if (item.MonthType == "Last")
                            {
                                dayNumStr = "L";
                            }

                            if(!string.IsNullOrEmpty(dayNumStr))
                            {
                                trigger = TriggerBuilder.Create()
                                  .WithIdentity("TriggerId#" + item.Id.ToString(), "JobGroup")
                                  .StartAt(beginDateTime)
                                  .WithSchedule(
                                        CronScheduleBuilder.CronSchedule($"0 {beginDateTime.Minute} {beginDateTime.Hour} {dayNumStr} {beginDateTime.Month}/{item.EveryNum} ? *")//cron表达式
                                        .WithMisfireHandlingInstructionDoNothing()
                                        )
                                  .Build();
                            }
                        }
                        else
                        {
                            int dayOfWeekCalendarOrder = 0;

                            string dayOfWeekOrder = "";
                            if (item.MonthType == "First")
                            {
                                dayOfWeekOrder = "#1";
                                dayOfWeekCalendarOrder = 1;
                            }
                            else if (item.MonthType == "Second")
                            {
                                dayOfWeekOrder = "#2";
                                dayOfWeekCalendarOrder = 2;
                            }
                            else if (item.MonthType == "Third")
                            {
                                dayOfWeekOrder = "#3";
                                dayOfWeekCalendarOrder = 3;
                            }
                            else if (item.MonthType == "Fourth")
                            {
                                dayOfWeekOrder = "#4";
                                dayOfWeekCalendarOrder = 4;
                            }
                            else if (item.MonthType == "Last")
                            {
                                dayOfWeekOrder = "L";
                                dayOfWeekCalendarOrder = -1;
                            }

                            string dayOfWeekDays = "";
                            if (item.MonthValue == "Weekday")
                            {
                                dayOfWeekDays = "MON-FRI";
                                dayOfWeekOrder = "";

                                dayOfWeekOrderCalendar = new DayOfWeekOrderCalendar(dayOfWeekCalendarOrder, item.MonthValue,beginDateTime);
                            }
                            else if(item.MonthValue == "WeekendDay")
                            {
                                dayOfWeekDays = "SAT-SUN";
                                dayOfWeekOrder = "";
                                dayOfWeekOrderCalendar = new DayOfWeekOrderCalendar(dayOfWeekCalendarOrder, item.MonthValue, beginDateTime);
                            }
                            else if (item.MonthValue == "Monday")
                            {
                                dayOfWeekDays = "MON";
                            }
                            else if (item.MonthValue == "Tuesday")
                            {
                                dayOfWeekDays = "TUE";
                            }
                            else if (item.MonthValue == "Wednesday")
                            {
                                dayOfWeekDays = "WED";
                            }
                            else if (item.MonthValue == "Thursday")
                            {
                                dayOfWeekDays = "THU";
                            }
                            else if (item.MonthValue == "Friday")
                            {
                                dayOfWeekDays = "FRI";
                            }
                            else if (item.MonthValue == "Saturday")
                            {
                                dayOfWeekDays = "SAT";
                            }
                            else if (item.MonthValue == "Sunday")
                            {
                                dayOfWeekDays = "SUN";
                            }

                            if (dayOfWeekOrderCalendar != null)
                            {
                                await _scheduler.AddCalendar("DayOfWeekOrderCalendar#" + item.Id.ToString(), dayOfWeekOrderCalendar, true, true);
                            }

                            var triggerBuilder = TriggerBuilder.Create()
                                     .WithIdentity("TriggerId#" + item.Id.ToString(), "JobGroup")
                                     .StartAt(beginDateTime)
                                     .WithSchedule(
                                           CronScheduleBuilder.CronSchedule($"0 {beginDateTime.Minute} {beginDateTime.Hour} ? {beginDateTime.Month}/{item.EveryNum} {dayOfWeekDays}{dayOfWeekOrder} *")
                                           .WithMisfireHandlingInstructionDoNothing()
                                           );

                            if (dayOfWeekOrderCalendar != null)
                            {
                                trigger = triggerBuilder.ModifiedByCalendar("DayOfWeekOrderCalendar#" + item.Id.ToString()).Build();
                            }
                            else
                            {
                                trigger = triggerBuilder.Build();
                            }
                        }
                    }

                    break;
            }

            if (trigger != null)
            {
                try
                {
                    await _scheduler.ScheduleJob(job, trigger);
                }
                catch (Exception err)
                {
                    string info = $"The scheduled task of the process<{item.Name}>triggers an exception. Exception information:{err}";
                    _logService.Error(info, _logger);
                    MessageBox.Show(info, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }

            var nextRunDTO = trigger?.GetNextFireTimeUtc()?.ToLocalTime();
            if (nextRunDTO < DateTime.Now)
            {
                nextRunDTO = trigger?.GetFireTimeAfter(DateTime.Now);

                if (item.FrequencyType == "Weekly")
                {
                    while (!everyWeekNumCalendar.IsTimeIncluded(nextRunDTO.Value))
                    {
                        nextRunDTO = trigger?.GetFireTimeAfter(nextRunDTO);
                    }
                }else if(item.FrequencyType == "Monthly" && item.MonthType !="Day" && (item.MonthValue == "Weekday" || item.MonthValue == "WeekendDay"))
                {
                    while (!dayOfWeekOrderCalendar.IsTimeIncluded(nextRunDTO.Value))
                    {
                        nextRunDTO = trigger?.GetFireTimeAfter(nextRunDTO);
                    }
                }
            }

            ProcessNextRunDTO(nextRunDTO, jobj);

            JobIdNextRunDescDict[item.Id.ToString()] = jobj;
        }



        public async void RemoveJob(ScheduledTasksTable item)
        {
            JobKey jobKey = new JobKey("JobId#" + item.Id.ToString(), "JobGroup");
            await _scheduler.DeleteJob(jobKey);
        }

        public void UpdateJob(ScheduledTasksTable item)
        {
            RemoveJob(item);
            AddJob(item);
        }


        public void ProcessNextRunDTO(DateTimeOffset? nextRunDTO, JObject jobj)
        {
            if (nextRunDTO == null)
            {
                jobj["type"] = "invalid";
                jobj["msg"] = "Invalid. The scheduled task is no longer triggered.";
            }
            else
            {
                if (nextRunDTO?.LocalDateTime >= DateTime.Now)
                {
                    jobj["msg"] = nextRunDTO?.LocalDateTime.ToString();
                }
                else
                {
                    jobj["type"] = "invalid";
                    jobj["msg"] = "Invalid, the trigger time of the scheduled task has passed.";
                }
            }
        }
    }
}
