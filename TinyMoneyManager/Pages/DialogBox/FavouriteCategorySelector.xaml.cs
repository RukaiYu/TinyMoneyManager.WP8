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
using System.Collections;
using TinyMoneyManager.ViewModels;
using System.Windows.Controls.Primitives;
using TinyMoneyManager.Component;
using System.Collections.ObjectModel;

namespace TinyMoneyManager.Pages.DialogBox
{
    public partial class FavouriteCategorySelector : UserControl
    {

        #region --- Dialog Base Members ---

        public event EventHandler Closed;

        public void Close(object sender, EventArgs e)
        {
            OnDialogClosed(e);
            // Unhook the BackKeyPress event handler
            ((PhoneApplicationPage)RootVisual.Content).BackKeyPress -= page_BackKeyPress;
            this.ChildWindowPopup.IsOpen = false;
            if (_reshowAppBar)
                ((PhoneApplicationPage)RootVisual.Content).ApplicationBar.IsVisible = true;
        }

        static bool _reshowAppBar;

        #region properties

        internal Popup ChildWindowPopup
        {
            get;
            private set;
        }

        private static PhoneApplicationFrame RootVisual
        {
            get
            {
                return Application.Current == null ? null : Application.Current.RootVisual as PhoneApplicationFrame;
            }
        }

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

        #endregion

        #region Methods
        public void Show()
        {
            if (this.ChildWindowPopup == null)
            {
                this.ChildWindowPopup = new Popup()
                {
                    Language = System.Windows.Markup.XmlLanguage.GetLanguage(LocalizedStrings.CultureName.Name)
                };

                try
                {
                    this.ChildWindowPopup.Child = this;
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("The control is already shown.");
                }
            }

            if (this.ChildWindowPopup != null && Application.Current.RootVisual != null)
            {
                //SystemTray.IsVisible = false;
                // Show popup
                this.ChildWindowPopup.IsOpen = true;
            }

            if (RootVisual != null)
            {
                var page = ((PhoneApplicationPage)RootVisual.Content);

                if (page.ApplicationBar != null && page.ApplicationBar.IsVisible)
                {
                    _reshowAppBar = true;
                    page.ApplicationBar.IsVisible = false;
                }

                // Hook up into the back key press event of the current page
                page.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(page_BackKeyPress);
            }

            AfterShow();
        }

        void page_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Unhook the BackKeyPress event handler 
            Close(sender, e);
            e.Cancel = true;
        }

        protected void OnDialogClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        #endregion

        #endregion

        public Category SelectedItem { get; set; }

        private CategoryViewModel categoryViewModel;

        public ObservableCollection<Category> Items { get; set; }

        private ItemType itemType;
        public ItemType ItemType
        {
            get { return itemType; }
            set
            {
                if (itemType != value)
                {
                    NeedReloadData = true;
                    itemType = value;
                }
            }
        }

        public FavouriteCategorySelector()
        {
            InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            NeedReloadData = true;
            this.categoryViewModel = ViewModelLocator.CategoryViewModel;
            Items = new ObservableCollection<Category>();
            this.FirstCategoryItems.DataContext = this;
            this.categoryViewModel.AddOrRemoveFavouriteCallback = AddOrRemoveFavouriteItem;

            this.FirstCategoryItems.SelectionChanged += new SelectionChangedEventHandler(FirstCategoryItems_SelectionChanged);

            Loaded += new RoutedEventHandler(FavouriteCategorySelector_Loaded);

        }

        void FirstCategoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FirstCategoryItems.SelectedItem == null)
                return;

            this.SelectedItem = FirstCategoryItems.SelectedItem as Category;
            this.FirstCategoryItems.SelectedItem = null;

            Close(sender, e);
        }

        void FavouriteCategorySelector_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void AddOrRemoveFavouriteItem(PageActionType addOrRemove, Category category)
        {
            if (addOrRemove == PageActionType.Delete)
                this.Items.Remove(category);
            if (addOrRemove == PageActionType.Add)
                this.Items.Add(category);
        }

        private void LoadData()
        {
            if (!NeedReloadData) return;
            categoryViewModel.LoadData();
            Items.Clear();
            categoryViewModel.GetCategories(p => p.CategoryType == ItemType
            && p.Favourite.HasValue && (bool)p.Favourite)
                .OrderBy(p => p.ParentCategoryId)
                .ToList()
                .ForEach(Items.Add);

            NeedReloadData = false;
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(LocalizedStrings.GetLanguageInfoByKey("DeleteAccountItemMessage"), App.AlertBoxTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var item = (sender as MenuItem).Tag as Category;

                Items.Remove(item);

                categoryViewModel.RemoveFavouriteItem(item);
            }
        }

        public static bool NeedReloadData { get; set; }

        private void AfterShow()
        {
            //First to load items; 
            if (NeedReloadData)
            {
                LoadData();
            }
        }
    }
}
