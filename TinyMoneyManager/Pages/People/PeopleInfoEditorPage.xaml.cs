using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using TinyMoneyManager.Component;
using TinyMoneyManager.Component.Common;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using NkjSoft.WPhone;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using Microsoft.Phone.Tasks;

namespace TinyMoneyManager.Pages.People
{
    public partial class PeopleInfoEditorPage : PhoneApplicationPage
    {
        public Controls.PeopleManager.PeopleInfoEditor peopleInfoEditor { get; set; }

        public static Func<Data.Model.PeopleProfile> PeopleProfileGetter;

        public PeopleInfoEditorPage()
        {
            InitializeComponent();

            TiltEffect.SetIsTiltEnabled(this, true);
            this.DataContext = this;
            peopleInfoEditor = new Controls.PeopleManager.PeopleInfoEditor(this);

            this.ContentPanel.Children.Add(peopleInfoEditor);

            Loaded += new RoutedEventHandler(PeopleInfoEditorPage_Loaded);

            InitializeApplicationBar();
        }

        private void InitializeApplicationBar()
        {
            this.ApplicationBar.GetIconButtonFrom(0)
                .Text = AppResources.Save;
            this.ApplicationBar.GetIconButtonFrom(1)
                .Text = AppResources.Cancel;

            this.ApplicationBar.GetMenuItemButtonFrom(0)
                .Text = AppResources.InportFromPhoneContacts;

        }

        bool pageLoaded = false;
        void PeopleInfoEditorPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (pageLoaded)
                return;
            pageLoaded = true;

            this.peopleInfoEditor.InitializeEditObject(PeopleProfileGetter());
            PageTitle.Text = peopleInfoEditor.ControlActionTitle;
            if (peopleInfoEditor.PageAction == PageActionType.Add)
            {
                this.ApplicationBar.GetMenuItemButtonFrom(0).IsEnabled = true ;
            }
            else
            {
                this.ApplicationBar.GetMenuItemButtonFrom(0).IsEnabled = false;
            }

        }

        private void SaveMenuButton_Click(object sender, EventArgs e)
        {
            if (peopleInfoEditor.Save())
            {
                this.SafeGoBack();
            }
        }

        private void CancelMenuButton_Click(object sender, EventArgs e)
        {
            this.peopleInfoEditor = null;
            this.SafeGoBack();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                return;
            }

            if (PeopleProfileGetter == null)
            {
                PeopleProfileGetter = () => new PeopleProfile();
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            PeopleProfileGetter = null;
        }

        private void InportFromPhoneContactsFromPeopleHubMenuitem_Click(object sender, EventArgs e)
        {
            DialogBox.PeopleImporter.ResultGetter = (result) =>
            {
                this.peopleInfoEditor.InitializeEditObject(result);
            };

            this.NavigateTo("/pages/DialogBox/PeopleImporter.xaml?current={0}", peopleInfoEditor.PeopleName.Text);

        }

    }
}