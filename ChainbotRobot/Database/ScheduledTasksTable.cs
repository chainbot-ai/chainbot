using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Database
{
    [Table("ScheduledTasks")]
    public class ScheduledTasksTable
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("begin_date")]
        public string BeginDate { get; set; }

        [Column("run_time")]
        public string RunTime { get; set; }

        [Column("frequency_type")]
        public string FrequencyType { get; set; }

        [Column("every_num")]
        public int EveryNum { get; set; }

        [Column("week_days")]
        public int WeekDays { get; set; }

        [Column("month_type")]
        public string MonthType { get; set; }

        [Column("month_value")]
        public string MonthValue { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }
}
