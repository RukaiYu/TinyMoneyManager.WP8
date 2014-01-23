namespace TinyMoneyManager.Pages.AppSettingPage
{
    using Coding4Fun.Phone.Controls;
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class CurrencyRateSettingPage : PhoneApplicationPage
    {


        private CurrencySettingViewModel currencySettingViewModel;
        private string editPivotTitle;
        private string editRateFormatter;

        public bool hasChangedRate;
        private bool hasLoaded;
        private bool hasSetImg;
        private bool isStartSync;

        private BitmapImage stop;
        private BitmapImage sync;
        private CurrencyWapper tempCurrencyWapperForEdit;
        private string tempValue;

        private string toCurrencyListTipsFormatter;

        public CurrencyRateSettingPage()
        {
            this.InitializeComponent();
            this.tempCurrencyWapperForEdit = AppSetting.Instance.CurrencyInfo;
            TiltEffect.SetIsTiltEnabled(this, true);
            base.Loaded += new RoutedEventHandler(this.CurrencyRateSettingPage_Loaded);
        }

        private void btnSyncRates_Click(object sender, RoutedEventArgs e)
        {
            this.isStartSync = !this.isStartSync;
            this.SyncStatusChanged(this.isStartSync);
            if (this.isStartSync)
            {
                this.currencySettingViewModel.SyncRateData();
            }
            else
            {
                this.currencySettingViewModel.Status = 3;
                this.currencySettingViewModel.IsStopByUser = true;
                this.currencySettingViewModel.StopSyncing();
            }
        }

        private void CurrencyRateSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeRateSyncingStuff();
        }

        private void currencySettingViewModel_RateSyncingCompleted(object sender, System.EventArgs e)
        {
            this.SyncStatusChanged(false);
            this.FooterInfo.Text = this.GetLanguageInfoByKey("AllRateDataIsFromWebServiceNet").FormatWith(new object[] { ApplicationHelper.LastSyncAtString });
            if (this.currencySettingViewModel.Status == 0)
            {
                this.Alert(LocalizedStrings.GetLanguageInfoByKey("NoAvailableNetworkMessage"), null);
            }
            else if (this.currencySettingViewModel.Status == 2)
            {
                if ((this.currencySettingViewModel.ExceptionWhenSyncing != null) && (this.currencySettingViewModel.ExceptionWhenSyncing.InnerException.GetType() == typeof(System.TimeoutException)))
                {
                    this.Alert(this.currencySettingViewModel.ExceptionWhenSyncing.Message, null);
                }
            }
            else if ((this.currencySettingViewModel.Status != 3) && (this.currencySettingViewModel.Status != 4))
            {
                this.Alert(this.GetLanguageInfoByKey("RateSyncingSuccessfullyMessage"), null);
                CurrencyHelper.RaiseRateChanging(this.tempFromCurrencyWapper.Currency);
            }
        }

        private void FromCurrencySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tempFromCurrencyWapper = this.FromCurrencySelector.SelectedItem as CurrencyWapper;
            if (this.tempFromCurrencyWapper != null)
            {
                this.currencySettingViewModel.LoadCurrencyRateListBy(this.tempFromCurrencyWapper);
            }
        }

        private void InitializeRateSyncingStuff()
        {
            if (!this.hasLoaded)
            {
                this.hasLoaded = true;
                this.toCurrencyListTipsFormatter = AppResources.ToCurrencyListFormatter;
                if (this.currencySettingViewModel == null)
                {
                    this.currencySettingViewModel = new CurrencySettingViewModel();
                }
                this.ToCurrencyListTips.Text = this.toCurrencyListTipsFormatter;
                this.FromCurrencySelector.ItemsSource = CurrencyHelper.CurrencyTable;
                this.FromCurrencySelector.SelectedItem = AppSetting.Instance.CurrencyInfo;
                this.ToCurrencyListBox.ItemsSource = CurrencyHelper.CurrencyTable;
                this.editRateFormatter = AppResources.EditRateFormatter;
                this.editPivotTitle = AppResources.Edit;
                this.FooterInfo.Text = AppResources.AllRateDataIsFromWebServiceNet.FormatWith(new object[] { ApplicationHelper.LastSyncAtString });
                this.currencySettingViewModel.RateSyncingCompleted += new System.EventHandler<System.EventArgs>(this.currencySettingViewModel_RateSyncingCompleted);
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            ApplicationHelper.SaveConversionRateTable();
            SettingPageViewModel.Update();
            if (this.hasChangedRate)
            {
                this.Alert(AppResources.TipsAfterSyncRate, null);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (((this.currencySettingViewModel != null) && (this.currencySettingViewModel.syncClient != null)) && (this.currencySettingViewModel.syncClient.State != CommunicationState.Closed))
            {
                this.currencySettingViewModel.Status = 4;
                this.currencySettingViewModel.syncClient.Abort();
                this.currencySettingViewModel.syncClient.CloseAsync();
            }
        }

        private void rateInputBox_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            System.Func<Decimal> callBackWhenFailed = null;
            if (((PopUpResult)e.PopUpResult) == PopUpResult.Ok)
            {
                if (callBackWhenFailed == null)
                {
                    callBackWhenFailed = () => this.tempValue.ToDecimal();
                }
                decimal rate = e.Result.ToDecimal(callBackWhenFailed);
                if ((rate > 0.0M) && (e.Result != this.tempValue))
                {
                    if (CurrencyWapper.ConversionRateHelper_UpdateRateRexy == null)
                    {
                        // 
                        CurrencyWapper.ConversionRateHelper_UpdateRateRexy = (from, to, val) =>
                        {
                            ConversionRateHelper.UpdateRate(from, to, val);
                        };
                    }

                    this.tempCurrencyWapperForEdit.SetCurrency(this.tempFromCurrencyWapper.Currency, rate);
                    this.hasChangedRate = true;
                }
            }
        }

        private void SyncStatusChanged(bool isStart)
        {
            this.isStartSync = isStart;
            if (!this.hasSetImg)
            {
                this.stop = new BitmapImage(new Uri("/icons/appBar/appbar.transport.pause.rest.png", UriKind.Relative));
                this.sync = new BitmapImage(new Uri("/icons/appBar/appbar.sync.rest.png", UriKind.Relative));
                this.hasSetImg = true;
            }
            if (isStart)
            {
                this.FooterInfo.Text = AppResources.ConversionRateProcessIsOnGoingMessage;
                this.btnSyncRates.ImageSource = this.stop;
                this.pb_SyncProgressBar.IsIndeterminate = true;
                this.pb_SyncProgressBar.Visibility = Visibility.Visible;
                this.ToCurrencyListBox.IsEnabled = false;
                this.FromCurrencySelector.IsEnabled = false;
            }
            else
            {
                this.FooterInfo.Text = AppResources.AllRateDataIsFromWebServiceNet.FormatWith(ApplicationHelper.LastSyncAtString);
                this.btnSyncRates.ImageSource = this.sync;
                this.pb_SyncProgressBar.IsIndeterminate = false;
                this.pb_SyncProgressBar.Visibility = Visibility.Collapsed;
                this.ToCurrencyListBox.IsEnabled = true;
                this.FromCurrencySelector.IsEnabled = true;
            }
        }

        private void ToCurrencyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrencyWapper selectedItem = this.ToCurrencyListBox.SelectedItem as CurrencyWapper;
            if (selectedItem != null)
            {
                this.tempCurrencyWapperForEdit = selectedItem;
                InputPrompt prompt2 = new InputPrompt
                {
                    InputScope = MoneyInputTextBox.NumberInputScope
                };
                InputPrompt prompt = prompt2;
                prompt.Completed += new System.EventHandler<PopUpEventArgs<string, PopUpResult>>(this.rateInputBox_Completed);
                prompt.Title = this.editPivotTitle;
                prompt.Message = this.editRateFormatter.FormatWith(new object[] { this.tempFromCurrencyWapper.CurrencyNameWithSymbol, this.tempCurrencyWapperForEdit.CurrencyNameWithSymbol });
                this.tempValue = this.tempCurrencyWapperForEdit.RateToCompareToCurrency;
                prompt.Value = this.tempValue;
                prompt.Show();
                this.ToCurrencyListBox.SelectedItem = null;
            }
        }

        private CurrencyWapper tempFromCurrencyWapper { get; set; }
    }
}

