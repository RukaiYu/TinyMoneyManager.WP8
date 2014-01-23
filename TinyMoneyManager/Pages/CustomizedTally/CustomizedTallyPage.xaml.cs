namespace TinyMoneyManager.Pages.CustomizedTally
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages;

    public partial class CustomizedTallyPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private TallySchedule accountItemTemplete;

        private ApplicationBarHelper apbh;

        private AccountItem current;

        private bool needAlertNotice;


        public event PropertyChangedEventHandler PropertyChanged;

        public CustomizedTallyPage()
        {
            InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.apbh = new ApplicationBarHelper(this);
            this.apbh.SelectContentWhenFocus = true;
            this.apbh.OriginalBar = null;
            this.apbh.AddTextBox(new TextBox[] { this.AmountValueInputBox });
            base.DataContext = this;
        }

        private void CreateAccountItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.accountItemTemplete.IsCompletedForToday || (this.AlertConfirm(AppResources.ConfirmWhenTemplateIsRecordedForToday, null, null) != MessageBoxResult.Cancel))
            {
                AccountItem current = this.current;
                this.current.CreateTime = System.DateTime.Now;
                this.current.Money = this.AmountValueInputBox.Text.ToDecimal();
                ViewModelLocator.AccountViewModel.HandleAccountItemAdding(current);
                this.needAlertNotice = true;
                ViewModelLocator.CustomizedTallyViewModel.UpdateToComplete(this.accountItemTemplete);
                this.SafeGoBack();
            }
        }

        private void CreateInDetailModeButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NewOrEditAccountItemPage.IsSelectionMode = false;
            NewOrEditAccountItemPage.currentEditObject = this.Current;
            this.NavigateTo("/Pages/NewOrEditAccountItemPage.xaml?action=preEdit");
        }

        private void EditTempleteInfoButton_Click(object sender, RoutedEventArgs e)
        {
            CustomizedTallyItemEditorPage.NavigateTo(this, this.accountItemTemplete, PageActionType.Edit);
        }

        public static void Go(PhoneApplicationPage fromPage, TallySchedule item)
        {
            System.Guid id = item.Id;
            fromPage.NavigateTo("/Pages/CustomizedTally/CustomizedTallyPage.xaml?id={0}", new object[] { id });
        }

        private void LoadTemplete(System.Guid itemId)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                TallySchedule data = (from p in ViewModelLocator.CustomizedTallyViewModel.AccountBookDataContext.TallyScheduleTable
                                      where ((int)p.ProfileRecordType) == 2
                                      select p).FirstOrDefault<TallySchedule>(p => p.Id == itemId);
                if (data != null)
                {
                    this.Dispatcher.BeginInvoke(delegate
                    {
                        this.AccountItemTemplete = data;
                        this.Current = this.accountItemTemplete.CreateEmptyAccountItem();
                    });
                }
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (this.needAlertNotice)
            {
                this.AlertNotification(AppResources.OperationSuccessfullyMessage, null);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.LoadTemplete(e.GetNavigatingParameter("id", null).ToGuid());
            }
        }

        protected virtual void OnNotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public TallySchedule AccountItemTemplete
        {
            get
            {
                return this.accountItemTemplete;
            }
            set
            {
                if (value != this.accountItemTemplete)
                {
                    this.accountItemTemplete = value;
                    this.OnNotifyPropertyChanged("AccountItemTemplete");
                }
            }
        }

        public AccountItem Current
        {
            get
            {
                return this.current;
            }
            set
            {
                if (value != this.current)
                {
                    this.current = value;
                    this.OnNotifyPropertyChanged("Current");
                }
            }
        }
    }
}

