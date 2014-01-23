namespace TinyMoneyManager.Pages.Summary
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class ParticularsDetails : PhoneApplicationPage
    {
        private MenuItemViewModel mainPageSummaryViewModel;

        private ParticularsViewModel particularsViewModel;

        public ParticularsDetails()
        {
            this.InitializeComponent();
            this.particularsViewModel = ViewModelLocator.instanceLoader.LoadSingelton<ParticularsViewModel>("ParticularsViewModel");
            base.DataContext = this.particularsViewModel;
            this.InitailizeApplicationBar();
            this.mainPageSummaryViewModel = ViewModelLocator.MainPageViewModel;
            this.mainPageSummaryViewModel.ThisMonthSummary.IncomeSummaryEntry.ComparationInfo.Amount = -1M;
            this.mainPageSummaryViewModel.ThisMonthSummary.ExpenseSummaryEntry.ComparationInfo.Amount = -1M;
        }

        private void InitailizeApplicationBar()
        {
        }

        private void LoadSummary()
        {
            this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { this.PageTitle.Text }));
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                this.particularsViewModel.LoadData();
                this.mainPageSummaryViewModel.UpdatingCompareingAccountInfo(true);
                base.Dispatcher.BeginInvoke(delegate
                {
                    this.particularsViewModel.MonthlyIncomExpenseChangesAmountInfo = "{0}/{1}".FormatWith(new object[] { this.mainPageSummaryViewModel.ThisMonthSummary.IncomeSummaryEntry.ComparationInfo.AmountInfoWithArrow, this.mainPageSummaryViewModel.ThisMonthSummary.ExpenseSummaryEntry.ComparationInfo.AmountInfoWithArrow });
                    this.WorkDone();
                });
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.LoadSummary();
            }
        }
    }
}

