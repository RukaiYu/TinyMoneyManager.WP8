namespace TinyMoneyManager.Data.Model
{
    using System;

    [Flags]
    public enum RecordActionType
    {
        CrateExpenseRecord,
        CreateIncomeRecord,
        CreateTransferingRecord,
        CreateBorrowRecord,
        CreateLeanRecord,
        BackupDataBase,
        CreateTranscationRecord,
    }
}

