using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TinyMoneyManager;
using TinyMoneyManager.Component;
using TinyMoneyManager.Controls;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.ViewModels.RepaymentManager;
using TinyMoneyManager.Language;
using System.ComponentModel;
using Microsoft.Phone.Scheduler;

namespace TinyMoneyManager.Pages.NotificationCenter
{
    public partial class NotificationEditor : PhoneApplicationPage
    {
        private ApplicationBarHelper applicationBarHelper;
        private Guid _id;
        private PageActionType _action;

        public TallySchedule Current { get; set; }

        public NotificationEditor()
        {
            InitializeComponent();

            this.applicationBarHelper = new ApplicationBarHelper(this);
            this.applicationBarHelper.SelectContentWhenFocus = true;

            this.applicationBarHelper.AddTextBox(false, new TextBox[] { this.SubjectValue });
            InitializedFrequencySelector();
            this.InitializeApplicationBarText();

            TiltEffect.SetIsTiltEnabled(this, true);
        }

        private void InitializedFrequencySelector()
        {
            var items = new System.Collections.Generic.Dictionary<RecurrenceInterval, string>();
            items.Add(RecurrenceInterval.None, AppResources.OnlyOnce);
            items.Add(RecurrenceInterval.Daily, AppResources.Daily);
            items.Add(RecurrenceInterval.Weekly, AppResources.Weekly);
            items.Add(RecurrenceInterval.Monthly, AppResources.Monthly);
            items.Add(RecurrenceInterval.EndOfMonth, AppResources.EndOfMonth);
            items.Add(RecurrenceInterval.Yearly, AppResources.Yearly);

            this.ItemSelector.ItemsSource = items;

            this.ItemSelector.SelectedIndex = 0;
        }

        private void InitializeApplicationBarText()
        {
            base.ApplicationBar.GetIconButtonFrom(0).Text = this.GetLanguageInfoByKey("Save");
            base.ApplicationBar.GetIconButtonFrom(1).Text = this.GetLanguageInfoByKey("Cancel");
        }

        private void Frequency_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void ItemSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Calcel_Click(object sender, EventArgs e)
        {
            this.SafeGoBack();
        }

        private void SaveAndClose_Click(object sender, EventArgs e)
        {
            var time1 = DateValue.Value.GetValueOrDefault().Date + TimeValue.Value.GetValueOrDefault().TimeOfDay;

            var time2 = ExpirationTime.Value.GetValueOrDefault().Date + TimeValue.Value.GetValueOrDefault().TimeOfDay;

            if (_action == PageActionType.Add)
            {
                if (time1 < DateTime.Now)
                {
                    this.AlertNotification(AppResources.NotAvaliableObjectMessageFormatter.FormatWith(AppResources.StartDate));
                    return;
                }
            }

            if (time2 < time1)
            {
                this.AlertNotification(AppResources.StartDateMustBeLargerThanEndDate);
                return;
            }

            var notific = Current;
            notific.Name = SubjectValue.Text;

            notific.RecurrenceInterval = ItemSelector.SelectedIndex;
            notific.Notes = NotesValue.Text;

            notific.EndTime = time2;
            notific.StartTime = time1;

            if (_action == PageActionType.Add)
            {
                notific.Id = Guid.NewGuid();
                ViewModelLocator.NotificationsViewModel.AddNotification(notific);
            }
            else
            {
                ViewModelLocator.NotificationsViewModel.UpdateNotification(notific);
            }


            this.Calcel_Click(sender, e);
        }

        private void EnableAlarmNotification_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void EnableAlarmNotification_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            (sender as Button).Visibility = System.Windows.Visibility.Collapsed;
            MoreInfoSettingPanel.Visibility = System.Windows.Visibility.Visible;
            ContentPanel.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                this._id = e.GetNavigatingParameter("id").ToGuid();
                this._action = e.GetNavigatingParameter("action").ToEnum<PageActionType>();

                System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                {
                    if (_action == PageActionType.Edit)
                    {
                        var item = ViewModelLocator.NotificationsViewModel.Notifications.FirstOrDefault(p => p.Id == _id);
                        if (item != null)
                        {
                            Current = item;
                        }
                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        if (_action == PageActionType.Add)
                        {
                            Current = new TallySchedule();
                            Current.StartTime = DateTime.Now.AddSeconds(20);
                            Current.EndTime = DateTime.Now.AddYears(1);
                        }
                        else
                        {
                            this.EnableAlarmNotification.IsEnabled = false;
                            this.PageTitle.Text = AppResources.Edit.ToLower();
                        }
                         
                        this.DataContext = Current;
                    });
                });

            }

            base.OnNavigatedTo(e);
        }

        public static void Go(PhoneApplicationPage fromPage, Guid id, PageActionType action)
        {
            fromPage.NavigateTo("/Pages/NotificationCenter/NotificationEditor.xaml?id={0}&action={1}", id, action);
        }
    }
}