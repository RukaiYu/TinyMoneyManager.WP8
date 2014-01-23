namespace TinyMoneyManager.Pages.AppSettingPage
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Tasks;
    using NkjSoft.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class DataSyncingSettingPage : PhoneApplicationPage
    {
        private SettingPageViewModel viewModel;

        public DataSyncingSettingPage()
        {
            this.InitializeComponent();
            this.viewModel = new SettingPageViewModel(AppSetting.Instance);
            TiltEffect.SetIsTiltEnabled(this, true);
            this.SyncSettingGrid.DataContext = this.viewModel;
        }

        private void CommonTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                base.Focus();
            }
        }

        private void GetPCclientDownloadUrlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SendingProcess();
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }


        private void IPNodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.PlatformKeyCode == 190)
            {
                TextBox box = sender as TextBox;
                if (box != null)
                {
                    this.registeTextBox(box.TabIndex);
                    e.Handled = true;
                }
            }
        }

        private void IPTextBoxFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            box.SelectionStart = 0;
            box.SelectionLength = box.Text.Length;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            SettingPageViewModel.Update();
        }

        private void registeTextBox(int index)
        {
            if (index == 10)
            {
                this.ServerIPTwo.Focus();
            }
            if (index == 11)
            {
                this.ServerIPThree.Focus();
            }
            if (index == 12)
            {
                this.ServerIPFour.Focus();
            }
            if (index == 13)
            {
                this.ServerPortTextBox.Focus();
            }
            if (index == 14)
            {
                this.ServerIPOne.Focus();
            }
        }

        private void SendingProcess()
        {
            string email = AppSetting.Instance.Email;
            new EmailComposeTask { To = email, Subject = "{0} PC Client download link".FormatWith(new object[] { App.AppName }), Body = AppResources.PCClientURL.FormatWith(new object[] { "https://skydrive.live.com/redir.aspx?cid=d9cb9d904309ae62&resid=D9CB9D904309AE62!551&parid=root", "2.3" }) }.Show();
        }
    }
}

