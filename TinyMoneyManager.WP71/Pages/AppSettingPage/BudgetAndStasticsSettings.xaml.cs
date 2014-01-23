using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Pages.DialogBox;
using NkjSoft.WPhone.Extensions;
using NkjSoft.Extensions;
using TinyMoneyManager.Language;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.ViewModels;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.Pages.AppSettingPage
{
    public partial class BudgetAndStasticsSettings : PhoneApplicationPage
    {
        public AppSetting Settings { get; set; }

        public BudgetAndStasticsSettings()
        {
            InitializeComponent();

            Settings = AppSetting.Instance;

            this.DataContext = this;

            TiltEffect.SetIsTiltEnabled(this, true);

            LoadDefaultValue();
        }

        /// <summary>
        /// Loads the default value.
        /// </summary>
        private void LoadDefaultValue()
        {
            SetDateInfo(this.BudgetStasticDate_StartDay, AppSetting.Instance.BudgetStatsicSettings.StartDay);
            SetDateInfo(this.BudgetStasticDate_EndDay, AppSetting.Instance.BudgetStatsicSettings.EndDay);
        }

        private void PaymentDueDate_EveryMonth_Day_Value_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var senderTextBox = sender as TextBox;

            DaySelectorPage.AfterConfirmed = delegate(int v)
            {
                SetDateInfo(senderTextBox, v);

                ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
            };

            this.NavigateTo("/Pages/DialogBox/DaySelectorPage.xaml?title={0}&defValue={1}", new object[] { AppResources.SelectDayOfMonth, 
                 senderTextBox.Tag.ToString().ToInt32() });
        }

        private void SetDateInfo(TextBox senderTextBox, int v)
        {
            senderTextBox.Tag = v;
            senderTextBox.Text = AppResources.FrequencyDayOfMonthFormatter.FormatWith(new object[] { v });

            var s_day = this.BudgetStasticDate_StartDay.Tag.ToString().ToInt32();
            var e_day = this.BudgetStasticDate_EndDay.Tag.ToString().ToInt32();

            if (e_day < s_day)
            {
                this.BudgetStasticDate_EndDay.Text = LocalizedStrings.GetCombinedText(LocalizedStrings.GetCombinedText(AppResources.Next,
                AppResources.Month).ToLower(), AppResources.FrequencyDayOfMonthFormatter.FormatWith(e_day));
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            var s_day = this.BudgetStasticDate_StartDay.Tag.ToString().ToInt32();
            var e_day = this.BudgetStasticDate_EndDay.Tag.ToString().ToInt32();

            AppSetting.Instance.BudgetStatsicSettings.StartDay = s_day;
            AppSetting.Instance.BudgetStatsicSettings.EndDay = e_day;
            
            SettingPageViewModel.Update();
            base.OnBackKeyPress(e);
        }

        /// <summary>
        /// Handles the 1 event of the BudgetStasticScope_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void BudgetStasticScope_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (BudgetStasticScope_CustomizedPanel != null)
            {
                BudgetStasticScope_CustomizedPanel.Visibility = (BudgetStasticScope.SelectedIndex == 0) ? Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
        }
    }
}