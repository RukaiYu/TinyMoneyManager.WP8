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
using Microsoft.Phone.Controls;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.Pages.DialogBox
{
    using TinyMoneyManager.Component;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using TinyMoneyManager.Data.Model;
    using Microsoft.Phone.Shell;
    using System.Collections.ObjectModel;
    using TinyMoneyManager.ViewModels;
    using System.Threading;
    using TinyMoneyManager.ViewModels.AccountItemManager;
    using TinyMoneyManager.Language;
    using System.ComponentModel;

    public partial class StatsticSummaryItemsViewer : PhoneApplicationPage, INotifyPropertyChanged
    {
        public static bool NeedReloadData = true;

        public event PropertyChangedEventHandler PropertyChanged;
        private string stasticItemsTips;
        public string StasticItemsTips
        {
            get { return stasticItemsTips; }
            set
            {
                if (value != stasticItemsTips)
                {
                    stasticItemsTips = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("StasticItemsTips"));
                }
            }
        }

        public StatsticSummaryItemsViewer()
        {
            InitializeComponent();

            TiltEffect.SetIsTiltEnabled(this, true);

            this.DataContext = this;

            this.Loaded += new RoutedEventHandler(StatsticSummaryItemsViewer_Loaded);
        }

        void StatsticSummaryItemsViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (NeedReloadData)
            {
                NeedReloadData = false;
                this.BusyForWork(AppResources.Loading);

                Thread th = new Thread(() =>
                       {
                           LoadData();
                       });

                th.Start();
            }
        }

        public static Func<AccountItem, bool> SearchingCondition;

        public static Func<IEnumerable<AccountItem>> DataSourceGetter;

        public void LoadData()
        {
            if (DataSourceGetter != null)
            {
                int totalRecords = 0;
                var data = ViewModelLocator
                   .AccountItemViewModel.GetGroupedRelatedItems(DataSourceGetter(), () => totalRecords++);

                Dispatcher.BeginInvoke(() =>
                {
                    this.RelatedItemsListControl.ItemsSource = data;
                    StasticItemsTips = AppResources.StasticItemsTips_HasItemsFormatter.FormatWith(totalRecords);
                    this.WorkDone();
                });
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void RelatedItemsListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = RelatedItemsListControl.SelectedItem as AccountItem;

            if (item != null)
            {
                this.NavigateTo("/Pages/AccountItemViews/AccountItemViewer.xaml?fromSelf=true&RelatedVisible=false&id={0}", item.Id);
                RelatedItemsListControl.SelectedItem = null;
            }

        }

        /// <summary>
        /// Shows the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="SearchingCondition">The searching condition.</param>
        /// <param name="pageFrom">The page from.</param>
        public static void Show(SummaryDetails item, DetailsCondition SearchingCondition, PhoneApplicationPage pageFrom)
        {
            var queryForSeeMore = QueryDataBySearchingCondition(SearchingCondition);

            var summaryItem = item.Tag as AccountItem;

            Guid categoryID = summaryItem.CategoryId;

            Account account = summaryItem.Account;

            if (SearchingCondition.ChartGroupMode == ChartGroupMode.ByCategoryName)
            {
                if (SearchingCondition.GroupCategoryMode == CategorySortType.ByParentCategory)
                {
                    IEnumerable<Guid> ids = null;

                    if (summaryItem.Category.IsParent)
                    {
                        categoryID = summaryItem.Category.Id;
                        ids = ViewModelLocator.CategoryViewModel.AccountBookDataContext.Categories.Where(p => p.ParentCategoryId == categoryID)
                        .Select(p => p.Id).ToList();


                    }
                    else
                    {
                        categoryID = summaryItem.Category.ParentCategoryId;
                        ids = ViewModelLocator.CategoryViewModel.AccountBookDataContext.Categories.Where(p => p.ParentCategoryId == summaryItem.Category.ParentCategory.Id)
                        .Select(p => p.Id).ToList();
                    }

                    queryForSeeMore = queryForSeeMore.Where(p => ids.Contains(p.CategoryId) || p.CategoryId == categoryID);
                }
                else
                {
                    queryForSeeMore = queryForSeeMore.Where(i => i.CategoryId == categoryID);
                }
            }
            else
            {
                if (account != null)
                {
                    queryForSeeMore = queryForSeeMore.Where(p => p.AccountId == account.Id);
                }
            }

            StatsticSummaryItemsViewer.NeedReloadData = true;

            Pages.DialogBox.StatsticSummaryItemsViewer.DataSourceGetter = () => queryForSeeMore;

            pageFrom.NavigateTo("/Pages/DialogBox/StatsticSummaryItemsViewer.xaml");
        }

        /// <summary>
        /// Queries the data by searching condition.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public static IEnumerable<AccountItem> QueryDataBySearchingCondition(DetailsCondition e)
        {
            IEnumerable<AccountItem> data;
            data = ViewModelLocator.AccountItemViewModel.AccountBookDataContext.AccountItems;
            if (e.SearchingScope != SearchingScope.All)
            {
                data = data.Where(p => p.CreateTime.Date >= e.StartDate.GetValueOrDefault().Date
                              && p.CreateTime.Date <= e.EndDate.GetValueOrDefault().Date);
            }

            if (e.IncomeOrExpenses != ItemType.All)
                data = data.Where(p => p.Type == e.IncomeOrExpenses);

            if (e.AccountIds.Count != 0)
            {
                data = data.Where(p => e.AccountIds.Contains(p.AccountId));
            }

            if (e.CategoryIds.Count != 0)
            {
                if (e.GroupCategoryMode == CategorySortType.ByChildCategory)
                    data = data.Where(p => e.CategoryIds.Contains(p.CategoryId));
                else
                {
                    data = data.Where(p => e.CategoryIds.Contains(p.Category.ParentCategoryId));
                }
            }

            if (e.ShowOnlyIsClaim.GetValueOrDefault())
            {
                data = data.Where(p => p.IsClaim == true);
            }

            if (!string.IsNullOrEmpty(e.NotesKey))
            {
                var keys = e.NotesKey.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                if (keys.Length > 0)
                {
                    data = data.Where(p => keys.Contains(p.Description));
                }
            }
            return data;
        }

    }
}