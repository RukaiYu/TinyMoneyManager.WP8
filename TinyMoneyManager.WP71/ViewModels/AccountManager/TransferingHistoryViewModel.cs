using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using NkjSoft.WPhone.Extensions;

using TinyMoneyManager.Data;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Component;
using System.Windows;

namespace TinyMoneyManager.ViewModels
{
    using NkjSoft.Extensions;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using TinyMoneyManager.ViewModels.Common;
    public class TransferingHistoryViewModel : NkjSoftViewModelBase
    {
        public static readonly string FilterDateProperty = "FilterDate";
        public static readonly string FilterTypeIndexProperty = "FilterTypeIndex";
        public static readonly string formatterForDay = "{0:yyyy/MM/dd}";
        public static readonly string formatterForMonth = "{0:MM/yyyy}";

        private string formatterForDate = formatterForMonth;

        private int filterTypeIndex = 0;
        private DateTime filterDate = DateTime.Now;

        #region --- Public Properties ---


        /// <summary>
        /// Gets or sets the filter date.
        /// </summary>
        /// <value>
        /// The filter date.
        /// </value>
        public DateTime FilterDate
        {
            get { return filterDate; }
            set
            {
                if (filterDate.Date != value.Date)
                {
                    filterDate = value;
                    OnNotifyPropertyChanged("FilterDate");
                }
            }
        }


        private List<DateTimeBasedGroupingViewModel<TransferingItem>> _groupItems;

        public List<DateTimeBasedGroupingViewModel<TransferingItem>> GroupItems
        {
            get { return _groupItems; }
            set
            {
                _groupItems = value;
                OnAsyncNotifyPropertyChanged("GroupItems");
            }
        }


        /// <summary>
        /// Gets or sets the index of the filter type.
        /// </summary>
        /// <value>
        /// The index of the filter type.
        /// </value>
        public int FilterTypeIndex
        {
            get { return filterTypeIndex; }
            set
            {
                if (filterTypeIndex != value)
                {
                    filterTypeIndex = value;
                    OnNotifyPropertyChanged("FilterTypeIndex");
                    OnNotifyPropertyChanged("FormatterForDate");
                }
            }
        }

        /// <summary>
        /// Gets or sets the formatter for date.
        /// </summary>
        /// <value>
        /// The formatter for date.
        /// </value>
        public string FormatterForDate
        {
            get
            {
                return this.filterTypeIndex == 0 ? formatterForMonth : formatterForDay; ;
            }
        }


        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferingHistoryViewModel"/> class.
        /// </summary>
        public TransferingHistoryViewModel()
        {

            this.GroupItems = new List<DateTimeBasedGroupingViewModel<TransferingItem>>();

            this.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(TransferingHistoryViewModel_PropertyChanged);
        }

        #region --- Public Methods ---

        public void GoStepDate(int stepOfDate)
        {
            if (filterTypeIndex == 0) //Month
            {
                this.FilterDate = this.filterDate.AddMonths(stepOfDate);
            }
            else if (filterTypeIndex == 1) //day
            {
                this.FilterDate = this.filterDate.AddDays(stepOfDate);
            }
        }

        /// <summary>
        /// Adds the specified from account.
        /// </summary>
        /// <param name="fromAccount">From account.</param>
        /// <param name="toAccount">To account.</param>
        /// <param name="amount">The amount.</param>
        public void Add(Account fromAccount, Account toAccount, decimal amount, decimal transferingPoundageAmountInfo, DateTime transferingDate, string notes)
        {
            TransferingItem entry = new TransferingItem();
            entry.Amount = amount;
            entry.TransferingPoundageAmount = transferingPoundageAmountInfo;
            entry.Currency = fromAccount.CurrencyType;
            entry.FromAccount = fromAccount;
            entry.ToAccount = toAccount;
            entry.TransferingDate = transferingDate;
            entry.FromAccountId = fromAccount.Id;
            entry.ToAccountId = toAccount.Id;
            entry.Notes = notes;

            //TransferingItemRepository.Instance.Add(entry);

            AddItemToGroup(entry);

            AccountBookDataContext.TransferingItems.InsertOnSubmit(entry);
        }

        private void AddItemToGroup(TransferingItem entry)
        {
            var group = this.GroupItems.FirstOrDefault(p => p.Key == entry.TransferingDate.Date);

            if (group == null)
            {
                group = new DateTimeBasedGroupingViewModel<TransferingItem>(entry.TransferingDate);

                this.GroupItems.AddRange(group);
            }

            group.Add(entry);
        }

        /// <summary>
        /// Creates the or update transfering item.
        /// </summary>
        /// <param name="fromAccount">From account.</param>
        /// <param name="toAccount">To account.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="transferingPoundageAmountInfo">The transfering poundage amount info.</param>
        /// <param name="transferingDate">The transfering date.</param>
        /// <param name="notes">The notes.</param>
        /// <param name="itemToUpdate">The item to update.</param>
        /// <returns></returns>
        public TransferingItem CreateOrUpdateTransferingItem(Account fromAccount, Account toAccount, decimal amount, decimal transferingPoundageAmountInfo, DateTime transferingDate, string notes, TransferingItem itemToUpdate = null)
        {
            itemToUpdate = itemToUpdate ?? new TransferingItem();
            itemToUpdate.Amount = amount;
            itemToUpdate.TransferingPoundageAmount = transferingPoundageAmountInfo;
            itemToUpdate.Currency = fromAccount.CurrencyType;
            itemToUpdate.TransferingDate = transferingDate;
            itemToUpdate.FromAccount = fromAccount;
            itemToUpdate.ToAccount = toAccount;
            itemToUpdate.FromAccountId = fromAccount.Id;
            itemToUpdate.ToAccountId = toAccount.Id;
            itemToUpdate.Notes = notes;
            itemToUpdate.Currency = fromAccount.CurrencyType;
            return itemToUpdate;
        }


        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="filterSelector">The filter selector.</param>
        public void LoadData(Func<TransferingItem, bool> filterSelector)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                var list = o as ObservableCollection<TransferingItem>;
                System.Collections.Generic.IEnumerable<TransferingItem> query = null;
                if (TargetAccount != null)
                {
                    query = AccountBookDataContext.TransferingItems
                        .Where(filterSelector)
                        .Where(p => p.ToAccountName == TargetAccount.Name
                                    || p.FromAccountName == TargetAccount.Name);
                }
                else
                {
                    query = AccountBookDataContext.TransferingItems
                      .Where(filterSelector).OrderByDescending(p => p.TransferingDate);
                }

                var dates = query.Select(p => p.TransferingDate.Date)
                    .OrderByDescending(p => p)
                    .Distinct().ToList();
                var tempGroups = new List<DateTimeBasedGroupingViewModel<TransferingItem>>();
                foreach (var date in dates)
                {
                    var group = new DateTimeBasedGroupingViewModel<TransferingItem>(date);

                    group.AddRange(query.Where(p => p.TransferingDate.Date == date));

                    tempGroups.Add(group);
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    GroupItems = tempGroups;
                });

                IsDataLoaded = true;
            });
        }

        /// <summary>
        /// Queries the by.
        /// </summary>
        /// <param name="filterSelector">The filter selector.</param>
        /// <returns></returns>
        public IEnumerable<TransferingItem> QueryBy(Func<TransferingItem, bool> filterSelector)
        {
            var query = AccountBookDataContext.TransferingItems
                     .Where(filterSelector);
            return query;
        }

        /// <summary>
        /// Loads the data by month.
        /// </summary>
        /// <param name="dateOfMonth">The date of month.</param>
        public void LoadDataByMonth(DateTime dateOfMonth)
        {
            LoadData(p => p.TransferingDate.AtSameYearMonth(dateOfMonth));
        }

        /// <summary>
        /// Loads the data by day.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        public void LoadDataByDay(DateTime dateTime)
        {
            LoadData(p => p.TransferingDate.Date == dateTime.Date);
        }

        /// <summary>
        /// Deletes the item.
        /// </summary>
        /// <param name="id">The id.</param>
        public void DeleteItem(TransferingItem item)
        {
            if (item == null) return;

            if (this.GroupItems != null)
            {
                var group = this.GroupItems.FirstOrDefault(p => p.Key.Date == item.TransferingDate.Date);
                if (group != null)
                {
                    group.Remove(item);
                }
            }

            AccountBookDataContext.TransferingItems.DeleteOnSubmit(item);
            AccountBookDataContext.SubmitChanges();
        }

        /// <summary>
        /// Performs the load data.
        /// </summary>
        public void PerformLoadData()
        {
            var type = filterTypeIndex;
            var date = filterDate;
            IsDataLoaded = false;

            if (type == 0) // By Month 
            {
                LoadDataByMonth(date.Date);
            }
            else
            {
                LoadDataByDay(date.Date);
            }
        }


        /// <summary>
        /// Gets the subject of history.
        /// </summary>
        /// <returns></returns>
        public string GetSubjectOfHistory(string historyTitle)
        {
            return "[{0}] {1}".FormatWith(string.Format(this.FormatterForDate, this.FilterDate),
                                 historyTitle);
        }

        /// <summary>
        /// Builds the history list to string.
        /// </summary>
        /// <returns></returns>
        public System.Text.StringBuilder BuildHistoryListToString()
        {
            System.Text.StringBuilder sb = new StringBuilder();

            foreach (var group in this.GroupItems)
            {
                var transferingItemList = group;

                foreach (var transferingItem in transferingItemList)
                {
                    sb.AppendFormat(@"
=======================
{0}
----
{1}         =>         {2}
{3}",
                                    transferingItem.TransferDateInfo, transferingItem.FromAccountName,
                                    transferingItem.ToAccountName, transferingItem.AmountInfo);
                }

                sb.AppendLine()
                    .Append("=======================");
            }

            return sb;
        }
        #endregion

        #region --- Private Methods ---

        /// <summary>
        /// Handles the PropertyChanged event of the TransferingHistoryViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void TransferingHistoryViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == FilterDateProperty || e.PropertyName == FilterTypeIndexProperty)
            {
                PerformLoadData();
            }
        }
        #endregion

        public Account TargetAccount { get; set; }

        /// <summary>
        /// return a bool value, indicate that the revert opreation is ok or not. If true, it's ok, otherwise, it's no cause of the 
        /// related accounts do not exist longer.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Revert(TransferingItem item)
        {
            var fromAccount = AccountBookDataContext.Accounts.FirstOrDefault(p => p.Id == item.FromAccountId);
            var toAccount = AccountBookDataContext.Accounts.FirstOrDefault(p => p.Id == item.ToAccountId);

            if (fromAccount != null && toAccount != null)
            {
                //exchange those two account.   
                ViewModelLocator
                    .AccountViewModel.Transfer(toAccount, fromAccount, item.Amount, item.Amount, false);

                //delete this transfering item.
                DeleteItem(item);

                return true;
            }

            return false;
        }
    }


}
