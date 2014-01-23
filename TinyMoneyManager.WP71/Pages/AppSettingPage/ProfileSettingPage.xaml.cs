namespace TinyMoneyManager.Pages.AppSettingPage
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Tasks;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class ProfileSettingPage : PhoneApplicationPage
    {

        private ApplicationBarHelper aph;

        private bool hasBindData;

        private string oldAppName = string.Empty;

        private System.Threading.Thread th;
        private SettingPageViewModel viewModel;

        public ProfileSettingPage()
        {
            base.Resources.Add("SameAsSystem", AppResources.SameAsSystem);
            base.Resources.Add("SupportDisplayLanguages", LanguageType.SupportDisplayLanguages);
            this.InitializeComponent();
            this.aph = new ApplicationBarHelper(this);
            this.aph.SelectContentWhenFocus = true;
            this.aph.AddTextBox(new TextBox[] { this.AppNameTextBox });
            TiltEffect.SetIsTiltEnabled(this, true);
            this.viewModel = new SettingPageViewModel(AppSetting.Instance);
            this.oldAppName = AppSetting.Instance.AppName;
            base.DataContext = this.viewModel;
            base.Loaded += new RoutedEventHandler(this.ProfileSettingPage_Loaded);
             
        }

        private void BindDataToControl()
        {
            this.EmailForSummary.Text = this.viewModel.AppSetting.Email;
            this.LanguageListPicker.SelectedItem = this.viewModel.AppSetting.DisplayLanguage.ToString();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo("/Pages/PasswordSetting.xaml?action=change");
        }

        private void ChooseBackgroundImageButton_Click(object sender, RoutedEventArgs e)
        {
            PhotoChooserTask task = new PhotoChooserTask();
            task.Completed += new System.EventHandler<PhotoResult>(this.selectphoto_Completed);
            task.Show();
        }

        private void CommonTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                base.Focus();
            }
        }

        private void EmailForSummary_LostFocus(object sender, RoutedEventArgs e)
        {
            string emailText = this.EmailForSummary.Text.Trim();
            if ((emailText.Length != 0) && !AppSetting.IsEmail(emailText))
            {
                this.NotAvaliableEmailAddressMessageBlock.Visibility = Visibility.Visible;
            }
            else
            {
                this.NotAvaliableEmailAddressMessageBlock.Visibility = Visibility.Collapsed;
                this.viewModel.AppSetting.Email = emailText;
                base.Focus();
            }
        }

        private void ime_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                base.Focus();
            }
        }


        private void IPTextBoxFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            box.SelectionStart = 0;
            box.SelectionLength = box.Text.Length;
        }

        private void LanguageListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.LanguageListPicker != null)
            {
                object selectedItem = this.LanguageListPicker.SelectedItem;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            string language = this.LanguageListPicker.SelectedItem.ToString();
            string text = this.AppNameTextBox.Text;
            if (language != AppSetting.Instance.DisplayLanguage)
            {
                App.LoadUIDisplayLanguage(language);
                AppSetting.Instance.DisplayLanguage = language;
                LocalizedStrings.SetLanguage(language);
                LocalizedStrings.InitializeLanguage();
                App.QuickNewRecordName = AppResources.AddRecordName;
                ViewModelLocator.MainPageViewModel.IsDataLoaded = false;
                ViewModelLocator.MainPageViewModel.LoadData();
            }
            SettingPageViewModel.Update();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);
            if (this.viewModel.AppSetting.EnablePoketLock)
            {
                this.ChangePasswordButton.Visibility = Visibility.Visible;
            }

        }

        private void PocketLockSwitcher_Checked(object sender, RoutedEventArgs e)
        {
            if (this.PocketLockSwitcher.IsChecked.GetValueOrDefault())
            {
                this.ChangePasswordButton.Visibility = Visibility.Visible;
                this.NavigateTo("/Pages/PasswordSetting.xaml?action=new");
            }
        }

        private void PocketLockSwitcher_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ChangePasswordButton.Visibility = Visibility.Collapsed;
            this.viewModel.AppSetting.EnablePoketLock = false;
            this.viewModel.AppSetting.Password = string.Empty;
        }

        private void ProfileSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.hasBindData)
            {
                System.Threading.ThreadStart start = null;
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
            this.UseBackgroundImageForMainPageSwitcher.Unchecked += new System.EventHandler<RoutedEventArgs>(this.UseBackgroundImageForMainPageSwitcher_CheckedChanged);
            this.UseBackgroundImageForMainPageSwitcher.Checked += new System.EventHandler<RoutedEventArgs>(this.UseBackgroundImageForMainPageSwitcher_CheckedChanged);
            this.LanguageListPicker.SelectionChanged += new SelectionChangedEventHandler(this.LanguageListPicker_SelectionChanged);
            this.PocketLockSwitcher.Click += new System.EventHandler<RoutedEventArgs>(this.PocketLockSwitcher_Checked);
        }

        private void selectphoto_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                AppSetting.Instance.BackgroundImageForMainPage = e.OriginalFileName;
                ApplicationHelper.SavePictureToIsolateStorage(e.ChosenPhoto, AppSetting.MainPageBackgroundPictureFileName);
                MainPage.backgroundImageSetter(e.ChosenPhoto);
            }
        }

        private void UseBackgroundImageForMainPageSwitcher_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ToggleSwitch switch2 = sender as ToggleSwitch;
            if (switch2 != null)
            {
                switch2.IsChecked.GetValueOrDefault();
                MainPage.backgroundImageSetter(null);
            }
        }
    }
}

