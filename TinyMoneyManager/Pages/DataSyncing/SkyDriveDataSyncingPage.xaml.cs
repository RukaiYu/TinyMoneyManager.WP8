namespace TinyMoneyManager.Pages.DataSyncing
{
    using Microsoft.Live;
    using Microsoft.Live.Controls;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net.NetworkInformation;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels.DataSyncing;

    public partial class SkyDriveDataSyncingPage : PhoneApplicationPage
    {

        public SkyDriveDataSyncingViewModel viewModel;

        public SkyDriveDataSyncingPage()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.viewModel = new SkyDriveDataSyncingViewModel();
            base.DataContext = this.viewModel;
            this.signInBtn.Click += new RoutedEventHandler(this.signInBtn_Click);
            this.signInBtn.SessionChanged += new System.EventHandler<LiveConnectSessionChangedEventArgs>(this.signInBtn_SessionChanged);
            this.loadContent();
        }

        private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/SkyDriveDataSyncingPage.xaml");
        }

        private void GoForMoreDataSyncingModeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Alert("DropBox, Google Drive COMING SOON!", null);
        }

        public void loadContent()
        {
            IApplicationBar applicationBar = base.ApplicationBar;
            ApplicationBarMenuItem item = new ApplicationBarMenuItem(AppResources.DataSyncViaPcClient);
            item.Click += new System.EventHandler(this.viaPCClientButton_Click);
            applicationBar.MenuItems.Add(item);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            this.WorkDone();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.BusyForWork(AppResources.LoginLiveIDMessage);
            }
        }

        private void signInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.LiveConnector != null)
            {
                if (this.viewModel.IsBusy)
                {
                    this.AlertNotification(AppResources.PleaseWaitWhileBusy, null);
                }
                else if (!this.viewModel.IsLogonToLiveId)
                {
                    this.BusyForWork(AppResources.LoginLiveIDMessage);
                }
            }
        }

        private void signInBtn_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            this.WorkDone();
            if ((e.Status != LiveConnectSessionStatus.Connected) && (e.Error != null))
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    this.Alert(AppResources.NoAvailableNetworkMessage, AppResources.LoginLiveIDMessage.ToUpperInvariant());
                }
                else
                {
                    this.Alert(AppResources.LiveConnectExceptionMessage, AppResources.LoginLiveIDMessage.ToUpperInvariant());
                }
            }
            else if (e.Status == LiveConnectSessionStatus.Connected)
            {
                this.viewModel.InitializeLiveConnector(e.Session);
                this.viewModel.IsLogonToLiveId = e.Status == LiveConnectSessionStatus.Connected;
            }
        }

        private void SyncDataButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SyncData();
        }

        private void SyncPicturesButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SyncPictures();
        }

        private void viaPCClientButton_Click(object sender, System.EventArgs e)
        {
            base.NavigationService.Navigate(new Uri("/Pages/DataSynchronizationPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}

