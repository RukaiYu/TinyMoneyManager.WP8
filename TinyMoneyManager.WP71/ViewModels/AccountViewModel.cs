namespace TinyMoneyManager.ViewModels
{
    using mangoProgressIndicator;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Linq;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class AccountViewModel : NkjSoftViewModelBase
    {
        private static readonly string[] AccentColors = new string[] { 
            "magenta", "purple", "teal", "lime", "brown", "pink", "orange", "blue", "red", "green", "#FF762C2C", "#FFF26D6D", "#FF6C8343", "#FF936A0F", "#FF2E9080", "#FF00620F", 
            "#FF2356AA", "#FFA31780"
         };
        private static Account[] defaultAccountsCache = null;
        private bool hasLoadAssociatedItemsForCurrentViewAccount;
        private AssociatedItemsSelectorOption searchingConditionForAssociatedItemsForCurrentViewAccount;

        public AccountViewModel()
        {
            this.Accounts = new ObservableCollection<Account>();
            AssociatedItemsSelectorOption option = new AssociatedItemsSelectorOption
            {
                SearchingScope = SearchingScope.CurrentMonth
            };

            this.SearchingConditionAssociatedItemsForCurrentViewAccount = option;
        }

        public Action DisableControl = null;

        public void AddAccount(params Account[] accountForAdds)
        {
            IQueryable<System.Guid> ids = from p in this.AccountBookDataContext.Accounts select p.Id;
            System.Collections.Generic.IEnumerable<Account> entities = from p in accountForAdds
                                                                       where !ids.Contains<System.Guid>(p.Id)
                                                                       select p;
            foreach (Account account in entities)
            {
                this.Accounts.Add(account);
            }
            this.AccountBookDataContext.Accounts.InsertAllOnSubmit<Account>(entities);
            this.SubmitChanges();
            base.IsDataLoaded = true;
        }

        public void DeleteItem(Account accountForDelete)
        {
            this.Accounts.Remove(accountForDelete);
            this.AccountBookDataContext.Accounts.DeleteOnSubmit(accountForDelete);
            this.SubmitChanges();
        }

        public bool EnsureUsed(System.Guid id)
        {
            return (this.AccountBookDataContext.AccountItems.Count<AccountItem>(p => (p.AccountId == id)) > 0);
        }

        public bool ExistAccount(PageActionType action, Account accountToCompare)
        {
            if (action == PageActionType.Add)
            {
                return (this.Accounts.Count<Account>(p => (p.Name == accountToCompare.Name)) > 0);
            }
            return (this.Accounts.Count<Account>(p => ((p.Name == accountToCompare.Name) && (p.Id != accountToCompare.Id))) > 0);
        }

        public int GetBankOrCreditAccountsCount()
        {
            return this.AccountBookDataContext.Accounts.Count<Account>(p => ((((int)p.Category) == 1) || (((int)p.Category) == 4)));
        }

        public void HandleAccountItemAdding(AccountItem accountItem)
        {
            ViewModelLocator.AccountItemViewModel.AddItem(accountItem);
            ViewModelLocator.AccountViewModel.WithdrawlOrDeposit(accountItem.Account, (accountItem.Type == ItemType.Expense) ? -accountItem.Money : accountItem.Money);
            ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
        }

        private void RecoverPeoples(AccountItem accountItem, TallySchedule taskInfo)
        {
            if (taskInfo.Peoples != null && taskInfo.Peoples.Length > 0)
            {
                var ids = taskInfo.Peoples;

                var originalData =
                    AccountBookDataContext.PeopleAssociationDatas
                    .Where(p => ids.Contains(p.PeopleId) && p.AttachedId == accountItem.AutoTokenId)
                    .ToList()
                    .Select(p => new PeopleAssociationData()
                    {
                        PeopleId = p.PeopleId,
                        Comments = p.Comments,
                        AttachedId = accountItem.Id,
                        CreateAt = DateTime.Now
                    }).ToList();

                foreach (var pic in originalData)
                {
                    AccountBookDataContext.PeopleAssociationDatas.InsertOnSubmit(pic);
                }
            }
        }

        /// <summary>
        /// Recovers the picutres.
        /// </summary>
        /// <param name="accountItem">The account item.</param>
        /// <param name="taskInfo">The task info.</param>
        private void RecoverPicutres(AccountItem accountItem, TallySchedule taskInfo)
        {
            if (taskInfo.Pictures != null && taskInfo.Pictures.Length > 0)
            {
                var ids = taskInfo.Pictures;

                var originalPictures =
                    AccountBookDataContext.PictureInfos
                    .Where(p => ids.Contains(p.PictureId) && p.Tag == PictureInfo.ScheduledAccountItemsTag)
                    .ToList()
                    .Select(p => new PictureInfo()
                    {
                        PictureId = p.PictureId,
                        AttachedId = accountItem.Id,
                        Comments = p.Comments,
                        CreateAt = DateTime.Now,
                        FullPath = p.FullPath,
                        FileName = p.FileName,
                        Tag = PictureInfo.AccountItemsTag
                    }).ToList();

                ViewModelLocator.PicturesViewModel.SavePicturesFrom(originalPictures, PictureInfo.ScheduledAccountItemsTag);

                foreach (var pic in originalPictures)
                {
                    //accountItem.Pictures.Add(pic);

                    AccountBookDataContext.PictureInfos.InsertOnSubmit(pic);
                    //accountItem.Pictures.Add(pic);
                }
            }
        }

        public void HandleAccountItemDeleting(AccountItem accountItem)
        {
            try
            {
                if (accountItem != null)
                {
                    this.HandleAccountItemEditing(accountItem, accountItem.Account, accountItem.Money, -accountItem.Money);
                    ViewModelLocator.PicturesViewModel.DeletePictures(accountItem.Pictures);
                    ViewModelLocator.AccountItemViewModel.RemoveItem(accountItem);
                    ViewModelLocator.AccountItemViewModel.SubmitChanges();
                }
            }
            catch (ChangeConflictException exception)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Handles the account item editing.
        /// </summary>
        /// <param name="currentEditObject">The current edit object.</param>
        /// <param name="oldAccount">The old account.</param>
        /// <param name="oldMoney">The old money.</param>
        /// <param name="oldBalance">The old balance.</param>
        public void HandleAccountItemEditing(AccountItem currentEditObject, Account oldAccount, decimal oldMoney, decimal oldBalance)
        {
            if (oldAccount == null) { return; }

            if (oldAccount.Id == currentEditObject.Account.Id)
            {
                if (oldBalance != 0.0M)
                {
                    oldBalance = (currentEditObject.Type == ItemType.Expense) ? -oldBalance : oldBalance;
                    this.WithdrawlOrDeposit(currentEditObject.Account, oldBalance);
                }
            }
            else
            {
                decimal amount = oldMoney;
                if (currentEditObject.Type == ItemType.Expense)
                {
                    this.WithdrawlOrDeposit(oldAccount, amount);
                    this.WithdrawlOrDeposit(currentEditObject.Account, -amount);
                }
                else if (currentEditObject.Type == ItemType.Income)
                {
                    this.WithdrawlOrDeposit(oldAccount, -amount);
                    this.WithdrawlOrDeposit(currentEditObject.Account, amount);
                }
            }
            ViewModelLocator.AccountItemViewModel.Update(currentEditObject);
            ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
        }

        public System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> LoadAssociatedItemsForAccount(
            Account itemCompareToCurrent, ObservableCollection<GroupByCreateTimeAccountItemViewModel> out_container)
        {
            HasLoadAssociatedItemsForCurrentViewAccount = true;

            var container = new List<GroupByCreateTimeAccountItemViewModel>();

            int recordCount = 0;
            var eachCount = 0;

            Action<int, string> showRecordTips = (count, name) =>
            {
                recordCount += count;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    mangoProgressIndicator.GlobalIndicator.Instance.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(AppResources.RecordCountTipsFormatter.FormatWith(count, name)));
                });

            };


            if (searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.All
                || searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.Transcations)
            {
                var accountItemsData = ViewModelLocator.AccountItemViewModel
                    .AccountBookDataContext.AccountItems.Where(p => p.AccountId == itemCompareToCurrent.Id);

                if (searchingConditionForAssociatedItemsForCurrentViewAccount.SearchingScope != SearchingScope.All)
                {
                    accountItemsData = accountItemsData.Where(p => p.CreateTime.Date >= searchingConditionForAssociatedItemsForCurrentViewAccount.StartDate.Value.Date
                                           && p.CreateTime.Date <= searchingConditionForAssociatedItemsForCurrentViewAccount.EndDate.Value.Date);
                }

                accountItemsData = accountItemsData.OrderByDescending(p => p.Type)
                .OrderByDescending(p => p.CreateTime.Date);

                eachCount = accountItemsData.Count();

                showRecordTips(eachCount, AppResources.SelectAccountItemsLabel);

                var dates = accountItemsData.Select(p => p.CreateTime.Date).Distinct().ToList();
                foreach (var item in dates)
                {
                    var agvm = new GroupByCreateTimeAccountItemViewModel(item);
                    accountItemsData.Where(p => p.CreateTime.Date == item.Date)
                        .ForEach(x =>
                        {
                            var cx = new AccountItem();
                            cx.Account = x.Account;
                            cx.Money = x.Money;
                            cx.CreateTime = x.CreateTime;
                            cx.Category = x.Category;
                            cx.CategoryId = x.CategoryId;
                            cx.Description = x.Description;
                            cx.Id = x.Id;
                            cx.State = x.State;
                            cx.Type = x.Type;

                            cx.TypeInfo = LocalizedStrings.GetLanguageInfoByKey(x.Type.ToString());
                            if (x.Type == ItemType.Expense)
                                cx.Money = -x.Money;
                            cx.SecondInfo = x.NameInfo;

                            cx.PageNameGetter = PictureInfo.AccountItemsTag;
                            cx.ThirdInfo = x.Description;
                            agvm.Add(cx);
                            //OnItemAdding(x);
                        });

                    container.Add(agvm);
                }
            }

            if (searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.All
               || searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.TransferingAccount)
            {
                // load Transfering list.

                var transfering = ViewModelLocator.TransferingHistoryViewModel
                    .AccountBookDataContext.TransferingItems.Where(p => (p.ToAccountId == itemCompareToCurrent.Id || p.FromAccountId == itemCompareToCurrent.Id)
                      );

                if (searchingConditionForAssociatedItemsForCurrentViewAccount.SearchingScope != SearchingScope.All)
                {
                    transfering = transfering.Where(p => p.TransferingDate.Date >= searchingConditionForAssociatedItemsForCurrentViewAccount.StartDate.Value.Date
                      && p.TransferingDate.Date <= searchingConditionForAssociatedItemsForCurrentViewAccount.EndDate.Value.Date);
                }

                transfering = transfering.OrderByDescending(p => p.TransferingDate.Date);

                eachCount = transfering.Count();

                var exchange = AppResources.TransferingAccount;
                showRecordTips(eachCount, exchange);

                var dates = transfering.Select(p => p.TransferingDate.Date).Distinct().ToList();

                foreach (var item in dates)
                {
                    var agvm = container.FirstOrDefault(p => p.Key.Date == item.Date);
                    bool hasToAdd = false;
                    if (agvm == null)
                    {
                        hasToAdd = true;
                        agvm = new GroupByCreateTimeAccountItemViewModel(item);
                    }

                    transfering.Where(p => p.TransferingDate.Date == item.Date)
                        .ForEach(x =>
                        {
                            var warp = new AccountItem();
                            warp.Account = itemCompareToCurrent;

                            warp.TypeInfo = exchange;

                            bool isOut = x.FromAccountId == itemCompareToCurrent.Id;
                            warp.Money = isOut ? -x.Amount : x.Amount;

                            var key = isOut ?
                                (AppResources.RolloutToSomeAccountName.FormatWith(x.ToAccountName))
                                : (AppResources.TransferFromAccountName.FormatWith(x.FromAccountName));

                            warp.SecondInfo = key;

                            warp.ThirdInfo = x.Notes;
                            agvm.Add(warp);
                        });

                    if (hasToAdd)
                        container.Add(agvm);
                }
            }

            if (searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.All
               || searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.BorrowAndLean)
            {
                // load Transfering list.

                var borrowLoans = ViewModelLocator.BorrowLeanViewModel
                    .AccountBookDataContext.Repayments.Where(p => (p.FromAccountId == itemCompareToCurrent.Id && p.RepaymentRecordType == RepaymentType.MoneyBorrowOrLeanRepayment));

                if (searchingConditionForAssociatedItemsForCurrentViewAccount.SearchingScope != SearchingScope.All)
                {
                    //borrowLoans = borrowLoans.Where(p => p.ExecuteDate.Value.Date >= searchingConditionForAssociatedItemsForCurrentViewAccount.StartDate.Value.Date
                    //  && ((DateTime)p.ExecuteDate).Date <= searchingConditionForAssociatedItemsForCurrentViewAccount.EndDate.Value.Date);
                }

                borrowLoans = borrowLoans.OrderByDescending(p => ((DateTime)p.ExecuteDate).Date);

                eachCount = borrowLoans.Count();
                var borrowLoanName = AppResources.BorrowAndLean;

                showRecordTips(eachCount, borrowLoanName);


                var dates = borrowLoans.Select(p => p.ExecuteDate.Value.Date).Distinct().ToList();

                foreach (var item in dates)
                {
                    var agvm = container.FirstOrDefault(p => p.Key.Date == item.Date);
                    bool hasToAdd = false;
                    if (agvm == null)
                    {
                        hasToAdd = true;
                        agvm = new GroupByCreateTimeAccountItemViewModel(item);
                    }

                    borrowLoans.Where(p => ((DateTime)p.ExecuteDate).Date == item.Date)
                        .ForEach(x =>
                        {
                            var warp = new AccountItem();
                            warp.Account = itemCompareToCurrent;
                            warp.Id = x.Id;

                            warp.TypeInfo = borrowLoanName;

                            bool isOut = x.BorrowOrLean == LeanType.LoanOut || x.BorrowOrLean == LeanType.Repayment;
                            warp.Money = isOut ? -x.Amount : x.Amount;

                            var key = x.BorrowLoanInfoWithoutAmountInfo;

                            warp.SecondInfo = key;

                            warp.PageNameGetter = x.IsRepaymentOrReceieve ? "RepaymentsTableForReceieveOrPayBack" : "Repayments";
                            warp.ThirdInfo = x.Notes;
                            agvm.Add(warp);
                        });

                    if (hasToAdd)
                        container.Add(agvm);
                }
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                container.OrderByDescending(p => p.Key).ToList()
                    .ForEach(out_container.Add);
            });

            return container;
        }

        public void LoadColors()
        {
            if (this.ColorsUnused == null)
            {
                this.ColorsUnused = new ObservableCollection<string>();
            }
            else
            {
                this.ColorsUnused.Clear();
            }
            AccentColors.Except<string>((from p in this.Accounts select p.Color).ToArray<string>()).ForEach<string>(delegate(string p)
            {
                this.ColorsUnused.Add(p);
            });
            this.IsColorLoaded = true;
        }

        public override void LoadData()
        {
            System.Threading.ThreadStart start = null;
            if (!base.IsDataLoaded)
            {
                try
                {
                    this.Accounts.Clear();
                    if (start == null)
                    {
                        start = delegate
                        {
                            System.Collections.Generic.List<Account> __accounts = (from p in this.AccountBookDataContext.Accounts
                                                                                   orderby p.Order
                                                                                   select p).ToList<Account>();
                            Deployment.Current.Dispatcher.BeginInvoke(delegate
                            {
                                System.Action<Account> action = null;
                                if (__accounts.Count<Account>() == 0)
                                {
                                    this.AddAccount(DefaultAccounts);
                                    __accounts = DefaultAccounts.ToList<Account>();
                                }
                                else
                                {
                                    if (action == null)
                                    {
                                        action = delegate(Account p)
                                        {
                                            this.Accounts.Add(p);
                                        };
                                    }
                                    __accounts.ForEach(action);
                                }

                                setDefaultAccount(__accounts);
                            });
                        };
                    }
                    new System.Threading.Thread(start).Start();
                }
                catch (System.Exception exception)
                {
                    if (!AppUpdater.EnsureItsADatabaseIssue(exception))
                    {
                        exception.AlertErrorMessage(AppResources.ErrorMessageWhenLoadingAccounts);
                    }
                }
                base.IsDataLoaded = true;
            }
        }

        /// <summary>
        /// Sets the default account.
        /// </summary>
        /// <param name="accounts">The accounts.</param>
        private void setDefaultAccount(IEnumerable<Account> accounts)
        {
            Account account = accounts.FirstOrDefault<Account>(p => p.Id == AppSetting.Instance.DefaultAccount);

            this.DefaultAccountForEditing = account;
            if (this.DefaultAccountForEditing == null)
            {
                this.DefaultAccountForEditing = this.Accounts.FirstOrDefault<Account>();
            }

            if (this.DefaultAccountForEditing != null)
            {
                AppSetting.Instance.DefaultAccount = this.DefaultAccountForEditing.Id;
            }
            else
            {
                MessageBox.Show(AppResources.OptNeedRestart);
                if (DisableControl != null)
                {
                    DisableControl();
                }
            }
        }

        public override void QuickLoadData()
        {
            this.Accounts.Clear();
            (from p in this.AccountBookDataContext.Accounts
             orderby p.Category
             select p).ToList<Account>()
             .OrderBy(p => p.Order).ForEach(new System.Action<Account>(this.Accounts.Add));

            base.IsDataLoaded = true;

            setDefaultAccount(this.Accounts);
        }

        public void SetMostOftenAccount(Account account)
        {
            AppSetting.Instance.DefaultAccount = account.Id;

            if (this.DefaultAccountForEditing == null)
            {
                return;
            }

            this.DefaultAccountForEditing.IsDefaultAccount = false;
            this.DefaultAccountForEditing = account;
            account.IsDefaultAccount = true;
            SettingPageViewModel.Update();
        }

        public override void SubmitChanges()
        {
            this.AccountBookDataContext.SubmitChanges();
            ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
        }

        public void Transfer(Account from, Account to, decimal amountFromFrom, decimal amountToTo, bool addTransferingHistory = true)
        {
            decimal? balance = from.Balance;
            decimal num = amountFromFrom;

            from.Balance = balance.HasValue ? new decimal?(balance.GetValueOrDefault() - num) : null;
            decimal? nullable3 = to.Balance;

            decimal num2 = amountToTo;
            to.Balance = nullable3.HasValue ? new decimal?(nullable3.GetValueOrDefault() + num2) : null;
            this.SubmitChanges();
        }

        /// <summary>
        /// Transfers the specified transfering item.
        /// </summary>
        /// <param name="transferingItem">The transfering item.</param>
        public void Transfer(TransferingItem transferingItem)
        {
            var from = this.AccountBookDataContext.Accounts.FirstOrDefault(p => p.Id == transferingItem.FromAccountId);
            var amountFrom = transferingItem.Amount;

            var to = this.AccountBookDataContext.Accounts.FirstOrDefault(p => p.Id == transferingItem.ToAccountId);

            decimal? balance = from.Balance;
            decimal num = amountFrom;

            from.Balance = balance.HasValue ? new decimal?(balance.GetValueOrDefault() - num) : null;
            decimal? nullable3 = to.Balance;

            CurrencyType currencyFrom = from.Currency;
            CurrencyType currency = to.Currency;

            var amountToTo = currencyFrom.GetConversionRateTo(currency) * amountFrom;

            to.Balance = nullable3.HasValue ? new decimal?(nullable3.GetValueOrDefault() + amountToTo) : null;

            AccountBookDataContext.TransferingItems.InsertOnSubmit(transferingItem);

            this.SubmitChanges();
        }

        public void Update()
        {
            this.SubmitChanges();
        }

        public AccountViewModel UpdateOnSubmit(Account accountForEdit)
        {
            this.SubmitChanges();
            return this;
        }

        public void WithdrawlOrDeposit(Account account, decimal amount)
        {
            decimal? balance = account.Balance;
            decimal num = amount;
            account.Balance = balance.HasValue ? new decimal?(balance.GetValueOrDefault() + num) : num;

            //Account account2 = this.Accounts.FirstOrDefault<Account>(p => p.Id == account.Id);
            //if (account2 != null)
            //{
            //    account2.Balance = account.Balance;
            //}
            this.SubmitChanges();
        }

        public ObservableCollection<Account> Accounts { get; set; }

        public ObservableCollection<string> ColorsUnused { get; set; }

        public Account CurrentForView { get; set; }

        public Account DefaultAccountForEditing { get; set; }

        public static Account[] DefaultAccounts
        {
            get
            {
                if (defaultAccountsCache == null)
                {
                    defaultAccountsCache = LocalizedFileHelper.LoadDataSourceFromLocalizedFile<Account>(AppSetting.Instance.DisplayLanguage, "DefaultAccounts.xml").ToArray();
                }
                return defaultAccountsCache;
            }
        }

        public bool HasLoadAssociatedItemsForCurrentViewAccount
        {
            get
            {
                return this.hasLoadAssociatedItemsForCurrentViewAccount;
            }
            set
            {
                if (this.hasLoadAssociatedItemsForCurrentViewAccount != value)
                {
                    this.OnNotifyPropertyChanging("HasLoadAssociatedItemsForCurrentViewAccount");
                    this.hasLoadAssociatedItemsForCurrentViewAccount = value;
                    this.OnNotifyPropertyChanged("HasLoadAssociatedItemsForCurrentViewAccount");
                }
            }
        }

        public bool IsColorLoaded { get; set; }

        public AssociatedItemsSelectorOption SearchingConditionAssociatedItemsForCurrentViewAccount
        {
            get
            {
                return this.searchingConditionForAssociatedItemsForCurrentViewAccount;
            }
            set
            {
                if (this.searchingConditionForAssociatedItemsForCurrentViewAccount != value)
                {
                    this.OnNotifyPropertyChanging("SearchingConditionAssociatedItemsForCurrentViewAccount");
                    this.searchingConditionForAssociatedItemsForCurrentViewAccount = value;
                    this.OnNotifyPropertyChanged("SearchingConditionAssociatedItemsForCurrentViewAccount");
                }
            }
        }

        /// <summary>
        /// Handles the account item adding for task.
        /// </summary>
        /// <param name="accountItem">The account item.</param>
        /// <param name="taskInfo">The task info.</param>
        public void HandleAccountItemAddingForTask(AccountItem accountItem, TallySchedule taskInfo)
        {
            if (taskInfo != null)
            {
                if (!taskInfo.PeopleIds.IsNullOrEmpty())
                {
                    // 
                    RecoverPeoples(accountItem, taskInfo);
                }

                if (!taskInfo.PictureIds.IsNullOrEmpty())
                {
                    //
                    RecoverPicutres(accountItem, taskInfo);
                }
            }

            ViewModelLocator.AccountItemViewModel.AddItem(accountItem);
            ViewModelLocator.AccountViewModel.WithdrawlOrDeposit(accountItem.Account, (accountItem.Type == ItemType.Expense) ? -accountItem.Money : accountItem.Money);
        }
    }
}

