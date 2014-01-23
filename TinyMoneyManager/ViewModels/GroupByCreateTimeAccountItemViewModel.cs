namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class GroupByCreateTimeAccountItemViewModel : GroupAccountItemViewModelBase<System.DateTime>
    {
        public GroupByCreateTimeAccountItemViewModel(System.DateTime createTime) : base(createTime)
        {
        }

        public override bool Equals(object obj)
        {
            GroupByCreateTimeAccountItemViewModel model = obj as GroupByCreateTimeAccountItemViewModel;
            return ((model != null) && (base.Key.Date == model.Key.Date));
        }

        public override int GetHashCode()
        {
            return base.Key.Date.GetHashCode();
        }

        public static void RemoveItemFrom(ObservableCollection<GroupByCreateTimeAccountItemViewModel> AssociatedItems, AccountItem item)
        {
            GroupByCreateTimeAccountItemViewModel model = AssociatedItems.FirstOrDefault<GroupByCreateTimeAccountItemViewModel>(p => p.Key.Date == item.CreateTime.Date);
            if (model != null)
            {
                try
                {
                    model.RemoveItem(item);
                }
                catch (System.Exception)
                {
                }
            }
        }

        public override string HeaderInfo
        {
            get
            {
                return base.Key.Date.ToString((base.Key.Year == System.DateTime.Now.Year) ? "M/d ddd" : ConstString.FormatWithShortDateAndWeekWithYear, LocalizedStrings.CultureName);
            }
        }
    }
}

