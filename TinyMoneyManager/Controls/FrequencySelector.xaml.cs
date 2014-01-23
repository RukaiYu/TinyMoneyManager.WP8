namespace TinyMoneyManager.Controls
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Common;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.DialogBox;

    public partial class FrequencySelector : UserControl
    {
        private FrequencyInfo frequency;

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(FrequencySelector), new PropertyMetadata(AppResources.Frequency));

        private static string[] localzedDayOfWeek;
        private PhoneApplicationPage pageAttachedTo;

        public FrequencySelector()
            : this(null)
        {
        }

        public FrequencySelector(PhoneApplicationPage page)
        {
            this.InitializeComponent();
            this.pageAttachedTo = page;
            this.frequency = new FrequencyInfo();
            base.DataContext = this;
            this.DayOfWeekSelector.ItemsSource = LocalzedDayOfWeek;
            this.Frequency_EveryMonth_Day_Value.Text = this.Frequency_EveryMonth_Day_Value.Text = AppResources.FrequencyDayOfMonthFormatter.FormatWith(new object[] { this.frequency.Day });
            this.FrequencyType.ItemCountThreshold = 7;
        }

        private void DayOfWeekSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.frequency.DayOfWeek = (System.DayOfWeek)this.DayOfWeekSelector.SelectedIndex;
        }

        private void Frequency_EveryMonth_Day_Value_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DaySelectorPage.AfterConfirmed = delegate(int v)
            {
                this.frequency.Day = v;
                this.Frequency_EveryMonth_Day_Value.Text = AppResources.FrequencyDayOfMonthFormatter.FormatWith(new object[] { v });
            };
            this.pageAttachedTo.NavigateTo("/Pages/DialogBox/DaySelectorPage.xaml?title={0}&defValue={1}", new object[] { AppResources.SelectDayOfMonth, this.frequency.Day });
        }

        private void FrequencyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FrequencyType != null)
            {
                ScheduleType frq = (ScheduleType)System.Convert.ToInt32(((ListPickerItem)this.FrequencyType.SelectedItem).Tag);
                this.SwitchUIByFrequency(frq);
            }
        }

        private void SwitchUIByFrequency(ScheduleType frq)
        {
            this.DayOfWeekSelector.Visibility = Visibility.Collapsed;
            this.Frequency_EveryMonth_Day_Value.Visibility = Visibility.Collapsed;
            if (frq == ScheduleType.EveryDay)
            {
                this.frequency.Frequency = ScheduleType.EveryDay;
            }
            else if (frq == ScheduleType.EveryWeek)
            {
                this.DayOfWeekSelector.Visibility = Visibility.Visible;
                this.frequency.Frequency = ScheduleType.EveryWeek;
            }
            else if (frq == ScheduleType.EveryMonth)
            {
                this.Frequency_EveryMonth_Day_Value.Visibility = Visibility.Visible;
                this.frequency.Frequency = ScheduleType.EveryMonth;
            }
            else if (frq == ScheduleType.Workday)
            {
                this.frequency.Frequency = ScheduleType.Workday;
            }
            else if (frq == ScheduleType.Weekend)
            {
                this.frequency.Frequency = ScheduleType.Weekend;
            }
            else if (frq == ScheduleType.SpecificDate)
            {
                this.frequency.Frequency = ScheduleType.SpecificDate;
            }
        }

        public void Update(TallySchedule scheduleDataInfo)
        {
            this.DayOfWeekSelector.SelectedIndex = (int)scheduleDataInfo.DayofWeek.Value;
            this.frequency.Frequency = scheduleDataInfo.Frequency;
            this.frequency.Day = scheduleDataInfo.ValueForFrequency.GetValueOrDefault(System.DateTime.Now.Day);
            this.Frequency_EveryMonth_Day_Value.Text = AppResources.FrequencyDayOfMonthFormatter.FormatWith(new object[] { this.frequency.Day });
            if (this.frequency.Frequency == ScheduleType.EveryMonth)
            {
                this.FrequencyType.SelectedIndex = 0;
            }
            else if (this.frequency.Frequency == ScheduleType.EveryWeek)
            {
                this.FrequencyType.SelectedIndex = 1;
            }
            else if (this.frequency.Frequency == ScheduleType.EveryDay)
            {
                this.FrequencyType.SelectedIndex = 2;
            }
            else if (this.frequency.Frequency == ScheduleType.Workday)
            {
                this.FrequencyType.SelectedIndex = 3;
            }
            else if (this.frequency.Frequency == ScheduleType.Weekend)
            {
                this.FrequencyType.SelectedIndex = 4;
            }
        }

        public FrequencyInfo Frequency
        {
            get
            {
                return this.frequency;
            }
            set
            {
                if (this.frequency != value)
                {
                    this.frequency = value;
                }
            }
        }

        public string Header
        {
            get
            {
                return (string)base.GetValue(HeaderProperty);
            }
            set
            {
                base.SetValue(HeaderProperty, value);
            }
        }

        public static string[] LocalzedDayOfWeek
        {
            get
            {
                if (localzedDayOfWeek == null)
                {
                    localzedDayOfWeek = new string[] { LocalizedStrings.CultureName.DateTimeFormat.GetDayName(System.DayOfWeek.Sunday), LocalizedStrings.CultureName.DateTimeFormat.GetDayName(System.DayOfWeek.Monday), LocalizedStrings.CultureName.DateTimeFormat.GetDayName(System.DayOfWeek.Tuesday), LocalizedStrings.CultureName.DateTimeFormat.GetDayName(System.DayOfWeek.Wednesday), LocalizedStrings.CultureName.DateTimeFormat.GetDayName(System.DayOfWeek.Thursday), LocalizedStrings.CultureName.DateTimeFormat.GetDayName(System.DayOfWeek.Friday), LocalizedStrings.CultureName.DateTimeFormat.GetDayName(System.DayOfWeek.Saturday) };
                }
                return localzedDayOfWeek;
            }
        }

        public PhoneApplicationPage PageAttachedTo
        {
            get
            {
                return this.pageAttachedTo;
            }
            set
            {
                this.pageAttachedTo = value;
            }
        }
    }
}

