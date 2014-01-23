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
using TinyMoneyManager.Language;

namespace TinyMoneyManager.Controls
{
    public partial class DateRangeSelector : UserControl, INotifyPropertyChanged
    {
        private SearchingScope seachingScope;

        public SearchingScope SearchingScope
        {
            get { return seachingScope; }
            set
            {
                if (seachingScope != value)
                {
                    //OnNotifyPropertyChanging("SearchingScope");
                    seachingScope = value;
                    OnNotifyPropertyChanged("SearchingScope");
                }
            }
        }

        private int searchingScopeIndex;

        public int SearchingScopeIndex
        {
            get { return searchingScopeIndex; }
            set
            {
                if (searchingScopeIndex != value)
                {

                    searchingScopeIndex = value;
                    OnNotifyPropertyChanged("SearchingScopeIndex");
                }
            }
        }


        public void OnNotifyPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }



        public DateRangeSelector()
        {
            InitializeComponent();

            SearchDuringDate.ItemsSource = new string[] { 
                LocalizedStrings.GetLanguageInfoByKey("Today").ToLowerInvariant(),
                LocalizedStrings.GetLanguageInfoByKey("CurrentWeek").ToLowerInvariant(),
                LocalizedStrings.GetLanguageInfoByKey("CurrentMonth").ToLowerInvariant(),
                LocalizedStrings.GetLanguageInfoByKey("CurrentYear").ToLowerInvariant(),
                LocalizedStrings.GetLanguageInfoByKey("LastMonth").ToLowerInvariant(),
                //LocalizedStrings.GetLanguageInfoByKey("Customize").ToLowerInvariant(),
               AppResources.All,
            };
            SearchDuringDate.ItemCountThreshold = 6;
            SearchDuringDate.DataContext = this;
            SearchDuringDate.SelectionChanged += new SelectionChangedEventHandler(SearchDuringDate_SelectionChanged);

            this.Loaded += new RoutedEventHandler(DateRangeSelector_Loaded);
        }

        void DateRangeSelector_Loaded(object sender, RoutedEventArgs e)
        {

        }

        void SearchDuringDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = SearchDuringDate.SelectedIndex;

            if (index == 4)
                seachingScope = Component.SearchingScope.LastMonth;
            else
                seachingScope = (Component.SearchingScope)index;
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}
