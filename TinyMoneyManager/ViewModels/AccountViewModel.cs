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

        public System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> LoadAssociatedItemsForAccount(Account itemCompareToCurrent, ObservableCollection<GroupByCreateTimeAccountItemViewModel> out_container)
        {
            this.HasLoadAssociatedItemsForCurrentViewAccount = true;
            System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> container = new System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel>();
            int recordCount = 0;
            int num = 0;
            System.Action<Int32, String> action = delegate(int count, string name)
            {
                recordCount += count;
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    GlobalIndicator.Instance.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.RecordCountTipsFormatter.FormatWith(new object[] { count, name }) }), new object[0]);
                });
            };
            if ((this.searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.All) || (this.searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.Transcations))
            {
                IQueryable<AccountItem> source = from p in ViewModelLocator.AccountItemViewModel.AccountBookDataContext.AccountItems
                                                 where p.AccountId == itemCompareToCurrent.Id
                                                 select p;
                if (this.searchingConditionForAssociatedItemsForCurrentViewAccount.SearchingScope != SearchingScope.All)
                {
                    source = from p in source
                             where (p.CreateTime.Date > this.searchingConditionForAssociatedItemsForCurrentViewAccount.StartDate.Value.Date) && (p.CreateTime.Date <= this.searchingConditionForAssociatedItemsForCurrentViewAccount.EndDate.Value.Date)
                             select p;
                }
                source = from p in source
                         orderby p.Type descending
                         select p into p
                         orderby p.CreateTime.Date descending
                         select p;
                num = source.Count<AccountItem>();
                action(num, AppResources.SelectAccountItemsLabel);
                using (System.Collections.Generic.List<DateTime>.Enumerator enumerator = (from p in source select p.CreateTime.Date).Distinct<System.DateTime>().ToList<System.DateTime>().GetEnumerator())
                {
                    System.DateTime item;
                    while (enumerator.MoveNext())
                    {
                        item = enumerator.Current;
                        GroupByCreateTimeAccountItemViewModel agvm = new GroupByCreateTimeAccountItemViewModel(item);
                        ((System.Collections.Generic.IEnumerable<AccountItem>)(from p in source
                                                                               where p.CreateTime.Date == item.Date
                                                                               select p)).ForEach<AccountItem>(delegate(AccountItem x)
                        {
                            AccountItem item1 = new AccountItem
                            {
                                Account = x.Account,
                                Money = x.Money,
                                CreateTime = x.CreateTime,
                                Category = x.Category,
                                CategoryId = x.CategoryId,
                                Description = x.Description,
                                Id = x.Id,
                                State = x.State,
                                PageNameGetter = "AccountItems",
                                Type = x.Type,
                                TypeInfo = LocalizedStrings.GetLanguageInfoByKey(x.Type.ToString())
                            };
                            if (x.Type == ItemType.Expense)
                            {
                                item1.Money = -x.Money;
                            }
                            item1.SecondInfo = x.NameInfo;
                            item1.ThirdInfo = x.Description;
                            agvm.Add(item1);
                        });
                        container.Add(agvm);
                    }
                }
            }
            if ((this.searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.All) || (this.searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.TransferingAccount))
            {
                IQueryable<TransferingItem> queryable2 = from p in ViewModelLocator.TransferingHistoryViewModel.AccountBookDataContext.TransferingItems
                                                         where (p.ToAccountId == itemCompareToCurrent.Id) || (p.FromAccountId == itemCompareToCurrent.Id)
                                                         select p;
                if (this.searchingConditionForAssociatedItemsForCurrentViewAccount.SearchingScope != SearchingScope.All)
                {
                    queryable2 = from p in queryable2
                                 where (p.TransferingDate.Date > this.searchingConditionForAssociatedItemsForCurrentViewAccount.StartDate.Value.Date) && (p.TransferingDate.Date <= this.searchingConditionForAssociatedItemsForCurrentViewAccount.EndDate.Value.Date)
                                 select p;
                }
                queryable2 = from p in queryable2
                             orderby p.TransferingDate.Date descending
                             select p;
                num = queryable2.Count<TransferingItem>();
                string exchange = AppResources.TransferingAccount;
                action(num, exchange);
                using (System.Collections.Generic.List<DateTime>.Enumerator enumerator2 = (from p in queryable2 select p.TransferingDate.Date).Distinct<System.DateTime>().ToList<System.DateTime>().GetEnumerator())
                {
                    System.Func<GroupByCreateTimeAccountItemViewModel, Boolean> predicate = null;
                    System.DateTime item;
                    while (enumerator2.MoveNext())
                    {
                        item = enumerator2.Current;
                        if (predicate == null)
                        {
                            predicate = p => p.Key.Date == item.Date;
                        }
                        GroupByCreateTimeAccountItemViewModel agvm = container.FirstOrDefault<GroupByCreateTimeAccountItemViewModel>(predicate);
                        bool flag = false;
                        if (agvm == null)
                        {
                            flag = true;
                            agvm = new GroupByCreateTimeAccountItemViewModel(item);
                        }
                        ((System.Collections.Generic.IEnumerable<TransferingItem>)(from p in queryable2
                                                                                   where p.TransferingDate.Date == item.Date
                                                                                   select p)).ForEach<TransferingItem>(delegate(TransferingItem x)
                        {
                            AccountItem item1 = new AccountItem
                            {
                                Account = itemCompareToCurrent,
                                TypeInfo = exchange
                            };
                            flag = x.FromAccountId == itemCompareToCurrent.Id;
                            item1.Money = flag ? -x.Amount : x.Amount;
                            string str = flag ? AppResources.RolloutToSomeAccountName.FormatWith(new object[] { x.ToAccountName }) : AppResources.TransferFromAccountName.FormatWith(new object[] { x.FromAccountName });
                            item1.SecondInfo = str;
                            item1.PageNameGetter = "TransferingItems";
                            item1.ThirdInfo = x.Notes;
                            agvm.Add(item1);
                        });
                        if (flag)
                        {
                            container.Add(agvm);
                        }
                    }
                }
            }
            if ((this.searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.All)
                || (this.searchingConditionForAssociatedItemsForCurrentViewAccount.DataType == AssociatedItemType.BorrowAndLean))
            {
                IQueryable<Repayment> queryable3 = from p in ViewModelLocator.BorrowLeanViewModel.AccountBookDataContext.Repayments
                                                   where (p.FromAccountId == itemCompareToCurrent.Id) && (((int?)p.RepaymentRecordType) == ((int?)RepaymentType.MoneyBorrowOrLeanRepayment))
                                                   select p;
                SearchingScope searchingScope = this.searchingConditionForAssociatedItemsForCurrentViewAccount.SearchingScope;
                queryable3 = from p in queryable3
                             orderby ((System.DateTime)p.ExecuteDate).Date descending
                             select p;
                num = queryable3.Count<Repayment>();
                string borrowLoanName = AppResources.BorrowAndLean;
                action(num, borrowLoanName);
                using (System.Collections.Generic.List<DateTime>.Enumerator enumerator3 = (from p in queryable3 select p.ExecuteDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>().GetEnumerator())
                {
                    System.Func<GroupByCreateTimeAccountItemViewModel, Boolean> func2 = null;
                    System.DateTime item;
                    while (enumerator3.MoveNext())
                    {
                        item = enumerator3.Current;
                        if (func2 == null)
                        {
                            func2 = p => p.Key.Date == item.Date;
                        }
                        GroupByCreateTimeAccountItemViewModel agvm = container.FirstOrDefault<GroupByCreateTimeAccountItemViewModel>(func2);
                        bool flag2 = false;
                        if (agvm == null)
                        {
                            flag2 = true;
                            agvm = new GroupByCreateTimeAccountItemViewModel(item);
                        }
                        ((System.Collections.Generic.IEnumerable<Repayment>)(from p in queryable3
                                                                             where ((System.DateTime)p.ExecuteDate).Date == item.Date
                                                                             select p)).ForEach<Repayment>(delegate(Repayment x)
                        {
                            LeanType? borrowOrLean = LeanType.BorrowIn;
                            AccountItem item1 = new AccountItem
                            {
                                Id = x.Id,
                                Account = itemCompareToCurrent,
                                TypeInfo = borrowLoanName
                            };

                            if (((LeanType)x.BorrowOrLean) != LeanType.LoanOut)
                            {
                                borrowOrLean = x.BorrowOrLean;
                            }
                            bool flag = (((LeanType)borrowOrLean.GetValueOrDefault()) != LeanType.Repayment) || borrowOrLean.HasValue;
                            item1.Money = flag ? -x.Amount : x.Amount;
                            string borrowLoanInfoWithoutAmountInfo = x.BorrowLoanInfoWithoutAmountInfo;
                            item1.PageNameGetter = x.IsRepaymentOrReceieve ? "RepaymentsTableForReceieveOrPayBack" : "Repayments";
                            item1.SecondInfo = borrowLoanInfoWithoutAmountInfo;
                            item1.ThirdInfo = x.Notes;
                            agvm.Add(item1);
                        });
                        if (flag2)
                        {
                            container.Add(agvm);
                        }
                    }
                }
            }
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                (from p in container
                 orderby p.Key descending
                 select p).ToList<GroupByCreateTimeAccountItemViewModel>().ForEach(new System.Action<GroupByCreateTimeAccountItemViewModel>(out_container.Add));
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
                                                                                   orderby p.Category
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
                                Account account = __accounts.FirstOrDefault<Account>(p => p.Id == AppSetting.Instance.DefaultAccount);
                                this.DefaultAccountForEditing = account;
                                if (this.DefaultAccountForEditing == null)
                                {
                                    this.DefaultAccountForEditing = this.Accounts.FirstOrDefault<Account>(p => !p.CanBeDeleted);
                                }
                                AppSetting.Instance.DefaultAccount = this.DefaultAccountForEditing.Id;
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

        public override void QuickLoadData()
        {
            this.Accounts.Clear();
            (from p in this.AccountBookDataContext.Accounts
             orderby p.Category
             select p).ToList<Account>().ForEach(new System.Action<Account>(this.Accounts.Add));
            base.IsDataLoaded = true;
        }

        public void SetMostOftenAccount(Account account)
        {
            AppSetting.Instance.DefaultAccount = account.Id;
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
            account.Balance = balance.HasValue ? new decimal?(balance.GetValueOrDefault() + num) : null;
            Account account2 = this.Accounts.FirstOrDefault<Account>(p => p.Id == account.Id);
            if (account2 != null)
            {
                account2.Balance = account.Balance;
            }
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
    }
}

