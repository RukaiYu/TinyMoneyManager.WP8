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
using JasonPopupDemo;
namespace TinyMoneyManager.Controls
{
    public partial class AssociatedItemsSelectorInAccountViewer : UserControl
    {
        public AssociatedItemsSelectorOption SelectorOption { get; set; }

        public event EventHandler<EventArgs> Confirmed;

        DateRangeSelector drs;

        public AssociatedItemsSelectorInAccountViewer(AssociatedItemsSelectorOption option)
        {
            InitializeComponent();
            drs = new DateRangeSelector();
            SelectorOption = option;
            this.DataContext = SelectorOption;
            
            SearchScopeSelector.Children.Add(drs);
            drs.SearchingScopeIndex = 2;
            drs.SearchingScope = SelectorOption.SearchingScope;
            //SearchScopeSelector.ItemsSource = new string[] { 
            //    LocalizedStrings.GetLanguageInfoByKey("Today").ToLowerInvariant(),
            //    LocalizedStrings.GetLanguageInfoByKey("CurrentWeek").ToLowerInvariant(),
            //    LocalizedStrings.GetLanguageInfoByKey("CurrentMonth").ToLowerInvariant(),
            //    LocalizedStrings.GetLanguageInfoByKey("CurrentYear").ToLowerInvariant(),
            //    LocalizedStrings.GetLanguageInfoByKey("LastMonth").ToLowerInvariant(),
            //    LocalizedStrings.GetLanguageInfoByKey("Customize").ToLowerInvariant(),
            //};

            //SearchScopeSelector.SelectionChanged += new SelectionChangedEventHandler(SearchScopeSelector_SelectionChanged);

        }

        void SearchScopeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ExportDataTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.CloseMeAsPopup();
            this.SelectorOption.SearchingScope = drs.SearchingScope;
            OnConfirmed(EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.CloseMeAsPopup();
        }

        protected virtual void OnConfirmed(EventArgs e)
        {
            var handler = this.Confirmed;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
