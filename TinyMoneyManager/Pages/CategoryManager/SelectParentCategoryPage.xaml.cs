namespace TinyMoneyManager.Pages.CategoryManager
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public  partial class SelectParentCategoryPage : PhoneApplicationPage
    {
        private string categoryName = string.Empty;

        public SelectParentCategoryPage()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            base.DataContext = this;
        }

        public static void Go(PhoneApplicationPage fromPage, Category category)
        {
            SelectionHandler = new PageActionHandler<Category>();
            SelectionHandler.AfterSelected = delegate(Category item)
            {
                if ((item != null) && (item.Id != category.ParentCategoryId))
                {
                    category.ParentCategory.Childrens.Remove(category);
                    category.ParentCategory = item;
                    if (!item.Childrens.Contains(category))
                    {
                        item.Childrens.Add(category);
                    }
                    ((System.Collections.Generic.IEnumerable<AccountItem>)category.AccountItems).ForEach<AccountItem>(delegate(AccountItem p)
                    {
                        p.RaisePropertyChangd("NameInfo");
                    });
                    ViewModelLocator.CategoryViewModel.Update(category);
                }
            };
            fromPage.NavigateTo("/pages/CategoryManager/SelectParentCategoryPage.xaml?id={0}&currentName={1}&type={2}&childName={3}", new object[] { category.ParentCategoryId, category.ParentCategory.Name, category.CategoryType, category.Name });
        }

        private void NameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            System.Action isOkToDo = null;
            Category item = (sender as HyperlinkButton).Tag as Category;
            if (item != null)
            {
                if (isOkToDo == null)
                {
                    isOkToDo = delegate
                    {
                        SelectionHandler.OnSelected(item);
                        this.SafeGoBack();
                    };
                }
                this.AlertConfirm(AppResources.MovingCategoryMessage.FormatWith(new object[] { this.categoryName, item.Name }), isOkToDo, AppResources.ConfirmDoingSomething.FormatWith(new object[] { AppResources.MovingCategory }).ToUpperInvariant());
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string str = this.GetNavigatingParameter("currentName", null);
                ItemType categoryType = (ItemType)System.Enum.Parse(typeof(ItemType), this.GetNavigatingParameter("type", null), true);
                this.CurrentCategoryParentIs.Text = str;
                this.categoryName = this.GetNavigatingParameter("childName", null);
                this.BusyForWork(AppResources.Loading);
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    ObservableCollection<Category> ParentCategories = new ObservableCollection<Category>();
                    System.Collections.Generic.List<Category> list = this.CategoryVM.GetDataFromDatabase(p => (p.CategoryType == categoryType) && p.IsParent).ToList<Category>();
                    this.Dispatcher.BeginInvoke(delegate
                    {
                        list.ForEach(delegate(Category p)
                        {
                            ParentCategories.Add(p);
                        });
                        this.ParentCategoriesList.ItemsSource = ParentCategories;
                        this.WorkDone();
                    });
                });
            }
        }

        public CategoryViewModel CategoryVM
        {
            get
            {
                return ViewModelLocator.CategoryViewModel;
            }
        }

        public static PageActionHandler<Category> SelectionHandler { get; set; }
    }
}

