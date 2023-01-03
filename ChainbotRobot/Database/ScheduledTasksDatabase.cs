using ChainbotRobot.Contracts;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Database
{
    public  class ScheduledTasksDatabase
    {
        private SQLiteConnection _db;

        private readonly string dbName = "ScheduledTasks.db";

        private IRobotPathConfigService _robotPathConfigService;

        public ScheduledTasksDatabase(IRobotPathConfigService robotPathConfigService)
        {
            _robotPathConfigService = robotPathConfigService;

            var dbPath = Path.Combine(_robotPathConfigService.AppProgramDataDir, dbName);
            _db = new SQLiteConnection(dbPath);
            _db.CreateTable<ScheduledTasksTable>();
        }

        public void InsertItem(ScheduledTasksTable item)
        {
            _db.Insert(item);
        }

        public void UpdateItem(ScheduledTasksTable item)
        {
            _db.Update(item);
        }

        public void DeleteItem(ScheduledTasksTable item)
        {
            _db.Delete(item);
        }

        public List<ScheduledTasksTable> GetItems()
        {
            var scheduledTasks = _db.Query<ScheduledTasksTable>("SELECT * FROM ScheduledTasks");
            return scheduledTasks;
        }

    }
}
