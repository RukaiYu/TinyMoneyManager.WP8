namespace TinyMoneyManager.ViewModels
{
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class ParticularsViewModel : NkjSoftViewModelBase
    {
        private string currentMontlyDailyExpenseAmount;
        public static readonly int DayCountOfThisMonth = System.DateTime.Now.GetCountDaysOfMonth();
        private string lastMonthMontlyDailyExpenseAmount;
        private AnalyziationSummarization monthlyIncomExpenseChanges;
        private string monthlyIncomExpenseChangesAmountInfo;
        private string monthlyIncomExpenseChangesTitle;
        private AnalyziationSummarization monthlyTopNExpenses;
        private AnalyziationSummarization monthlyWeeksExpenseAvgInfo;
        private string montlyDailyExpenseAmountTitle;
        private string pageTitle;
        private AnalyziationSummarization totalInDebt;
        private AnalyziationSummarization totalLoanOut;
        private AnalyziationSummarization totalTransaction;
        private AnalyziationSummarization yearlyMonthsIncomeExpenseAvgInfo;

        public ParticularsViewModel()
        {
            ViewModelLocator.MainPageViewModel.PropertyChanged += new PropertyChangedEventHandler(this.MainPageViewModel_PropertyChanged);
            string loading = AppResources.Loading;
            AnalyziationSummarization summarization = new AnalyziationSummarization("/")
            {
                AmountInfo = loading,
                ScopeForSummary = SearchingScope.CurrentYear,
                Title = AppResources.YearlyMonthsIncomeExpenseAvgInfo.FormatWith(new object[] { string.Empty }).Trim()
            };
            this.YearlyMonthsIncomeExpenseAvgInfo = summarization;
            AnalyziationSummarization summarization2 = new AnalyziationSummarization("/")
            {
                AmountInfo = loading,
                Title = AppResources.MonthlyWeeksExpenseAvgInfo.FormatWith(new object[] { AppResources.CurrentMonth }),
                ScopeForSummary = SearchingScope.CurrentMonth
            };
            this.MonthlyWeeksExpenseAvgInfo = summarization2;
            AnalyziationSummarization summarization3 = new AnalyziationSummarization("/")
            {
                AmountInfo = loading,
                Title = AppResources.MonthlyTopNExpenses.FormatWith(new object[] { AppResources.CurrentMonth, 5 }),
                ScopeForSummary = SearchingScope.CurrentMonth
            };
            this.MonthlyTopNExpenses = summarization3;
            this.totalInDebt = new AnalyziationSummarization("/");
            this.totalLoanOut = new AnalyziationSummarization("/");
            AnalyziationSummarization summarization4 = new AnalyziationSummarization("/")
            {
                ScopeForSummary = SearchingScope.CurrentMonth
            };
            this.TotalTransaction = summarization4;
            string str2 = LocalizedStrings.CultureName.DateTimeFormat.AbbreviatedMonthNames[DateTime.Now.Month - 1];

            var lastMonth = DateTime.Now.AddMonths(-1);
            var lastMonthValue = lastMonth.Month;

            lastMonthValue = 12;

            if (lastMonthValue == 1)
            { lastMonthValue = 0; }
            else if (lastMonthValue == 12)
            {
                lastMonthValue = 11;
            }

            string str3 = LocalizedStrings.CultureName.DateTimeFormat.AbbreviatedMonthNames[lastMonthValue];
            this.MonthlyIncomExpenseChangesTitle = AppResources.MonthlyIncomExpenseChangesTitle.FormatWith(new object[] { str2, str3 });
            this.MontlyDailyExpenseAmountTitle = AppResources.MontlyDailyExpenseAmount.FormatWith(new object[] { str2 });
            this.PageTitle = "{0}".FormatWith(new object[] { AppResources.Particulars, str2 });
        }

        private string GetAmountInfo(string symOfCurrency, string amount)
        {
            return AppSetting.Instance.DefaultCurrency.GetAmountInfoWithCurrencySymbol(symOfCurrency, amount);
        }

        public override void LoadData()
        {
            DetailsCondition ds = new DetailsCondition
            {
                SearchingScope = this.yearlyMonthsIncomeExpenseAvgInfo.ScopeForSummary
            };

            System.Collections.Generic.List<AccountItem> list = (from p in this.AccountBookDataContext.AccountItems
                                                                 where (p.CreateTime.Date > ds.StartDate.GetValueOrDefault().Date) && (p.CreateTime.Date < ds.EndDate.GetValueOrDefault().Date)
                                                                 select p).ToList<AccountItem>();

            //decimal val = ((System.Collections.Generic.IEnumerable<Decimal>)(from p in list
            //                                                                 where p.Type == ItemType.Expense
            //                                                                 select p.GetMoney().Value)).Sum() / 12.0M;
            //decimal num2 = ((System.Collections.Generic.IEnumerable<Decimal>)(from p in list
            //                                                                  where p.Type == ItemType.Income
            //                                                                  select p.GetMoney().Value)).Sum() / 12M;

            string symOfCurrency = AppSetting.Instance.DefaultCurrency.GetCurrentString();

            // this.YearlyMonthsIncomeExpenseAvgInfo.AmountInfo = "{0}/{1}".FormatWith(new object[] { AppSetting.Instance.DefaultCurrency.GetAmountInfoWithCurrencySymbol(symOfCurrency, num2.ToMoneyF2()), AppSetting.Instance.DefaultCurrency.GetAmountInfoWithCurrencySymbol(symOfCurrency, val.ToMoneyF2()) });

            ds.SearchingScope = this.monthlyWeeksExpenseAvgInfo.ScopeForSummary;
            decimal? nullable = ViewModelLocator.BorrowLeanViewModel.QueryBorrowLoan(p => ((int?)p.BorrowOrLean) == ((int?)LeanType.BorrowIn) && p.Status != RepaymentStatus.Completed).ToList<Repayment>().Sum<Repayment>(p => p.GetMoney());
            decimal? nullable2 = ViewModelLocator.BorrowLeanViewModel.QueryBorrowLoan(p => ((int?)p.BorrowOrLean) == ((int?)LeanType.LoanOut) && p.Status != RepaymentStatus.Completed).ToList<Repayment>().Sum<Repayment>(p => p.GetMoney());

            this.totalInDebt.TotalExpenseAmount = nullable.GetValueOrDefault();
            this.TotalInDebt.AmountInfo = this.GetAmountInfo(symOfCurrency, nullable.GetValueOrDefault().ToMoneyF2());

            this.totalLoanOut.TotalExpenseAmount = nullable2.GetValueOrDefault();

            this.TotalLoanOut.AmountInfo = this.GetAmountInfo(symOfCurrency, nullable2.GetValueOrDefault().ToMoneyF2());

            this.TotalTransaction.AmountInfo = (from p in this.AccountBookDataContext.TransferingItems
                                                where (p.TransferingDate.Date > ds.StartDate.Value.Date) && (p.TransferingDate.Date <= ds.EndDate.Value.Date)
                                                select p).Count<TransferingItem>().ToString();
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.CurrentMontlyDailyExpenseAmount = "{0}{1}".FormatWith(new object[] { symOfCurrency, (this.MainPageViewModel.ThisMonthSummary.TotalExpenseAmount / DayCountOfThisMonth).ToMoneyF2() });
            });
        }

        private void MainPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSummaryListLoaded")
            {
                if (!ViewModelLocator.MainPageViewModel.IsSummaryListLoaded)
                {
                    base.IsDataLoaded = false;
                }
                else if (base.IsDataLoaded)
                {
                    base.IsDataLoaded = false;
                }
            }
        }

        public string CurrentMontlyDailyExpenseAmount
        {
            get
            {
                return this.currentMontlyDailyExpenseAmount;
            }
            set
            {
                if (this.currentMontlyDailyExpenseAmount != value)
                {
                    this.OnNotifyPropertyChanging("CurrentMontlyDailyExpenseAmount");
                    this.currentMontlyDailyExpenseAmount = value;
                    this.OnNotifyPropertyChanged("CurrentMontlyDailyExpenseAmount");
                }
            }
        }

        public string LastMonthMontlyDailyExpenseAmount
        {
            get
            {
                return this.lastMonthMontlyDailyExpenseAmount;
            }
            set
            {
                if (this.lastMonthMontlyDailyExpenseAmount != value)
                {
                    this.OnNotifyPropertyChanging("LastMonthMontlyDailyExpenseAmount");
                    this.lastMonthMontlyDailyExpenseAmount = value;
                    this.OnNotifyPropertyChanged("LastMonthMontlyDailyExpenseAmount");
                }
            }
        }

        public MenuItemViewModel MainPageViewModel
        {
            get
            {
                return ViewModelLocator.MainPageViewModel;
            }
        }

        public string MonthlyIncomExpenseChangesAmountInfo
        {
            get
            {
                return this.monthlyIncomExpenseChangesAmountInfo;
            }
            set
            {
                if (this.monthlyIncomExpenseChangesAmountInfo != value)
                {
                    this.OnNotifyPropertyChanging("MonthlyIncomExpenseChangesAmountInfo");
                    this.monthlyIncomExpenseChangesAmountInfo = value;
                    this.OnNotifyPropertyChanged("MonthlyIncomExpenseChangesAmountInfo");
                }
            }
        }

        public string MonthlyIncomExpenseChangesTitle
        {
            get
            {
                return this.monthlyIncomExpenseChangesTitle;
            }
            set
            {
                if (this.monthlyIncomExpenseChangesTitle != value)
                {
                    this.OnNotifyPropertyChanging("MonthlyIncomExpenseChangesTitle");
                    this.monthlyIncomExpenseChangesTitle = value;
                    this.OnNotifyPropertyChanged("MonthlyIncomExpenseChangesTitle");
                }
            }
        }

        public AnalyziationSummarization MonthlyTopNExpenses
        {
            get
            {
                return this.monthlyTopNExpenses;
            }
            set
            {
                if (this.monthlyTopNExpenses != value)
                {
                    this.OnNotifyPropertyChanging("MonthlyTopNExpenses");
                    this.monthlyTopNExpenses = value;
                    this.OnNotifyPropertyChanged("MonthlyTopNExpenses");
                }
            }
        }

        public AnalyziationSummarization MonthlyWeeksExpenseAvgInfo
        {
            get
            {
                return this.monthlyWeeksExpenseAvgInfo;
            }
            set
            {
                if (this.monthlyWeeksExpenseAvgInfo != value)
                {
                    this.OnNotifyPropertyChanging("MonthlyWeeksExpenseAvgInfo");
                    this.monthlyWeeksExpenseAvgInfo = value;
                    this.OnNotifyPropertyChanged("MonthlyWeeksExpenseAvgInfo");
                }
            }
        }

        public string MontlyDailyExpenseAmountTitle
        {
            get
            {
                return this.montlyDailyExpenseAmountTitle;
            }
            set
            {
                if (this.montlyDailyExpenseAmountTitle != value)
                {
                    this.OnNotifyPropertyChanging("MontlyDailyExpenseAmountTitle");
                    this.montlyDailyExpenseAmountTitle = value;
                    this.OnNotifyPropertyChanged("MontlyDailyExpenseAmountTitle");
                }
            }
        }

        public string PageTitle
        {
            get
            {
                return this.pageTitle;
            }
            set
            {
                if (this.pageTitle != value)
                {
                    this.OnNotifyPropertyChanging("PageTitle");
                    this.pageTitle = value;
                    this.OnNotifyPropertyChanged("PageTitle");
                }
            }
        }

        public AnalyziationSummarization TotalInDebt
        {
            get
            {
                return this.totalInDebt;
            }
            set
            {
                if (this.totalInDebt != value)
                {
                    this.OnNotifyPropertyChanging("TotalInDebt");
                    this.totalInDebt = value;
                    this.OnNotifyPropertyChanged("TotalInDebt");
                }
            }
        }

        public AnalyziationSummarization TotalLoanOut
        {
            get
            {
                return this.totalLoanOut;
            }
            set
            {
                if (this.totalLoanOut != value)
                {
                    this.OnNotifyPropertyChanging("TotalLoanOut");
                    this.totalLoanOut = value;
                    this.OnNotifyPropertyChanged("TotalLoanOut");
                }
            }
        }

        public AnalyziationSummarization TotalTransaction
        {
            get
            {
                return this.totalTransaction;
            }
            set
            {
                if (this.totalTransaction != value)
                {
                    this.OnNotifyPropertyChanging("TotalTransaction");
                    this.totalTransaction = value;
                    this.OnNotifyPropertyChanged("TotalTransaction");
                }
            }
        }

        public AnalyziationSummarization YearlyMonthsIncomeExpenseAvgInfo
        {
            get
            {
                return this.yearlyMonthsIncomeExpenseAvgInfo;
            }
            set
            {
                if (this.yearlyMonthsIncomeExpenseAvgInfo != value)
                {
                    this.OnNotifyPropertyChanging("YearlyMonthsIncomeExpenseAvgInfo");
                    this.yearlyMonthsIncomeExpenseAvgInfo = value;
                    this.OnNotifyPropertyChanged("YearlyMonthsIncomeExpenseAvgInfo");
                }
            }
        }
    }
}

