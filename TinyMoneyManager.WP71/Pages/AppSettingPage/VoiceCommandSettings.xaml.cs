using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AccountBook.VoiceCommandData;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Pages.VoiceCommand;
using Windows.Phone.Speech.Recognition;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component.Common;
using TinyMoneyManager.Component;
namespace TinyMoneyManager.Pages.AppSettingPage
{
    using NkjSoft.Extensions;
    using TinyMoneyManager.Language;
    using System.Windows.Controls.Primitives;
    public partial class VoiceCommandSettings : PhoneApplicationPage
    {

        /// <summary>
        /// Gets or sets the speech language.
        /// </summary>
        /// <value>
        /// The speech language.
        /// </value>
        public string SpeechLanguage
        {
            get { return (string)GetValue(SpeechLanguageProperty); }
            set { SetValue(SpeechLanguageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpeechLanguage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpeechLanguageProperty =
            DependencyProperty.Register("SpeechLanguage", typeof(string), typeof(VoiceCommandSettings), new PropertyMetadata(""));
        private string _oldLanguage;

        public AppSetting AppSettng { get; set; }

        public VoiceCommandSettings()
        {
            InitializeComponent();

            TiltEffect.SetIsTiltEnabled(this, true);

            AppSettng = AppSetting.Instance;

            this.DataContext = this;

            Loaded += VoiceCommandSettings_Loaded;
        }

        void VoiceCommandSettings_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._oldLanguage != (InstalledSpeechRecognizers.Default.Language))
            {
                this._oldLanguage = InstalledSpeechRecognizers.Default.Language;
                SpeechLanguage = new System.Globalization.CultureInfo(_oldLanguage)
                    .DisplayName;
            }

            if (this.CommandPrefix.Text.Trim().Length == 0)
            {
                this.CommandPrefix.Text = AppResources.AppName;
            }
        }

        private void SettupCategoriesForVoiceCommand_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettupAccountsForVoiceCommand_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateVoiceCommand_Click(object sender, RoutedEventArgs e)
        {
            UpdateVoiceCommandData();
        }

        private void ChangeNumbericRange_Click(object sender, RoutedEventArgs e)
        {
            ChangeNumbericRange.HasValueChangedCallback = () =>
            {
                CustomMessageBox cm = new CustomMessageBox();
                cm.Title = App.AlertBoxTitle;
                cm.Message = AppResources.ConfirmUpdateVoiceDataWhenPriceChanged;
                cm.LeftButtonContent = AppResources.Update;// "更新";
                cm.IsLeftButtonEnabled = true;

                cm.Content = new TextBlock()
                {
                    Height = 300,
                    Margin = new Thickness(12, 48, 0, 0),
                    Style = Resources["PhoneTextNormalStyle"] as Style,
                    Text = AppResources.Tips_NeedUpdateVoiceDataToEnableSetting,
                };

                cm.RightButtonContent = AppResources.No_After;// "不，稍后";
                cm.IsRightButtonEnabled = true;

                cm.Dismissed += cm_Dismissed;
                cm.Show();
            };


            TinyMoneyManager.Pages.VoiceCommand.ChangeNumbericRange.Go(this);
        }

        void cm_Dismissed(object sender, DismissedEventArgs e)
        {
            if (e.Result == CustomMessageBoxResult.LeftButton)
            {
                UpdateVoiceCommandData();
            }
        }

        private async void UpdateVoiceCommandData()
        {
            this.UpdateVoiceCommand.IsEnabled = false;
            if (this.CommandPrefix.Text.Trim().Length == 0)
            {
                this.Alert(AppResources.SomethingIsRequired.FormatWith(AppResources.VoiceCommandPrefix));

                this.UpdateVoiceCommand.IsEnabled = true;
                return;
            }

            var msg = "";
            try
            {
                AppSettng.VoiceCommandSetting_CommandPrefix = CommandPrefix.Text;

                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(AppResources.VoiceCommandData));

                var lang = InstalledSpeechRecognizers.Default;

                Manager.Instance.GenerateVoiceCommandDefinition(CommandPrefix.Text, lang.Language, AppSettng.VoiceCommandSettingUnitOfPrice);

                var isDone = await Manager.Instance.InitCommand(lang.Language,
                      AppSetting.Instance.EnableVoiceCommand);

                msg = (isDone == InitlaizingStatus.Completed) ? AppResources.Completed : AppResources.Failed;

                this.AlertNotification(msg);

                this.WorkDone();
            }
            catch (Exception ex)
            {
                // Define a variable that holds the error for the speech recognition privacy policy. 
                // This value maps to the SPERR_SPEECH_PRIVACY_POLICY_NOT_ACCEPTED error, 
                // as described in the Windows.Phone.Speech.Recognition error codes section later on.
                const int languageIsNotSupportHResult = unchecked((int)0x80045584);

                const int commandPrefixInvalid = unchecked((int)0x8004555B);

                const int itemElementInvalid = unchecked((int)0x8004556F);
                const int privacyPolicyHResult = unchecked((int)0x80045509);

                // Check whether the error is for the speech recognition privacy policy.
                if (ex.HResult == privacyPolicyHResult)
                {
                    // "You will need to accept the speech privacy policy in order to use speech recognition in this app.";
                    msg = AppResources.Message_RequiredPrivacyAcceptionToUseVoiceCommand;
                }
                // Handle other types of errors here.
                else if (ex.HResult == languageIsNotSupportHResult)
                {
                    msg = AppResources.Formatter_NotSupportTheSpecifiedLanguageInVoiceCommand.FormatWith(SpeechLanguage);
                }
                else if (ex.HResult == commandPrefixInvalid)
                {
                    msg = AppResources.Message_InvalidVoiceCommandPrefix;// "“语音命令前缀”无效，请确保它没有与其他语音命令前缀冲突。";
                }
                else if (ex.HResult == itemElementInvalid)
                {
                    msg = AppResources.Message_ContaionsInvalidAccountOrCategoryNameInVoiceData;// "在语音命令数据中，包含无效的“类别”或者“账户”名称，请确保以上两类信息中不包含一些特殊符号。比如 >,<,& 等等。";
                }
                else
                {
                    msg = ex.Message;
                }

                CustomMessageBox cm = new CustomMessageBox();
                cm.Title = App.AlertBoxTitle;
                cm.Message = AppResources.Message_ErrorOccuredWhenUpdatingVoiceCommandData;// "抱歉，在更新语音命令数据的时候，发生错误。请根据错误信息进行操作，然后重试更新。";

                StackPanel sp = new StackPanel();
                sp.Margin = new Thickness(12, 48, 0, 0);

                var msgBlock = new TextBlock();
                msgBlock.Text = msg;
                msgBlock.Style = Resources["PhoneTextNormalStyle"] as Style;

                sp.Children.Add(msgBlock);
                cm.Content = sp;

                cm.IsLeftButtonEnabled = true;
                cm.LeftButtonContent = AppResources.OK;
                cm.Show();
            }
            finally
            {
                this.UpdateVoiceCommand.IsEnabled = true;
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            ViewModels.SettingPageViewModel.Update();
        }

        private void PrivaryStatement_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox cm = new CustomMessageBox();
            cm.Title = App.AlertBoxTitle;
            cm.Message = AppResources.VoiceCommandSetting;// "您已经更改价格范围，是否立即更新语音命令数据？";
            cm.LeftButtonContent = AppResources.OK;
            cm.IsLeftButtonEnabled = true;

            cm.Content = new TextBlock()
            {
                Height = 400,
                Margin = new Thickness(12, 48, 0, 0),
                Style = Resources["PhoneTextNormalStyle"] as Style,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                Text = AppResources.VoiceCommandSetting_Tips,
            };

            cm.Show();
        }
    }
}