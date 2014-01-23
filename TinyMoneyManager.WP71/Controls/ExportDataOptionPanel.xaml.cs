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

using JasonPopupDemo;
namespace TinyMoneyManager.Controls
{
    public partial class ExportDataOptionPanel : UserControl
    {

        private string fileNameTag;
        private string subjectNameTag;


        public ExportDataOption ExportDataOption { get; set; }

        public event EventHandler<EventArgs> Confirmed;


        public ExportDataOptionPanel()
            : this(null)
        {

        }


        public ExportDataOptionPanel(ExportDataOption option, Func<string> expentionScope = null)
        {
            InitializeComponent();

            fileNameTag = LocalizedStrings.GetLanguageInfoByKey("FileName");
            subjectNameTag = LocalizedStrings.GetLanguageInfoByKey("SubjectName");

            SearchScopeSelector.ItemsSource = new string[] { 
                LocalizedStrings.GetLanguageInfoByKey("Today"),
                LocalizedStrings.GetLanguageInfoByKey("CurrentWeek"),
                LocalizedStrings.GetLanguageInfoByKey("CurrentMonth"),
                LocalizedStrings.GetLanguageInfoByKey("CurrentYear"),
                expentionScope==null?string.Empty:expentionScope(),
            };


            ExportDataOption = option ?? new ExportDataOption();
            ExportDataOption.ExportDataMode = ExportDataOption.ExportDataMode;
            ExportDataOption.ExportDataType = SummaryDataType.Statistics;
            ExportDataOption.SearchingScopeIndex = 2;//Component.SearchingScope.CurrentMonth;
            this.DataContext = ExportDataOption;

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            OnConfirmed(EventArgs.Empty);
        }

        protected virtual void OnConfirmed(EventArgs e)
        {
            var handler = this.Confirmed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ExportDataModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilenameBox == null || ExportDataModeSelector == null) return;

            FileOrSubjectNameLabel.Text = ExportDataModeSelector.SelectedIndex == 1 ? fileNameTag : subjectNameTag;

            if (ExportDataModeSelector.SelectedIndex > -1)
            {
                ExportDataOption.ExportDataMode = (Component.SummarySendingMode)ExportDataModeSelector.SelectedIndex;
            }
        }

        private void ExportDataTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExportDataTypeSelector == null)
                return;

            var index = ExportDataTypeSelector.SelectedIndex;

            if (index > -1)
            {
                ExportDataOption.ExportDataType = (SummaryDataType)index;
            }
        }

        private void FilenameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        private void FilenameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FilenameBox.SelectAll();
        }


    }
}
