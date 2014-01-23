namespace TinyMoneyManager.Data
{
    using System;

    public class ScheduleTaskManagement
    {
        private TinyMoneyDataContext DataContext;

        public ScheduleTaskManagement(TinyMoneyDataContext db)
        {
            this.DataContext = db;
        }

        public void LoadCurrent()
        {
        }
    }
}

