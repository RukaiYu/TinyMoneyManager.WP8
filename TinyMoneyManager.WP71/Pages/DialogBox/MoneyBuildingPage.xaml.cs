using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Controls;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;

namespace TinyMoneyManager.Pages.DialogBox
{
    public partial class MoneyBuildingPage : PhoneApplicationPage
    {
        /// <summary>
        /// Gets or sets the item list.
        /// </summary>
        /// <value>
        /// The item list.
        /// </value>
        public System.Collections.ObjectModel.ObservableCollection<TypedKeyValuePair<string, decimal>> ItemList
        {
            get { return (System.Collections.ObjectModel.ObservableCollection<TypedKeyValuePair<string, decimal>>)GetValue(ItemListProperty); }
            set { SetValue(ItemListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemListProperty =
            DependencyProperty.Register("ItemList", typeof(System.Collections.ObjectModel.ObservableCollection<TypedKeyValuePair<string, decimal>>), typeof(MoneyBuildingPage), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the item value summary.
        /// </summary>
        /// <value>
        /// The item value summary.
        /// </value>
        public string ItemValueSummary
        {
            get { return (string)GetValue(ItemValueSummaryProperty); }
            set { SetValue(ItemValueSummaryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemValueSummary.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemValueSummaryProperty =
            DependencyProperty.Register("ItemValueSummary", typeof(string), typeof(MoneyBuildingPage), new PropertyMetadata(""));

        private bool _hasDataLoaded = false;

        public string CurrencySymbol { get; set; }

        public string CurrentNotes { get; set; }

        public static PageActionHandler<TypedKeyValuePair<string, decimal>> BuildingHandler;

        private ApplicationBarHelper aph;

        static MoneyBuildingPage()
        {
            BuildingHandler = new PageActionHandler<TypedKeyValuePair<string, decimal>>();
        }

        public MoneyBuildingPage()
        {
            InitializeComponent();

            this.DataContext = this;
            this.Loaded += MoneyBuildingPage_Loaded;

            aph = new ApplicationBarHelper(this);

            PageTitle.Text = LocalizedStrings.GetCombinedText(AppResources.Edit, AppResources.Notes).ToUpper();
        }

        void MoneyBuildingPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._hasDataLoaded)
            {
                this.ApplicationBar.GetIconButtonFrom(0)
                    .Text = AppResources.Save;

                this.ApplicationBar.GetIconButtonFrom(1)
                    .Text = AppResources.Cancel;

                this._hasDataLoaded = true;

                ItemList = new System.Collections.ObjectModel.ObservableCollection<TypedKeyValuePair<string, decimal>>();

                ItemList.CollectionChanged += ItemList_CollectionChanged;

                EnsureCurrentNotes();

                this.ItemListBox.ItemsSource = ItemList;

                aph.SelectContentWhenFocus = true;
                aph.OriginalBar = this.ApplicationBar;

                aph.AddTextBox(this.ItemNameBoxAdding,
                    this.ItemValueBoxAdding);

            }
        }

        private void EnsureCurrentNotes()
        {
            if (!CurrentNotes.IsNullOrEmpty())
            {
                // safasdf, 3435.00;adsfadfa,4523423;

                var items = CurrentNotes.Split(new char[] { ';' });

                foreach (var item in items)
                {
                    var itemInfos = item.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    if (itemInfos.Length == 2)
                    {
                        ItemList.Add(new TypedKeyValuePair<string, decimal>(itemInfos[0].Trim(), itemInfos[1].ToDecimal()));
                    }
                }
            }
        }

        void ItemList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var sum = this.ItemList.Sum(p => p.Value);

            this.ItemValueSummary = AccountItemMoney.GetMoneyInfoWithCurrency(CurrencySymbol, sum);
        }

        private void AddItemButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.ItemNameBoxAdding.Text.Trim().IsNullOrEmpty())
            {
                return;
            }

            var key = this.ItemNameBoxAdding.Text;

            var value = this.ItemValueBoxAdding.Text.ToDecimal();

            this.ItemList.Add(new TypedKeyValuePair<string, decimal>(key, value));

            this.ItemNameBoxAdding.Text = string.Empty;
            this.ItemValueBoxAdding.Text = string.Empty;
            this.ItemNameBoxAdding.Focus();
        }

        /// <summary>
        /// Goes to this page from <para>fromPage</para>.
        /// </summary>
        /// <param name="fromPage">From page.</param>
        public static void GoTo(PhoneApplicationPage fromPage, string currencySymbol, string currencyNotes)
        {
            fromPage.NavigateTo("/Pages/DialogBox/MoneyBuildingPage.xaml?currency={0}&currencyNotes={1}", currencySymbol, currencyNotes);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                this.CurrencySymbol = e.GetNavigatingParameter("currency");
                this.CurrentNotes = e.GetNavigatingParameter("currencyNotes");
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (BuildingHandler != null)
            {
                this.ItemList.CollectionChanged -= ItemList_CollectionChanged;

                if (this.ItemList.Count == 0)
                {
                    this.ItemList.Add(new TypedKeyValuePair<string, decimal>(this.ItemNameBoxAdding.Text, this.ItemValueBoxAdding.Text.ToDecimal()));
                }

                var allItems = this.ItemList.Select(p => "{0}:{1};".FormatWith(p.Key, p.Value.ToMoneyF2()))
                    .ToStringLine("");

                var sum = this.ItemList.Sum(p => p.Value);
                BuildingHandler.OnSelected(new TypedKeyValuePair<string, decimal>(allItems, sum));
            }

            this.SafeGoBack();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.SafeGoBack();
        }

        private void ItemNameBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            this.ApplicationBar = aph.ApplicationBarForEditor;
            (sender as TextBox).SelectAll();
        }

        private void ItemNameBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            this.ApplicationBar = aph.OriginalBar;
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Coding4Fun.Phone.Controls.RoundButton).Tag as TypedKeyValuePair<string, decimal>;

            if (item != null)
            {
                this.ItemList.Remove(item);
            }
        }

        private void ItemValueBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            ItemList_CollectionChanged(this.ItemList, null);
        }

    }
}