namespace TinyMoneyManager.Pages.CategoryManager
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class CategoryManagment : PhoneApplicationPage
    {

        private ItemType CategoryType;
        private bool HasLoadParents
        {
            get { return CategoryVM.HasLoadParents; }
            set
            {
                CategoryVM.HasLoadParents = value;
            }
        }
        private bool hasRegistered;

        public CategoryViewModel CategoryVM { get; set; }

        public bool SelectionMode { get; set; }

        public static PageActionHandler<Category> SelectionModeHandler;

        public CategoryManagment()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.CategoryVM = ViewModelLocator.CategoryViewModel;
            base.DataContext = this;
            this.ff.FullModeHeader = AppResources.Choose.ToUpperInvariant();
            System.Collections.Generic.Dictionary<ItemType, String> dictionary2 = new System.Collections.Generic.Dictionary<ItemType, String>();
            dictionary2.Add(ItemType.Expense, AppResources.BlankWithFormatter.FormatWith(new object[] { AppResources.Expense, AppResources.Category }).ToLowerInvariant());
            dictionary2.Add(ItemType.Income, AppResources.BlankWithFormatter.FormatWith(new object[] { AppResources.Income, AppResources.Category }).ToLowerInvariant());
            System.Collections.Generic.Dictionary<ItemType, String> dictionary = dictionary2;
            this.ff.ItemsSource = dictionary;
        }

        private void AddAsFavourite_Click(object sender, RoutedEventArgs e)
        {
            Category tag = (sender as MenuItem).Tag as Category;
            if (tag != null)
            {
                this.CategoryVM.ToggleFavorite(tag);
            }
        }

        private void AddButton_Click(object sender, System.EventArgs e)
        {
            if (MainPivot.SelectedIndex == 0)
            {
                this.AddCategory(null);
            }
            else
            {
                this.AddCategory(this.SecondCategoryItems.DataContext as Category);
            }

        }

        private void AddCategory(Category parentCategory = null)
        {
            Category category = new Category();
            if (parentCategory != null)
            {
                category = parentCategory;
            }

            CategoryInfoEditor.Go(this, category, this.CategoryType, PageActionType.Add);
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.AddCategory((sender as MenuItem).Tag as Category);
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Category category = (sender as MenuItem).Tag as Category;
            if (this.CategoryVM.EnsureUsingCategory(category))
            {
                this.Alert(AppResources.CategoryIsBeenUsedMessage, null);
            }
            else
            {
                this.CategoryVM.DeletingObjectService<Category>(category, i => AppResources.Category.ToLowerInvariant(), delegate
                {
                    category.ParentCategory.Childrens.Remove(category);
                });
            }
        }

        private void EditCategory(object sender)
        {
            Category tag = (sender as MenuItem).Tag as Category;
            CategoryInfoEditor.Go(this, tag, this.CategoryType, PageActionType.Edit);
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.EditCategory(sender);
        }

        private void ff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ff.SelectedItem != null)
            {
                System.Collections.Generic.KeyValuePair<ItemType, String> selectedItem = (System.Collections.Generic.KeyValuePair<ItemType, String>)this.ff.SelectedItem;
                if (((ItemType)selectedItem.Key) != this.CategoryType)
                {
                    this.CategoryType = selectedItem.Key;
                    this.HasLoadParents = false;
                    this.LoadParents();
                }
            }
        }

        private void FirstCategoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FirstCategoryItems.SelectedItem != null)
            {
                Category selectedItem = this.FirstCategoryItems.SelectedItem as Category;
                this.SecondCategoryPivot.Header = selectedItem.Name;
                this.SecondCategoryItems.DataContext = selectedItem;
                this.SecondCategoryItems.ItemsSource = selectedItem.Childrens;
                this.MainPivot.SelectedIndex = 1;
                this.FirstCategoryItems.SelectedItem = null;
            }
        }

        public static void Go(PhoneApplicationPage fromPage, PageActionHandler<Category> categorySelectorHandler, ItemType categoryType, bool selectionMode = false)
        {
            SelectionModeHandler = categorySelectorHandler;
            fromPage.NavigateTo("/Pages/CategoryManager/CategoryManagment.xaml?type={0}&selectionMode={1}", new object[] { categoryType, selectionMode });
        }

        private void LoadParents()
        {
            if (!this.HasLoadParents)
            {
                this.ParentCategoriesPivot.Header = AppResources.BlankWithFormatter.FormatWith(new object[] { LocalizedStrings.GetLanguageInfoByKey(this.CategoryType.ToString()), AppResources.Category }).ToLowerInvariant();
                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { this.ParentCategoriesPivot.Header }));
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    var items = new ObservableCollection<Category>(this.CategoryVM.GetDataFromDatabase(p => p.IsParent && (p.CategoryType == this.CategoryType)));

                    base.Dispatcher.BeginInvoke(delegate
                    {
                        this.CategoryVM.Parents = items;

                        this.SecondCategoryItems.ItemsSource = null;
                        this.SecondCategoryPivot.Header = AppResources.SencondaryCategoryName;
                        this.WorkDone();
                        this.HasLoadParents = true;
                        if (this.SelectionMode)
                        {
                            this.ToggleCategoryTypeButton.Visibility = Visibility.Collapsed;
                            if (SelectionModeHandler != null)
                            {
                                Category category = SelectionModeHandler.OnGetSelected();
                                if (category != null)
                                {
                                    this.FirstCategoryItems.SelectedItem = category.ParentCategory;
                                }
                            }
                        }
                    });
                });
            }
        }

        private void MoveToMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Category tag = (sender as MenuItem).Tag as Category;
            if (tag != null)
            {
                SelectParentCategoryPage.Go(this, tag);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string source = this.GetNavigatingParameter("type", ItemType.Expense);
                this.SelectionMode = this.GetNavigatingParameter("selectionMode", null).ToBoolean(false);
                if (!source.IsNullOrEmpty())
                {
                    this.CategoryType = (ItemType)System.Enum.Parse(typeof(ItemType), source, true);
                    this.HasLoadParents = false;
                    this.LoadParents();
                }
            }
        }

        private void SecondCategoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SecondCategoryItems.SelectedItem != null)
            {
                Category selectedItem = this.SecondCategoryItems.SelectedItem as Category;
                if (this.SelectionMode)
                {
                    if (SelectionModeHandler != null)
                    {
                        SelectionModeHandler.OnSelected(selectedItem);
                    }
                    this.SafeGoBack();
                }
                else
                {
                    CategoryInfoViewer.Go(selectedItem.Id, this);
                }
                this.SecondCategoryItems.SelectedItem = null;
            }
        }

        private void SecondEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.EditCategory(sender);
        }

        private void ToggleCategoryTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.hasRegistered)
            {
                this.ff.SelectionChanged += new SelectionChangedEventHandler(this.ff_SelectionChanged);
                this.hasRegistered = true;
            }
            this.ff.Open();
        }

        private void ViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Category tag = (sender as MenuItem).Tag as Category;
            if (tag != null)
            {
                CategoryInfoViewer.Go(tag.Id, this);
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}

