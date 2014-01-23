namespace TinyMoneyManager.ViewModels
{
    using System;
    using TinyMoneyManager.Component;

    public class ExpensesViewModel : GroupViewModel
    {
        public ExpensesViewModel(AccountItemDataLodingHandler dataLodingHandler) : base(dataLodingHandler)
        {
            base.AccountItemType = ItemType.Expense;
        }
    }
}

