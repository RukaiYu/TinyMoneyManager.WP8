namespace TinyMoneyManager.ViewModels.AccountItemManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.ViewModels;

    public class AccountItemViewerViewModel : NkjSoftViewModelBase
    {
        public static bool NeedReloadData = true;

        public System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> GetGroupedRelatedItems(AccountItem itemCompareTo, bool searchingOnlyCurrentMonthData = true, System.Action<AccountItem> itemAdded = null)
        {
            System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> list = new System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel>();
            IOrderedEnumerable<AccountItem> source = from p in this.GetRelatedItems(itemCompareTo, searchingOnlyCurrentMonthData)
                                                     orderby p.CreateTime descending
                                                     select p;
            if (itemAdded == null)
            {
                itemAdded = delegate(AccountItem ai)
                {
                };
            }

            var dates = (from p in source select p.CreateTime.Date).Distinct<System.DateTime>();

            foreach (var item in dates)
            {
                GroupByCreateTimeAccountItemViewModel agvm = new GroupByCreateTimeAccountItemViewModel(item);

                source.Where(p => p.CreateTime.Date == item.Date).ToList<AccountItem>().ForEach(delegate(AccountItem x)
                {
                    agvm.Add(x);
                    itemAdded(x);
                });
                list.Add(agvm);
            }

            return list;
        }

        public System.Collections.Generic.IEnumerable<AccountItem> GetRelatedItems(AccountItem itemCompareTo, bool searchingOnlyCurrentMonth = true)
        {
            System.Func<AccountItem, Boolean> predicate = null;
            System.Collections.Generic.IEnumerable<AccountItem> source = from p in this.AccountBookDataContext.AccountItems
                                                                         where ((((int)p.Type) == ((int)itemCompareTo.Type)) && (p.CategoryId == itemCompareTo.CategoryId)) && (p.Id != itemCompareTo.Id)
                                                                         select p;
            System.DateTime createTime = itemCompareTo.CreateTime;
            int year = createTime.Year;
            int month = createTime.Month;
            int day = createTime.Day;
            if (!searchingOnlyCurrentMonth)
            {
                return source;
            }
            if (predicate == null)
            {
                predicate = p => (p.CreateTime.Date.Year == year) && (p.CreateTime.Date.Month == month);
            }
            return source.Where<AccountItem>(predicate);
        }
    }
}

