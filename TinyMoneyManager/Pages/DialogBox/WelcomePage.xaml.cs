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
using TinyMoneyManager.ViewModels;

using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Data;
using Microsoft.Phone.Shell;
using System.Reflection;
using System.IO;
using TinyMoneyManager.Pages;
using System.ComponentModel;
using TinyMoneyManager.Controls;
using TinyMoneyManager.ViewModels;
using NkjSoft.Extensions;
using mangoProgressIndicator;
using System.Threading;
using TinyMoneyManager.Language;

namespace TinyMoneyManager.Pages.DialogBox
{
    public partial class WelcomePage : PhoneApplicationPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomePage"/> class.
        /// </summary>
        public WelcomePage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(WelcomePage_Loaded);

            TiltEffect.SetIsTiltEnabled(this, true);
            PageTitle.Text = AppResources.Welcome.ToUpperInvariant();

            InitAppBar();
        }

        /// <summary>
        /// Inits the app bar.
        /// </summary>
        private void InitAppBar()
        {
            var appBar = new ApplicationBar();
            var okButton = IconUirs.CreateIconButton(AppResources.OK, IconUirs.CheckIcon);
            okButton.Click += new EventHandler(okButton_Click);
            appBar.Buttons.Add(okButton);


            this.ApplicationBar = appBar;
        }


        /// <summary>
        /// Handles the Loaded event of the WelcomePage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void WelcomePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUpdates();
        }

        /// <summary>
        /// Loads the updates.
        /// </summary>
        private void LoadUpdates()
        {
            //UpdatesShow = new StackPanel();

            var lines = AboutPageViewModel.GetTips(3).Take(30);

            var blankCount = 0;

            foreach (var line in lines)
            {
                if (line.Text.Trim().Length == 0)
                {
                    blankCount++;
                }

                if (blankCount == 2)
                {
                    break;
                }

                TextBlock tb = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = line.Text
                };

                bool needAccent = false;

                var text = line.Text.TrimStart();

                if (text.StartsWith("What"))
                {
                    needAccent = true;
                    tb.FontWeight = FontWeights.Bold;
                }

                if (text.StartsWith("v") || needAccent)
                {
                    tb.Style = (Style)Application.Current.Resources["PhoneTextAccentStyle"];
                }
                else
                {
                    tb.Margin = new Thickness(12, 0, 0, 0);
                    tb.Style = (Style)Application.Current.Resources["PhoneTextNormalStyle"];
                }

                UpdatesShow.Children.Add(tb);
            }

            UpdatesShow.Children.Add(new System.Windows.Shapes.Rectangle() { Height = 100 });
        }

        /// <summary>
        /// Shows the welcome.
        /// </summary>
        /// <param name="startPos">The start pos.</param>
        internal static void ShowWelcome(PhoneApplicationPage startPos)
        {
            IsolatedAppSetingsHelper.ShowTipsByVerion(IsolatedAppSetingsHelper.HasShowUpdatedKey, () =>
            {
                startPos.NavigateTo("/Pages/DialogBox/WelcomePage.xaml");
            });
        }

        void okButton_Click(object sender, EventArgs e)
        {

            this.SafeGoBack();
        }
    }
}