namespace TinyMoneyManager.Pages.People
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls.PeopleManager;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public partial class PeopleProfileViewerPage : PhoneApplicationPage
    {

        public ApplicationBar applicationBarForProfilePivot;

        private PeopleProfile current;

        public static System.Func<PeopleProfile> PeopleProfileGetter;
        private PeopleProfileViewer peopleProfileViewer;

        public PeopleProfileViewerPage()
        {
            this.InitializeComponent();
            this.InitializeApplicationBar();
            this.peopleProfileViewer = new PeopleProfileViewer(this);
            this.peopleProfileViewer.MainPivot.SelectionChanged += new SelectionChangedEventHandler(this.MainPivot_SelectionChanged);
            TiltEffect.SetIsTiltEnabled(this, true);
            TextBlock block = new TextBlock
            {
                Text = AppResources.Loading,
                Style = base.Resources["PhoneTextNormalStyle"] as Style
            };
            this.ContentPanel.Children.Add(block);
        }

        private void deleteButton_Click(object sender, System.EventArgs e)
        {
            this.AlertConfirm(AppResources.DeleteAccountItemMessage, delegate
            {
                this.peopleProfileViewer.peopleProfileViewModel.Delete<PeopleProfile>(this.current);
                this.SafeGoBack();
            }, AppResources.DeletingObject.FormatWith(new object[] { AppResources.People }));
        }

        private void editButton_Click(object sender, System.EventArgs e)
        {
            PeopleInfoEditorPage.PeopleProfileGetter = () => this.current;
            this.NavigateTo("/Pages/People/PeopleInfoEditorPage.xaml?action=view");
        }

        public void InitializeApplicationBar()
        {
            this.applicationBarForProfilePivot = new ApplicationBar();
            ApplicationBarIconButton button = IconUirs.CreateDeleteButton();
            button.Click += new System.EventHandler(this.deleteButton_Click);
            ApplicationBarIconButton button2 = IconUirs.CreateEditButton();
            button2.Click += new System.EventHandler(this.editButton_Click);
            this.applicationBarForProfilePivot.Buttons.Add(button2);
            this.applicationBarForProfilePivot.Buttons.Add(button);
            base.ApplicationBar = this.applicationBarForProfilePivot;
        }



        private void LoadPeopleInfo(System.Guid id)
        {
            this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.Profile.ToLowerInvariant() }));
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                System.Action a = null;

                PeopleProfile item = ViewModelLocator.PeopleViewModel.AccountBookDataContext.Peoples.FirstOrDefault<PeopleProfile>(p => p.Id == id);
                if (item == null)
                {
                    if (a == null)
                    {
                        a = delegate
                        {
                            this.ContentPanel.Children.Clear();
                            TextBlock block = new TextBlock
                            {
                                Text = AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { AppResources.Profile.ToLowerInvariant() })
                            };
                            this.ContentPanel.Children.Add(block);
                        };
                    }
                    this.Dispatcher.BeginInvoke(a);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(delegate
                    {
                        this.current = item;
                        this.peopleProfileViewer.InitializeViewing(this.current);
                        this.ContentPanel.Children.Clear();
                        this.ContentPanel.Children.Add(this.peopleProfileViewer);
                        this.WorkDone();
                    });
                }
            });
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.peopleProfileViewer != null)
            {
                base.ApplicationBar.IsVisible = this.peopleProfileViewer.MainPivot.SelectedIndex == 0;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.peopleProfileViewer.relatedItemsLoaded = false;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                System.Guid id = this.GetNavigatingParameter("id", null).ToGuid();
                this.LoadPeopleInfo(id);
            }
        }

        internal static void View(System.Guid peopleId, PhoneApplicationPage fromPage)
        {
            fromPage.NavigateTo("/Pages/People/PeopleProfileViewerPage.xaml?id={0}", new object[] { peopleId });
        }
    }
}

