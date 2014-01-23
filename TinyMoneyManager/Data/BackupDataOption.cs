namespace TinyMoneyManager.Data
{
    using System;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Data.Model;

    public class BackupDataOption
    {
        public BackupDataRange Range { get; set; }

        public System.Func<AccountItem, Boolean> RangeForAccountItem { get; set; }

        public int TotalEffects { get; set; }
    }
}

