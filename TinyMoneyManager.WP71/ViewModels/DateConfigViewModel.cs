namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class DateConfigViewModel : NotionObject
    {
        private ViewModeConfig searchingConfig;

        public DateConfigViewModel()
        {
            this.yearWithMonths = new System.Collections.Generic.Dictionary<Int32, List<Int32>>();
        }

        public System.Collections.Generic.IEnumerable<Int32> LoadMonthsByYear(int year)
        {
            if (!this.yearWithMonths.ContainsKey(year))
            {
                this.yearWithMonths[year] = (from p in
                                                 (from p in ViewModelLocator.AccountItemViewModel.AccountBookDataContext.AccountItems
                                                  where p.CreateTime.Year == year
                                                  select p.CreateTime.Month).Distinct<int>()
                                             orderby p descending
                                             select p).ToList<int>();
            }
            return this.yearWithMonths[year];
        }

        public System.Collections.Generic.IEnumerable<Int32> LoadYears(ItemType itemType)
        {
            System.Collections.Generic.List<DateTime> source = (from p in
                                                                    (from p in ViewModelLocator.AccountItemViewModel.AccountBookDataContext.AccountItems
                                                                     where ((int)p.Type) == ((int)itemType)
                                                                     select p.CreateTime.Date).Distinct<System.DateTime>()
                                                                orderby p.Year descending
                                                                select p).ToList<System.DateTime>();
            System.Collections.Generic.List<Int32> list2 = (from p in source select p.Year).Distinct<int>().ToList<int>();
            System.Collections.Generic.List<Int32> list3 = null;
            using (System.Collections.Generic.List<int>.Enumerator enumerator = list2.GetEnumerator())
            {
                System.Func<DateTime, Boolean> predicate = null;
                int year;
                while (enumerator.MoveNext())
                {
                    year = enumerator.Current;
                    if (predicate == null)
                    {
                        predicate = p => p.Year == year;
                    }
                    list3 = (from p in
                                 (from p in source.Where<System.DateTime>(predicate) select p.Month).Distinct<int>()
                             orderby p descending
                             select p).ToList<int>();
                    if (list3.Count != 0)
                    {
                        this.yearWithMonths[year] = list3;
                    }
                }
            }
            return list2;
        }

        public ViewModeConfig SearchingConfig
        {
            get
            {
                return this.searchingConfig;
            }
            set
            {
                if (this.searchingConfig != value)
                {
                    this.OnNotifyPropertyChanging("SearchingConfig");
                    this.searchingConfig = value;
                    this.OnNotifyPropertyChanged("SearchingConfig");
                }
            }
        }

        public System.Collections.Generic.Dictionary<Int32, List<Int32>> yearWithMonths { get; set; }
    }
}

