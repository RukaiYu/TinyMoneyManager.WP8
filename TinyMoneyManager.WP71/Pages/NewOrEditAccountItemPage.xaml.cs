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
using TinyMoneyManager.ViewModels;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Controls;
using TinyMoneyManager.Pages.DialogBox;
using Microsoft.Phone.Shell;
using TinyMoneyManager.ScheduledAgentLib;
using TinyMoneyManager.Pages.DialogBox.PictureManager;
using TinyMoneyManager.Language;
using TinyMoneyManager.Pages.AccountItemViews;
using System.Windows.Navigation;
using TinyMoneyManager.Data;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using Coding4Fun.Phone.Controls;
using TinyMoneyManager.Pages.CategoryManager;
using TinyMoneyManager.Pages.BorrowAndLean;

namespace TinyMoneyManager.Pages
{
    public partial class NewOrEditAccountItemPage : PhoneApplicationPage
    {
        private AccountItemListViewModel accountItemViewModel;

        private PageActionType actionType;
        public static System.Action<AccountItem> AfterCreated;

        public static TallySchedule ScheduledTaskInfo;

        private PageActionHandler<Category> categorySelectorHandler;

        private CategoryViewModel categoryViewModel;

        public static AccountItem currentEditObject = null;

        private ApplicationBarHelper editingApplicationBarManager;
        private FavouriteCategorySelector favouriteCategorySelectorDialogBox;
        private bool fromMainPage;
        private IApplicationBar funcBar;
        public ApplicationBarIconButton goToPriceInputBoxButton;
        private bool hasLoaded;
        private bool hasNavied;
        private bool hasSetAutoDataForDescription;
        private bool hasSetControlLocalizedString;

        private bool isDataNavigated;
        private bool isFocusOnTextBox;
        private bool isFromList;

        public const string LinkNameTag = "QuickNewRecordBackgroundImage.png";

        private bool needAlertNotice;

        public static Uri NewRecordStartScreenBackgroundImageLink = new Uri("/images/newRecordBackgroundImage.png", UriKind.RelativeOrAbsolute);
        public static Uri NewRecordStartScreenLink = ScheduledAgentConfiguration.NewRecordStartScreenLink;
        private PageActionHandler<PeopleAssociationData> peopleHandler;

        public PictureActionHandler picHandler;

        private static string pinToStartLink = "/Pages/NewOrEditAccountItemPage.xaml?action=Add&fromStartScreen=true";
        private ApplicationBarMenuItem pintToStart;
        private ApplicationSafetyService safetyService;

        private ApplicationBarIconButton SaveAndCloseButton;
        private ApplicationBarIconButton saveAndContinue;
        private Category tempCategory;
        private ApplicationBarMenuItem toMainPageButton;
        private int totalDaysOfThisMonth;
        private Guid _specifiedAccountId;

        public AccountItem Current { get { return currentEditObject; } }
        private Account oldAccount;
        private Account _newAccount;

        public NewOrEditAccountItemPage()
        {
            this.accountItemViewModel = ViewModelLocator.AccountItemViewModel;
            this.actionType = PageActionType.Add;
            this.totalDaysOfThisMonth = 30;
            this.InitializeComponent();
            base.ApplicationBar.Opacity = 1.0;
            TiltEffect.SetIsTiltEnabled(this, true);
            this.categoryViewModel = ViewModelLocator.CategoryViewModel;
            base.Loaded += new RoutedEventHandler(this.NewOrEditAccountItemPage_Loaded);
            this.goToPriceInputBoxButton = new ApplicationBarIconButton(IconUirs.CalcutorIconButton);
            this.goToPriceInputBoxButton.Text = AppResources.Calcutor;
            this.goToPriceInputBoxButton.Click += new System.EventHandler(this.goToPriceInputBoxButton_Click);
            this.editingApplicationBarManager = new ApplicationBarHelper(this);
            this.editingApplicationBarManager.SelectContentWhenFocus = true;
            this.editingApplicationBarManager.AddTextBox(new TextBox[] { this.TotalMoneyBox, this.DescriptionTextBox });

            ViewModelLocator.AccountViewModel.DisableControl = () =>
            {
                this.ApplicationBar.IsMenuEnabled = false;

                if (saveAndContinue != null)
                {
                    this.saveAndContinue.IsEnabled = false;
                }

                if (SaveAndStay != null)
                {
                    this.SaveAndStay.IsEnabled = false;
                }
                if (SaveAndClose != null)
                {
                    this.SaveAndClose.IsEnabled = false;
                }
            };

            AccountName.SelectionChanged += AccountName_SelectionChanged;
            this.editingApplicationBarManager.If(p => p.Name == "TotalMoneyBox", p =>
            {
                goToPriceInputBoxButton.Click -= goToBuildAmountBoxButton_Click;
                goToPriceInputBoxButton.Click += goToPriceInputBoxButton_Click;
                p.Buttons.Add(this.goToPriceInputBoxButton);
            }, p =>
            {
                goToPriceInputBoxButton.Click -= goToPriceInputBoxButton_Click;
                p.Buttons.Remove(this.goToPriceInputBoxButton);
            })
                .If(p => p.Name == DescriptionTextBox.Name, p =>
                {
                    goToPriceInputBoxButton.Click -= goToPriceInputBoxButton_Click;
                    goToPriceInputBoxButton.Click += goToBuildAmountBoxButton_Click;
                    p.Buttons.Add(this.goToPriceInputBoxButton);
                }, p =>
                {
                    goToPriceInputBoxButton.Click -= goToBuildAmountBoxButton_Click;
                    p.Buttons.Remove(this.goToPriceInputBoxButton);
                });
        }

        /// <summary>
        /// Handles the Click event of the goToBuildAmountBoxButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void goToBuildAmountBoxButton_Click(object sender, EventArgs e)
        {
            MoneyBuildingPage.BuildingHandler.AfterSelected = (result) =>
            {
                this.DescriptionTextBox.Text = result.Key;

                this.AlertConfirm(AppResources.ConfirmAmountFromBuildingReplaceTotalAmount, () =>
                {
                    this.TotalMoneyBox.Text = result.Value.ToMoneyF2();
                });

            };

            MoneyBuildingPage.GoTo(this, this.MoneyCurrency.Text, this.DescriptionTextBox.Text);
        }

        private void AccountName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Account selectedItem = this.AccountName.SelectedItem as Account;
            if (selectedItem != null)
            {
                this.MoneyCurrency.Text = selectedItem.CurrencyType.GetCurrentString();

                this._newAccount = selectedItem;

                this.PaymentByInstalmentsSettingPanel.Visibility = selectedItem.IsCreditCard ? Visibility.Visible : System.Windows.Visibility.Collapsed;

                if (!selectedItem.IsCreditCard)
                {
                    this.PaymentByInstalmentsSettingCheckBox.IsChecked = false;
                }
            }
        }

        private void BindData(AccountItem item)
        {
            this.CategoryType.SelectedIndex = (item.Type == ItemType.Expense) ? 0 : 1;
            this.CategoryNameButton.DataContext = item.NameInfo;
            this.tempCategory = item.Category;
            this.oldAccount = item.Account;

            if (item.Type == ItemType.Expense)
            {
                this.IsClaim.IsChecked = new bool?(item.IsClaim.GetValueOrDefault());
            }

            this.MoneyCurrency.Text = item.Account.CurrencyType.GetCurrentString();
            this.TotalMoneyBox.Text = item.Money.ToMoneyF2();

            if (item.Account != null)
            {
                this.AccountName.SelectedItem = item.Account;
                this.PaymentByInstalmentsSettingPanel.Visibility = item.Account.IsCreditCard ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            }

            this.CreateDate.Value = new System.DateTime?(item.CreateTime);

            this.CreateTime.Value = item.CreateTime;

            this.DescriptionTextBox.Text = item.Description;
        }

        private void CategoryNameButton_Click(object sender, RoutedEventArgs e)
        {
            this.InitializeCategorySelectorDialogBox();
            CategoryManagment.Go(this, this.categorySelectorHandler, (ItemType)this.CategoryType.SelectedIndex, true);
        }

        private void CategoryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tempCategory = null;
            this.CategoryNameButton.DataContext = null;
            this.EnsureIsClaim();
        }

        private void ChooseCategoryFromFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.favouriteCategorySelectorDialogBox == null)
            {
                this.ShowTipsOnce("HasShowHowToAddCategoryToFavourite", "AddAsFavourite", null);
                this.favouriteCategorySelectorDialogBox = new FavouriteCategorySelector();
                this.favouriteCategorySelectorDialogBox.Title = LocalizedStrings.GetLanguageInfoByKey("ChooseCategoryPageTitle");
                this.favouriteCategorySelectorDialogBox.Closed += new System.EventHandler(this.favouriteCategorySelectorDialogBox_Closed);
            }
            this.favouriteCategorySelectorDialogBox.ItemType = (ItemType)this.CategoryType.SelectedIndex;
            this.favouriteCategorySelectorDialogBox.Show();
        }

        private void DeleteItemButton_Click(object sender, System.EventArgs e)
        {
        }

        private void DescriptionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DescriptionTextBox_LostFocus(sender, e);
            }
        }

        private void DescriptionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            base.Focus();
            base.ApplicationBar = this.funcBar;
        }

        private bool DoSaveBudget()
        {
            if (this.tempCategory == null)
            {
                this.AlertNotification(AppResources.RequestCategoryMessage, null);
                return false;
            }
            decimal result = 0.0M;
            if (this.TotalMoneyBox.Text.Trim().IsNullOrEmpty() || !decimal.TryParse(this.TotalMoneyBox.Text, out result))
            {
                this.AlertNotification(AppResources.RequestMoneyAvailableMessage, null);
                return false;
            }
            Account selectedItem = (Account)this.AccountName.SelectedItem;
            if (selectedItem == null)
            {
                this.AlertNotification(AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { this.AccountName.Header.ToString() }), null);
                return false;
            }

            bool flag = !AppSetting.Instance.EnableAllAccountOverdraft;
            if (((!IsSelectionMode && (this.CategoryType.SelectedIndex == 0)) && (!selectedItem.EnableOverdraft && ((selectedItem.Balance.GetValueOrDefault() - result) <= 0.0M)))
                && flag)
            {
                this.AlertNotification(AppResources.EnableUsingAccountWhenItHasNegativeValueMessage, null);
                return false;
            }

            if ((this.actionType == PageActionType.Add) || IsSelectionMode)
            {
                if (!IsSelectionMode)
                {
                    App.LastAccessTime = new System.DateTime?(System.DateTime.Now);
                }

                return this.SubmitAddData(result);
            }

            this.SubmitEditData(result);
            return true;
        }

        private void DownAmountButton_Click(object sender, RoutedEventArgs e)
        {
            double result = 0.0;
            double.TryParse(this.TotalMoneyBox.Text, out result);
            result--;
            if (result <= 0.0)
            {
                result = 0.0;
            }
            this.TotalMoneyBox.Text = result.ToString("#0.00");
        }

        private void EnsureEnablePinToStart()
        {
            if (this.pintToStart == null)
            {
                this.SetControlLocalizedString();
            }
            this.pintToStart.IsEnabled = !TinyMoneyManager.Component.ShellTileHelper.IsPinned(pinToStartLink);
        }

        private void EnsureIsClaim()
        {
            this.IsClaim.IsEnabled = this.CategoryType.SelectedIndex == 0;
        }

        private void favouriteCategorySelectorDialogBox_Closed(object sender, System.EventArgs e)
        {
            if (this.favouriteCategorySelectorDialogBox.SelectedItem != null)
            {
                this.tempCategory = this.favouriteCategorySelectorDialogBox.SelectedItem;
                this.CategoryNameButton.DataContext = this.favouriteCategorySelectorDialogBox.SelectedItem;

                if (this.Current != null && this.Current.IsInstallmentsItem.GetValueOrDefault())
                {
                }
                else
                {
                    this.TotalMoneyBox.Text = this.tempCategory.DefaultAmountString;
                }
            }
        }

        private ItemType GetItemType()
        {
            return ((this.CategoryType.SelectedIndex == 0) ? ItemType.Expense : ItemType.Income);
        }

        public string GetPinToStartLink()
        {
            return GetPinToStartLink(this.CategoryType.SelectedIndex, (this.AccountName.SelectedItem == null) ? string.Empty : (this.AccountName.SelectedItem as Account).Id.ToString(), (this.tempCategory == null) ? string.Empty : this.tempCategory.Id.ToString());
        }

        public static string GetPinToStartLink(int itemTypeIndex, string accountId, string categoryId)
        {
            return pinToStartLink;
        }

        private void goToBorrowLoanPage_Click(object sender, System.EventArgs e)
        {
            BorrowLeanEditor.GetEditObject = null;
            this.NavigateTo("/Pages/BorrowAndLean/BorrowLeanEditor.xaml");
        }

        private void goToPriceInputBoxButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo(Calcutor.PageRef, new object[] { this.TotalMoneyBox.Text });
            AmountInputBox.Confirmed = delegate(object s, double v)
            {
                this.TotalMoneyBox.Text = v.ToString("F2", LocalizedStrings.CultureName);
            };
        }

        private void goToTransferingPageButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/AccountTransferingPage.xaml");
        }

        private bool InCurrentMonth(AccountItem accountItem)
        {
            return accountItem.CreateTime.AtSameYearMonth(currentEditObject.CreateTime);
        }

        protected void InitializeAddData()
        {
            this.NewOrEditPage.Text = AppResources.AddRecordName.ToUpperInvariant();
            this.CategoryNameButton.DataContext = null;
            this.CreateDate.Visibility = IsSelectionMode ? Visibility.Collapsed : Visibility.Visible;
            if (!IsSelectionMode)
            {
                ApplicationBarMenuItem item = new ApplicationBarMenuItem(AppResources.TransferingAccount);
                item.Click += new System.EventHandler(this.goToTransferingPageButton_Click);
                this.funcBar.MenuItems.Add(item);
                ApplicationBarMenuItem item2 = new ApplicationBarMenuItem(AppResources.BorrowAndLean);
                item2.Click += new System.EventHandler(this.goToBorrowLoanPage_Click);
                this.funcBar.MenuItems.Add(item2);
            }
        }

        private PageActionHandler<Category> InitializeCategorySelectorDialogBox()
        {
            System.Action<Category> action = null;
            System.Func<Category> func = null;
            if (this.categorySelectorHandler == null)
            {
                this.categorySelectorHandler = new PageActionHandler<Category>();
                if (action == null)
                {
                    action = delegate(Category item)
                    {
                        if (item != null)
                        {
                            this.CategoryNameButton.DataContext = item.CategoryInfo;
                            this.tempCategory = item;
                            if (this.actionType == PageActionType.Add)
                            {
                                this.TotalMoneyBox.Text = item.DefaultAmountString;
                            }
                        }
                    };
                }
                this.categorySelectorHandler.AfterSelected = action;
                if (func == null)
                {
                    func = () => this.tempCategory;
                }
                this.categorySelectorHandler.GetSelected = func;
            }
            return this.categorySelectorHandler;
        }

        protected void InitializeEditData(System.Guid id)
        {
            this.initializeEditingMode();
        }

        private void initializeEditingMode()
        {
            this.NewOrEditPage.Text = AppResources.Edit.ToUpperInvariant();
            this.BindData(IsSelectionMode ? accountItemForSchedule : currentEditObject);
            this.CategoryType.IsEnabled = IsSelectionMode;

            this.ApplicationBar.GetIconButtonFrom(0)
                .IsEnabled = false;

            this.CreateDate.Visibility = IsSelectionMode ? Visibility.Collapsed : Visibility.Visible;
            this.CreateTime.Visibility = this.CreateDate.Visibility;
        }

        private void InitializeSafeService()
        {
            if (this.safetyService == null)
            {
                this.safetyService = this.RegisterNeedInputPasswordService(delegate
                {
                    this.ContentGrid.Visibility = Visibility.Collapsed;
                    if (base.ApplicationBar != null)
                    {
                        base.ApplicationBar.IsVisible = false;
                    }
                }, delegate(string msg)
                {
                    this.ContentGrid.Visibility = Visibility.Visible;
                    this.ContentGrid.Children.Clear();
                    if (this.funcBar != null)
                    {
                        this.funcBar.IsVisible = false;
                    }
                    this.saveAndContinue.IsEnabled = false;
                    this.SaveAndCloseButton.IsEnabled = false;
                    TextBlock block = new TextBlock
                    {
                        Margin = new Thickness(12.0, 20.0, 2.0, 0.0),
                        FontSize = 48.0,
                        TextWrapping = TextWrapping.Wrap,
                        Text = msg
                    };
                    this.ContentGrid.Children.Add(block);
                }, delegate
                {
                    if (this.funcBar != null)
                    {
                        this.funcBar.IsVisible = true;
                    }
                    this.ContentGrid.Visibility = Visibility.Visible;
                    App.HasLogined = true;
                });
            }
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            this.MoreInfoPanel.Visibility = System.Windows.Visibility.Visible;
            this.MoreInfoButton.Visibility = Visibility.Collapsed;
            this.EnsureIsClaim();
        }

        private void NewOrEditAccountItemPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.isDataNavigated)
            {
                if (!ViewModelLocator.AccountViewModel.IsDataLoaded)
                {
                    ViewModelLocator.AccountViewModel.QuickLoadData();
                }

                this.totalDaysOfThisMonth = System.DateTime.Now.GetCountDaysOfMonth();
                this.SetControlLocalizedString();
                this.AccountName.ItemsSource = ViewModelLocator.AccountViewModel.Accounts;
                if (base.NavigationContext.QueryString.ContainsKey("action"))
                {
                    string str = base.NavigationContext.QueryString["action"];

                    if (!this.hasNavied)
                    {
                        if ((str == "Edit") || (IsSelectionMode && (accountItemForSchedule != null)))
                        {
                            this.actionType = PageActionType.Edit;
                            //this.CategoryType.BorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);
                            this.initializeEditingMode();
                            this.DataContext = currentEditObject;
                        }
                        else
                        {

                            var defaultAccount = ViewModelLocator.AccountViewModel.DefaultAccountForEditing;

                            if (this._specifiedAccountId != Guid.Empty)
                            {
                                defaultAccount = ViewModelLocator.AccountViewModel.Accounts.FirstOrDefault(p => p.Id == _specifiedAccountId);
                            }

                            if (defaultAccount == null)
                            {
                                defaultAccount = ViewModelLocator.AccountViewModel.Accounts.FirstOrDefault();
                            }

                            if (defaultAccount != null)
                            {
                                this.AccountName.SelectedItem = defaultAccount;
                            }

                            if (base.NavigationContext.QueryString.ContainsKey("categoryType"))
                            {
                                int num = int.Parse(base.NavigationContext.QueryString["categoryType"]);
                                this.CategoryType.SelectedIndex = num;
                            }

                            this.actionType = PageActionType.Add;
                            if (str != "preEdit")
                            {
                                currentEditObject = new AccountItem();
                            }
                            base.DataContext = currentEditObject;
                            this.InitializeAddData();
                            if (str == "preEdit")
                            {
                                this.BindData(currentEditObject);
                            }
                        }
                        if (str == "selection")
                        {
                            string source = this.GetNavigatingParameter("title", null);
                            this.NewOrEditPage.Text = source.IsNullOrEmpty() ? AppResources.CreateAccountItemForSchedule.ToUpperInvariant() : source;
                        }

                        this.SetControls();
                        this.hasNavied = true;
                    }
                }

                this.isFromList = this.GetNavigatingParameter("isFromList", null).ToBoolean(false);
                this.fromMainPage = this.GetNavigatingParameter("FromMainPlus", null).ToBoolean(false);
                if (this.FromStartScreen)
                {
                    ApplicationBarMenuItem item = new ApplicationBarMenuItem
                    {
                        Text = LocalizedStrings.GetLanguageInfoByKey("GoToMainPage")
                    };
                    this.toMainPageButton = item;
                    this.toMainPageButton.Click += new System.EventHandler(this.toMainPageButton_Click);
                    base.ApplicationBar.MenuItems.Add(this.toMainPageButton);
                }
            }


            if (!this.hasLoaded)
            {
                if (this.hasLoaded)
                {
                    this.hasLoaded = true;
                }
                else
                {
                    bool fromStartScreen = this.FromStartScreen;
                    if (!this.isDataNavigated)
                    {
                        this.CategoryType.SelectionChanged += new SelectionChangedEventHandler(this.CategoryType_SelectionChanged);
                        this.isDataNavigated = true;
                    }
                    this.hasLoaded = true;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (this.FromStartScreen && (AppSetting.Instance.ActionAfterAddNewRecord == AfterAddNewRecordAction.TurnToHistoryPage))
            {
                ViewModelLocator.MainPageViewModel.LoadSummaryForAgent();
            }
            if (this.needAlertNotice)
            {
                this.AlertNotification(AppResources.OperationSuccessfullyMessage, null);
                this.needAlertNotice = false;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.EnsureEnablePinToStart();

            if (e.NavigationMode != NavigationMode.Back)
            {
                this._specifiedAccountId = e.GetNavigatingParameter("specifiedAccount")
                    .ToGuid();
            }

            base.OnNavigatedTo(e);

        }

        private void PeopleTotalInfoTakerButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.peopleHandler == null)
            {
                this.peopleHandler = new PageActionHandler<PeopleAssociationData>();

                this.peopleHandler.AfterAdd = (i) =>
                    {
                        if (!IsSelectionMode)
                        {
                            currentEditObject.Peoples.Add(i);

                            if (this.actionType == PageActionType.Edit)
                            {
                                this.accountItemViewModel.Update(currentEditObject);
                            }
                        }
                        else
                        {
                            //...
                            //i.Tag = PictureInfo.ScheduledAccountItemsTag;
                            i.AttachedId = currentEditObject.AutoTokenId;
                            // record pictures added.
                            currentEditObject.AddPeople(i);

                            if (actionType == PageActionType.Edit)
                            {
                                ViewModelLocator.PeopleViewModel.InsertAndSubmit(i);
                            }
                        }
                    };


                this.peopleHandler.Update = (i) =>
                    {
                        if (actionType == PageActionType.Edit)
                        {
                            if (IsSelectionMode)
                            {
                                var peopleInfo = ViewModelLocator.AccountBookDataContext.PeopleAssociationDatas.FirstOrDefault(p => p.Id == i.Id);

                                if (peopleInfo != null)
                                {
                                    peopleInfo.Comments = i.Comments;

                                    ViewModelLocator.PeopleViewModel.Update<PeopleAssociationData>(peopleInfo);
                                }
                            }
                            else
                            {
                                this.accountItemViewModel.Update(currentEditObject);
                            }
                        }
                    };

                this.peopleHandler.AfterDelete = (i) =>
                    {
                        if (!IsSelectionMode)
                        {
                            currentEditObject.Peoples.Remove(i);

                            if (this.actionType == PageActionType.Edit)
                            {
                                this.accountItemViewModel.Update(currentEditObject);
                            }
                        }

                        else
                        {
                            // record pictures removed.
                            currentEditObject.RemovePeople(i);

                            if (actionType == PageActionType.Edit)
                            {
                                //...
                                ViewModelLocator.PeopleViewModel.Delete(i);
                            }
                        }
                    };

                this.peopleHandler.GetItems = () =>
                {
                    if (IsSelectionMode)
                    {
                        return currentEditObject.PeoplesOutOfDatabase;
                    }
                    else
                    {
                        return currentEditObject.Peoples;
                    }
                };

                AttachPeoplePage.PeopleSelectorHandler = this.peopleHandler;
            }

            if (IsSelectionMode)
            {
                AttachPeoplePage.Go(currentEditObject.AutoTokenId, this);
            }
            else
            {
                AttachPeoplePage.Go(currentEditObject.Id, this);
            }
        }

        private void PictureTotalInfoTakerButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.picHandler == null)
            {
                this.picHandler = new PictureActionHandler();

                this.picHandler.AfterAdd = (i) =>
                    {
                        if (!IsSelectionMode && this.actionType == PageActionType.Edit)
                        {
                            ViewModelLocator.PicturesViewModel.SavePicture(new PictureInfo[] { i });
                        }

                        if (!IsSelectionMode)
                        {
                            currentEditObject.Pictures.Add(i);
                        }

                        if (!IsSelectionMode && this.actionType == PageActionType.Edit)
                        {
                            this.accountItemViewModel.Update(currentEditObject);
                        }

                        if (IsSelectionMode)
                        {
                            //...
                            i.Tag = PictureInfo.ScheduledAccountItemsTag;
                            i.AttachedId = currentEditObject.AutoTokenId;
                            // record pictures added.
                            currentEditObject.AddPicture(i);

                            if (actionType == PageActionType.Edit)
                            {
                                ViewModelLocator.PicturesViewModel.SavePicture(i);
                                ViewModelLocator.PicturesViewModel.InsertAndSubmit(i);
                            }
                        }
                    };

                this.picHandler.Update = (i) =>
                    {
                        if (this.actionType == PageActionType.Edit)
                        {
                            if (IsSelectionMode)
                            {
                                // update pic info
                                var pic = ViewModelLocator.PicturesViewModel.AccountBookDataContext.PictureInfos.FirstOrDefault(p => p.Id == i.Id);

                                if (pic != null)
                                {
                                    pic.Comments = i.Comments;
                                    ViewModelLocator.PicturesViewModel.Update(pic);
                                }

                            }
                            else
                            {
                                this.accountItemViewModel.Update(currentEditObject);
                            }
                        }
                    };

                this.picHandler.AfterDelete = (i) =>
                {
                    if (!IsSelectionMode)
                    {
                        currentEditObject.Pictures.Remove(i);
                    }

                    if (this.actionType == PageActionType.Edit && !IsSelectionMode)
                    {
                        ViewModelLocator.PicturesViewModel.DeletePicture(i);
                        this.accountItemViewModel.Update(currentEditObject);
                    }

                    if (IsSelectionMode)
                    {
                        // record pictures removed.
                        currentEditObject.RemovePicture(i);

                        if (actionType == PageActionType.Edit)
                        {
                            //...
                            ViewModelLocator.PicturesViewModel.DeletePicture(i);
                        }
                    }
                };
                this.picHandler.GetItems = () =>
                {
                    if (IsSelectionMode)
                    {
                        return currentEditObject.PicturesOutOfDatabase;
                    }
                    else
                    {
                        return currentEditObject.Pictures;
                    }
                };

                PictureBrowser.PictureHandler = this.picHandler;
            }
            if (currentEditObject != null)
            {
                if (IsSelectionMode)
                {
                    PictureBrowser.Go(currentEditObject.AutoTokenId, PictureInfo.ScheduledAccountItemsTag, this);
                }
                else
                {
                    PictureBrowser.Go(currentEditObject.Id, PictureInfo.AccountItemsTag, this);
                }
            }
        }

        private void pintToStart_Click(object sender, System.EventArgs e)
        {
            if (this.AlertConfirm(LocalizedStrings.GetLanguageInfoByKey("PinQuickAccountItemEditorPageToStart"), null, null) == MessageBoxResult.OK)
            {
                TileInfoUpdatingAgent.CreateShellTile(App.AppName, string.Empty, LocalizedStrings.GetLanguageInfoByKey("QuickAddBudgetItemTips"), string.Empty, "QuickNewRecordBackgroundImage.png");
                Uri newRecordStartScreenBackgroundImageLink = NewRecordStartScreenBackgroundImageLink;
                StandardTileData data2 = new StandardTileData
                {
                    BackContent = string.Empty,
                    BackgroundImage = newRecordStartScreenBackgroundImageLink
                };
                StandardTileData initialData = data2;
                initialData.Title = LocalizedStrings.GetLanguageInfoByKey("AddRecordName");
                TinyMoneyManager.Component.ShellTileHelper.Pin(NewRecordStartScreenLink, initialData);
            }
        }

        /// <summary>
        /// Registers the selection mode.
        /// </summary>
        /// <param name="accountItem">The account item.</param>
        /// <param name="afterCreated">The after created.</param>
        public static void RegisterSelectionMode(AccountItem accountItem, System.Action<AccountItem> afterCreated)
        {
            if (!IsSelectionMode)
            {
                IsSelectionMode = true;
            }
            accountItemForSchedule = accountItem;
            currentEditObject = accountItemForSchedule;
            AfterCreated = afterCreated;
        }

        private void SaveAndClose_Click(object sender, System.EventArgs e)
        {
            if (this.DoSaveBudget())
            {
                if (!IsSelectionMode)
                {
                    this.ToMainPage(false);
                }
                else
                {
                    AfterCreated(currentEditObject);
                    if (base.NavigationService.CanGoBack)
                    {
                        base.NavigationService.GoBack();
                    }
                }
            }
        }

        private void SaveAndStay_Click(object sender, System.EventArgs e)
        {
            ToastPrompt prompt = new ToastPrompt
            {
                MillisecondsUntilHidden = 0x3e8,
                Title = ""
            };
            if (this.DoSaveBudget())
            {
                prompt.Message = AppResources.OperationSuccessfullyMessage;
                if (this.actionType == PageActionType.Add)
                {
                    this.DescriptionTextBox.Text = string.Empty;
                }
                prompt.Show();
                currentEditObject = new AccountItem();
            }
        }

        private void SetControlLocalizedString()
        {
            if (!this.hasSetControlLocalizedString)
            {
                this.hasSetControlLocalizedString = true;
                this.SaveAndCloseButton = base.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                this.SaveAndCloseButton.Text = AppResources.SaveAndClose;
                this.saveAndContinue = base.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                this.saveAndContinue.Text = AppResources.SaveAndContinue;
                this.pintToStart = new ApplicationBarMenuItem();
                this.pintToStart.Text = AppResources.PinToStart;
                this.pintToStart.Click += new System.EventHandler(this.pintToStart_Click);
                ApplicationBarMenuItem item = new ApplicationBarMenuItem(AppResources.PreferenceSetting);
                item.Click += delegate(object o, System.EventArgs e)
                {
                    this.NavigateTo("/Pages/AppSettingPage/PreferenceSettingPage.xaml?goto={0}", new object[] { 1 });
                };
                base.ApplicationBar.MenuItems.Add(this.pintToStart);
                base.ApplicationBar.MenuItems.Add(item);
                item.IsEnabled = !IsSelectionMode;
                this.saveAndContinue.IsEnabled = !IsSelectionMode;
                this.pintToStart.IsEnabled = !IsSelectionMode;
                this.PictureTotalInfoTakerButton.IsEnabled = CanLinkPictureAndPeople;
                this.PeopleTotalInfoTakerButton.IsEnabled = CanLinkPictureAndPeople;
                this.funcBar = base.ApplicationBar;
            }
        }

        private void SetControls()
        {
            PaymentByInstalmentsSettingResultPanel.DataContext = this.Current;
        }

        public string ShowPeopleNameAfterSeleted(System.Collections.IList people)
        {
            if (people == null)
            {
                return string.Empty;
            }
            return (from p in people.OfType<PeopleProfile>() select p.Name).ToStringLine<string>(",");
        }

        protected bool SubmitAddData(decimal money)
        {
            AccountItem currentEditObject = NewOrEditAccountItemPage.currentEditObject;

            if (currentEditObject != null && !currentEditObject.IsInstallmentsItem.GetValueOrDefault()
                && !currentEditObject.HasInstalmentInformation)
            {
                currentEditObject.Money = money;
            }

            currentEditObject.Category = this.tempCategory;
            currentEditObject.CategoryId = this.tempCategory.Id;

            if (!currentEditObject.IsInstallmentsItem.GetValueOrDefault())
            {
                currentEditObject.Account = this.AccountName.SelectedItem as Account;
                if ((currentEditObject.Account == null) && (this.AccountName.Items.Count > 0))
                {
                    currentEditObject.Account = this.AccountName.Items[0] as Account;
                }
                if (currentEditObject.Account == null)
                {
                    this.AlertNotification(AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { this.AccountName.Header }), null);
                    return false;
                }
                currentEditObject.AccountId = currentEditObject.Account.Id;
            }
            else
            {
                currentEditObject.Account = _newAccount;
            }

            currentEditObject.IsClaim = new bool?(this.IsClaim.IsChecked.GetValueOrDefault());
            currentEditObject.Type = (this.CategoryType.SelectedIndex == 0) ? ItemType.Expense : ItemType.Income;
            currentEditObject.Description = this.DescriptionTextBox.Text.Trim();

            currentEditObject.CreateTime = getCreateTime();

            if (!IsSelectionMode)
            {
                ViewModelLocator.PicturesViewModel.SavePictures(currentEditObject.Pictures);

                ViewModelLocator.AccountViewModel.HandleAccountItemAdding(currentEditObject);
            }

            return true;
        }

        private DateTime getCreateTime()
        {

            var date = this.CreateDate.Value.GetValueOrDefault()
                .Date;

            var time = this.CreateTime.Value.GetValueOrDefault();

            return date.AddHours(time.Hour).AddMinutes(time.Minute).AddSeconds(time.Second);
        }

        protected void SubmitEditData(decimal money)
        {
            decimal oldMoney = currentEditObject.Money;
            decimal oldBalance = money - oldMoney;

            if (currentEditObject != null && !currentEditObject.IsInstallmentsItem.GetValueOrDefault()
                && !currentEditObject.HasInstalmentInformation)
            {
                currentEditObject.Money = money;
            }

            currentEditObject.Category = this.tempCategory;
            currentEditObject.Type = this.GetItemType();
            currentEditObject.Description = this.DescriptionTextBox.Text.Trim();

            if (!currentEditObject.IsInstallmentsItem.GetValueOrDefault())
            {
                currentEditObject.Account = this.AccountName.SelectedItem as Account;
            }
            else
            {
                currentEditObject.Account = _newAccount;
            }

            currentEditObject.IsClaim = new bool?(this.IsClaim.IsChecked.GetValueOrDefault());
            if (!IsSelectionMode)
            {
                bool needUpdateDate = false;
                System.DateTime createTime = currentEditObject.CreateTime;
                System.DateTime valueOrDefault = getCreateTime();

                needUpdateDate = valueOrDefault.Date != currentEditObject.CreateTime.Date;

                currentEditObject.CreateTime = valueOrDefault;

                ViewModelLocator.AccountViewModel.HandleAccountItemEditing(currentEditObject, oldAccount, oldMoney, oldBalance);

                if (needUpdateDate)
                {
                    ViewModelLocator.AccountItemViewModel.UpdateItemByDate(currentEditObject, createTime);
                }
            }
        }

        private void TextBoxFocus(object sender, RoutedEventArgs e)
        {
            base.ApplicationBar = this.editingApplicationBarManager.ApplicationBarForEditor;
        }

        public void ToMainPage(bool fromSave = false)
        {
            if (this.isFromList)
            {
                AccountItemListPage.fromAddOrEdit = true;
                if (base.NavigationService.CanGoBack)
                {
                    base.NavigationService.GoBack();
                }
            }
            else if (!this.FromStartScreen && (AppSetting.Instance.ActionAfterAddNewRecord == AfterAddNewRecordAction.BackOrExit))
            {
                this.needAlertNotice = true;
                this.SafeGoBack();
            }
            else
            {
                AccountItemListPage.fromAddOrEdit = true;
                bool fromStartScreen = this.FromStartScreen;
                bool fromTemplete = this.FromTemplete;
                if (this.FromTemplete)
                {
                    this.fromMainPage = true;
                    fromStartScreen = true;
                }
                this.NavigateTo("/Pages/AccountItemListPage.xaml?FromMainPlus={0}&fromStartScreen={1}&removeTwo={2}", new object[] { this.fromMainPage, fromStartScreen, fromTemplete });
            }
        }

        private void toMainPageButton_Click(object sender, System.EventArgs e)
        {
            hasCheckFromQuickNewRecordToMainPage = false;
            this.NavigateTo("/MainPage.xaml?FromMainPlus={0}&fromStartScreen={1}", new object[] { this.fromMainPage, this.FromStartScreen });
        }

        private void TotalMoneyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (AppSetting.Instance.AlertWhenBudgetIsOver && (this.tempCategory != null))
            {
                BudgetItem item = ViewModelLocator.BudgetProjectViewModel.AccountBookDataContext.BudgetItems.FirstOrDefault<BudgetItem>(p => p.BudgetTargetId == this.tempCategory.Id);
                if (item != null)
                {
                    decimal num = item.Amount / ((((DuringMode)item.SettleType) == DuringMode.ByMonth) ? 1 : this.totalDaysOfThisMonth);
                    decimal num2 = this.TotalMoneyBox.Text.ToDecimal();
                    if (num2 > num)
                    {
                        this.AlertNotification(AppResources.MessageWhenAmountForOneCategoryOverBudgetItem.FormatWith(new object[] { (num2 - num).ToMoneyF2() }), null);
                    }
                }
            }
        }

        private void UpAmountButton_Click(object sender, RoutedEventArgs e)
        {
            double result = 0.0;
            double.TryParse(this.TotalMoneyBox.Text, out result);
            if (result <= 0.0)
            {
                result = 0.0;
            }
            this.TotalMoneyBox.Text = (result + 1.0).ToString("#0.00");
        }

        public static void UpdateTileForNewRecord()
        {
            if (TinyMoneyManager.Component.ShellTileHelper.IsPinned(pinToStartLink))
            {
                StandardTileData data = ViewModelLocator.MainPageViewModel.CreateShellTileData();
                data.BackTitle = App.AppName;
                data.BackgroundImage = NewRecordStartScreenBackgroundImageLink;
                data.Title = LocalizedStrings.GetLanguageInfoByKey("AddRecordName");
                TinyMoneyManager.Component.ShellTileHelper.GetAppTile(NewRecordStartScreenLink).Update(data);
            }
        }

        private void Yesterday_Click(object sender, RoutedEventArgs e)
        {
        }

        private static AccountItem accountItemForSchedule
        {
            get;
            set;
        }

        public bool FromStartScreen
        {
            get
            {
                return this.GetNavigatingParameter("fromStartScreen", null).ToBoolean(false);
            }
        }

        public bool FromTemplete
        {
            get
            {
                return (this.GetNavigatingParameter("action", null) == "preEdit");
            }
        }

        public static bool hasCheckFromQuickNewRecordToMainPage
        {
            get;
            set;
        }

        public static bool IsSelectionMode
        {
            get;
            set;
        }

        public static ScheduleRecordType SelectionObjectType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance can link picture and people.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can link picture and people; otherwise, <c>false</c>.
        /// </value>
        public bool CanLinkPictureAndPeople
        {
            get
            {
                if (IsSelectionMode)
                {
                    return SelectionObjectType == ScheduleRecordType.ScheduledRecord;
                }

                return true;
            }
        }

        private void BaymentByInstalmentsSetting_Click_1(object sender, RoutedEventArgs e)
        {
            Pages.AccountItemViews.InstallmentsItemEditor.Go(this, this.Current);
        }

        private void PaymentByInstalmentsSettingCheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (this.Current != null)
            {
                this.Current.IsInstallmentsItem = true;
            }
        }

        private void PaymentByInstalmentsSettingCheckBox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (this.Current != null)
            {
                this.Current.IsInstallmentsItem = false;
            }
        }

    }

}