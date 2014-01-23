namespace TinyMoneyManager.Controls.PeopleManager
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.Linq;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.ViewModels;

    public partial class PeopleProfileViewer : UserControl
    {

        public PhoneApplicationPage attachedPage;

        public PeopleViewModel peopleProfileViewModel;

        public bool relatedItemsLoaded;
        public PeopleProfileViewer(PhoneApplicationPage attachedPage)
        {
            this.InitializeComponent();
            this.attachedPage = attachedPage;
            this.peopleProfileViewModel = ViewModelLocator.PeopleViewModel;
        }

        public void InitializeViewing(PeopleProfile peopleProfile)
        {
            this.Current = peopleProfile;
            base.DataContext = this;
        }

        private void NameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            this.attachedPage.NavigateToEditValueInTextBoxEditorPage(AppResources.Name, this.Current.Name, delegate(TextBox t)
            {
                t.SelectAll();
            }, delegate(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    this.AlertNotification("{0} {1}".FormatWith(new object[] { AppResources.Name, AppResources.EmptyTextMessage }), null);
                    return false;
                }
                return true;
            }, delegate(string s)
            {
                this.Current.Name = s;
                this.peopleProfileViewModel.Update(this.Current);
            });
        }

        private void PersonalEmailPanel_Click(object sender, RoutedEventArgs e)
        {
            this.attachedPage.NavigateToEditValueInTextBoxEditorPage(AppResources.PersonalEmailAddress, this.Current.PersonalEmail, delegate(TextBox t)
            {
                t.SelectAll();
            }, delegate(string email)
            {
                bool flag = AppSetting.IsEmail(email);
                if (!flag)
                {
                    this.AlertNotification(AppResources.NotAvaliableEmailAddressMessage, null);
                }
                return flag;
            }, delegate(string s)
            {
                this.Current.PersonalEmail = s;
                this.peopleProfileViewModel.Update(this.Current);
            });
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((this.MainPivot.SelectedIndex == 1) && !this.relatedItemsLoaded)
            {
                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.Related }));
                this.relatedItemsLoaded = true;

                System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                    {
                        ObservableCollection<GroupByCreateTimeAccountItemViewModel> data = this.peopleProfileViewModel.LoadRelatedItems(this.Current);
                        base.Dispatcher.BeginInvoke(delegate
                        {
                            this.RelatedItemsListControl.ItemsSource = data.OrderByDescending(p => p.Key);
                            this.WorkDone();
                        });
                    });
            }
        }

        private void RelatedItemsListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountItem selectedItem = (sender as LongListSelector).SelectedItem as AccountItem;
            if (selectedItem != null)
            {
                NavigationDataInfoFromAccountItem dataProvider = new NavigationDataInfoFromAccountItem
                {
                    Key = selectedItem.Id,
                    PageName = selectedItem.PageNameGetter
                };
                TinyMoneyManagerNavigationService.Instance.Navigate(this.attachedPage, dataProvider);
                this.RelatedItemsListControl.SelectedItem = null;
            }
        }

        public PeopleProfile Current { get; set; }
    }
}

