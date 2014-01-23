using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.Generic;
using TinyMoneyManager.Data.Model;
using System.Collections.ObjectModel;

namespace TinyMoneyManager.Component
{
    /// <summary>
    /// A class used to expose the Key property on a dynamically-created Linq grouping.
    /// The grouping will be generated as an internal class, so the Key property will not
    /// otherwise be available to databind.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TElement">The type of the items.</typeparam>
    public class PublicGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly IGrouping<TKey, TElement> _internalGrouping;

        public PublicGrouping(IGrouping<TKey, TElement> internalGrouping)
        {
            _internalGrouping = internalGrouping;
        }

        public override bool Equals(object obj)
        {
            PublicGrouping<TKey, TElement> that = obj as PublicGrouping<TKey, TElement>;

            return (that != null) && (this.Key.Equals(that.Key));
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        #region IGrouping<TKey,TElement> Members

        public TKey Key
        {
            get { return _internalGrouping.Key; }
        }

        #endregion

        #region IEnumerable<TElement> Members

        public IEnumerator<TElement> GetEnumerator()
        {
            return _internalGrouping.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _internalGrouping.GetEnumerator();
        }

        #endregion


    }

    public class AccountItemSummaryGroup : PublicGrouping<string, SummaryDetails>
    {
        public AccountItemSummaryGroup(IGrouping<string, SummaryDetails> g)
            : base(g)
        {

        }

        public int GroupCount { get; set; }

        public decimal? GroupTotalAmout { get; set; }

        public string GroupTotalAmoutInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(this.GroupTotalAmout);
            }
        }
    }

    public class AccountItemGroup : PublicGrouping<DateTime, AccountItem>
    {
        public AccountItemGroup(IGrouping<DateTime, AccountItem> g)
            : base(g)
        {

        }

        public string HeaderInfo
        {
            get
            {
                return this.DateTime.ToString("M月d日");
            }
        }

        public DayOfWeek Week { get; set; }

        public DateTime DateTime { get; set; }

    }



    public class AccountItemGroupItem
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string HearderInfo { get; set; }

        public AccountItem Item { get; set; }
    }


}
