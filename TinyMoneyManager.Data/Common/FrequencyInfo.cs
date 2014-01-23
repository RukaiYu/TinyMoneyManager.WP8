namespace TinyMoneyManager.Data.Common
{
    using System;
    using System.Runtime.InteropServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class FrequencyInfo : NotionObject
    {
        private int day;
        private System.DayOfWeek dayOfWeek;
        private ScheduleType frequency;

        public FrequencyInfo()
        {
            this.Day = System.DateTime.Now.Day;
            this.Frequency = ScheduleType.EveryMonth;
            this.DayOfWeek = System.DayOfWeek.Sunday;
        }

        public string GetSummary(string weekName)
        {
            return string.Format("{0} {1}", LocalizedObjectHelper.GetLocalizedStringFrom(this.Frequency.ToString()), (this.Frequency == ScheduleType.EveryMonth) ? string.Format(LocalizedObjectHelper.GetLocalizedStringFrom("FrequencyDayOfMonthFormatter"), this.Day) : weekName);
        }

        public static string GetSummary(ScheduleType frequency, int day, System.DayOfWeek dayOfWeek, string weekName = "")
        {
            string str = LocalizedObjectHelper.GetLocalizedStringFrom(frequency.ToString());
            if (((frequency == ScheduleType.EveryDay) || (frequency == ScheduleType.Weekend)) || (frequency == ScheduleType.Workday))
            {
                return str;
            }
            return string.Format("{0} {1}", str, (frequency == ScheduleType.EveryMonth) ? string.Format(LocalizedObjectHelper.GetLocalizedStringFrom("FrequencyDayOfMonthFormatter"), day) : weekName);
        }

        public int Day
        {
            get
            {
                return this.day;
            }
            set
            {
                if (this.day != value)
                {
                    this.OnNotifyPropertyChanging("Day");
                    this.day = value;
                    this.OnNotifyPropertyChanged("Day");
                }
            }
        }

        public System.DayOfWeek DayOfWeek
        {
            get
            {
                return this.dayOfWeek;
            }
            set
            {
                if (this.dayOfWeek != value)
                {
                    this.OnNotifyPropertyChanging("DayOfWeek");
                    this.dayOfWeek = value;
                    this.OnNotifyPropertyChanged("DayOfWeek");
                }
            }
        }

        public ScheduleType Frequency
        {
            get
            {
                return this.frequency;
            }
            set
            {
                if (this.frequency != value)
                {
                    this.OnNotifyPropertyChanging("Frequency");
                    this.frequency = value;
                    this.OnNotifyPropertyChanged("Frequency");
                }
            }
        }
    }
}

