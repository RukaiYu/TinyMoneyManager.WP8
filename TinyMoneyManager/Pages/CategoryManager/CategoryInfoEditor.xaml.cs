namespace TinyMoneyManager.Pages.CategoryManager
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class CategoryInfoEditor : PhoneApplicationPage
    {
        private ApplicationBarHelper appBarHelper;

        private ItemType CategoryClassType;

        public CategoryViewModel CategoryVM;

        public ApplicationBar functionAppBar;

        public PageActionType pageAction;
        private Category ParentCategory;


        public CategoryInfoEditor()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.CategoryVM = ViewModelLocator.CategoryViewModel;
            this.InitializeAppBar();
        }

        private void AddCategory()
        {
            this.Current = new Category();
            this.Current.Id = System.Guid.NewGuid();
            this.Current.Name = this.CategoryName.Text;
            this.Current.Order = new int?(this.OrderValue.Text.ToInt32());
            this.Current.CategoryType = this.CategoryClassType;
            this.Current.ParentCategory = this.ParentCategory ?? this.ParentCategory;
            this.Current.DefaultAmount = this.DefaultAmount.Text.ToDecimal();

            if (!this.Current.IsParent)
            {
                this.ParentCategory.Childrens.Add(this.Current);
            }

            this.CategoryVM.AddCategory(this.Current);
        }

        public void Close()
        {
            this.SafeGoBack();
        }

        private void closeButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void EditCategory()
        {
            this.Current.Name = this.CategoryName.Text;
            this.Current.Order = new int?(this.OrderValue.Text.ToInt32());
            this.Current.DefaultAmount = this.DefaultAmount.Text.ToDecimal();
            this.CategoryVM.Update(this.Current);
        }

        private void EnsureParentCategory()
        {
            if (this.ParentCategory != null)
            {
                this.ParentCategoryNameTextBox.Text = this.ParentCategory.Name;
                this.DefaultAmount.IsEnabled = true;
            }
            else
            {
                this.DefaultAmount.IsEnabled = false;
                this.ParentCategoryNameTextBox.Text = AppResources.ParentCategoryClass.ToLowerInvariant();
            }
            this.ParentCategoryNameTextBox.IsEnabled = false;
        }

        public static void Go(PhoneApplicationPage fromPage, Category category, ItemType categoryType, PageActionType pageAction = PageActionType.Add)
        {
            System.Guid guid = (category == null) ? System.Guid.Empty : category.Id;
            fromPage.NavigateTo("/Pages/CategoryManager/CategoryInfoEditor.xaml?id={0}&pageAction={1}&CategoryClassType={2}", new object[] { guid, pageAction, categoryType });
        }

        public void InitializeAppBar()
        {
            this.appBarHelper = new ApplicationBarHelper(this);
            this.functionAppBar = new ApplicationBar();
            ApplicationBarIconButton button = IconUirs.CreateIconButton(AppResources.Save, IconUirs.SaveIcon);
            button.Click += new System.EventHandler(this.saveButton_Click);
            ApplicationBarIconButton button2 = IconUirs.CreateIconButton(AppResources.Cancel, IconUirs.CancelIconButton);
            button2.Click += new System.EventHandler(this.closeButton_Click);
            this.functionAppBar.AddButtons(new ApplicationBarIconButton[] { button, button2 });
            this.appBarHelper.OriginalBar = this.functionAppBar;
            this.appBarHelper.SelectContentWhenFocus = true;
            this.appBarHelper.AddTextBox(new TextBox[] { this.DefaultAmount, this.CategoryName, this.OrderValue });
            base.ApplicationBar = this.functionAppBar;
        }
        private void LoadAddingObject(System.Guid categoryId)
        {
            System.Func<Category, Boolean> resultSelector = null;
            this.CategoryManagementPageTitle.Text = LocalizedStrings.GetCombinedText(AppResources.Add, AppResources.Category, false).ToUpperInvariant();
            if (categoryId != System.Guid.Empty)
            {
                if (resultSelector == null)
                {
                    resultSelector = p => p.Id == categoryId;
                }
                this.ParentCategory = this.CategoryVM.GetDataFromDatabase(resultSelector).FirstOrDefault<Category>();
            }
            if (this.ParentCategory != null)
            {
                this.CategoryManagementPageTitle.Text = LocalizedStrings.GetCombinedText(AppResources.Add, AppResources.SencondaryCategoryName, false).ToUpperInvariant();
            }
            this.EnsureParentCategory();
        }

        public void LoadEditingObject(System.Guid categoryId)
        {
            this.CategoryManagementPageTitle.Text = LocalizedStrings.GetCombinedText(AppResources.Edit, AppResources.CategoryInfo, false).ToUpperInvariant();
            this.Current = this.CategoryVM.GetDataFromDatabase(p => p.Id == categoryId).FirstOrDefault<Category>();
            if (this.Current != null)
            {
                this.CategoryName.Text = this.Current.Name;
                this.DefaultAmount.Text = this.Current.DefaultAmountString;
                this.OrderValue.Text = this.Current.Order.GetValueOrDefault().ToString();
                this.ParentCategory = this.Current.ParentCategory;
            }
            this.CategoryType.SelectedIndex = (int)this.CategoryClassType;
            this.CategoryType.IsEnabled = false;
            this.EnsureParentCategory();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.pageAction = this.GetNavigatingParameter("pageAction", null).ToEnum<PageActionType>(PageActionType.Add);
                this.CategoryClassType = this.GetNavigatingParameter("CategoryClassType", null).ToEnum<ItemType>(ItemType.Expense);
                string source = e.GetNavigatingParameter("id", null);
                if (this.pageAction == PageActionType.Edit)
                {
                    this.LoadEditingObject(source.ToGuid());
                }
                else
                {
                    this.LoadAddingObject(source.ToGuid());
                }
            }
        }

        public void Save()
        {
            System.Func<Category, Boolean> func = null;
            System.Func<Category, Boolean> func2 = null;
            if (this.CategoryName.Text.Trim().Length == 0)
            {
                this.AlertNotification(AppResources.EmptyTextMessage, null);
            }
            else
            {
                System.Action action = delegate
                {
                    this.Alert(AppResources.RecordAlreadyExist, null);
                };
                if (this.pageAction == PageActionType.Add)
                {
                    if (func == null)
                    {
                        func = p => ((p.Name == this.CategoryName.Text) && (p.CategoryType == this.CategoryClassType)) && (p.ParentCategoryId == this.ParentCategory.Id);
                    }
                    if (this.CategoryVM.Exists(func))
                    {
                        action();
                        return;
                    }
                    this.AddCategory();
                }
                else
                {
                    if (func2 == null)
                    {
                        func2 = p => (((p.CategoryType == this.CategoryClassType) && (p.Id != this.Current.Id)) && (p.Name == this.CategoryName.Text)) && (p.ParentCategoryId == this.ParentCategory.Id);
                    }
                    if (this.CategoryVM.Exists(func2))
                    {
                        action();
                        return;
                    }
                    this.EditCategory();
                }
                this.Close();
            }
        }

        private void saveButton_Click(object sender, System.EventArgs e)
        {
            this.Save();
        }

        public Category Current { get; set; }
    }
}

