namespace TinyMoneyManager.Pages.AccountItemViews
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages;
    using TinyMoneyManager.Pages.CategoryManager;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.Pages.DialogBox.PictureManager;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.ViewModels.AccountItemManager;

    public partial class AccountItemViewer : PhoneApplicationPage, INotifyPropertyChanged
    {
        public AccountItemViewerViewModel accountItemViewerViewModel;
        public AccountItemListViewModel accountItemViewModel;

        public static System.Action<AccountItem> DeleteItemHandler;


        private AccountItem nextOne;
        private ApplicationBarIconButton nextOneItemButton;
        private PageActionHandler<PeopleAssociationData> peopleHandler;
        private PictureActionHandler picHandler;

        private AccountItem prevOne;
        private ApplicationBarIconButton prevOneItemButton;

        public System.Action resetAction;
        private System.Func<Guid, DateTime, AccountItem> sameTypeItemListGetter;
        private string stasticItemsTips;

        public event PropertyChangedEventHandler PropertyChanged;

        public AccountItemViewer()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.accountItemViewModel = ViewModelLocator.AccountItemViewModel;
            base.Loaded += new RoutedEventHandler(this.AccountItemViewer_Loaded);
            this.NeedReloadData = true;
            this.accountItemViewerViewModel = new AccountItemViewerViewModel();

            this.RelatedItemsPivot.DataContext = this;

            var textTitle = new TextBlock();
            textTitle.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("CreateDateInfo"));
            textTitle.Tap += textTitle_Tap;
            MainPivot.Title = textTitle;
        }

        void textTitle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // change date;
        }

        private void AccountItemViewer_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void AccountNameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            AccountInfoViewer.CurrentAccountGetter = this.CurrentObject.Account;
            this.NavigateTo("/Pages/DialogBox/AccountInfoViewer.xaml?action=view&id={0}&to=1&fromSelf=true", new object[] { this.CurrentObject.Account.Id });
        }

        private void AmountValueEditor_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.Amount, this.CurrentObject.Money.ToMoneyF2(), delegate(TextBox t)
            {
                t.SelectAll();
                t.InputScope = MoneyInputTextBox.NumberInputScope;
            }, s => s.ToDecimal(delegate
            {
                this.AlertNotification(AppResources.AmountShouldBeGreatThanZeroMessage, null);
                return -9999999999.01M;
            }) > -9999999999.01M, delegate(string s)
            {
                decimal oldMoney = this.CurrentObject.Money;
                decimal num2 = s.ToDecimal();
                decimal oldBalance = num2 - oldMoney;
                this.CurrentObject.Money = num2;
                ViewModelLocator.AccountViewModel.HandleAccountItemEditing(this.CurrentObject, this.CurrentObject.Account, oldMoney, oldBalance);
            });
        }

        private void ByInstalmentsEditor_Click(object sender, RoutedEventArgs e)
        {
            Pages.AccountItemViews.InstallmentsItemEditor.Go(this, this.CurrentObject);
        }

        private void DeleteItemButton_Click(object sender, System.EventArgs e)
        {
            if (this.AlertConfirm(AppResources.DeleteAccountItemMessage, null, null) == MessageBoxResult.OK)
            {
                if (DeleteItemHandler == null)
                {
                    DeleteItemHandler = new System.Action<AccountItem>(ViewModelLocator.AccountViewModel.HandleAccountItemDeleting);
                }
                if (DeleteItemHandler != null)
                {
                    DeleteItemHandler(this.CurrentObject);
                }
                this.GoBack();
            }
        }

        private void EditMenuItem_Click(object sender, System.EventArgs e)
        {
            this.NeedReloadData = true;
            NewOrEditAccountItemPage.currentEditObject = this.CurrentObject;
            this.NavigateTo("/Pages/NewOrEditAccountItemPage.xaml?action={0}&isFromList=true", new object[] { PageActionType.Edit });
        }

        private void GoBack()
        {
            base.NavigationService.GoBack();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryInfoViewer.Go(this.CurrentObject.CategoryId, this);
        }

        private void ChangeDateButton_Click(object sender, RoutedEventArgs e)
        {
            //var uri = CreateDatePicker.PickerPageUri;
            //this.NavigationService.Navigate(uri);
        }

        private void IndicateNavigatingItem(int target)
        {
            base.Dispatcher.BeginInvoke(delegate
            {
                System.Guid currentId = this.CurrentObject.Id;
                System.DateTime date = this.CurrentObject.CreateTime.Date;
                ItemType type = this.CurrentObject.Type;
                if ((target == -1) || (target == 0))
                {
                    this.prevOne = this.accountItemViewModel.GetItemByPosition(-1, currentId, date, type);
                    this.prevOneItemButton.IsEnabled = this.prevOne != null;
                }
                if ((target == 1) || (target == 0))
                {
                    this.nextOne = this.accountItemViewModel.GetItemByPosition(1, currentId, date, type);
                    this.nextOneItemButton.IsEnabled = this.nextOne != null;
                }
            });
        }

        private void InitializeMenuItemText()
        {
            base.ApplicationBar.GetIconButtonFrom(0).Text = this.GetLanguageInfoByKey("Edit");
            base.ApplicationBar.GetIconButtonFrom(1).Text = this.GetLanguageInfoByKey("Delete");

            this.IncomeOrExpenseDetailsPivot.Header = LocalizedStrings.GetLanguageInfoByKey("{0}{1}", new string[] { this.CurrentObject.Type.ToString(), "Details" });

            return;
            this.nextOneItemButton = new ApplicationBarIconButton(new Uri("/icons/appBar/appbar.next.rest.png", UriKind.Relative));
            this.nextOneItemButton.Text = this.GetLanguageInfoByKey("NextItem");
            this.nextOneItemButton.IsEnabled = false;
            this.nextOneItemButton.Click += new System.EventHandler(this.NextOneButton_Click);
            this.prevOneItemButton = new ApplicationBarIconButton(new Uri("/icons/appBar/appbar.back.rest.png", UriKind.Relative));
            this.prevOneItemButton.Text = this.GetLanguageInfoByKey("PrevItem");
            this.prevOneItemButton.IsEnabled = false;
            this.prevOneItemButton.Click += new System.EventHandler(this.PrevOneButton_Click);
            base.ApplicationBar.Buttons.Add(this.prevOneItemButton);
            base.ApplicationBar.Buttons.Add(this.nextOneItemButton);
        }

        private void LoadRelatedItems()
        {
            if (this.NeedReloadData)
            {
                this.NeedReloadData = false;
                this.BusyForWork(AppResources.Loading);
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    int totalRecords = 0;
                    decimal sumOfAmount = 0.0M;
                    System.Action<AccountItem> itemAdded = delegate(AccountItem ai)
                    {
                        totalRecords++;
                        sumOfAmount += ai.GetMoney().GetValueOrDefault();
                    };
                    System.Collections.Generic.List<GroupByCreateTimeAccountItemViewModel> data = this.accountItemViewerViewModel.GetGroupedRelatedItems(this.CurrentObject, true, itemAdded);
                    string str = AccountItemMoney.GetMoneyInfoWithCurrency(AppSetting.Instance.CurrencyInfo.CurrencyString, new decimal?(sumOfAmount), "{0}{1}");
                    string sumText = AppResources.RecordsAndAmountInfoForAccountRealtedItems.FormatWith(new object[] { totalRecords, LocalizedObjectHelper.GetLocalizedStringFrom(this.CurrentObject.Type.ToString()), str });
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        this.RelatedItemsListControl.ItemsSource = data;
                        this.StasticItemsTips = sumText;
                        this.WorkDone();
                    });
                });
            }
        }

        private void NextOneButton_Click(object sender, System.EventArgs e)
        {
            if (this.nextOne != null)
            {
                this.CurrentObject = this.nextOne;
                this.IndicateNavigatingItem(0);
            }
        }

        private void NotesInfoEditorButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.Notes, this.CurrentObject.Description, delegate(TextBox t)
            {
                t.SelectAll();
            }, null, delegate(string s)
            {
                this.CurrentObject.Description = s;
                this.accountItemViewModel.Update(this.CurrentObject);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                System.Guid _id = this.GetNavigatingParameter("id", System.Guid.Empty).ToGuid();
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    AccountItem item = ViewModelLocator.AccountItemViewModel.AccountBookDataContext.AccountItems.FirstOrDefault<AccountItem>(p => p.Id == _id);
                    if (item != null)
                    {
                        if (item.Type == ItemType.Expense)
                        {
                            this.sameTypeItemListGetter = (id, date) => this.accountItemViewModel.FindGroup(this.accountItemViewModel.ExpenseItems, date)
                                .FirstOrDefault<AccountItem>(p => p.Id == id);
                        }
                        else
                        {
                            this.sameTypeItemListGetter = (id, date) =>
                              this.accountItemViewModel.FindGroup(this.accountItemViewModel.IncomeItems, date)
                              .FirstOrDefault<AccountItem>(p => p.Id == id);
                        }

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            this.CurrentObject = item;
                            //  this.IncomeOrExpenseDetailsPivot.DataContext = this.CurrentObject;
                            this.MainPivot.DataContext = this.CurrentObject;
                            (this.MainPivot.Title as TextBlock).DataContext = this.CurrentObject;
                            this.InitializeMenuItemText();
                        });
                    }
                });
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (this.resetAction != null)
            {
                this.resetAction();
                this.resetAction = null;
            }
        }

        private void PeopleEditor_Click(object sender, RoutedEventArgs e)
        {
            System.Action<PeopleAssociationData> action = null;
            System.Action<PeopleAssociationData> action2 = null;
            System.Action<PeopleAssociationData> action3 = null;
            System.Func<IEnumerable<PeopleAssociationData>> func = null;
            if (this.peopleHandler == null)
            {
                this.peopleHandler = new PageActionHandler<PeopleAssociationData>();
                if (action == null)
                {
                    action = delegate(PeopleAssociationData i)
                    {
                        this.CurrentObject.Peoples.Add(i);
                        this.accountItemViewModel.Update(this.CurrentObject);
                    };
                }
                this.peopleHandler.AfterAdd = action;
                if (action2 == null)
                {
                    action2 = delegate(PeopleAssociationData i)
                    {
                        this.accountItemViewModel.Update(this.CurrentObject);
                    };
                }
                this.peopleHandler.Update = action2;
                if (action3 == null)
                {
                    action3 = delegate(PeopleAssociationData i)
                    {
                        this.CurrentObject.Peoples.Remove(i);
                        this.accountItemViewModel.Update(this.CurrentObject);
                    };
                }
                this.peopleHandler.AfterDelete = action3;
                if (func == null)
                {
                    func = () => this.CurrentObject.Peoples;
                }
                this.peopleHandler.GetItems = func;
                AttachPeoplePage.PeopleSelectorHandler = this.peopleHandler;
            }
            AttachPeoplePage.Go(this.CurrentObject.Id, this);
        }

        private void PicturesEditor_Click(object sender, RoutedEventArgs e)
        {
            System.Action<PictureInfo> action = null;
            System.Action<PictureInfo> action2 = null;
            System.Action<PictureInfo> action3 = null;
            System.Func<IEnumerable<PictureInfo>> func = null;
            if (this.picHandler == null)
            {
                this.picHandler = new PictureActionHandler();
                if (action == null)
                {
                    action = delegate(PictureInfo i)
                    {
                        ViewModelLocator.PicturesViewModel.SavePicture(new PictureInfo[] { i });
                        this.CurrentObject.Pictures.Add(i);
                        this.accountItemViewModel.Update(this.CurrentObject);
                    };
                }
                this.picHandler.AfterAdd = action;
                if (action2 == null)
                {
                    action2 = delegate(PictureInfo i)
                    {
                        this.accountItemViewModel.Update(this.CurrentObject);
                    };
                }
                this.picHandler.Update = action2;
                if (action3 == null)
                {
                    action3 = delegate(PictureInfo i)
                    {
                        this.CurrentObject.Pictures.Remove(i);
                        ViewModelLocator.PicturesViewModel.DeletePicture(i);
                        this.accountItemViewModel.Update(this.CurrentObject);
                    };
                }
                this.picHandler.AfterDelete = action3;
                if (func == null)
                {
                    func = () => this.CurrentObject.Pictures;
                }
                this.picHandler.GetItems = func;
                PictureBrowser.PictureHandler = this.picHandler;
            }
            PictureBrowser.Go(this.CurrentObject.Id, "AccountItems", this);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MainPivot.SelectedIndex == 1)
            {
                this.LoadRelatedItems();
            }
        }

        private void PrevOneButton_Click(object sender, System.EventArgs e)
        {
            if (this.prevOne != null)
            {
                this.CurrentObject = this.prevOne;
                this.IndicateNavigatingItem(0);
            }
        }

        private void RelatedItemsListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountItem selectedItem = this.RelatedItemsListControl.SelectedItem as AccountItem;
            if (selectedItem != null)
            {
                this.NavigateTo("/Pages/AccountItemViews/AccountItemViewer.xaml?fromSelf=true&id={0}", new object[] { selectedItem.Id });
                this.RelatedItemsListControl.SelectedItem = null;
            }
        }

        /// <summary>
        /// Gets or sets the current object.
        /// </summary>
        /// <value>
        /// The current object.
        /// </value>
        public AccountItem CurrentObject
        {
            get;
            set;
        }

        public bool NeedReloadData { get; set; }

        public string StasticItemsTips
        {
            get
            {
                return this.stasticItemsTips;
            }
            set
            {
                if (value != this.stasticItemsTips)
                {
                    this.stasticItemsTips = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StasticItemsTips"));
                    }
                }
            }
        }
    }
}