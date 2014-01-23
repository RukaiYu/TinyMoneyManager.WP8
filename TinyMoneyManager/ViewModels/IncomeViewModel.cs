namespace TinyMoneyManager.ViewModels
{
    using System;
    using TinyMoneyManager.Component;

    public class IncomeViewModel : GroupViewModel
    {
        public IncomeViewModel(AccountItemDataLodingHandler dataLodingHandler) : base(dataLodingHandler)
        {
            base.AccountItemType = ItemType.Income;
        }
    }
}

