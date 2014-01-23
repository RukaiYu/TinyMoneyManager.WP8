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
using TinyMoneyManager.Data;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Component;

using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.ViewModels;
using TinyMoneyManager.Controls;
using Coding4Fun.Phone.Controls;

namespace TinyMoneyManager.Pages
{
    using NkjSoft.Extensions;
    using System.IO;
    using Microsoft.Phone.Tasks;
    using System.Windows.Media.Imaging;
    using System.IO.IsolatedStorage;
    using System.Threading;
    public partial class SettingPage : PhoneApplicationPage
    {
        public static SettingPageViewModel viewModel;

        public SettingPage()
        {
            InitializeComponent();
            if (viewModel == null)
                viewModel = new SettingPageViewModel(AppSetting.Instance);

            TiltEffect.SetIsTiltEnabled(this, true);

            SettingEntries.DataContext = viewModel;

            this.Loaded += new RoutedEventHandler(SettingPage_Loaded);
        }



        void SettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.LoadEntries();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void SettingEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingEntries == null || SettingEntries.SelectedItem == null)
                return;

            var item = SettingEntries.SelectedItem as TinyMoneyManager.Component.Common.ITitleInfoListener;

            if (item != null && !string.IsNullOrEmpty(item.NavigateUri))
            {

                this.NavigateTo(item.NavigateUri);

                SettingEntries.SelectedItem = null;
            }
        }

    }

}