using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.Pages.AppSettingPage
{
    public partial class TileSettings : PhoneApplicationPage
    {
        public TileSettings()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.MainPageViewModel.UpdateTileData(false);
        }
    }
}