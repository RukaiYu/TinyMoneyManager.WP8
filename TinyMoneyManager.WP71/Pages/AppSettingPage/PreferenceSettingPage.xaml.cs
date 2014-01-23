namespace TinyMoneyManager.Pages.AppSettingPage
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.CustomizedTally;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.ViewModels.CustomizedTallyManager;

    public partial class PreferenceSettingPage : PhoneApplicationPage
    {
        private ApplicationBar AppbarForDeletingItem;

        private bool hasRegistered;

        public PreferenceSettingPage()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.ActionAfterAddNewRecord.DataContext = AppSetting.Instance;
            base.DataContext = AppSetting.Instance;
            this.InitializeDataBind();
            base.Loaded += new RoutedEventHandler(this.PreferenceSettingPage_Loaded);
            this.LoadContent();
            this.CustomizedTallyViewModel = ViewModelLocator.CustomizedTallyViewModel;
            this.RulesPivot.DataContext = this.CustomizedTallyViewModel;

        }

        void Rulelistbox_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void ActionAfterAddNewRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSetting.Instance.ActionAfterAddNewRecord = (AfterAddNewRecordAction)this.ActionAfterAddNewRecord.SelectedIndex;

        }

        private void addButton_Click(object sender, System.EventArgs e)
        {
            CustomizedTallyItemEditorPage.NavigateTo(this, null, PageActionType.Add);
        }

        private void BudgetProjectItemButton_Click(object sender, RoutedEventArgs e)
        {
            TallySchedule tag = (sender as HyperlinkButton).Tag as TallySchedule;
            if (tag != null)
            {
                CustomizedTallyPage.Go(this, tag);
            }
        }

        private void delButon_Click(object sender, System.EventArgs e)
        {
        }

        private void Delete_Item_Click(object sender, RoutedEventArgs e)
        {
            TallySchedule tag = (sender as MenuItem).Tag as TallySchedule;
            if (tag != null)
            {
                this.CustomizedTallyViewModel.DeletingObjectService<TallySchedule>(tag, p => AppResources.TallyTemplate.ToLowerInvariant(), null);
            }
        }

        private void Edit_Item_Click(object sender, RoutedEventArgs e)
        {
            TallySchedule tag = (sender as MenuItem).Tag as TallySchedule;
            if (tag != null)
            {
                CustomizedTallyItemEditorPage.NavigateTo(this, tag, PageActionType.Edit);
            }
        }

        private void ff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ff.SelectedItem != null)
            {
                System.Collections.Generic.KeyValuePair<ScheduleType, String> selectedItem = (System.Collections.Generic.KeyValuePair<TinyMoneyManager.Data.Model.ScheduleType, string>)this.ff.SelectedItem;
                if (((TinyMoneyManager.Data.Model.ScheduleType)selectedItem.Key) != this.ScheduleType)
                {
                    this.ScheduleType = selectedItem.Key;
                    this.CustomizedTallyViewModel.IsDataLoaded = false;
                    this.ToggleCategoryTypeButtonTitle.Text = (AppResources.AppearPeriod + " : " + selectedItem.Value).ToLowerInvariant();
                    this.LoadTemplates();
                }
            }
        }

        public void InitializeDataBind()
        {
            this.ActionAfterAddNewRecord.SelectedIndex = (int)AppSetting.Instance.ActionAfterAddNewRecord;
            this.ActionAfterAddNewRecord.SelectionChanged += new SelectionChangedEventHandler(this.ActionAfterAddNewRecord_SelectionChanged);
            this.ff.FullModeHeader = LocalizedStrings.GetCombinedText(AppResources.Choose, AppResources.AppearPeriod, false).ToUpperInvariant();
            System.Collections.Generic.Dictionary<ScheduleType, String> dictionary2 = new System.Collections.Generic.Dictionary<TinyMoneyManager.Data.Model.ScheduleType, string>();
            dictionary2.Add(TinyMoneyManager.Data.Model.ScheduleType.None, AppResources.All.ToLowerInvariant());
            dictionary2.Add(TinyMoneyManager.Data.Model.ScheduleType.EveryMonth, AppResources.EveryMonth.ToLowerInvariant());
            dictionary2.Add(TinyMoneyManager.Data.Model.ScheduleType.EveryWeek, AppResources.EveryWeek.ToLowerInvariant());
            dictionary2.Add(TinyMoneyManager.Data.Model.ScheduleType.EveryDay, AppResources.EveryDay.ToLowerInvariant());
            dictionary2.Add(TinyMoneyManager.Data.Model.ScheduleType.Workday, AppResources.Workday.ToLowerInvariant());
            dictionary2.Add(TinyMoneyManager.Data.Model.ScheduleType.Weekend, AppResources.Weekend.ToLowerInvariant());
            System.Collections.Generic.Dictionary<ScheduleType, String> dictionary = dictionary2;
            this.ff.ItemsSource = dictionary;
            this.ToggleCategoryTypeButtonTitle.Text = (AppResources.AppearPeriod + " : " + AppResources.All).ToLowerInvariant();
        }

        private void LoadContent()
        {
            this.MainPivot.SelectionChanged += new SelectionChangedEventHandler(this.MainPivot_SelectionChanged);
            this.applicationBar = new ApplicationBar();
            ApplicationBarIconButton button = IconUirs.CreateIconButton(AppResources.Add, IconUirs.AddPlusIconButton);
            button.Click += new System.EventHandler(this.addButton_Click);
            this.applicationBar.Buttons.Add(button);
            this.AppbarForDeletingItem = new ApplicationBar();
            ApplicationBarIconButton button2 = IconUirs.CreateDeleteButton();
            button2.Click += new System.EventHandler(this.delButon_Click);
            this.AppbarForDeletingItem.Buttons.Add(button2);
        }

        private void LoadTemplates()
        {
            TinyMoneyManager.Data.Model.ScheduleType scheduleType = this.ScheduleType;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                this.CustomizedTallyViewModel.LoadDataIfNot();
            });
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = this.MainPivot.SelectedIndex;
            base.ApplicationBar = (selectedIndex == 0) ? null : ((IApplicationBar)this.applicationBar);
            if (selectedIndex == 1)
            {
                this.LoadTemplates();
            }
        }

        private void MultiselectList_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var list = sender as LongListMultiSelector;
            if (list != null)
            {
                this.MainPivot.IsLocked = list.IsSelectionEnabled;
                base.ApplicationBar = list.IsSelectionEnabled ? this.AppbarForDeletingItem : this.applicationBar;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (this.MainPivot.IsLocked)
            {
                this.MainPivot.IsLocked = false;
                this.Rulelistbox.IsSelectionEnabled = false;
                e.Cancel = true;
            }
            else
            {
                SettingPageViewModel.Update();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                int num = e.GetNavigatingParameter("goto", null).ToInt32();
                this.MainPivot.SelectedIndex = num;
            }
        }

        private void PreferenceSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ToggleCategoryTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.hasRegistered)
            {
                this.ff.SelectionChanged += new SelectionChangedEventHandler(this.ff_SelectionChanged);
                this.hasRegistered = true;
            }
            this.ff.Open();
        }

        public ApplicationBar applicationBar { get; set; }

        public TinyMoneyManager.ViewModels.CustomizedTallyManager.CustomizedTallyViewModel CustomizedTallyViewModel { get; set; }

        private TinyMoneyManager.Data.Model.ScheduleType ScheduleType
        {
            get
            {
                return this.CustomizedTallyViewModel.ScheduleType;
            }
            set
            {
                this.CustomizedTallyViewModel.ScheduleType = value;
            }
        }
    }
}

