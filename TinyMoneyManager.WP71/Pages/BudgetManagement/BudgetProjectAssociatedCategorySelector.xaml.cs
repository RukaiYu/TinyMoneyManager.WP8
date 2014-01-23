using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Windows.Controls.Primitives;
using System.Collections;

using TinyMoneyManager.Data.Model;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.ViewModels;
using System.Collections.Generic;
using TinyMoneyManager.Language;
using Microsoft.Phone.Shell;


namespace TinyMoneyManager.Pages.BudgetManagement
{
    public partial class BudgetProjectAssociatedCategorySelector : PhoneApplicationPage
    {
        #region --- Dialog Base Members ---



        private string title;
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
                this.SelectorPagesPivot.Title = value;
            }
        }

        public Category SelectedItem { get; set; }

        private Category _firstItem;
        #endregion

        #region Methods
        public void Show()
        {
            ReloadData();
        }


        #endregion

        ApplicationBar applicationBarForChoice;


        /// <summary>
        /// 
        /// </summary>
        public Func<Category, IList> GetSecondCategoryItems;

        private CategoryViewModel categoryViewModel;

        /// <summary>
        /// Gets or sets a value, specific the status of Dialog box, whether it is at Manage Mode or Selector Mode. Default is false.
        /// </summary>
        public bool ManageMode { get; set; }

        public BudgetProjectAssociatedCategorySelector()
        {
            InitializeComponent();
            //this.DefaultStyleKey = typeof(CategoryPickerDialogBox);
            TiltEffect.SetIsTiltEnabled(this, true);
            InitializePivotInfo();
            categoryViewModel = ViewModelLocator.CategoryViewModel;

            ManageMode = false;
            this.Loaded += new RoutedEventHandler(CategoryPickerDialogBox_Loaded);
            this.DataReloading += new EventHandler<EventArgs>(CategoryPickerDialogBox_DataReloading);

            this.GetSecondCategoryItems = (parent) => categoryViewModel.GetChildCategories(parent).ToList();

            initializeAppBar();

            SecondCategoryItems.IsSelectionEnabledChanged += new DependencyPropertyChangedEventHandler(SecondCategoryItems_IsSelectionEnabledChanged);
            this.SelectorPagesPivot.SelectionChanged += new SelectionChangedEventHandler(SelectorPagesPivot_SelectionChanged);
            SecondCategoryItems.SelectionChanged += new SelectionChangedEventHandler(SecondCategoryItems_SelectionChanged);
        }

        void SelectorPagesPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.okButton.IsEnabled = SelectorPagesPivot.SelectedIndex == 1;
        }

        void SecondCategoryItems_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        ApplicationBarIconButton okButton;
        private void initializeAppBar()
        {
            applicationBarForChoice = new ApplicationBar();

            okButton = new ApplicationBarIconButton(IconUirs.CheckIcon);

            okButton.Text = AppResources.OK;
            okButton.IsEnabled = false;
            okButton.Click += new EventHandler(okButton_Click);

            applicationBarForChoice.Buttons.Add(okButton);
            this.ApplicationBar = applicationBarForChoice;
        }

        void CategoryPickerDialogBox_DataReloading(object sender, EventArgs e)
        {
            var data = categoryViewModel.GetCategories(p => p.IsParent && p.CategoryType == this.CategoryType).ToList();
            this.FirstCategoryItems.ItemsSource = data;
        }

        void CategoryPickerDialogBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null && SelectedItem.ParentCategory != null)
            {
                //SecondCategoryPivot.Header = SelectedItem.ParentCategory.Name;&& CurrentListBox == SecondCategoryItems
                TurnToSecond(SelectedItem);

                SecondCategoryItems.IsSelectionEnabled = true;
            }
        }

        private void TurnToSecond(Category SelectedItem)
        {
            togglePivot(SelectedItem.ParentCategory);
        }

        public void InitializePivotInfo()
        {
            FirstCategoryPivot.Header = LocalizedStrings.GetLanguageInfoByKey("CategoryName");
            SecondCategoryPivot.Header = LocalizedStrings.GetLanguageInfoByKey("SencondaryCategoryName");
        }

        public void ResetSelector()
        {
            InitializePivotInfo();
            this.SecondCategoryItems.ItemsSource = null;

            this.SelectorPagesPivot.SelectedIndex = 0;
            this.FirstItem = null;
            this.SelectedItem = null;
        }

        private void togglePivot(Category category)
        {
            this.SelectorPagesPivot.SelectedIndex = 1;

            _firstItem = category;
            SecondCategoryPivot.Header = _firstItem.Name;
            if (GetSecondCategoryItems != null)
            {
                SecondCategoryItems.ItemsSource = (GetSecondCategoryItems(category)).OfType<Category>().ToList();
            }
        }

        bool fromEditor = false;
        void editor_Closed(object sender, EventArgs e)
        {
            ReloadData();
            fromEditor = false;
        }

        bool needReload = true;
        public void ReloadData()
        {
            if (!needReload) return;
            else
            {
                this.SecondCategoryItems.ItemsSource = new List<Category>().ToList();
                SecondCategoryPivot.Header = AppResources.SencondaryCategoryName;

                if (!fromEditor)
                    SelectorPagesPivot.SelectedIndex = 0;
            }

            if (DataReloading != null)
                DataReloading(this, EventArgs.Empty);
            if (GetSecondCategoryItems != null && _firstItem != null)
            {
                this.SecondCategoryItems.ItemsSource = GetSecondCategoryItems(_firstItem);
            }


        }

        public event EventHandler<EventArgs> DataReloading;

        public Category FirstItem
        {
            get { return _firstItem; }
            set { _firstItem = value; }
        }

        private ItemType categoryType;
        public ItemType CategoryType
        {
            get
            {
                return categoryType;
            }
            set
            {
                if (categoryType != value)
                {
                    needReload = true;
                    categoryType = value;
                }
            }
        }

        public MessageBoxResult DialogResult { get; set; }

        private void FirstCategoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FirstCategoryItems == null || FirstCategoryItems.SelectedItem == null)
                return;

            if (sender == null)
            {
                MessageBox.Show(AppResources.NoneCategorySelectedMessage);
                return;
            }

            var category = FirstCategoryItems.SelectedItem as Category;
            togglePivot(category);
            FirstCategoryItems.SelectedItem = null;
        }

        private void SecondCategoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.okButton.IsEnabled = SecondCategoryItems.SelectedItems.Count > 0;
        }

        private void AddAsFavourite_Click(object sender, RoutedEventArgs e)
        {
            var category = (sender as MenuItem).Tag as Category;
            if (category != null)
            {
                categoryViewModel.ToggleFavorite(category);
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                this.Title = this.GetNavigatingParameter("title");
                Show();
            }

        }

        void okButton_Click(object sender, EventArgs e)
        {
            var categories = this.SecondCategoryItems.SelectedItems.OfType<Category>().ToArray();

            ViewModelLocator.BudgetProjectViewModel.UpdatingAssociatedCategoriesForCurrentEditInstance(categories);

            this.SafeGoBack();

        }


        private void CategoryItem_Click(object sender, RoutedEventArgs e)
        {
            var category = (sender as HyperlinkButton).Tag as Category;
            ViewModelLocator.BudgetProjectViewModel.UpdatingAssociatedCategoriesForCurrentEditInstance(category);

            this.SafeGoBack();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (SecondCategoryItems.IsSelectionEnabled)
            {
                SecondCategoryItems.IsSelectionEnabled = false;
                e.Cancel = true;
                return;
            }

            base.OnBackKeyPress(e);
        }

        private void ChooseAsResult_Click(object sender, RoutedEventArgs e)
        {
            var category = (sender as MenuItem).Tag as Category;
            ViewModelLocator.BudgetProjectViewModel.UpdatingAssociatedCategoriesForCurrentEditInstance(category);

            this.SafeGoBack();
        }
    }
}
