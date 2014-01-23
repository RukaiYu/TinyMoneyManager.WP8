namespace TinyMoneyManager.Data.Model
{
    using System;

    public enum PlanStatus
    {
        Pending,
        Executed,
        CancelRestartFromNextCycle,
        Cancel,
        CancelForMissingData
    }
}

