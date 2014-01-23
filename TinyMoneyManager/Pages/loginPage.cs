namespace TinyMoneyManager.Pages
{
    using Coding4Fun.Phone.Controls;
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;

    public class loginPage : PhoneApplicationPage
    {
        private bool _contentLoaded;
        internal TextBlock ApplicationTitle;
        public bool backFromMain;
        private bool canGoMainPage;
        internal Grid ContentPanel;
        internal TextBlock FooterText;
        private string lastAccessTime = "";
        internal TextBlock LastTimeAccess;
        internal Grid LayoutRoot;
        internal RoundButton OpenButton;
        internal TinyMoneyManager.Controls.PasswordTextBox PasswordTextBox;
        internal Grid PasswordTextBoxRow;
        internal TextBlock PasswordToLogin;
        private string PasswordToLoginText = "";
        private string startTo = string.Empty;
        internal StackPanel TitlePanel;
        private string WithPasswordToLoginText = "";

        public loginPage()
        {
            this.InitializeComponent();
            this.hasLogin = false;
            this.PasswordToLoginText = this.WithPasswordToLoginText = LocalizedStrings.GetLanguageInfoByKey("WithPasswordToLoginText");
            this.startTo = LocalizedStrings.GetLanguageInfoByKey("StartTo");
            this.lastAccessTime = LocalizedStrings.GetLanguageInfoByKey("LastTimeAccess").Replace("#NewLine#", System.Environment.NewLine);
            this.FooterText.Text = "\r\n                                          {0}\r\n                              Copyright \x00a9 {1} Rukai".FormatWith(new object[] { "1.9.7", System.DateTime.Now.Year });
            TiltEffect.SetIsTiltEnabled(this, true);
            base.Loaded += new RoutedEventHandler(this.loginPage_Loaded);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (base.NavigationService.CanGoBack)
            {
                base.NavigationService.GoBack();
            }
        }

        private void GotoMainPage()
        {
            this.NavigateTo("/../MainPage.xaml");
            this.backFromMain = true;
            this.hasLogin = true;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Application.LoadComponent(this, new Uri("/TinyMoneyManager;component/loginPage.xaml", UriKind.Relative));
                this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
                this.TitlePanel = (StackPanel) base.FindName("TitlePanel");
                this.ApplicationTitle = (TextBlock) base.FindName("ApplicationTitle");
                this.ContentPanel = (Grid) base.FindName("ContentPanel");
                this.PasswordToLogin = (TextBlock) base.FindName("PasswordToLogin");
                this.PasswordTextBoxRow = (Grid) base.FindName("PasswordTextBoxRow");
                this.PasswordTextBox = (TinyMoneyManager.Controls.PasswordTextBox) base.FindName("PasswordTextBox");
                this.OpenButton = (RoundButton) base.FindName("OpenButton");
                this.LastTimeAccess = (TextBlock) base.FindName("LastTimeAccess");
                this.FooterText = (TextBlock) base.FindName("FooterText");
            }
        }

        private void Login()
        {
            if (this.PasswordTextBox.Value.Trim() == AppSetting.Instance.Password)
            {
                this.hasLogin = true;
                this.PasswordTextBox.Text = string.Empty;
                this.PasswordTextBox.Visibility = Visibility.Collapsed;
                this.GotoMainPage();
            }
            else
            {
                this.PasswordTextBox.Focus();
            }
        }

        private void loginPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AppSetting.Instance.EnablePoketLock && !this.hasLogin)
            {
                this.GotoMainPage();
            }
            else
            {
                this.LayoutRoot.Visibility = Visibility.Visible;
                if (App.LastAccessTime.HasValue)
                {
                    this.LastTimeAccess.Text = string.Format(LocalizedStrings.CultureName, this.lastAccessTime, new object[] { App.LastAccessTime.GetValueOrDefault() });
                }
                else
                {
                    this.LastTimeAccess.Text = this.startTo;
                }
            }
            if (this.PasswordTextBox.Visibility == Visibility.Visible)
            {
                this.PasswordTextBox.Focus();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            base.State["__hasLogin"] = this.hasLogin;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!base.State.ContainsKey("__hasLogin") || base.State["__hasLogin"].ToBoolean(false))
            {
                this.PasswordTextBox.Visibility = AppSetting.Instance.EnablePoketLock ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            this.Login();
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Login();
            }
        }

        public bool hasLogin { get; set; }
    }
}

