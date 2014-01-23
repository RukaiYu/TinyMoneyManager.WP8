namespace TinyMoneyManager.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TinyMoneyManager.Data.Model;

    public class AccountCategoryHelper
    {
        private static System.Collections.Generic.IEnumerable<AccountCategoryWapper> accountCategories;

        public static AccountCategoryWapper[] GetDefaultWrappers()
        {
            return new AccountCategoryWapper[] { new AccountCategoryWapper(AccountCategory.Cash, "magenta"), new AccountCategoryWapper(AccountCategory.BankCard, "purple"), new AccountCategoryWapper(AccountCategory.DebitCard, "teal"), new AccountCategoryWapper(AccountCategory.Paypal, "blue"), new AccountCategoryWapper(AccountCategory.CreditCard, "brown"), new AccountCategoryWapper(AccountCategory.IdealMoneyAccount, "pink"), new AccountCategoryWapper(AccountCategory.FixedAssets, "lime") };
        }

        public static AccountCategoryWapper GetWapperByType(AccountCategory accountCategory)
        {
            return AccountCategories.FirstOrDefault<AccountCategoryWapper>(p => (p.Category == accountCategory));
        }

        public static System.Collections.Generic.IEnumerable<AccountCategoryWapper> InitalizeDefault()
        {
            AccountCategoryWapper[] accountCategoryWappers = AppSetting.Instance.AccountCategoryWappers;
            if (((accountCategoryWappers == null) || (accountCategoryWappers.Length <= 0)) || (accountCategoryWappers.Length != 7))
            {
                accountCategoryWappers = GetDefaultWrappers();
                AppSetting.Instance.AccountCategoryWappers = accountCategoryWappers;
            }
            return accountCategoryWappers;
        }

        public static System.Collections.Generic.IEnumerable<AccountCategoryWapper> AccountCategories
        {
            get
            {
                if (accountCategories == null)
                {
                    accountCategories = InitalizeDefault();
                }
                return accountCategories;
            }
        }
    }
}

