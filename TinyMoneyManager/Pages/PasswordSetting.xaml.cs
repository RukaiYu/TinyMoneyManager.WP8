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

using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.ViewModels;
using TinyMoneyManager.Data.Model;
using System.Windows.Media.Imaging;
using TinyMoneyManager.Controls;
namespace TinyMoneyManager.Pages
{
    public partial class PasswordSetting : PhoneApplicationPage
    {
        private PasswordSettingViewModel viewModel;

        private const string newPassword = "new";

        private const string changePassword = "change";

        PasswordTextBox temp = null; 

        private string action = newPassword;

        public PasswordSetting()
        {
            InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);

            this.Loaded += new RoutedEventHandler(PasswordSetting_Loaded);
        }



        void PasswordSetting_Loaded(object sender, RoutedEventArgs e)
        {
            temp.Focus();
        }



        private void ime_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var message = viewModel.ValidatePassword(CurrentPasswordTextBox.Value, NewPasswordTextBox.Value, ConfirmNewPasswordTextBox.Value);

            if (message.Length == 0)
            {
                viewModel.Submit(NewPasswordTextBox.Value);
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show(message, App.AppName, MessageBoxButton.OK);
                temp.Focus();
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            action = NavigationContext.QueryString["action"] ?? newPassword;

            if (action == newPassword)
            {
                this.viewModel = new NewPasswordViewModel(AppSetting.Instance);
                temp = this.NewPasswordTextBox;
            }
            else
            {
                this.viewModel = new ChangePasswordViewModel(AppSetting.Instance);
                temp = this.CurrentPasswordTextBox;
            }
            this.DataContext = viewModel;
        }

        private void TextBoxFocus(object sender, RoutedEventArgs e)
        {
            temp = sender as PasswordTextBox;
        }

    }
}