namespace TinyMoneyManager.ViewModels.RepaymentManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class AddOrEditRepaymentViewModel : NkjSoftViewModelBase
    {
        private PageActionType action;
        private Repayment backCurrent;
        private Repayment current;
        private string editorPageTitle;

        public AddOrEditRepaymentViewModel()
        {
            this.Accounts = ViewModelLocator.AccountViewModel.Accounts.ToList<Account>();
            this.BankOrCreditAccounts = ViewModelLocator.AccountViewModel.Accounts.Where<Account>(delegate(Account p)
            {
                if (p.Category != AccountCategory.BankCard)
                {
                    return (p.Category == AccountCategory.CreditCard);
                }
                return true;
            }).ToList<Account>();
        }

        private void AddRepayment(Repayment current)
        {
            ViewModelLocator.RepaymentManagerViewModel.AddRepayment(current);
        }

        public PageActionType InitializeAction(string id)
        {
            if (id.Length == 0)
            {
                this.Action = PageActionType.Add;
            }
            else
            {
                this.backCurrent = this.AccountBookDataContext.Repayments.FirstOrDefault<Repayment>(p => p.Id.ToString() == id);
                this.Current = this.backCurrent.Clone();
                this.Action = PageActionType.Edit;
            }
            return this.action;
        }

        public void InitializeCurrent(string id)
        {
            if (this.action == PageActionType.Add)
            {
                Repayment repayment = new Repayment
                {
                    Id = System.Guid.NewGuid(),
                    RepayAt = System.DateTime.Now.AddDays(1.0),
                    PayToAccount = this.BankOrCreditAccounts.FirstOrDefault<Account>(),
                    PayFromAccount = this.Accounts.FirstOrDefault<Account>(),
                    Status = RepaymentStatus.OnGoing,
                    DueDate = System.DateTime.Now.AddDays(3.0)
                };
                this.Current = repayment;
            }
        }

        public void Submit(Repayment repayment, bool useAlarmOrReminder)
        {
            AlarmManager.AddRepaymentAlarmNotification(repayment, false);
            if (this.Action == PageActionType.Add)
            {
                this.AddRepayment(this.current);
            }
            else
            {
                this.backCurrent.RestoreFrom(this.current);
                this.Update();
                this.current = null;
            }
        }

        public override void SubmitChanges()
        {
            this.AccountBookDataContext.SubmitChanges();
        }

        private void Update()
        {
            if (this.backCurrent.Status == RepaymentStatus.Completed)
            {
                ViewModelLocator.RepaymentManagerViewModel.CompleteRepayment(this.backCurrent);
            }
            this.SubmitChanges();
        }

        public System.Collections.Generic.List<Account> Accounts { get; set; }

        public PageActionType Action
        {
            get
            {
                return this.action;
            }
            set
            {
                this.action = value;
                this.EditorPageTitle = LocalizedStrings.GetLanguageInfoByKey(value.ToString()) + LocalizedStrings.GetLanguageInfoByKey("RepaymentItem");
                this.OnNotifyPropertyChanged("Action");
            }
        }

        public System.Collections.Generic.List<Account> BankOrCreditAccounts { get; set; }

        public Repayment Current
        {
            get
            {
                return this.current;
            }
            set
            {
                this.current = value;
                this.OnNotifyPropertyChanged("Current");
            }
        }

        public string EditorPageTitle
        {
            get
            {
                return this.editorPageTitle;
            }
            set
            {
                this.editorPageTitle = value;
                this.OnNotifyPropertyChanged("EditorPageTitle");
            }
        }
    }
}

