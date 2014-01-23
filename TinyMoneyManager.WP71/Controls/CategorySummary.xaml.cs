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
using System.ComponentModel;
using Visifire.Charts;
namespace TinyMoneyManager.Controls
{
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using TinyMoneyManager.Data.Model;
    using Microsoft.Phone.Controls;
    using TinyMoneyManager.Language;
    using Microsoft.Xna.Framework.Media;
    using System.IO;
    using System.Windows.Media.Imaging;
    using System.IO.IsolatedStorage;

    public partial class CategorySummary : UserControl, ISummaryChart, INotifyPropertyChanged
    {
        public SummaryDetailsCollection ExpensesSummaryDetailsCollection { get; set; }

        public bool HasLoadData { get; set; }

        private DataMappingCollection MappingForCount;
        private DataMappingCollection MappingForTotalAmount;
        private Title ChartTitle;
        private Title summaryTitle;

        private string chartTitleForCount = string.Empty;
        private string chartTitleForAmount = string.Empty;
        public const string DependentValuePathForCount = "Count";
        public const string DependentValuePathForAmount = "TotalAmout";

        public CategorySummary()
        {
            InitializeComponent();
            NeedRefreshData = false;
            var currencySymbol = AppSetting.Instance.DefaultCurrency.GetCurrencyStringWithNameFirst();
            chartTitleForCount = LocalizedStrings.GetLanguageInfoByKey("{0}({1})", new string[] { "CategoryTitleForCount", "ItemUnit" });
            chartTitleForAmount = "{0}({1})".FormatWith(LocalizedStrings.GetLanguageInfoByKey("CategoryTitleForAmount"), currencySymbol);

            DataMapping dm_Name = new DataMapping() { MemberName = "AxisXLabel", Path = "Name" };
            DataMapping dm_Count = new DataMapping() { MemberName = "YValue", Path = DependentValuePathForCount };
            DataMapping dm_TotalAmount = new DataMapping() { MemberName = "YValue", Path = DependentValuePathForAmount };
            MappingForCount = new DataMappingCollection() { dm_Name, dm_Count };
            MappingForTotalAmount = new DataMappingCollection() { dm_Name, dm_TotalAmount };

            this.PieChart.Series[0].DataMappings = MappingForCount;

            this.PieChart.Series[0].LabelText = "#AxisXLabel, {0}#YValue".FormatWith(AppSetting.Instance.DefaultCurrency.GetCurrentString());

            ChartTitle = new Title() { Text = string.Empty };
            summaryTitle = new Title() { Text = string.Empty };
            this.PieChart.Titles.Add(summaryTitle);
            this.PieChart.Titles.Add(ChartTitle);
            SetChartTitle(DependentValuePathForCount);
        }

        private void SetChartTitle(string titleKey)
        {
            if (titleKey == DependentValuePathForCount)
            {
                this.ChartTitle.Text = chartTitleForCount;
            }
            else
            {
                this.ChartTitle.Text = chartTitleForAmount;
            }
        }

        void CategorySummary_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void LoadData(DetailsCondition dc, IEnumerable<SummaryDetails> dataForBinding)
        {
            if (dc == null || dataForBinding == null) return;
            if (!NeedRefreshData) return;

            if (dc.IncomeOrExpenses == ItemType.Expense)
                dataForBinding = dataForBinding.Where(p => p.AccountItemType == ItemType.Expense);
            else if (dc.IncomeOrExpenses == ItemType.Income)
                dataForBinding = dataForBinding.Where(p => p.AccountItemType == ItemType.Income);

            this.PieChart.Series[0].DataSource = dataForBinding.ToList();
            NeedRefreshData = false;

        }

        private bool needRefreshData;

        public bool NeedRefreshData
        {
            get { return needRefreshData; }
            set
            {
                if (needRefreshData != value)
                {
                    needRefreshData = value;
                    OnPropertyChanged("NeedRefreshData");
                }
            }
        }


        #region --- Member Of INotifyPropertChanged ---

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private string dependentValuePath;
        public string DependentValuePath
        {
            get
            {
                return dependentValuePath;
            }
            set
            {
                if (value != dependentValuePath)
                {
                    SetChartTitle(value);
                    ToggleMapping(value);
                    dependentValuePath = value;
                    NeedRefreshData = true;
                }
            }
        }

        public void ToggleMapping(string mappingKey)
        {
            var series = this.PieChart.Series[0];
            if (mappingKey == DependentValuePathForAmount)
            {
                series.DataMappings = MappingForTotalAmount;
                series.YValueFormatString = "#0.00";
            }
            else if (mappingKey == DependentValuePathForCount)
            {
                series.YValueFormatString = "";
                series.DataMappings = MappingForCount;
            }
        }

        public string SummaryTitle
        {
            get { return summaryTitle.Text; }
            set { summaryTitle.Text = value; }
        }

        private void FullScreenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ToShowChartInFullScreen != null)
                ToShowChartInFullScreen(this);
        }

        private void ToggleLegendMenu_Click(object sender, RoutedEventArgs e)
        {
            var isShown = PieChart.Series[0].ShowInLegend;
            PieChart.Series[0].ShowInLegend = !isShown;
            PieChart.Legends[0].HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            (sender as MenuItem).Header = isShown
                .GetValueOrDefault()
                .ToggleString(AppResources.Show,
                AppResources.Hide) + " " + AppResources.Legend;
        }

        public Action<UserControl> ToShowChartInFullScreen
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int ImageWidth = 480;
        /// <summary>
        /// 
        /// </summary>
        public int ImageHeight = 600;

        private void SaveChartAsPictureMenu_Click(object sender, RoutedEventArgs e)
        {
            var title = this.SummaryTitle;

            ImageWidth = (int)this.ActualWidth;

            ImageHeight = (int)this.ActualHeight;

            SummaryChartBase.SaveImageToPictureAlbum(this.PieChart, title, ImageWidth, ImageHeight);

        }

        /// <summary>
        /// Detects the width height by orientation.
        /// </summary>
        /// <param name="currentOrientation">The current orientation.</param>
        public void DetectImageSize(PageOrientation currentOrientation)
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
