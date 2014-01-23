namespace TinyMoneyManager.Pages.AppSettingPage
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.ViewModels;

    public partial class CommonSettingPage : PhoneApplicationPage
    {
        public static AppSettingRepository appSettingRepository;

        public ObservableCollection<string> currencyStyleList;

        private bool hasBindData;

        private System.Threading.Thread th;
        private SettingPageViewModel viewModel;

        public CommonSettingPage()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.viewModel = new SettingPageViewModel(AppSetting.Instance);
            base.DataContext = this.viewModel;
            this.DefaultCurrency.ItemsSource = CurrencyHelper.CurrencyTable;
            this.currencyStyleList = new ObservableCollection<string> { "", "" };
            this.SetCurrencyStyle(AppSetting.Instance.CurrencyInfo);
            this.CurrencySymbolStyle.ItemsSource = this.currencyStyleList;
            base.BackKeyPress += new System.EventHandler<CancelEventArgs>(this.CommonSettingPage_BackKeyPress);
        }

        private void BindDataToControl()
        {
            this.DefaultCurrency.SelectedItem = this.viewModel.AppSetting.CurrencyInfo;
            this.CurrencySymbolStyle.SelectedIndex = (int)this.viewModel.AppSetting.GlobleCurrencySymbolStyle;
        }

        private void CommonSettingPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            SettingPageViewModel.Update();
        }

        private void DefaultCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrencyWapper selectedItem = this.DefaultCurrency.SelectedItem as CurrencyWapper;
            if (selectedItem != this.viewModel.AppSetting.CurrencyInfo)
            {
                ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
            }
            this.viewModel.AppSetting.CurrencyInfo = selectedItem;
            this.SetCurrencyStyle(selectedItem);
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            System.Threading.ThreadStart start = null;
            base.OnNavigatedTo(e);
            if (!this.hasBindData)
            {
                this.hasBindData = true;
                if (start == null)
                {
                    start = delegate
                    {
                        this.InvokeInThread(delegate
                        {
                            this.BindDataToControl();
                            this.RegisterControlEvent();
                        });
                    };
                }
                this.th = new System.Threading.Thread(start);
                this.th.Start();
            }
        }

        private void RegisterControlEvent()
        {
            this.CurrencySymbolStyle.SelectionChanged += delegate(object o, SelectionChangedEventArgs e)
            {
                TinyMoneyManager.Data.Model.CurrencySymbolStyle selectedIndex = (TinyMoneyManager.Data.Model.CurrencySymbolStyle)this.CurrencySymbolStyle.SelectedIndex;
                if (selectedIndex != this.viewModel.AppSetting.GlobleCurrencySymbolStyle)
                {
                    ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
                    this.viewModel.AppSetting.GlobleCurrencySymbolStyle = selectedIndex;
                }
            };
            this.DefaultCurrency.SelectionChanged += new SelectionChangedEventHandler(this.DefaultCurrency_SelectionChanged);
        }

        private void SetCurrencyStyle(CurrencyWapper currency)
        {
            this.currencyStyleList[0] = currency.CurrencyString + 100000M.ToMoneyF2();
            this.currencyStyleList[1] = currency.CurrencyStringWithNameFirst + 100000M.ToMoneyF2();
        }
    }
}

