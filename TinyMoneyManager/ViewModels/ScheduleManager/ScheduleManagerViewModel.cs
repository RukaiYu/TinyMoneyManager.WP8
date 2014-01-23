namespace TinyMoneyManager.ViewModels.ScheduleManager
{
    using mangoProgressIndicator;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Data.ScheduleManager;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public class ScheduleManagerViewModel : NkjSoftViewModelBase
    {
        private SchedulePlanningManager manager;
        public static bool NeedReloadData = true;

        public ScheduleManagerViewModel()
        {
            this.Current = new TallySchedule();
            this.Current.ProfileRecordType = ScheduleRecordType.ScheduledRecord;
            this.SearchingCondition = new AssociatedItemsSelectorOption();
            this.SearchingCondition.SearchingScope = SearchingScope.CurrentMonth;
            this.Tasks = new ObservableCollection<TallySchedule>();
            this.manager = new SecondSchedulePlanningManager(this.AccountBookDataContext);
            this.HasLoadAssociatedItemsForCurrentViewAccount = false;
        }

        public void CreateAccountItemScheduleItem(TallySchedule scheduleInfo)
        {
            this.AccountBookDataContext.TallyScheduleTable.InsertOnSubmit(scheduleInfo);
            this.Tasks.Add(scheduleInfo);
            this.AccountBookDataContext.SubmitChanges();
            this.manager.AddTask(scheduleInfo);
        }

        public void Delete(TallySchedule obj)
        {
            this.AccountBookDataContext.TallyScheduleTable.DeleteOnSubmit(obj);
            this.Tasks.Remove(obj);
            this.AccountBookDataContext.SubmitChanges();
            this.manager.DeleteTask(obj);
        }

        public void ExecuteTask(TallySchedule scheduleItem)
        {
            if (this.manager.ExecuteOnePlan(scheduleItem))
            {
                ViewModelLocator.AccountItemViewModel.IsDataLoaded = false;
                ViewModelLocator.AccountViewModel.IsDataLoaded = false;
                ViewModelLocator.AccountViewModel.HasLoadAssociatedItemsForCurrentViewAccount = false;
                ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
                CommonExtensions.AlertNotification(null, AppResources.TaskCompletedMessageFormatter.FormatWith(new object[] { 1 }), null);
            }
        }

        public System.Collections.Generic.IEnumerable<String> GetAllScheduledDataResult()
        {
            return (from p in this.manager.GetScheduledDataFiles()
                    where !p.EndsWith(".err.xml")
                    select p);
        }

        public System.Collections.Generic.IEnumerable<GroupByCreateTimeAccountItemViewModel> GetGroupedRelatedItems(TallySchedule itemCompareTo, bool searchingOnlyCurrentMonthData = true, System.Action<AccountItem> itemAdded = null)
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
            using (System.Collections.Generic.IEnumerator<DateTime> enumerator = (from p in source select p.CreateTime.Date).Distinct<System.DateTime>().GetEnumerator())
            {
                System.Func<AccountItem, Boolean> predicate = null;
                System.DateTime item;
                while (enumerator.MoveNext())
                {
                    item = enumerator.Current;
                    GroupByCreateTimeAccountItemViewModel agvm = new GroupByCreateTimeAccountItemViewModel(item);
                    if (predicate == null)
                    {
                        predicate = p => p.CreateTime.Date == item.Date;
                    }
                    (from p in source.Where<AccountItem>(predicate)
                     orderby p.Money
                     select p).ToList<AccountItem>().ForEach(delegate(AccountItem x)
                    {
                        agvm.Add(x);
                        itemAdded(x);
                    });
                    list.Add(agvm);
                }
            }
            this.HasLoadAssociatedItemsForCurrentViewAccount = true;
            return list;
        }

        public System.Collections.Generic.IEnumerable<AccountItem> GetRelatedItems(TallySchedule itemCompareTo, bool searchingOnlyCurrentMonth = true)
        {
            System.Func<AccountItem, Boolean> predicate = null;
            System.Collections.Generic.IEnumerable<AccountItem> source = from p in this.AccountBookDataContext.AccountItems
                                                                         where ((((int)p.Type) == ((int)itemCompareTo.RecordType)) && (p.AutoTokenId == itemCompareTo.Id)) && (p.AutoTokenId != System.Guid.Empty)
                                                                         select p;
            if (!searchingOnlyCurrentMonth)
            {
                return source;
            }
            if (predicate == null)
            {
                predicate = p => (p.CreateTime.Date >= this.SearchingCondition.StartDate.GetValueOrDefault().Date) && (p.CreateTime.Date <= this.SearchingCondition.EndDate.GetValueOrDefault().Date);
            }
            return source.Where<AccountItem>(predicate);
        }

        public override void LoadDataIfNot()
        {
            if (!base.IsDataLoaded)
            {
                ((System.Collections.Generic.IEnumerable<TallySchedule>)(from p in
                                                                             (from p in this.AccountBookDataContext.TallyScheduleTable
                                                                              where ((int)p.ProfileRecordType) != 2
                                                                              select p).AsEnumerable<TallySchedule>()
                                                                         orderby p.Frequency
                                                                         select p)).ForEach<TallySchedule>(new System.Action<TallySchedule>(this.Tasks.Add));
                base.IsDataLoaded = true;
            }
        }

        public AccountItem ReBuildAccountItem(TallySchedule scheduleDataInfo)
        {
            return new AccountItem { Account = scheduleDataInfo.FromAccount, AccountId = scheduleDataInfo.FromAccountId, AutoTokenId = scheduleDataInfo.Id, Category = scheduleDataInfo.AssociatedCategory, CategoryId = scheduleDataInfo.CategoryId, Description = scheduleDataInfo.Notes, IsClaim = new bool?(scheduleDataInfo.IsClaim), Money = scheduleDataInfo.Money, State = AccountItemState.Active, Type = scheduleDataInfo.RecordType };
        }

        public void RecoveryDatas(bool forceToLoad = false, System.Action callBack = null)
        {
            if (HasLoadedOnce)
            {
                if (callBack != null)
                {
                    callBack();
                }
                if (!forceToLoad)
                {
                    return;
                }
            }
            HasLoadedOnce = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(this.recoveryWorker_DoWork);
            worker.RunWorkerCompleted += delegate(object o, RunWorkerCompletedEventArgs e)
            {
                if (callBack != null)
                {
                    callBack();
                }
            };
            worker.RunWorkerAsync();
        }

        private void recoveryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Collections.Generic.List<String> files = this.GetAllScheduledDataResult().ToList<string>();
            if (files.Count > 0)
            {
                GlobalIndicator.Instance.BusyForWork(AppResources.ScheduleManager_RecoveryingRecordsMessage, new object[0]);
                AccountViewModel model = new AccountViewModel();
                int counts = this.ScheduleManager.RecoverDataToDatabase(files, new System.Action<AccountItem>(model.HandleAccountItemAdding));
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    if (counts > 0)
                    {
                        ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
                        CommonExtensions.AlertNotification(null, AppResources.TaskCompletedMessageFormatter.FormatWith(new object[] { counts }), null);
                    }
                    else
                    {
                        CommonExtensions.AlertNotification(null, AppResources.OperationSuccessfullyMessage, null);
                    }
                    GlobalIndicator.Instance.WorkDone();
                });
            }
        }

        public void SetupPlanningFirstTime()
        {
            IsolatedAppSetingsHelper.ShowTipsByVerion("SetupPlanningFirstTime", delegate
            {
                this.manager.SetupTaskInfoProviderFile();
            });
        }

        public void Test()
        {
            try
            {
                if (!this.manager.CheckScheduleTaskAvaliable())
                {
                    this.manager.SetupTaskInfoProviderFile();
                }
                this.manager.ExecutePlan(null);
                int executedCount = this.manager.ExecutedCount;
            }
            catch (System.Exception exception)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    throw exception;
                }
                MessageBox.Show(exception.Message);
            }
        }

        public void Update(TallySchedule scheduleInfo)
        {
            this.AccountBookDataContext.SubmitChanges();
            this.manager.UpdateTaskInfo(scheduleInfo);
        }

        public TallySchedule Current { get; set; }

        public bool HasLoadAssociatedItemsForCurrentViewAccount { get; set; }

        public static bool HasLoadedOnce
        {
            get;
            set;
        }

        public SchedulePlanningManager ScheduleManager
        {
            get
            {
                return this.manager;
            }
            set
            {
                this.manager = value;
            }
        }

        public AssociatedItemsSelectorOption SearchingCondition { get; set; }

        public ObservableCollection<TallySchedule> Tasks { get; set; }
    }
}

