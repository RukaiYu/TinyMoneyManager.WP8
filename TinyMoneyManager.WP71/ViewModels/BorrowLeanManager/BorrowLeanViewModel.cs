using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TinyMoneyManager.Data.Model;
using System.Collections.ObjectModel;
using TinyMoneyManager.Component;
using TinyMoneyManager.Language;


using NkjSoft.Extensions;
using System.Collections.Generic;
using System.Linq.Expressions;
namespace TinyMoneyManager.ViewModels.BorrowLeanManager
{
    public class BorrowLeanViewModel : TinyMoneyManager.Component.NkjSoftViewModelBase
    {
        private ObservableCollection<BorrowLeanGroupByTimeViewModel> borrowLeanList;

        public ObservableCollection<BorrowLeanGroupByTimeViewModel> BorrowLeanList
        {
            get { return borrowLeanList; }
            set
            {
                if (borrowLeanList != value)
                {
                    OnNotifyPropertyChanging("BorrowLeanList");
                    borrowLeanList = value;
                    OnNotifyPropertyChanged("BorrowLeanList");
                }
            }
        }

        private BorrowLeanSeachingCondition searchingCondition;

        public BorrowLeanSeachingCondition SearchingCondition
        {
            get { return searchingCondition; }
            set
            {
                if (searchingCondition != value)
                {
                    OnNotifyPropertyChanging("SearchingCondition");
                    searchingCondition = value;
                    OnNotifyPropertyChanged("SearchingCondition");
                }
            }
        }

        public BorrowLeanViewModel()
        {
            if (BorrowLeanList == null)
            {
                BorrowLeanList = new ObservableCollection<BorrowLeanGroupByTimeViewModel>();
            }

            this.searchingCondition = new BorrowLeanSeachingCondition();
            this.searchingCondition.Status = RepaymentStatus.InCompleted;
        }

        /// <summary>
        /// Saves the or insert.
        /// </summary>
        /// <param name="entry">The repayment.</param>
        /// <param name="amount">The old amount value.</param>
        public void SaveOrInsert(Data.Model.Repayment entry, decimal oldAmount = 0.0m)
        {
            if (entry.Id == Guid.Empty)
            {
                entry.Id = Guid.NewGuid();

                if (entry.RepayToId != Guid.Empty)
                {
                    entry.RepayToOrGetBackFrom.RepayToOrGetBackFromItems.Add(entry);
                }

                EnsureStatus(entry);

                AccountBookDataContext.Repayments.InsertOnSubmit(entry);

                HandlingItemChanged(entry, g => g.Add(entry), false);

            }
            else
            {
                var group = FindGroup(entry.ExecuteDate.GetValueOrDefault());
                var amountChanged = entry.Amount != oldAmount;
                if (amountChanged)
                {
                    var amount = entry.Amount - oldAmount;

                    var borrowOrLeanType = entry.BorrowOrLean.GetValueOrDefault();

                    if (borrowOrLeanType == LeanType.BorrowIn || borrowOrLeanType == LeanType.Receipt)
                    {
                        ViewModelLocator.AccountViewModel.WithdrawlOrDeposit(entry.PayFromAccount, amount);
                    }
                    else
                    {
                        entry.PayFromAccount.Balance = entry.PayFromAccount.Balance - amount;
                    }

                    group.RaiseTotalMoneyChanged();
                }

                EnsureStatus(entry);
                SubmitChanges();
            }
        }

        /// <summary>
        /// Ensures the status.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public static void EnsureStatus(Repayment entry)
        {
            Repayment target = entry;

            var sum = 0.0M;
            var current = 0.0M;

            if (entry.RepayToId != Guid.Empty)
            {
                //  
                target = entry.RepayToOrGetBackFrom;
            }

            if (target != null)
            {
                sum = target.Amount;
                current = target.RepayToOrGetBackFromItems.Sum(p => p.GetMoneyForRepayOrReceive());

                target.Status = sum <= current ? RepaymentStatus.Completed : RepaymentStatus.InCompleted;
            }
        }

        /// <summary>
        /// Deletes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Delete(Repayment entry)
        {
            if (entry.RepayToId != Guid.Empty)
            {
                if (entry.RepayToOrGetBackFrom != null && entry.RepayToOrGetBackFrom.RepayToOrGetBackFromItems != null)
                {
                    entry.RepayToOrGetBackFrom.Status = RepaymentStatus.InCompleted;
                    entry.RepayToOrGetBackFrom.RepayToOrGetBackFromItems.Remove(entry);
                }
            }
            else
            {
                // remove all RepayToOrGetBackFromItems.

                var items = entry.RepayToOrGetBackFromItems.ToList();

                entry.Status = RepaymentStatus.InCompleted;

                foreach (var item in items)
                {
                    HandlingItemChanged(item, g => g.Remove(item), true);

                    AccountBookDataContext.Repayments.DeleteOnSubmit(item);
                }
            }

            HandlingItemChanged(entry, g => g.Remove(entry), true);
        }

        /// <summary>
        /// Handlings the item changed when add or remove item.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="actionToGroup">The action to group.</param>
        /// <param name="isToDelete">if set to <c>true</c> [is to delete].</param>
        public void HandlingItemChanged(Repayment entry, Action<BorrowLeanGroupByTimeViewModel> actionToGroup, bool isToDelete = false)
        {
            var group = FindGroup(entry.ExecuteDate.GetValueOrDefault());

            HandlingMoney(entry, 0.0m, isToDelete);

            group.RaiseTotalMoneyChanged();

            if (actionToGroup != null)
            {
                actionToGroup(group);
            }

            SubmitChanges();
        }

        /// <summary>
        /// Creates the borrow lean entry.
        /// </summary>
        /// <returns></returns>
        public Repayment CreateBorrowLeanEntry()
        {
            return new Repayment()
            {

                RepaymentRecordType = RepaymentType.MoneyBorrowOrLeanRepayment,

                CreateAt = DateTime.Now,
                RepayAt = DateTime.Now,
                DueDate = DateTime.Now,
                ExecuteDate = DateTime.Now,
            };
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        public override void LoadData()
        {
            var query = AccountBookDataContext.Repayments
                .Where(p => p.RepaymentRecordType == RepaymentType.MoneyBorrowOrLeanRepayment);

            if (searchingCondition.Status != RepaymentStatus.All)
            {
                query = query.Where(p => p.Status == searchingCondition.Status);
            }

            //if (searchingCondition.StartDate.HasValue)
            //{
            //    query = query.Where(p => ((DateTime)p.ExecuteDate).Date >= searchingCondition.StartDate.Value.Date);
            //}

            //if (searchingCondition.EndDate.HasValue)
            //{
            //    query = query.Where(p => ((DateTime)p.ExecuteDate).Date <= searchingCondition.EndDate.Value.Date);
            //}

            if (searchingCondition.FromPeoples != null && searchingCondition.FromPeoples.Count() > 0)
            {
                query = query.Where(p => searchingCondition.FromPeoples.Contains(p.FromPeopleId.GetValueOrDefault()));
            }

            if (!searchingCondition.NotesKey.IsNullOrEmpty())
            {
                query = query.Where(p => p.Notes.Contains(searchingCondition.NotesKey));
            }

            query = query.OrderByDescending(p => p.ExecuteDate);

            var dates = query.Select(p => p.ExecuteDate.Value.Date).Distinct()
                .OrderByDescending(p => p.Date);

            var tempList = new ObservableCollection<BorrowLeanGroupByTimeViewModel>();
            foreach (var dateTime in dates)
            {
                var itemsForDate = query.Where(p => ((DateTime)p.ExecuteDate).Date == dateTime.Date);

                var group = new BorrowLeanGroupByTimeViewModel(dateTime);

                group.AddRange(itemsForDate.ToList());

                tempList.Add(group);

            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.borrowLeanList.Clear();
                tempList.ForEach(p => this.BorrowLeanList.Add(p));

                mangoProgressIndicator.GlobalIndicator.Instance.WorkDone();

            });
        }

        /// <summary>
        /// Finds the group.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual BorrowLeanGroupByTimeViewModel FindGroup(DateTime key)
        {
            var group = this.borrowLeanList.FirstOrDefault(p => p.Key.Date == key.Date);

            if (group == null)
            {
                group = new BorrowLeanGroupByTimeViewModel(key);
                this.borrowLeanList.Add(group);
            }

            return group;
        }

        /// <summary>
        /// Deletes the specified obj.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        public override void Delete<T>(T obj)
        {
            var repayment = obj as Repayment;
            Delete(repayment);
            base.Delete<T>(obj);
        }

        /// <summary>
        /// Handlings the money.
        /// </summary>
        /// <param name="repayment">The repayment.</param>
        /// <param name="amountChanged">The amount changed.</param>
        /// <param name="isToRemove">if set to <c>true</c> [is to remove].</param>
        public void HandlingMoney(Repayment repayment, decimal amountChanged, bool isToRemove = false)
        {
            var loanType = repayment.BorrowOrLean.GetValueOrDefault();
            var hasChanged = repayment.Amount != amountChanged;
            /*
             * 收款,	               +amount,      -amount,   +(cha)              -(cha).

                借出,	              -amount,      +amount,   -(cha)               +(cha). 
            */
            var amountToHandle = 0.0m;

            if (loanType == LeanType.BorrowIn || loanType == LeanType.Receipt)
            {
                amountToHandle = repayment.Amount;
            }
            else
            {
                amountToHandle = -repayment.Amount;
            }

            if (isToRemove)
            {
                amountToHandle = -amountToHandle;
            }

            repayment.PayFromAccount.Balance = repayment.PayFromAccount.Balance + amountToHandle;

            //ViewModelLocator.AccountViewModel.WithdrawlOrDeposit(repayment.PayFromAccount, amountToHandle);

        }

        /// <summary>
        /// Queries the borrow loan.
        /// </summary>
        /// <param name="whereCondition">The where condition.</param>
        /// <returns></returns>
        public IQueryable<Repayment> QueryBorrowLoan(Expression<Func<Repayment, bool>> whereCondition)
        {
            return this.AccountBookDataContext.Repayments.Where(p => p.RepaymentRecordType == RepaymentType.MoneyBorrowOrLeanRepayment)
                .Where(whereCondition);
        }

        /// <summary>
        /// Creates the repay or receieve entry.
        /// </summary>
        /// <param name="repayment">The repayment.</param>
        /// <returns></returns>
        internal Repayment CreateRepayOrReceieveEntry(Repayment repayment)
        {
            return new Repayment()
            {
                RepayToOrGetBackFrom = repayment,
                BorrowOrLean = repayment.GetReverseRepaymentType(),
                RepaymentRecordType = RepaymentType.MoneyBorrowOrLeanRepayment,
                ToPeople = repayment.ToPeople,
                PayFromAccount = repayment.PayFromAccount,

                CreateAt = DateTime.Now,
                RepayAt = DateTime.Now,
                DueDate = DateTime.Now,
                ExecuteDate = DateTime.Now,
            };
        }

        /// <summary>
        /// Loads the history list.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns></returns>
        internal ObservableCollection<BorrowLeanGroupByTimeViewModel> LoadHistoryList(IEnumerable<Repayment> data)
        {
            data = data.OrderByDescending(p => p.ExecuteDate);

            var dates = data.Select(p => p.ExecuteDate.Value.Date).Distinct()
                .OrderByDescending(p => p.Date).ToList();

            var tempList = new ObservableCollection<BorrowLeanGroupByTimeViewModel>();

            foreach (var dateTime in dates)
            {
                var itemsForDate = data.Where(p => ((DateTime)p.ExecuteDate).Date == dateTime.Date);

                var group = new BorrowLeanGroupByTimeViewModel(dateTime);

                group.AddRange(itemsForDate.ToList());

                tempList.Add(group);
            }

            return tempList;
        }

        /// <summary>
        /// Deletings the object service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceOfT">The instance of T.</param>
        /// <param name="titleGetter">The title getter.</param>
        /// <returns></returns>
        public override bool DeletingObjectService<T>(T instanceOfT, Func<T, string> titleGetter, Action callback = null)
        {
            if (instanceOfT == null)
                return false;

            var entry = instanceOfT as Repayment;

            var msg = AppResources.DeleteAccountItemMessage;
            var loanType = titleGetter(instanceOfT);

            if (!entry.IsRepaymentOrReceieve)
            {
                msg = AppResources.ConfirmMessageWhenDeletingBorrowOrLoan
                    .FormatWith(entry.ReverseRepaymentTypeName);
            }

            return CommonExtensions.AlertConfirm(null,
                  msg, () =>
                  {
                      this.Delete<Repayment>(entry);
                  }, AppResources.DeletingObject.FormatWith(titleGetter(instanceOfT))) == MessageBoxResult.OK;
        }


    }


}
