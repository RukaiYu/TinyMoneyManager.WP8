namespace TinyMoneyManager.ViewModels
{
    using System;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;

    public class GroupByAccountItemTypeViewModel : GroupAccountItemViewModelBase<ItemType>
    {
        public GroupByAccountItemTypeViewModel(ItemType type)
            : base(type)
        {
        }

        public override bool Equals(object obj)
        {
            GroupByAccountItemTypeViewModel model = obj as GroupByAccountItemTypeViewModel;
            return ((model != null) && (base.Key == model.Key));
        }

        public bool Equals(GroupByAccountItemTypeViewModel other)
        {
            return !object.ReferenceEquals(null, other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string HeaderInfo
        {
            get
            {
                return LocalizedStrings.GetLanguageInfoByKey(base.Key.ToString());
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class DateTimeBasedGroupingViewModel<TKey> : ViewModels.Common.ObjectGroupingViewModel<DateTime, TKey> where TKey : IMoney
    {
        public DateTimeBasedGroupingViewModel(DateTime key)
            : base
                (key)
        {

        }

        public override string HeaderInfo
        {
            get
            {
                return this.Key.Date.ToString((base.Key.Year == System.DateTime.Now.Year) ? "M/d ddd" : ConstString.FormatWithShortDateAndWeekWithYear, LocalizedStrings.CultureName);
            }
        }
    }
}

