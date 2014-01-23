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
        private int _pivotIndexTo;
        private bool _hasLoadPage;

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

            this.Loaded += PeopleProfileViewerPage_Loaded;
        }

        void PeopleProfileViewerPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._hasLoadPage)
            {
                this._hasLoadPage = true;

                System.Guid id = this.GetNavigatingParameter("id", null).ToGuid();
                _pivotIndexTo = this.GetNavigatingParameter("pivotTo", 0).ToInt32();
                this.LoadPeopleInfo(id);
            }
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
                PeopleProfile item = ViewModelLocator.PeopleViewModel.AccountBookDataContext.Peoples.FirstOrDefault<PeopleProfile>(p => p.Id == id);
                if (item == null)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        this.ContentPanel.Children.Clear();
                        TextBlock block = new TextBlock
                        {
                            Text = AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { AppResources.Profile.ToLowerInvariant() })
                        };
                        this.ContentPanel.Children.Add(block);

                        this.WorkDone();
                    });
                }
                else
                {
                    this.Dispatcher.BeginInvoke(delegate
                    {
                        this.current = item;
                        this.peopleProfileViewer.InitializeViewing(this.current, this._pivotIndexTo);
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
            }
        }

        internal static void View(System.Guid peopleId, PhoneApplicationPage fromPage, int? pivotIndexTo = 0)
        {
            fromPage.NavigateTo("/Pages/People/PeopleProfileViewerPage.xaml?id={0}&pivotTo={1}", peopleId, pivotIndexTo);
        }
    }
}

