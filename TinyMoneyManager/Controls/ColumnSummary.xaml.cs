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



namespace TinyMoneyManager.Controls
{
    public partial class ColumnSummary : UserControl, ISummaryChart
    {
        private DataMappingCollection dmc;
        private Title ChartTitle;
        private Title summaryTitle;

        public bool HasLoadData { get; set; }
        public bool NeedRefreshData { get; set; }

        /// <summary>
        /// Gets or sets the summary title.
        /// </summary>
        /// <value>
        /// The summary title.
        /// </value>
        public string SummaryTitle
        {
            get { return summaryTitle.Text; }
            set { summaryTitle.Text = value; }
        }

        public SummaryDetailsCollection ExpensesSummaryDetailsCollection { get; set; }


        private DataSeries IncomeSeries
        {
            get { return LineChart.Series[0]; }

        }

        private DataSeries ExpenseSeries
        {
            get { return LineChart.Series[1]; }
        }

        public ColumnSummary()
        {
            InitializeComponent();
            NeedRefreshData = true;
            Loaded += new RoutedEventHandler(ColumnSummary_Loaded);
            summaryTitle = new Title() { Text = string.Empty };
            ChartTitle = new Title() { Text = string.Empty };


            InitializeSeries();

            this.ChartTitle.Text = LocalizedStrings.GetLanguageInfoByKey("ColumnSummaryChartTitle");
            this.LineChart.Titles.Add(summaryTitle);
            this.LineChart.Titles.Add(ChartTitle);
        }

        private void InitializeSeries()
        {
            dmc = new DataMappingCollection()
            { 
                new DataMapping(){ MemberName = "AxisXLabel",Path="Name"},
                new DataMapping(){ MemberName = "YValue",Path="TotalAmout"},
            };

            //IncomeSeries = new DataSeries();
            IncomeSeries.LegendText = LocalizedStrings.GetLanguageInfoByKey("Income");

            //ExpenseSeries = new DataSeries();
            ExpenseSeries.LegendText = LocalizedStrings.GetLanguageInfoByKey("Expense");

            this.IncomeSeries.DataMappings = dmc;
            this.ExpenseSeries.DataMappings = dmc;

        }

        void ColumnSummary_Loaded(object sender, RoutedEventArgs e)
        {
        }


        public void LoadData(DetailsCondition dc, IEnumerable<SummaryDetails> source)
        {
            if (dc.IncomeOrExpenses == ItemType.All)
            {
                IncomeSeries.DataSource = loopData(dc, ItemType.Income);
                ExpenseSeries.DataSource = loopData(dc, ItemType.Expense);
            }
            else
            {
                if (dc.IncomeOrExpenses == ItemType.Expense)
                { ExpenseSeries.DataSource = loopData(dc, ItemType.Expense); }
                if (dc.IncomeOrExpenses == ItemType.Income)
                { IncomeSeries.DataSource = loopData(dc, ItemType.Income); }
            }

            NeedRefreshData = false;
            this.SummaryTitle = dc.SummaryTitle;
        }

        private IEnumerable<SummaryDetails> loopData(DetailsCondition dc, ItemType itemType)
        {
            var temp = loadDailyData(dc, itemType).ToList();
            return temp;
        }


        public IEnumerable<SummaryDetails> loadDailyData(DetailsCondition e, ItemType it)
        {
            var data = ViewModelLocator.AccountItemViewModel.AccountBookDataContext.AccountItems.Where(p => p.Type == it);
            if (e.SearchingScope != SearchingScope.All)
            {
                data = data.Where(p => p.CreateTime.Date >= e.StartDate && p.CreateTime.Date <= e.EndDate);
            }
            var currentYear = DateTime.Now.Year;
            var dataL = data.ToList();

            var duringMode = e.DuringMode;

            IEnumerable<SummaryDetails> dataForBinding = null;

            if (duringMode == DuringMode.ByDay)
            {
                dataForBinding = (from d in dataL
                                  orderby d.CreateTime.Date ascending
                                  group d by d.CreateTime.Date into g
                                  select new SummaryDetails()
                                          {
                                              Name = g.Key.ToString(g.Key.Year == currentYear ? ConstString.formatWithoutYearNoDDD : ConstString.formatWithYearNoDDD),
                                              AccountItemType = e.IncomeOrExpenses,
                                              Count = g.Count(),
                                              TotalAmout = g.Sum(p => p.GetMoney()),
                                          });
            }
            else
            {
                var groups = (from d in dataL
                              orderby d.CreateTime.Date ascending
                              group d by BuildGroupByMode(d, duringMode));

                dataForBinding = from g in groups
                                 select new SummaryDetails()
                                          {
                                              Name = g.Key,
                                              AccountItemType = e.IncomeOrExpenses,
                                              Count = g.Count(),
                                              TotalAmout = g.Sum(p => p.GetMoney()),
                                          };
            }

            return dataForBinding;
        }

        private string BuildGroupByMode(AccountItem ai, DuringMode mode)
        {
            var result = string.Empty;
            if (mode == DuringMode.ByMonth)
            {
                result = string.Format("{0}/{1}", ai.CreateTime.Year, ai.CreateTime.Month);
            }
            else if (mode == DuringMode.ByYear)
            {
                result = ai.CreateTime.Year.ToString();
            }
            return result;
        }


        public IEnumerable<SummaryDetails> GetDataForSummary(ItemType itemType)
        {
            var dataSourceTarget = (IncomeSeries.DataSource);
            if (itemType == ItemType.Expense)
            {
                dataSourceTarget = ExpenseSeries.DataSource;
            }
            return dataSourceTarget as IEnumerable<SummaryDetails>;
        }

        /// <summary>
        /// Gets or sets to show chart in full screen.
        /// </summary>
        /// <value>
        /// To show chart in full screen.
        /// </value>
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
            ImageWidth = (int)this.ActualWidth;

            ImageHeight = (int)this.ActualHeight;

            SummaryChartBase.SaveImageToPictureAlbum(this.LayoutRoot, title, ImageWidth, ImageHeight);
        }

        public int ImageWidth = 480;
        public int ImageHeight = 600;

        public void DetectImageSize(Microsoft.Phone.Controls.PageOrientation currentOrientation)
        {
            if (currentOrientation == PageOrientation.Landscape ||
                 currentOrientation == PageOrientation.LandscapeLeft
                   || currentOrientation == PageOrientation.LandscapeRight)
            {
                ImageWidth = 800;
                ImageHeight = 400;
            }
            else
            {
                ImageWidth = 480;
                ImageHeight = 600;
            }
        }
    }
}
