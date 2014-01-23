namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public delegate System.Collections.Generic.IEnumerable<AccountItem> AccountItemDataLodingHandler(ViewModeConfig viewModeConfig, ItemType itemType);
}

