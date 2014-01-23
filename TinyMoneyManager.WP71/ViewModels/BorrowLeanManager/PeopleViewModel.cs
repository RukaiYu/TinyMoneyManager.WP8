namespace TinyMoneyManager.ViewModels
{
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class PeopleViewModel : NkjSoftViewModelBase
    {

        public ObservableCollection<PeopleProfile> PeopleList { get; set; }

        public AssociatedItemsSelectorOption SearchingCondition { get; set; }

        public PeopleViewModel()
        {
            this.PeopleList = new ObservableCollection<PeopleProfile>();
            AssociatedItemsSelectorOption option = new AssociatedItemsSelectorOption
            {
                DataType = AssociatedItemType.All
            };
            this.SearchingCondition = option;
        }

        public override void Delete<T>(T obj)
        {
            this.PeopleList.Remove(obj as PeopleProfile);
            base.Delete<T>(obj);
        }

        public override void LoadDataIfNot()
        {
            if (!base.IsDataLoaded)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    this.PeopleList.Clear();
                });
                this.AccountBookDataContext.Peoples.ToList<PeopleProfile>().ForEach(delegate(PeopleProfile p)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        this.PeopleList.Add(p);
                    });
                });
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    base.IsDataLoaded = true;
                });
            }
        }

        public ObservableCollection<GroupByCreateTimeAccountItemViewModel> LoadRelatedItems(PeopleProfile current)
        {
            ObservableCollection<GroupByCreateTimeAccountItemViewModel> source = new ObservableCollection<GroupByCreateTimeAccountItemViewModel>();
            if ((this.SearchingCondition.DataType == AssociatedItemType.All) || (this.SearchingCondition.DataType == AssociatedItemType.BorrowAndLean))
            {
                IQueryable<Repayment> queryable = from p in ViewModelLocator.BorrowLeanViewModel.AccountBookDataContext.Repayments
                                                  where (p.ToPeopleId == current.Id) && (((int?)p.RepaymentRecordType) == ((int?)RepaymentType.MoneyBorrowOrLeanRepayment))
                                                  select p;
                SearchingScope searchingScope = this.SearchingCondition.SearchingScope;
                queryable = from p in queryable
                            orderby ((System.DateTime)p.ExecuteDate).Date descending
                            select p;
                string borrowLoanName = AppResources.BorrowAndLean;
                using (System.Collections.Generic.List<DateTime>.Enumerator enumerator = (from p in queryable select p.ExecuteDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>().GetEnumerator())
                {
                    System.Func<GroupByCreateTimeAccountItemViewModel, Boolean> predicate = null;
                    System.DateTime item;
                    while (enumerator.MoveNext())
                    {
                        item = enumerator.Current;
                        if (predicate == null)
                        {
                            predicate = p => p.Key.Date == item.Date;
                        }
                        GroupByCreateTimeAccountItemViewModel agvm = source.FirstOrDefault<GroupByCreateTimeAccountItemViewModel>(predicate);
                        bool flag = false;
                        if (agvm == null)
                        {
                            flag = true;
                            agvm = new GroupByCreateTimeAccountItemViewModel(item);
                        }
                        ((System.Collections.Generic.IEnumerable<Repayment>)(from p in queryable
                                                                             where ((System.DateTime)p.ExecuteDate).Date == item.Date
                                                                             select p)).ForEach<Repayment>(delegate(Repayment x)
                        {
                            AccountItem item1 = new AccountItem
                            {
                                Id = x.Id,
                                Account = x.PayFromAccount,
                                TypeInfo = borrowLoanName,
                                Money = x.Amount,
                                PageNameGetter = x.IsRepaymentOrReceieve ? "RepaymentsTableForReceieveOrPayBack" : "Repayments"
                            };
                            string borrowLoanInfoForPeople = x.GetBorrowLoanInfoForPeople(AppResources.My + "({0})" + AppResources.AccountName, false);
                            item1.SecondInfo = borrowLoanInfoForPeople;
                            item1.ThirdInfo = x.Notes;
                            agvm.Add(item1);
                        });
                        if (flag)
                        {
                            source.Add(agvm);
                        }
                    }
                }
            }
            if ((this.SearchingCondition.DataType == AssociatedItemType.All) || (this.SearchingCondition.DataType == AssociatedItemType.Transcations))
            {
                foreach (PeopleAssociationData data in from p in this.AccountBookDataContext.PeopleAssociationDatas
                                                       where p.PeopleId == current.Id
                                                       select p)
                {
                    AccountItem accountItem = data.AccountItem;

                    if (accountItem == null)
                    {
                        continue;
                    }

                    GroupByCreateTimeAccountItemViewModel model = source.FirstOrDefault<GroupByCreateTimeAccountItemViewModel>(p => p.Key.Date == accountItem.CreateTime.Date);
                    bool flag2 = false;
                    if (model == null)
                    {
                        flag2 = true;
                        model = new GroupByCreateTimeAccountItemViewModel(accountItem.CreateTime.Date);
                    }
                    AccountItem item = new AccountItem
                    {
                        Account = accountItem.Account,
                        Money = accountItem.Money,
                        CreateTime = accountItem.CreateTime,
                        Category = accountItem.Category,
                        CategoryId = accountItem.CategoryId,
                        Description = accountItem.Description,
                        Id = accountItem.Id,
                        State = accountItem.State,
                        Type = accountItem.Type,
                        PageNameGetter = TableNames.AccountItemsTable,
                        TypeInfo = LocalizedStrings.GetLanguageInfoByKey(accountItem.Type.ToString()),
                        SecondInfo = accountItem.NameInfo,
                        ThirdInfo = data.Comments.IsNullOrEmpty() ? accountItem.Description : data.Comments
                    };
                    model.Add(item);
                    if (flag2)
                    {
                        source.Add(model);
                    }
                }
            }
            return source;
        }

        /// <summary>
        /// Saves the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="isToAdd">if set to <c>true</c> [is to add].</param>
        public void Save(PeopleProfile obj, bool isToAdd)
        {
            if (isToAdd)
            {
                this.PeopleList.Add(obj);
                base.AccountBookDataContext.Peoples.InsertOnSubmit(obj);
            }
            base.AccountBookDataContext.SubmitChanges();
        }

        /// <summary>
        /// Updates the specified current.
        /// </summary>
        /// <param name="Current">The current.</param>
        public void Update(PeopleProfile Current)
        {
            base.AccountBookDataContext.SubmitChanges();
        }

        /// <summary>
        /// Calculates the relation ship, and gets a value indicates whether the people has borrow loan relation ship with me.
        /// </summary>
        /// <param name="from">From.</param>
        /// <returns></returns>
        public bool CalculateLoaningRelationShip(PeopleProfile from, LeanType? loanType = null)
        {
            // 
            var dataRelatedWithFrom = AccountBookDataContext.Repayments.Where(p => p.RepaymentRecordType != null &&
                p.RepaymentRecordType.Value == RepaymentType.MoneyBorrowOrLeanRepayment);

            dataRelatedWithFrom = dataRelatedWithFrom.Where(p => p.BorrowOrLean != null
                && p.ToPeopleId == from.Id);

            var data = dataRelatedWithFrom.Where(p => p.Status != RepaymentStatus.Completed).ToList();

            Decimal? totalMoney = 0.0m;

            var hasRelationShipInfo = false;

            if (data.Count != 0)
            {
                hasRelationShipInfo = true;

                if (loanType == null || loanType.Value == LeanType.BorrowIn)
                {
                    totalMoney = data.Where(p => p.BorrowOrLean == LeanType.BorrowIn).Sum(p => p.GetMoney() - p.RepayToOrGetBackFromItems.Sum(x => x.GetMoney()));

                    from.TotalOwnHim = totalMoney.Value;

                    if (totalMoney <= 0)
                    {
                        hasRelationShipInfo = false;
                    }
                }

                if (loanType == null || loanType.Value == LeanType.LoanOut)
                {
                    totalMoney = data.Where(p => p.BorrowOrLean == LeanType.LoanOut).Sum(p => p.GetMoney() - p.RepayToOrGetBackFromItems.Sum(x => x.GetMoney()));

                    from.TotalOwnMe = totalMoney.Value;

                    if (totalMoney <= 0)
                    {
                        hasRelationShipInfo = false;
                    }
                }

                if (loanType != null)
                {
                    from.LoaningShortInfo = loanType.GetValueOrDefault() == LeanType.BorrowIn ? from.TotalOwnHimInfo : from.TotalOwnMeInfo;
                }
            }

            return hasRelationShipInfo;
        }

    }
}

