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

namespace TinyMoneyManager.Pages.DialogBox
{
    using TinyMoneyManager.Component;
    using NkjSoft.WPhone.Extensions;
    using NkjSoft.Extensions;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Language;
    public partial class StatisticCharShower : PhoneApplicationPage
    {
        public static Func<UserControl> ContentToShowGetter;

        public StatisticCharShower()
        {
            InitializeComponent();

            this.ApplicationBar.GetMenuItemButtonFrom(0)
                .SetPropertyValue(p => p.Text = AppResources.SaveChartAsPicture)
                .Click += new EventHandler(StatisticCharShower_Click);

            //this.Content = ContentToShowGetter();
        }

        void StatisticCharShower_Click(object sender, EventArgs e)
        {

        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
    }
}