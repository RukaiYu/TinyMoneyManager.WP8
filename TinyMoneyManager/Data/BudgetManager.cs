namespace TinyMoneyManager.Data
{
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public class BudgetManager : NotionObject
    {
        [System.Runtime.CompilerServices.DecimalConstant(0, 0, (uint)0, (uint)0, (uint)470)]
        public static readonly decimal NoneBudgetAmountLength = 470M;

        static BudgetManager()
        {
            Current = new BudgetManager();
        }

        public BudgetManager()
        {
            this.CurrentMonthBudgetSummary = new AnalyziationSummarization("/");
            this.CurrentMonthBudgetSummary.TotalIncomeAmountInfo = AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.MonthlyBudget });
            this.CurrentMonthBudgetSummary.MoneyInfo = new AccountItemMoney();
            this.CurrentMonthBudgetSummary.MoneyInfo.Money = 470M;
        }

        public static void BugFixingFor1_8_9()
        {
            if (!IsolatedAppSetingsHelper.HasFixedMonthlyBudgetReportBug)
            {
                if ((System.DateTime.Now.Year == 0x7dc) && (System.DateTime.Now.Month == 2))
                {
                    try
                    {
                        (from p in ViewModelLocator.AccountBookDataContext.BudgetMonthlyReports
                         where (p.Year == 0x7dc) && (p.Month == 2)
                         select p).ToList<BudgetMonthlyReport>().ForEach(delegate(BudgetMonthlyReport p)
                        {
                            p.Month = 1;
                        });
                        ViewModelLocator.AccountBookDataContext.SubmitChanges();
                    }
                    catch (System.Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
                IsolatedAppSetingsHelper.HasFixedMonthlyBudgetReportBug = true;
            }
        }

        public System.Collections.Generic.IEnumerable<BudgetItem> GetBudgetItems(System.Func<BudgetItem, Boolean> whereCondition)
        {
            return ViewModelLocator.AccountBookDataContext.BudgetItems;
        }

        public decimal GetBudgetTotalAmount()
        {
            return (from p in ViewModelLocator.AccountBookDataContext.BudgetProjects select p.TotalAmount).AsEnumerable<decimal?>().Sum().GetValueOrDefault();
        }

        public void NoneBudgetAmountForSummary()
        {
            this.CurrentMonthBudgetSummary.MoneyInfo.Money = 470M;
        }

        public void SaveCurrentMonthBudgetSettleReport(TinyMoneyDataContext db, System.DateTime date)
        {
            BugFixingFor1_8_9();
            if (db.BudgetMonthlyReports.Count<BudgetMonthlyReport>(p => ((p.Year == date.Year) && (p.Month == (date.Month - 1)))) == 0)
            {
                Table<BudgetProject> budgetProjects = db.BudgetProjects;
                if (budgetProjects.Count<BudgetProject>() > 0)
                {
                    foreach (BudgetProject project in budgetProjects)
                    {
                        BudgetMonthlyReport entity = new BudgetMonthlyReport
                        {
                            Month = date.Month - 1,
                            Year = date.Year,
                            Amount = project.GetMoney().GetValueOrDefault(),
                            BudgetProjectId = project.Id,
                            ItemType = project.ItemType
                        };
                        db.BudgetMonthlyReports.InsertOnSubmit(entity);
                    }
                    db.SubmitChanges();
                }
            }
        }

        public void UpdateCurrentMonthBudgetSummary(decimal currentMonthTotalExpenseAmount)
        {
            if (ViewModelLocator.AccountBookDataContext.BudgetProjects.Count<BudgetProject>(p => (((int)p.ProjectType) == 0)) == 0)
            {
                this.NoneBudgetAmountForSummary();
                this.CurrentMonthBudgetSummary.TotalIncomeAmountInfo = AppResources.UnsertMonthlyBudget;
            }
            else
            {
                this.CurrentMonthBudgetSummary.TotalIncomeAmount = this.GetBudgetTotalAmount();
                this.CurrentMonthBudgetSummary.TotalIncomeAmount = (this.CurrentMonthBudgetSummary.TotalIncomeAmount <= 0M) ? 0.0001M : this.CurrentMonthBudgetSummary.TotalIncomeAmount;
                decimal Dvalue = this.CurrentMonthBudgetSummary.TotalIncomeAmount - currentMonthTotalExpenseAmount;
                decimal num = Dvalue;
                if (Dvalue <= 0M)
                {
                    num = 0M;
                }
                this.CurrentMonthBudgetSummary.MoneyInfo.Money = ((num / this.CurrentMonthBudgetSummary.TotalIncomeAmount) * 1.0M) * 470M;
                this.CurrentMonthBudgetSummary.TotalExpenseAmount = Dvalue;
                this.CurrentMonthBudgetSummary.TotalExpenseAmountInfo = "({0})".FormatWith(new object[] { Dvalue.ToMoneyF2() });
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    System.Action a = null;
                    num = ViewModelLocator.MainPageViewModel.ThisMonthSummary.TotalExpenseAmount / ParticularsViewModel.DayCountOfThisMonth;
                    int num2 = ParticularsViewModel.DayCountOfThisMonth - System.DateTime.Now.Day;
                    decimal num3 = num * num2;
                    bool isOverBudget = (num * num2) > Dvalue;
                    if (isOverBudget && ((Dvalue - num3) > 0M))
                    {
                        if (a == null)
                        {
                            a = delegate
                            {
                                string budgetIsInControl = AppResources.BudgetIsInControl;
                                if (isOverBudget)
                                {
                                    budgetIsInControl = AppResources.BudgetIsOverTaken;
                                }

                                this.CurrentMonthBudgetSummary.Title = "{0}({1})".FormatWith(new object[] { AppResources.MonthlyBudget, budgetIsInControl.ToLowerInvariant() });
                            };
                        }
                        Deployment.Current.Dispatcher.BeginInvoke(a);
                    }
                });
            }
        }

        public static BudgetManager Current
        {
            get;
            set;
        }

        public AnalyziationSummarization CurrentMonthBudgetSummary { get; set; }

        public bool NeedUpdate { get; set; }
    }
}

