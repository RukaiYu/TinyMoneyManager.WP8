namespace TinyMoneyManager.ViewModels
{
    using Microsoft.Phone.UserData;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class AccountItemListViewModel : NkjSoftViewModelBase
    {
        private ObservableCollection<AccountItem> _allItems;
        private System.Collections.Generic.List<String> dateTimeHasLoaded;
        public const string ReportLineFormatter = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}";

        public AccountItemListViewModel()
        {
            this.AllItems = new ObservableCollection<AccountItem>();
            this.dateTimeHasLoaded = new System.Collections.Generic.List<String>();
            this.IncomeItemListViewModel = new IncomeViewModel(new AccountItemDataLodingHandler(this.LoadingAllItem));
            this.ExpenseItemListViewModel = new ExpensesViewModel(new AccountItemDataLodingHandler(this.LoadingAllItem));
            this.SelectedItems = new ObservableCollection<System.Guid>();
            if (this.Peoples == null)
            {
                this.Peoples = new ObservableCollection<PeopleProfile>();
            }
        }

        public void AddingItemToAllItems(AccountItem accountItem)
        {
            this.AllItems.Add(accountItem);
        }

        public void AddItem(AccountItem item)
        {
            this.AccountBookDataContext.AccountItems.InsertOnSubmit(item);
            this.AccountBookDataContext.SubmitChanges();
            switch (item.Type)
            {
                case ItemType.Expense:
                    this.FindGroup(this.ExpenseItems, item.CreateTime.Date).Add(item);
                    return;

                case ItemType.Income:
                    this.FindGroup(this.IncomeItems, item.CreateTime.Date).Add(item);
                    return;
            }
        }

        public void AddToSelectedItems(System.Guid id)
        {
            if (!this.SelectedItems.Contains(id))
            {
                this.SelectedItems.Add(id);
            }
        }

        public void AddYearAndMonth(System.DateTime time)
        {
            this.dateTimeHasLoaded.Add(time.ToString("yyyy-MM"));
        }

        public string BuildAccountItemReport(System.Collections.Generic.IEnumerable<AccountItem> dataSource)
        {
            string templeteFilePath = "Resources/templetes/summaryTemplete.xls";
            string income = LocalizedStrings.GetLanguageInfoByKey("Income");
            string expense = LocalizedStrings.GetLanguageInfoByKey("Expense");
            string title = LocalizedStrings.GetLanguageInfoByKey("ExpenseIncomeHistoryReport").ToUnicodeInt64Value();
            string columnHeader = "\r\n <tr class=3Dxl78 height=3D22 style=3D'height:16.5pt'>\r\n  <td height=3D22 class=3Dxl77 style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl77>{1}</td>\r\n  <td class=3Dxl77>{2}</td>\r\n  <td class=3Dxl77>{3}</td>\r\n  <td class=3Dxl77>{4}</td>\r\n  <td class=3Dxl77>{5}</td>\r\n  <td class=3Dxl77>{6}</td>\r\n </tr>".FormatWith(new object[] { LocalizedStrings.GetLanguageInfoByKey("CreateDate"), LocalizedStrings.GetLanguageInfoByKey("CategoryType"), LocalizedStrings.GetLanguageInfoByKey("CategoryName"), LocalizedStrings.GetLanguageInfoByKey("SencondaryCategoryName"), LocalizedStrings.GetLanguageInfoByKey("AccountName"), LocalizedStrings.GetLanguageInfoByKey("Amount"), LocalizedStrings.GetLanguageInfoByKey("Description") }).ToUnicodeInt64Value();
            return ExcelSummaryBuilder.Build<AccountItem>(templeteFilePath, title, dataSource, columnHeader, accountItem => "\r\n <tr height=3D22 style=3D'height:16.5pt'>\r\n  <td height=3D22 class=3Dxl80 style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl79 style=3D'border-left:none'>{5}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{6}</td>\r\n </tr>\r\n".FormatWith(new object[] { accountItem.CreateTime.ToLocalizedDateString(), ((accountItem.Type == ItemType.Expense) ? expense : income), accountItem.Category.ParentCategory.Name, accountItem.Category.Name, accountItem.AccountName, accountItem.MoneyInfo, accountItem.Description }).ToUnicodeInt64Value());
        }

        public string BuildReports(ExportDataOption option)
        {
            System.Text.StringBuilder contentForEmail = new System.Text.StringBuilder();
            string income = LocalizedStrings.GetLanguageInfoByKey("Income");
            string expense = LocalizedStrings.GetLanguageInfoByKey("Expense");
            (from p in this.AccountBookDataContext.AccountItems
             where (p.CreateTime.Date > option.StartDate.Value.Date) && (p.CreateTime.Date <= option.EndDate.Value.Date)
             orderby p.CreateTime descending
             select p into p
             orderby p.Type
             select p).ToList<AccountItem>().ForEach(delegate(AccountItem x)
            {
                contentForEmail.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", new object[] { x.CreateTime, (x.Type == ItemType.Expense) ? expense : income, x.Category.ParentCategory.Name, x.Category.Name, x.AccountName, x.MoneyInfo, x.Description });
                contentForEmail.AppendLine();
            });
            return contentForEmail.ToString();
        }

        public string BuildReportsForExcel(ExportDataOption exportDataOption)
        {
            IOrderedQueryable<AccountItem> dataSource = from p in this.AccountBookDataContext.AccountItems
                                                        where (p.CreateTime.Date > exportDataOption.StartDate.Value.Date) && (p.CreateTime.Date <= exportDataOption.EndDate.Value.Date)
                                                        orderby p.CreateTime descending
                                                        select p into p
                                                        orderby p.Type
                                                        select p;
            return this.BuildAccountItemReport(dataSource);
        }

        public bool ContainsAddedTime(System.DateTime time)
        {
            return this.dateTimeHasLoaded.Contains(time.ToString("yyyy - MM"));
        }

        public void Delete(System.Func<AccountItem, Boolean> accountItemSelector)
        {
            foreach (AccountItem item in this.AccountBookDataContext.AccountItems.Where<AccountItem>(accountItemSelector))
            {
                this.AllItems.Remove(item);
                this.AccountBookDataContext.AccountItems.DeleteOnSubmit(item);
                switch (item.Type)
                {
                    case ItemType.Expense:
                        {
                            this.FindGroup(this.ExpenseItems, item.CreateTime.Date).Remove(item);
                            continue;
                        }
                    case ItemType.Income:
                        {
                            this.FindGroup(this.IncomeItems, item.CreateTime.Date).Remove(item);
                            continue;
                        }
                }
            }
            ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
            this.AccountBookDataContext.SubmitChanges();
        }

        public void Delete(AccountItem accountItem)
        {
            this.AllItems.Remove(accountItem);
            this.AccountBookDataContext.AccountItems.DeleteOnSubmit(accountItem);
            switch (accountItem.Type)
            {
                case ItemType.Expense:
                    this.FindGroup(this.ExpenseItems, accountItem.CreateTime.Date).Remove(accountItem);
                    break;

                case ItemType.Income:
                    this.FindGroup(this.IncomeItems, accountItem.CreateTime.Date).Remove(accountItem);
                    break;
            }
            ViewModelLocator.PicturesViewModel.DeletePictures(accountItem.Pictures);
        }

        public void DeleteAllById(System.Guid[] arrIds)
        {
            this.SelectedItems.Clear();
        }

        public void FetchingData(GroupViewModel vm)
        {
            vm.Load();
            System.DateTime viewDateTime = vm.ViewModeInfo.ViewDateTime;
            this.AddYearAndMonth(viewDateTime);
        }

        public GroupByCreateTimeAccountItemViewModel FindGroup(ObservableCollection<GroupByCreateTimeAccountItemViewModel> container, System.DateTime time, bool createGroupIfNot = true)
        {
            GroupByCreateTimeAccountItemViewModel item = container.FirstOrDefault<GroupByCreateTimeAccountItemViewModel>(p => p.Key.Date == time.Date);
            if (item == null && createGroupIfNot)
            {
                item = new GroupByCreateTimeAccountItemViewModel(time);
                if (container.Count > 0)
                {
                    container.Insert(0, item);
                    return item;
                }
                container.Add(item);
            }
            return item;
        }

        private GroupByCreateTimeAccountItemViewModel FindGroup(ItemType itemType, System.DateTime dateTime)
        {
            return this.FindGroup((itemType == ItemType.Expense) ? this.ExpenseItems : this.IncomeItems, dateTime);
        }

        public System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> GetGroupedRelatedItems(System.Collections.Generic.IEnumerable<AccountItem> source, System.Action callBackWhenItemsAddedToGroup = null)
        {
            System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> list = new System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel>();
            IOrderedEnumerable<AccountItem> enumerable = from p in source
                                                         orderby p.CreateTime descending
                                                         select p;

            var dates = (from p in enumerable select p.CreateTime.Date).Distinct<System.DateTime>();

            foreach (var item in dates)
            {
                GroupByCreateTimeAccountItemViewModel agvm = new GroupByCreateTimeAccountItemViewModel(item);

                enumerable.Where(p => p.CreateTime.Date == item.Date).ToList().ForEach(x =>
                {
                    agvm.Add(x);
                    callBackWhenItemsAddedToGroup();
                });

                list.Add(agvm);
            }

            return list;
        }

        public System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> GetGroupedRelatedItems(System.Func<AccountItem, bool> searchingCondtion, bool searchingOnlyCurrentMonthData = true)
        {
            System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> list = new System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel>();
            IOrderedEnumerable<AccountItem> source = from p in this.AccountBookDataContext.AccountItems.Where<AccountItem>(searchingCondtion)
                                                     orderby p.CreateTime descending
                                                     select p;

            var dates = (from p in source select p.CreateTime.Date).Distinct<System.DateTime>();

            foreach (var item in dates)
            {
                GroupByCreateTimeAccountItemViewModel agvm = new GroupByCreateTimeAccountItemViewModel(item);

                source.Where(p => p.CreateTime.Date == item.Date).ToList<AccountItem>().ForEach(delegate(AccountItem x)
                {
                    agvm.Add(x);
                });
                list.Add(agvm);
            }
            return list;
        }

        public AccountItem GetItemByPosition(int direction, System.Guid currentId, System.DateTime date, ItemType itemType)
        {
            this.FindGroup(itemType, date);
            return null;
        }

        public System.Collections.Generic.IEnumerable<AccountItem> GetOneMonthData(System.DateTime date)
        {
            return this.GetOneMonthData(date.Year, date.Month);
        }

        public System.Collections.Generic.IEnumerable<AccountItem> GetOneMonthData(int year, int month)
        {
            return (from accountItem in this.AccountBookDataContext.AccountItems
                    where (accountItem.CreateTime.Date.Year == year) && (accountItem.CreateTime.Month == month)
                    select accountItem);
        }

        public System.Collections.Generic.IEnumerable<AccountItem> GetOneYearData(int year)
        {
            return (from accountItem in this.AccountBookDataContext.AccountItems
                    where accountItem.CreateTime.Date.Year == year
                    select accountItem);
        }

        public void LoadCollectionsFromDatabase()
        {
            var items = from accountItem in this.AccountBookDataContext.AccountItems
                        where accountItem.CreateTime.AtSameYearMonth(System.DateTime.Now)
                        select accountItem;
            this.AddYearAndMonth(System.DateTime.Now);
        }

        public System.Collections.Generic.IEnumerable<AccountItem> LoadingAllItem(ViewModeConfig viewModeConfig, ItemType itemType)
        {
            if (viewModeConfig.CustomizedSearchingIndex != -1)
            {
                System.DateTime startDate = viewModeConfig.SearchingCondition.StartDate.Value.Date;
                System.DateTime endDate = viewModeConfig.SearchingCondition.EndDate.Value.Date;
                return (from accountItem in this.AccountBookDataContext.AccountItems
                        where ((accountItem.CreateTime.Date > startDate) && (accountItem.CreateTime.Date <= endDate)) && (((int)accountItem.Type) == ((int)itemType))
                        orderby accountItem.CreateTime descending
                        select accountItem);
            }
            return (from accountItem in this.AccountBookDataContext.AccountItems
                    where ((accountItem.CreateTime.Year == viewModeConfig.Year) && (accountItem.CreateTime.Month == viewModeConfig.Month)) && (((int)accountItem.Type) == ((int)itemType))
                    orderby accountItem.CreateTime descending
                    select accountItem);
        }

        public void LoadPeople(System.Action<System.Collections.Generic.IEnumerable<PeopleProfile>> callBackWhenLoaded)
        {
            if (!this.HasLoadPeople)
            {
                Contacts contacts = new Contacts();
                contacts.SearchCompleted += delegate(object o, ContactsSearchEventArgs e)
                {
                    this.Peoples.Clear();
                    PeopleProfile[] ts = (from p in e.Results.Take<Contact>(300) select new PeopleProfile { Name = p.DisplayName }).ToArray<PeopleProfile>();
                    this.Peoples.AddRange<PeopleProfile>(ts);
                    this.HasLoadPeople = true;
                    if (callBackWhenLoaded != null)
                    {
                        callBackWhenLoaded(ts);
                    }
                };
                contacts.SearchAsync(string.Empty, FilterKind.None, null);
            }
        }

        public void RemoveFromSelectedItems(System.Guid id)
        {
            if (this.SelectedItems.Contains(id))
            {
                this.SelectedItems.Remove(id);
            }
        }

        public void RemoveItem(AccountItem itemForDelete)
        {
            this.AllItems.Remove(itemForDelete);
            this.AccountBookDataContext.AccountItems.DeleteOnSubmit(itemForDelete);
            switch (itemForDelete.Type)
            {
                case ItemType.Expense:
                    this.FindGroup(this.ExpenseItems, itemForDelete.CreateTime.Date).Remove(itemForDelete);
                    return;

                case ItemType.Income:
                    this.FindGroup(this.IncomeItems, itemForDelete.CreateTime.Date).Remove(itemForDelete);
                    return;
            }
        }

        public override void SubmitChanges()
        {
            base.SubmitChanges();
            ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
        }

        public void Update(AccountItem currentEditObject)
        {
            this.FindGroup(currentEditObject.Type, currentEditObject.CreateTime).RaiseTotalMoneyChanged();
            this.AccountBookDataContext.SubmitChanges();
        }

        public void UpdateItemByDate(AccountItem editObject, System.DateTime oldDate)
        {
            var group = this.FindGroup(editObject.Type, oldDate);

            var oldItem = group.FirstOrDefault(p => p.Id == editObject.Id);

            if (group != null)
            {
                group.RemoveItem(oldItem);

            }

            group = this.FindGroup(editObject.Type, editObject.CreateTime);

            if (group != null)
            {
                group.Add(oldItem);
            }
        }

        public ObservableCollection<AccountItem> AllItems
        {
            get
            {
                return this._allItems;
            }
            set
            {
                this._allItems = value;
                this.OnNotifyPropertyChanged("AllItems");
            }
        }

        public ExpensesViewModel ExpenseItemListViewModel { get; set; }

        public ObservableCollection<GroupByCreateTimeAccountItemViewModel> ExpenseItems
        {
            get
            {
                return this.ExpenseItemListViewModel.GroupItems;
            }
        }

        public bool HasLoadPeople { get; set; }

        public IncomeViewModel IncomeItemListViewModel { get; set; }

        public ObservableCollection<GroupByCreateTimeAccountItemViewModel> IncomeItems
        {
            get
            {
                return this.IncomeItemListViewModel.GroupItems;
            }
        }

        public bool IsExpensesItemsLoaded
        {
            get
            {
                return this.ExpenseItemListViewModel.IsDataLoaded;
            }
            set
            {
                this.ExpenseItemListViewModel.IsDataLoaded = value;
            }
        }

        public bool IsIncomeItemsLoaded
        {
            get
            {
                return this.IncomeItemListViewModel.IsDataLoaded;
            }
            set
            {
                this.IncomeItemListViewModel.IsDataLoaded = value;
            }
        }

        public ObservableCollection<PeopleProfile> Peoples { get; set; }

        public ObservableCollection<System.Guid> SelectedItems { get; set; }
    }
}

