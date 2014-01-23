using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager.Pages.VoiceCommand
{
    using NkjSoft.Extensions;
    public partial class ChangeNumbericRange : PhoneApplicationPage
    {
        private double _maximumValue;
        private double _mininumValue;
        private bool _hasInitializedData;
        public static Action HasValueChangedCallback;

        public ChangeNumbericRange()
        {
            InitializeComponent();

            TiltEffect.SetIsTiltEnabled(this, true);

            Loaded += ChangeNumbericRange_Loaded;
        }

        void ChangeNumbericRange_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._hasInitializedData)
            {
                this._hasInitializedData = true;
                this.UnitValue.Text = AppSetting.Instance.VoiceCommandSettingUnitOfPrice;
                ResetValues();
            }
        }

        /// <summary>
        /// Resets the values.
        /// </summary>
        private void ResetValues()
        {
            this._mininumValue = AppSetting.Instance.VoiceCommandSettingMininumValue;
            this.MininumValue.Text = this._mininumValue.ToString();

            MaximumValue.Maximum = Math.Round(((this._mininumValue) + 1999.0), 2);
            this.MaximumValue.ValueChanged += MaximumValue_ValueChanged_1;

            this.MaximumValue.Value = AppSetting.Instance.VoiceCommandSettingMaximumValue;
            this.NeedDigits.IsChecked = AppSetting.Instance.VoiceCommandSettingWithDigits;

        }

        /// <summary>
        /// Goes the specified from page.
        /// </summary>
        /// <param name="fromPage">From page.</param>
        public static void Go(PhoneApplicationPage fromPage)
        {
            fromPage.NavigationService.Navigate(new Uri("/Pages/VoiceCommand/ChangeNumbericRange.xaml", UriKind.RelativeOrAbsolute));
        }

        private void MininumValue_LostFocus_1(object sender, RoutedEventArgs e)
        {
            if (MaximumValue != null)
            {
                double.TryParse(MininumValue.Text, out this._mininumValue);

                MaximumValue.Maximum = Math.Round(((this._mininumValue) + 1999.0), 2);
            }
        }

        private void MaximumValue_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MaximumValueShower != null)
            {
                this._maximumValue = (Math.Round(((double)(MaximumValue.Value)) * 2) / 2);

                MaximumValueShower.Text = this._maximumValue.ToString();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var hasChanged = false;

            hasChanged = AppSetting.Instance.VoiceCommandSettingMininumValue != this._mininumValue;

            if (!hasChanged)
            {
                hasChanged = AppSetting.Instance.VoiceCommandSettingMininumValue != this._mininumValue;
            }

            if (!hasChanged)
            {
                hasChanged = AppSetting.Instance.VoiceCommandSettingWithDigits != NeedDigits.IsChecked.GetValueOrDefault();
            }

            AppSetting.Instance.VoiceCommandSettingMininumValue = this._mininumValue;
            AppSetting.Instance.VoiceCommandSettingMaximumValue = this._maximumValue;
            AppSetting.Instance.VoiceCommandSettingWithDigits = NeedDigits.IsChecked.GetValueOrDefault();
            AppSetting.Instance.VoiceCommandSettingUnitOfPrice = UnitValue.Text;

            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }

            if (hasChanged)
            {
                if (HasValueChangedCallback != null)
                {
                    HasValueChangedCallback();
                }
            }
        }

        private void NeedDigits_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (MaximumValue != null)
            {
                this.MaximumValue.LargeChange = 1.0;
                this.MaximumValue.SmallChange = 1.0;
            }
        }

        private void NeedDigits_Checked_1(object sender, RoutedEventArgs e)
        {
            if (MaximumValue != null)
            {
                this.MaximumValue.LargeChange = 0.5;
                this.MaximumValue.SmallChange = 0.5;
            }
        }
    }
}