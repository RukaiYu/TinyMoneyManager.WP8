namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class RepaymentManagerViewModel : NkjSoftViewModelBase
    {
        public RepaymentManagerViewModel()
        {
            this.Repayments = new ObservableCollection<Repayment>();
        }

        public void AddRepayment(Repayment repaymentItem)
        {
            repaymentItem.RepaymentRecordType = RepaymentType.CreditCardRepayment;
            this.Repayments.Add(repaymentItem);
            this.AccountBookDataContext.Repayments.InsertOnSubmit(repaymentItem);
            this.SubmitChanges();
        }

        internal void CancelRepayment(Repayment repaymentItem)
        {
            repaymentItem.Cancel();
            this.SubmitChanges();
        }

        internal void CompleteRepayment(Repayment repaymentItem)
        {
            repaymentItem.Completed();
            ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
            ViewModelLocator.AccountViewModel.Transfer(repaymentItem.PayFromAccount, repaymentItem.PayToAccount, repaymentItem.Amount, repaymentItem.Amount, true);
            this.SubmitChanges();
        }

        internal void DeleteRepayment(Repayment repaymentItem)
        {
            this.Repayments.Remove(repaymentItem);
            AlarmManager.RemoveAlarmByName(repaymentItem.Id.ToString());
            this.AccountBookDataContext.Repayments.DeleteOnSubmit(repaymentItem);
            this.SubmitChanges();
        }

        public int GetOnGoingRepaymentItems(int daysEspire)
        {
            System.DateTime date = System.DateTime.Now.Date;
            return (from p in base.AccountBookDataContext.Repayments
                    where (((((int)p.Status) == 3) && (((int?)p.RepaymentRecordType) == ((int?)RepaymentType.CreditCardRepayment))) && (p.RepayAt.Date.Month == date.Month)) && (p.RepayAt.Date.Year == date.Year)
                    select p).Count<Repayment>();
        }

        public override void LoadDataIfNot()
        {
            if (!base.IsDataLoaded)
            {
                this.Repayments.Clear();
                (from p in this.AccountBookDataContext.Repayments
                 where (((int)p.RepaymentRecordType.Value) != 1) || (((int)p.RepaymentRecordType.Value) == 0)
                 select p).ToList<Repayment>().ForEach(new System.Action<Repayment>(this.Repayments.Add));
                base.IsDataLoaded = true;
            }
        }

        public ObservableCollection<Repayment> Repayments { get; set; }
    }
}

