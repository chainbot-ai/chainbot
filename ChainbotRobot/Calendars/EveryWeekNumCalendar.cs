using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Calendars
{
    [Serializable]
    public class EveryWeekNumCalendar : BaseCalendar
    {
        private int _num;

        private DateTime _beginDateTime;

        public EveryWeekNumCalendar(int num,DateTime beginDateTime)
        {
            _num = num;
            _beginDateTime = beginDateTime;
        }
        
        private int GetWeekOfYear(DateTime dt)
        {
            Calendar cal = new CultureInfo("en-US").Calendar;
            int week = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return week;
        }

        public override bool IsTimeIncluded(DateTimeOffset timeUtc)
        {
            timeUtc = TimeZoneUtil.ConvertTime(timeUtc, TimeZone);
            int beginWeekOfYear = GetWeekOfYear(_beginDateTime);
            int currentWeekOfYear = GetWeekOfYear(timeUtc.DateTime);

            if((currentWeekOfYear - beginWeekOfYear) % _num == 0)
            {
                return true;
            }

            return false;
        }

        public override DateTimeOffset GetNextIncludedTimeUtc(DateTimeOffset timeUtc)
        {
            timeUtc = TimeZoneUtil.ConvertTime(timeUtc, TimeZone);
            return DateTimeOffset.MinValue;
        }

        public override ICalendar Clone()
        {
            EveryWeekNumCalendar clone = new EveryWeekNumCalendar(_num, _beginDateTime);
            CloneFields(clone);
            return clone;
        }

       
    }
}

