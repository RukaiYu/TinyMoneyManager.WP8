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
using TinyMoneyManager.Component;

using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component.Common;
using TinyMoneyManager.Language;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Pages.DialogBox;
using System.Windows.Data;
using TinyMoneyManager.Data.Model;
using JasonPopupDemo;
using TinyMoneyManager.Controls;
using TinyMoneyManager.Data;
using System.Threading;

namespace TinyMoneyManager.Pages.BudgetManagement
{
    public partial class BudgetProjectEditPage : PhoneApplicationPage, INkjSoftPhonePageContract
    {
        private PageActionType pageAction;
        public PageActionType PageAction
        {
            get { return this.pageAction; }
            set
            {
                this.pageAction = value;
                var title = AppResources.EditBudgetProjectInfo;

                if (value == PageActionType.Add)
                {
                    title = AppResources.AddBudgetProject;

                    deleteMenuItem.IsEnabled = false;
                }

                if (value == PageActionType.ViewDetails)
                {
                    title = AppResources.ViewBudgetProjectInfo;
                }

                ProjectInfoPivot.Header = title;
            }
        }
        ApplicationBarMenuItem deleteMenuItem = new ApplicationBarMenuItem(AppResources.Delete);
        ViewModels.BudgetManagement.BudgetProjectManagementViewModel budgetProjectInfoViewModel;

        ApplicationBar applicationForEdit;
        ApplicationBar applicationForRelateCategory;
        private IApplicationBar applicationForBudgetItems;

        public BudgetProjectEditPage()
        {
            InitializeComponent();
            budgetProjectInfoViewModel = ViewModelLocator.BudgetProjectViewModel;

            TiltEffect.SetIsTiltEnabled(this, true);

            this.ProjectInfoPivot.DataContext = budgetProjectInfoViewModel.CurrentEditOrViewProject;
            this.ProjectsBudgetItemsListPivot.DataContext = budgetProjectInfoViewModel;

            this.Loaded += new RoutedEventHandler(BudgetProjectEditPage_Loaded);

            InitializeApplicationForRelateCategory();

            BudgetItemListBox.IsSelectionEnabledChanged += new DependencyPropertyChangedEventHandler(BudgetItemListBox_IsSelectionEnabledChanged);
            BudgetItemAmountEditor.CheckedPassed = budgetProjectInfoViewModel.UpdateTotalAmountInfoByBudgetItemAmountChanged;
            ExpenseOfCurrencyMonthTitle.Text = "{0} {1}".FormatWith(AppResources.CurrentMonth, AppResources.SettleExpense);

        }

        void BudgetItemListBox_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.MainPivot.IsLocked = true;
                this.ApplicationBar = LoadApplicationForBudgetItems();
            }
            else
            {
                this.MainPivot.IsLocked = false;
                this.ApplicationBar = applicationForRelateCategory;
            }
        }

        private void InitializeApplicationForRelateCategory()
        {
            applicationForRelateCategory = new ApplicationBar();

            ApplicationBarIconButton linkCategoryButton = new ApplicationBarIconButton(IconUirs.LikeToIcon);
            linkCategoryButton.Text = AppResources.LinkTo;
            linkCategoryButton.Click += new EventHandler(linkCategoryButton_Click);

            applicationForRelateCategory.Buttons.Add(linkCategoryButton);

            MainPivot.SelectionChanged += new SelectionChangedEventHandler(MainPivot_SelectionChanged);
        }

        void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = MainPivot.SelectedIndex;

            if (index == 1)
            {

            }
        }

        void linkCategoryButton_Click(object sender, EventArgs e)
        {
            ViewModelLocator.CategoryViewModel.LoadData();

            this.NavigateTo("/Pages/BudgetManagement/BudgetProjectAssociatedCategorySelector.xaml?title={0}", AppResources.LinkTo + " " + AppResources.CategoryName);
        }


        public void InitializeApplicationBar()
        {
            if (applicationForEdit == null)
            {
                applicationForEdit = new ApplicationBar();

                ApplicationBarIconButton saveButton = new ApplicationBarIconButton(IconUirs.SaveIcon);
                saveButton.Text = AppResources.Save;
                saveButton.Click += new EventHandler(saveButton_Click);


                deleteMenuItem.Click += new EventHandler(deleteMenuItem_Click);


                ApplicationBarIconButton cancelButton = new ApplicationBarIconButton(IconUirs.CancelIconButton);

                cancelButton.Text = AppResources.Cancel;
                cancelButton.Click += new EventHandler(cancelButton_Click);

                applicationForEdit.Buttons.Add(saveButton);
                applicationForEdit.MenuItems.Add(deleteMenuItem);

                applicationForEdit.Buttons.Add(cancelButton);
            }

            this.ApplicationBar = applicationForEdit;
        }

        void deleteMenuItem_Click(object sender, EventArgs e)
        {
            if (budgetProjectInfoViewModel.DeleteMultipleProjects(budgetProjectInfoViewModel.CurrentEditOrViewProject))
            {
                cancelButton_Click(sender, e);
            }
        }

        void BudgetProjectEditPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void InitializeText()
        {
            if (budgetProjectInfoViewModel.CurrentEditOrViewProject.Name.IsNullOrEmpty())
            {
                this.NameBlock.Text = AppResources.WatermaskForName;
            }

            if (budgetProjectInfoViewModel.CurrentEditOrViewProject.Notes.IsNullOrEmpty())
            {
                this.NotesBlock.Text = AppResources.WatermaskForNotes;
            }

            //if (budgetProjectInfoViewModel.BugetItemsForAdd.Count == 0)
            //{
            //    this.AmountInfoTextBlock.Text = AppResources.WatermaskForAmountInfo;
            //}

            if (PageAction != PageActionType.ViewDetails &&
                !budgetProjectInfoViewModel.CurrentEditOrViewProject.CreateAt.HasValue)
            {
                this.CreateAtBlock.Text = AppResources.UnCreatedYet;// DateTime.Now.ToString();}
            }
            else
            {
                ExpenseOfCurrencyMonthPanel.Visibility = System.Windows.Visibility.Visible;

                Thread th = new Thread(() =>
                {
                    var countForLastSettleAmountForCategory =
                         ViewModelLocator.BudgetProjectViewModel
                         .CountForSettleAmountForBudgetProject(budgetProjectInfoViewModel.CurrentEditOrViewProject, SearchingScope.CurrentMonth);
                    
                    this.Dispatcher.BeginInvoke(() =>
                    ExpenseOfCurrencyMonthBlock.Text = AccountItemMoney.GetMoneyInfoWithCurrency(countForLastSettleAmountForCategory));

                });

                th.IsBackground = true;
                th.Start();
            }

        }

        void cancelButton_Click(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            if (CheckSave())
            {
                cancelButton_Click(sender, e);
            }
        }

        public bool CheckSave()
        {
            if (budgetProjectInfoViewModel.CurrentEditOrViewProject.Name.Trim().Length == 0)
            {
                this.AlertNotification(AppResources.SomethingIsRequired.FormatWith(AppResources.Name));
                return false;
            }

            if (budgetProjectInfoViewModel.CheckSave())
            {
                this.AlertNotification(AppResources.BudgetProjectWithSameNameExists);
                return false;
            }

            budgetProjectInfoViewModel.Save();
            return true;
        }

        private void NameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            EditValueInTextBoxEditor.ResultCallBack = (s) => budgetProjectInfoViewModel.CurrentEditOrViewProject.Name = s;

            this.NavigateTo(ViewPath.EditValueInTextBoxEditor, AppResources.Name, budgetProjectInfoViewModel.CurrentEditOrViewProject.Name);
        }

        private void NotesEditorButton_Click(object sender, RoutedEventArgs e)
        {
            EditValueInTextBoxEditor.ResultCallBack = (s) => budgetProjectInfoViewModel.CurrentEditOrViewProject.Notes = s;

            this.NavigateTo(ViewPath.EditValueInTextBoxEditor, AppResources.Notes, budgetProjectInfoViewModel.CurrentEditOrViewProject.Notes);

        }

        private void AmountEditorButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.Amount,

            budgetProjectInfoViewModel.CurrentEditOrViewProject.Amount.GetValueOrDefault().ToMoneyF2(),
            (tb) =>
            {
                tb.InputScope = MoneyInputTextBox.NumberInputScope;
                tb.SelectAll();
            },
            (s) =>
            {
                var money = s.ToDecimal(() => -1m);

                if (money == -1m)
                    return false;

                var minmix = budgetProjectInfoViewModel.CurrentEditOrViewProject.TotalAmount.GetValueOrDefault();

                if (money >= minmix)
                    return true;

                this.Alert(AppResources.AmountValueShouldBeGreatThanMinimixValueAlertMessage.FormatWith(minmix.ToMoneyF2()));

                return false;

            },
            (result) =>
            {
                budgetProjectInfoViewModel.CurrentEditOrViewProject.Amount = result.ToDecimal();
                BudgetManager.Current.NeedUpdate = true;
                budgetProjectInfoViewModel.CheckSave();
            });
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = MainPivot.SelectedIndex;

            if (index == 0)
            {
                InitializeApplicationBar();
            }
            else
            {
                this.ApplicationBar = applicationForRelateCategory;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            PageAction = (PageActionType)this.GetNavigatingParameter("action").ToInt32();

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {

                Binding b = new Binding("Name");
                b.Source = budgetProjectInfoViewModel.CurrentEditOrViewProject;

                NameBlock.SetBinding(TextBlock.TextProperty, b);

                Binding bNotes = new Binding("Notes");
                bNotes.Source = budgetProjectInfoViewModel.CurrentEditOrViewProject;
                NotesBlock.SetBinding(TextBlock.TextProperty, bNotes);

                Binding bAmountInfo = new Binding("AmountInfo");
                bAmountInfo.Source = budgetProjectInfoViewModel.CurrentEditOrViewProject;
                AmountInfoTextBlock.SetBinding(TextBlock.TextProperty, bAmountInfo);

                InitializeText();

                return;
            }

            InitializeText();

            InitializeApplicationBar();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (BudgetItemListBox.IsSelectionEnabled)
            {
                e.Cancel = true;

                BudgetItemListBox.IsSelectionEnabled = false;

            }
            else
            {
                if (PageAction == PageActionType.Add)
                {
                    if (budgetProjectInfoViewModel.BugetItemsForAdd.Count > 0)
                    {
                        this.AlertConfirm(AppResources.BeforeCloseAddBudgetProjectWithBudgetItemsLinkedMessage, () =>
                        {
                            if (!CheckSave())
                            {
                                e.Cancel = true;
                            }
                        });
                    }
                }
            }

            base.OnBackKeyPress(e);
        }

        ApplicationBarIconButton removeSelectedItems;
        private IApplicationBar LoadApplicationForBudgetItems()
        {
            if (applicationForBudgetItems == null)
            {
                applicationForBudgetItems = new ApplicationBar();

                removeSelectedItems = new ApplicationBarIconButton(IconUirs.DeleteIcon);

                removeSelectedItems.Text = AppResources.Remove;
                removeSelectedItems.Click += new EventHandler(removeSelectedItems_Click);

                applicationForBudgetItems.Buttons.Add(removeSelectedItems);
            }

            return applicationForBudgetItems;
        }

        void removeSelectedItems_Click(object sender, EventArgs e)
        {
            if (this.AlertConfirm(AppResources.DeleteSelectedItems, () =>
            {
                budgetProjectInfoViewModel.RemoveAssociatedBudgetItems(BudgetItemListBox.SelectedItems.OfType<BudgetItem>());
            }) == MessageBoxResult.Cancel)
            {
                BudgetItemListBox.IsSelectionEnabled = false;
            }
        }

        private void BudgetItemElement_Click(object sender, RoutedEventArgs e)
        {
            var budgetItem = (sender as HyperlinkButton).Tag as BudgetItem;
            BudgetItemAmountEditor.BudgetItemGetter = () => budgetItem;

            BudgetItemAmountEditor.DeleteItemCallback = (item) =>
            {
                this.AlertConfirm(AppResources.DeleteAccountItemMessage, () =>
                {
                    budgetProjectInfoViewModel.RemoveAssociatedBudgetItems(new BudgetItem[] { item });
                });
            };

            this.NavigateTo("/Pages/BudgetManagement/BudgetItemAmountEditor.xaml?lastScope={0}&currentScope={1}", AppResources.LastMonth, AppResources.CurrentMonth);


        }
    }
}