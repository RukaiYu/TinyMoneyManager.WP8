using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.ViewModels;
namespace TinyMoneyManager.Pages.DialogBox.WelcomePages
{
    public partial class QuickSettingPage : PhoneApplicationPage
    {
        private Data.Model.AppSetting appSettings;

        public QuickSettingPage()
        {
            InitializeComponent();

            appSettings = AppSetting.Instance;

            this.DefaultCurrency.ItemsSource = CurrencyHelper.CurrencyTable;
            this.DataContext = appSettings;
        }

        /// <summary>
        /// Handles the Click event of the StartButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.SafeGoBack();
        }

        /// <summary>
        /// Goes the specified from page.
        /// </summary>
        /// <param name="fromPage">From page.</param>
        public static void Go(PhoneApplicationPage fromPage)
        {
            fromPage.NavigateTo("/Pages/DialogBox/WelcomePages/QuickSettingPage.xaml");
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
        }


        private void DefaultCurrency_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                CurrencyWapper selectedItem = this.DefaultCurrency.SelectedItem as CurrencyWapper;
                if (selectedItem != this.appSettings.CurrencyInfo)
                {
                    ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
                }

                this.appSettings.CurrencyInfo = selectedItem;
                SettingPageViewModel.Update();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                if (this.NavigationService.BackStack.Count() > 0)
                {
                    this.NavigationService.RemoveBackEntry();
                }
            }
        }
    }
}