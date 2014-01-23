namespace TinyMoneyManager.ScheduledAgentLib
{
    using System;
    using System.Runtime.CompilerServices;

    public class ScheduledAgentConfigurationData
    {
        public int TodayCost;

        public string CostName { get; set; }

        public System.DateTime LastDateUpdateTile { get; set; }

        public string LastSumCost { get; set; }

        public string SecondCostName { get; set; }

        public string[] TodayInfoFormatter { get; set; }
    }
}

