using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Calendars
{
    [Serializable]
    public class DayOfWeekOrderCalendar : BaseCalendar
    {
        private int _dayOfWeekCalendarOrder;
        private string _monthValue;
        private DateTime _beginDateTime;

        public DayOfWeekOrderCalendar(int dayOfWeekCalendarOrder, string monthValue, DateTime beginDateTime)
        {
            this._dayOfWeekCalendarOrder = dayOfWeekCalendarOrder;
            this._monthValue = monthValue;
            this._beginDateTime = beginDateTime;
        }


        public DateTime? FindMonthWeekday(int year, int month, int offset)
        {
            if (offset == 0 || offset > DateTime.DaysInMonth(year, month))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if(offset > 0)
            {
                DateTime moment = new DateTime(year, month, 1);
                while (moment.Month == month)
                {
                    DayOfWeek dayOfWeek = moment.DayOfWeek;
                    if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
                    {
                        offset--;
                    }
                    if (offset == 0)
                    {
                        return moment;
                    }
                    moment = moment.AddDays(1);
                }
            }
            else if (offset < 0)
            {
                DateTime moment = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                while (moment.Month == month)
                {
                    DayOfWeek dayOfWeek = moment.DayOfWeek;
                    if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
                    {
                        offset++;
                    }

                    if (offset == 0)
                    {
                        return moment;
                    }
                    moment = moment.AddDays(-1);
                }
            }
            else
            {

            }

            return null;
        }


        public DateTime? FindMonthWeekendDay(int year, int month, int offset)
        {
            if (offset == 0 || offset > DateTime.DaysInMonth(year, month))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (offset > 0)
            {
                DateTime moment = new DateTime(year, month, 1);
                while (moment.Month == month)
                {
                    DayOfWeek dayOfWeek = moment.DayOfWeek;
                    if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                    {
                        offset--;
                    }
                    if (offset == 0)
                    {
                        return moment;
                    }
                    moment = moment.AddDays(1);
                }
            }
            else if (offset < 0)
            {
                DateTime moment = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                while (moment.Month == month)
                {
                    DayOfWeek dayOfWeek = moment.DayOfWeek;
                    if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                    {
                        offset++;
                    }

                    if (offset == 0)
                    {
                        return moment;
                    }
                    moment = moment.AddDays(-1);
                }
            }
            else
            {

            }

            return null;
        }

        public override bool IsTimeIncluded(DateTimeOffset timeUtc)
        {
            timeUtc = TimeZoneUtil.ConvertTime(timeUtc, TimeZone);

            if(_monthValue == "Weekday")
            {
                var dt = FindMonthWeekday(timeUtc.Year, timeUtc.Month, _dayOfWeekCalendarOrder);
                if(dt != null)
                {
                    if(timeUtc.Date == dt.Value.Date)
                    {
                        if (timeUtc >= _beginDateTime)
                        {
                            return true;
                        }
                    }
                }
            }
            else if(_monthValue == "WeekendDay")
            {
                var dt = FindMonthWeekendDay(timeUtc.Year, timeUtc.Month, _dayOfWeekCalendarOrder);
                if (dt != null)
                {
                    if (timeUtc.Date == dt.Value.Date)
                    {
                        if (timeUtc >= _beginDateTime)
                        {
                            return true;
                        }
                    }
                }
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
            DayOfWeekOrderCalendar clone = new DayOfWeekOrderCalendar(_dayOfWeekCalendarOrder, _monthValue,_beginDateTime);
            CloneFields(clone);
            return clone;
        }

    }
}
