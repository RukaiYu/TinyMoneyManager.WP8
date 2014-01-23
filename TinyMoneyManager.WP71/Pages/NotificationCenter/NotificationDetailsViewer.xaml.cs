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
    public partial class NotificationDetailsViewer : PhoneApplicationPage
    {
        public TinyMoneyManager.Data.Model.TallySchedule Current { get; set; }

        public NotificationDetailsViewer()
        {
            InitializeComponent();

            PageTitle.Text = AppResources.Details.Trim();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {

                var _id = e.GetNavigatingParameter("id").ToGuid();

                if (_id != Guid.Empty)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                    {
                        var item = ViewModelLocator.NotificationsViewModel.GetNotificationById(_id);

                        this.Current = item;

                        Dispatcher.BeginInvoke(() =>
                            {
                                this.DataContext = this;
                            });
                    });
                }
            }

            base.OnNavigatedTo(e);
        }
    }
}