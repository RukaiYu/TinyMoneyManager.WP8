using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using Visifire.Charts;
using Microsoft.Phone.Controls;
using TinyMoneyManager.Language;


namespace TinyMoneyManager.Controls
{
    public partial class BudgetSettleMonthlyReportChart : UserControl, ISummaryChart
    {
        public string SummaryTitle
        {
            get { return viewModel.ChartTitle; }
            set { viewModel.ChartTitle = value; }
        }

        private DataMappingCollection dmc;

        public ViewModels.BudgetManagement.BudgetProjectMonthReportViewModel viewModel;

        private DataSeries BudgetSerise
        {
            get { return BudgetSettleChart.Series[0]; }

        }

        private DataSeries SettleSeries
        {
            get { return BudgetSettleChart.Series[1]; }
        }

        public BudgetSettleMonthlyReportChart()
        {
            InitializeComponent();

            InitializeSeries();
            this.NeedRefreshData = true;
        }

        private void InitializeSeries()
        {
            viewModel = new ViewModels.BudgetManagement.BudgetProjectMonthReportViewModel();

            var x = new DataMapping() { MemberName = "AxisXLabel", Path = "Name" };
            var y = new DataMapping() { MemberName = "YValue", Path = "TotalAmout" };

            //IncomeSeries = new DataSeries();
            BudgetSerise.LegendText = AppResources.BudgetManagement;

            //ExpenseSeries = new DataSeries();
            SettleSeries.LegendText = AppResources.SettleExpense;

            this.BudgetSerise.DataMappings = new DataMappingCollection() { y, y };
            this.SettleSeries.DataMappings = new DataMappingCollection() { x, y };

            this.BudgetSettleChart.DataContext = viewModel;

        }

        public Component.SummaryDetailsCollection ExpensesSummaryDetailsCollection
        {
            get;
            set;
        }

        public bool HasLoadData
        {
            get;
            set;
        }

        public bool NeedRefreshData
        {
            get;
            set;
        }

        public void LoadData(Component.DetailsCondition dc, IEnumerable<Component.SummaryDetails> source)
        {
            if (!NeedRefreshData)
                return;

            if (dc.IncomeOrExpenses == ItemType.Expense)
            {
                var data = viewModel.LoadBudgetMonthlyReport(dc).ToList();

                if (dc.SearchingScope == SearchingScope.CurrentYear)
                {
                    data.Add(viewModel.LoadBudgetMonthlyReport(ItemType.Expense, SearchingScope.CurrentMonth));
                }

                this.BudgetSerise.DataSource = data;
                //if (System.Diagnostics.Debugger.IsAttached)
                //{
                //    var data1 = viewModel.LoadTestData(dc, 300, 1200).ToList();
                //    this.BudgetSerise.DataSource = data1;
                //}

                var data2 = viewModel.LoadSettleMonthlyReport(dc).ToList();
                //if (System.Diagnostics.Debugger.IsAttached)
                //{
                //    data = viewModel.LoadTestData(dc, 200, 2000).ToList();
                //}

                EnsureTwoDataMatch(data, data2);


                this.SettleSeries.DataSource = data2;
            }

            NeedRefreshData = false;
            this.SummaryTitle = dc.SummaryTitle;
        }

        private void EnsureTwoDataMatch(List<SummaryDetails> data, List<SummaryDetails> data2)
        {
            foreach (var item in data)
            {
                if (data2.Count(p => p.Name == item.Name) == 0)
                {
                    data2.Add(new SummaryDetails()
                    {
                        Name = item.Name,
                        Count = 0,
                        Date = item.Date,
                        TotalAmout = 0,
                        AccountItemType = item.AccountItemType,
                    });
                }
            }
        }

        public void DetectImageSize(Microsoft.Phone.Controls.PageOrientation pageOrientation)
        {
            throw new NotImplementedException();
        }

        public Action<UserControl> ToShowChartInFullScreen
        {
            get;
            set;
        }

        private void FullScreenMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveChartAsPictureMenu_Click(object sender, RoutedEventArgs e)
        {
            var title = this.SummaryTitle;
            var ImageWidth = (int)this.ActualWidth;

            var ImageHeight = (int)this.ActualHeight;

            SummaryChartBase.SaveImageToPictureAlbum(this.LayoutRoot, title, ImageWidth, ImageHeight);
        }
    }
}