namespace TinyMoneyManager.Data.Model
{
    using System;

    public enum ScheduleType
    {
        None,
        EveryDay,
        EveryWeek,
        EveryMonth,
        EveryYear,
        EveryOtherDay,
        EveryOtherWeek,
        EveryOtherMonth,
        Workday,
        Weekend,
        Customize,
        SpecificDate
    }
}

