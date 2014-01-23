namespace TinyMoneyManager.ViewModels
{
    using Microsoft.Phone.Net.NetworkInformation;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.CurrencyConverterByWebService;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class CurrencySettingViewModel : NkjSoftViewModelBase
    {
        private CurrencyType fromOne;
        public bool IsStopByUser;
        private int nextCurrencyIndex;
        private CurrencyType onGoingOne;
        private int stepIndex;
        public CurrencyConvertorSoapClient syncClient;

        public event System.EventHandler<EventArgs> RateSyncingCompleted;

        public CurrencySettingViewModel()
        {
            this.CurrentSected = AppSetting.Instance.CurrencyInfo;
        }

        public System.Collections.Generic.IEnumerable<CurrencyWapper> LoadCurrencyRateListBy(CurrencyWapper fromCurrency)
        {
            if (fromCurrency.Currency != CurrencyWapper.CompareToCurrency)
            {
                CurrencyWapper.CompareToCurrency = fromCurrency.Currency;
                CurrencyHelper.RaiseRateChanging(fromCurrency.Currency);
            }
            return CurrencyHelper.CurrencyTable;
        }

        protected void OnRateSyncingCompleted(System.EventArgs e)
        {
            if (this.RateSyncingCompleted != null)
            {
                this.RateSyncingCompleted(this, e);
            }
        }

        internal void StopSyncing()
        {
            this.IsStopByUser = true;
            this.nextCurrencyIndex = 0;
            this.stepIndex = 0;
            ApplicationHelper.LastSyncAt = System.DateTime.Now;
            this.syncClient.Abort();
            this.syncClient.CloseAsync();
            CommonExtensions.Alert(null, AppResources.TipsAfterSyncRate, null);
        }

        public TinyMoneyManager.CurrencyConverterByWebService.Currency SwitchCurrency(CurrencyType localVer)
        {
            TinyMoneyManager.CurrencyConverterByWebService.Currency cNY = TinyMoneyManager.CurrencyConverterByWebService.Currency.CNY;
            switch (localVer)
            {
                case CurrencyType.CNY:
                    return cNY;

                case CurrencyType.USD:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.USD;

                case CurrencyType.NTD:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.TWD;

                case CurrencyType.HKD:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.HKD;

                case CurrencyType.AUD:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.AUD;

                case CurrencyType.EUR:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.EUR;

                case CurrencyType.JPY:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.JPY;

                case CurrencyType.GBP:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.GBP;

                case CurrencyType.MYR:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.MYR;

                case CurrencyType.SGD:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.SGD;

                case CurrencyType.THP:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.THB;

                case CurrencyType.PKR:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.PKR;

                case CurrencyType.INR:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.INR;

                case CurrencyType.KRW:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.KRW;

                case CurrencyType.IDR:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.IDR;

                case CurrencyType.BYR:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.RUB;

                case CurrencyType.PHP:
                    return TinyMoneyManager.CurrencyConverterByWebService.Currency.PHP;
            }
            return cNY;
        }

        private void syncClient_CloseCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.OnRateSyncingCompleted(System.EventArgs.Empty);
        }

        private void syncClient_ConversionRateCompleted(object sender, ConversionRateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                double result = e.Result;
                ConversionRateHelper.UpdateRate(this.fromOne, this.onGoingOne, System.Convert.ToDecimal(result));
                if (this.nextCurrencyIndex == 15)
                {
                    this.nextCurrencyIndex++;
                }
                if (this.stepIndex == 15)
                {
                    this.stepIndex++;
                    this.nextCurrencyIndex = 0;
                }
                if (this.nextCurrencyIndex == 0x11)
                {
                    this.stepIndex++;
                    this.nextCurrencyIndex = 0;
                }
                if (this.stepIndex == 0x11)
                {
                    this.StopSyncing();
                }
                else
                {
                    this.SyncStep(this.stepIndex, this.nextCurrencyIndex);
                    this.nextCurrencyIndex++;
                }
            }
            else
            {
                this.IsStopByUser = true;
                this.Status = 2;
                this.ExceptionWhenSyncing = e.Error;
                this.StopSyncing();
            }
        }

        public void SyncRateData()
        {
            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                this.Status = 0;
            }
            else
            {
                this.Status = 1;
                this.syncClient = new CurrencyConvertorSoapClient();
                this.syncClient.ConversionRateCompleted += new System.EventHandler<ConversionRateCompletedEventArgs>(this.syncClient_ConversionRateCompleted);
                this.syncClient.CloseCompleted += new System.EventHandler<AsyncCompletedEventArgs>(this.syncClient_CloseCompleted);
                this.nextCurrencyIndex = 0;
                this.stepIndex = 0;
                this.SyncStep(0, this.nextCurrencyIndex++);
            }
        }

        private void SyncStep(int fromCurrencyIndex, int nextToCurrency)
        {
            ConversionCell[,] conversionRateTable = ConversionRateHelper.ConversionRateTable;
            this.fromOne = (CurrencyType)fromCurrencyIndex;
            ConversionCell userState = conversionRateTable[fromCurrencyIndex, nextToCurrency];
            TinyMoneyManager.CurrencyConverterByWebService.Currency fromCurrency = this.SwitchCurrency(this.fromOne);
            TinyMoneyManager.CurrencyConverterByWebService.Currency toCurrency = this.SwitchCurrency(userState.Currency);
            this.onGoingOne = userState.Currency;
            if (this.IsStopByUser)
            {
                this.StopSyncing();
            }
            else
            {
                this.syncClient.ConversionRateAsync(fromCurrency, toCurrency, userState);
            }
        }

        public CurrencyWapper CurrentSected { get; set; }

        public System.Exception ExceptionWhenSyncing { get; private set; }

        public int Status { get; set; }
    }
}

