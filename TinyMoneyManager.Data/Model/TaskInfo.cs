namespace TinyMoneyManager.Data.Model
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class TaskInfo
    {
        public RecordActionType TaskType { get; set; }

        public bool Active { get; set; }

        public int Date { get; set; }

        public string DayOfWeek { get; set; }

        public string Frequency { get; set; }

        public System.Guid Id { get; set; }

        public bool EnsureToRun(System.DateTime executingDate)
        {
            if (!this.Active)
            {
                return false;
            }
            bool flag = false;
            string str = this.Frequency.ToLowerInvariant();
            string str2 = this.DayOfWeek.ToLowerInvariant();
            string str3 = executingDate.DayOfWeek.ToString().ToLowerInvariant();
            if (str == "everymonth")
            {
                if (this.Date == executingDate.Day)
                {
                    flag = true;
                }
                return flag;
            }
            if ((str == "everyweek") && (str3 == str2))
            {
                return true;
            }
            if (str == "everyday")
            {
                return true;
            }
            if ((str == "workday") && !AppSetting.Weekends.Contains<string>(str3))
            {
                return true;
            }
            if ((str == "weekend") && AppSetting.Weekends.Contains<string>(str3))
            {
                return true;
            }
            bool flag1 = str == "specificdate";
            return flag;
        }

        public static bool EnsureToRun(TallySchedule tallySchedule, System.DateTime executingDate)
        {
            if (!tallySchedule.IsActive.GetValueOrDefault())
            {
                return false;
            }
            bool flag = false;
            string str = tallySchedule.Frequency.ToString().ToLowerInvariant();
            string str2 = tallySchedule.DayofWeek.GetValueOrDefault().ToString().ToLowerInvariant();
            string str3 = executingDate.DayOfWeek.ToString().ToLowerInvariant();
            if (str == "everymonth")
            {
                if (tallySchedule.StartDate.GetValueOrDefault() == executingDate.Day)
                {
                    flag = true;
                }
                return flag;
            }
            if ((str == "everyweek") && (str3 == str2))
            {
                return true;
            }
            if (str == "everyday")
            {
                return true;
            }
            if ((str == "workday") && !AppSetting.Weekends.Contains<string>(str3))
            {
                return true;
            }
            if ((str == "weekend") && AppSetting.Weekends.Contains<string>(str3))
            {
                return true;
            }
            bool flag1 = str == "specificdate";
            return flag;
        }


    }
}

