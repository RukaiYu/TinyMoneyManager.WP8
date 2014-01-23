
namespace TinyMoneyManager.Pages
{
    using System.Windows;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Tasks;
    using TinyMoneyManager.ViewModels;
    using NkjSoft.WPhone.Extensions;
    using NkjSoft.Extensions;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Language;
    using System;
    using System.Windows.Controls;
    using System.Windows.Resources;
    using System.IO;
    using Microsoft.Xna.Framework;
    using TinyMoneyManager.Data.Model;

    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();

            this.VersionInfo.Text = AppResources.Version.FormatWith(
               "{0} (Build {1})".FormatWith(App.Version, App.SeniorVersion));

            this.DatabaseVersionInfo.Text = AppResources.DatabaseVersion.FormatWith(DatabaseVersion.CurrentVersion);

            Footer.Text = "© {0} 103Studio, China".FormatWith(DateTime.Now.Year);


            this.Loaded += new RoutedEventHandler(AboutPage_Loaded);

            this.mainPivot.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(mainPivot_SelectionChanged);

        }

        void mainPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var index = mainPivot.SelectedIndex;

            if (index == 1)
            {
                LoadUpdateLogs();
            }

            if (index == 2)
            {
                LoadHelps();
            }
        }

        private void LoadHelps()
        {
            if (!hasLoadHelps)
            {
                hasLoadHelps = true;
                TipsListBox.ItemsSource = AboutPageViewModel.GetTips(1);
            }
        }

        StackPanel updateLogs;
        bool hasLoadHelps = false;
        private void LoadUpdateLogs()
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (updateLogs != null)
                    return;

                updateLogs = new StackPanel();

                var lines = AboutPageViewModel.GetTips(3);
                foreach (var line in lines)
                {
                    TextBlock tb = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = line.Text
                    };

                    bool needAccent = false;

                    if (line.Text.StartsWith("What"))
                    {
                        needAccent = true;
                        tb.FontWeight = FontWeights.Bold;
                    }

                    if (line.Text.StartsWith("v") || needAccent)
                    {
                        tb.Style = (Style)Application.Current.Resources["PhoneTextAccentStyle"];
                    }
                    else
                    {
                        tb.Margin = new Thickness(12, 0, 0, 0);
                        tb.Style = (Style)Application.Current.Resources["PhoneTextNormalStyle"];
                    }

                    updateLogs.Children.Add(tb);
                }

                updateLogs.Children.Add(new System.Windows.Shapes.Rectangle() { Height = 100 });

                sv1.Content = updateLogs;
            });
        }

        void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            var lang = AppSetting.Instance.DisplayLanguage.ToUpper();
            if (lang != "EN-US" &&
             lang != "PT-BR")
            {
                DonateButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void SendFeedBackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var phoneInfo = "{0}\r\n{1}"
                    .FormatWith(
                Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceName"),
                  System.Environment.OSVersion.Version);

                Microsoft.Phone.Tasks.EmailComposeTask emailCompose =
                    new EmailComposeTask();
                emailCompose.To = "rukai.yu@outlook.com";
                emailCompose.Body = AppResources.SendFeedBackBody.FormatWith(App.SingleVersionInfo, System.Globalization.CultureInfo.CurrentCulture.NativeName) + "\r\n"
                    + phoneInfo;


                emailCompose.Subject = AppResources.SendFeedBackSubject;

                emailCompose.Show();

            }
            catch
            {
            }
        }

        private void ReviewAppButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Tasks.MarketplaceReviewTask reviewTask =
                new MarketplaceReviewTask();
            reviewTask.Show();
        }

        private void ShowMoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMoreInfoButton.Visibility = System.Windows.Visibility.Collapsed;
            MoreInfoPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var launcher = new Microsoft.Phone.Tasks.WebBrowserTask();

                launcher.Uri = new Uri("https://me.alipay.com/accountbook");
                launcher.Show();
            }
            catch (Exception)
            {
            }
        }
    }
}