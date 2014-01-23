namespace TinyMoneyManager.Pages.DialogBox
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.UserData;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using PhoneToolkitSample.Data;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using Microsoft.Phone.Shell;

    public partial class PeopleImporter : PhoneApplicationPage
    {
        private bool allPeopleLoaded;
        private LongListSelector currentSelector;

        public static PageActionHandler<PeopleProfile> PeopleSelectorHandler;
        public static System.Action<PeopleProfile> ResultGetter;


        public PeopleImporter()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.currentSelector = this.buddies;
            base.Loaded += new RoutedEventHandler(this.PeopleImporter_Loaded);
            System.Action<ApplicationBarIconButton>[] setters = new System.Action<ApplicationBarIconButton>[] { delegate (ApplicationBarIconButton p) {
                p.Text = AppResources.ShowGroups;
            } };
            base.ApplicationBar.GetIconButtonFrom(0).SetPropertyValue(setters).IsEnabled = false;
        }

        private void _swivelHide_Completed(object sender, System.EventArgs e)
        {
            if (this.currentSelector != null)
            {
                this.currentSelector.CloseGroupView();
                this.currentSelector = null;
            }
        }

        private void buddies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.buddies != null)
            {
                PeopleProfile selectedItem = this.buddies.SelectedItem as PeopleProfile;
                if (selectedItem != null)
                {
                    ResultGetter(selectedItem);
                    this.buddies.SelectedItem = null;
                    this.SafeGoBack();
                }
            }
        }

        private void FliterBox_TextChanged(object sender, RoutedEventArgs e)
        {
            this.PeopleFliterOut.Clear();
        }

        public static void Go(PageActionHandler<PeopleProfile> selectorHandler, PhoneApplicationPage fromPage)
        {
            PeopleSelectorHandler = selectorHandler;
            fromPage.NavigateTo("/pages/DialogBox/PeopleImporter.xaml");
        }

        private void LoadAllPeople()
        {
            System.Threading.WaitCallback callBack = null;
            this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.People }));
            if (AllPeople.Current.HasLoaded)
            {
                this.PeopleList = new PeopleByFirstName();
                base.DataContext = this;
                this.WorkDone();
            }
            else if (PeopleSelectorHandler == null)
            {
                this.LoadPeopleListFromPhoneContacts();
            }
            else
            {
                if (callBack == null)
                {
                    callBack = delegate(object o)
                    {
                        System.Collections.Generic.Dictionary<Int32, PeopleProfile> _peoples = new System.Collections.Generic.Dictionary<int, PeopleProfile>();
                        int count = 1;
                        PeopleSelectorHandler.OnGetItems().ForEach<PeopleProfile>(delegate(PeopleProfile p)
                        {
                            _peoples.Add(count++, p);
                        });
                        AllPeople.AllPeopleGetter = () => _peoples;
                        AllPeople.Current.HasLoaded = true;
                        this.InvokeInThread(delegate
                        {
                            this.PeopleList = new PeopleByFirstName();
                            base.DataContext = this;
                            this.WorkDone();
                        });
                    };
                }
                System.Threading.ThreadPool.QueueUserWorkItem(callBack);
            }
        }

        public void LoadPeopleListFromPhoneContacts()
        {
            Contacts contacts = new Contacts();
            contacts.SearchCompleted += delegate(object o, ContactsSearchEventArgs e)
            {
                System.Action a = null;
                if (e.Results != null)
                {
                    int key = 0;
                    System.Collections.Generic.Dictionary<Int32, PeopleProfile> resultToShow = new System.Collections.Generic.Dictionary<int, PeopleProfile>();
                    foreach (Contact contact in e.Results)
                    {
                        PeopleProfile profile2 = new PeopleProfile
                        {
                            Name = contact.DisplayName,
                            Notes = contact.Notes.FirstOrDefault<string>()
                        };
                        PeopleProfile profile = profile2;
                        if (contact.CompleteName != null)
                        {
                            profile.FirstName = contact.CompleteName.FirstName;
                            profile.LastName = contact.CompleteName.LastName;
                        }
                        if (contact.PhoneNumbers.Count<ContactPhoneNumber>() > 0)
                        {
                            ContactPhoneNumber number = contact.PhoneNumbers.FirstOrDefault<ContactPhoneNumber>(x => x.Kind == PhoneNumberKind.Mobile);
                            profile.Telephone = (number == null) ? string.Empty : number.PhoneNumber;
                        }
                        if (contact.Addresses.Count<ContactAddress>() > 0)
                        {
                            ContactAddress address = contact.Addresses.FirstOrDefault<ContactAddress>(x => x.Kind == AddressKind.Home);
                            profile.HomeAddress = ((address == null) || (address.PhysicalAddress == null)) ? string.Empty : address.PhysicalAddress.ToString();
                        }
                        if (contact.EmailAddresses.Count<ContactEmailAddress>() > 0)
                        {
                            ContactEmailAddress address2 = contact.EmailAddresses.FirstOrDefault<ContactEmailAddress>(x => x.Kind == EmailAddressKind.Personal);
                            profile.PersonalEmail = (address2 == null) ? string.Empty : address2.EmailAddress;
                        }
                        key++;
                        resultToShow.Add(key, profile);
                    }
                    AllPeople.AllPeopleGetter = () => resultToShow;
                    AllPeople.Current.HasLoaded = true;
                    if (a == null)
                    {
                        a = delegate
                        {
                            this.PeopleList = new PeopleByFirstName();
                            base.DataContext = this;
                            this.WorkDone();
                        };
                    }
                    base.Dispatcher.BeginInvoke(a);
                }
            };
            contacts.SearchAsync(string.Empty, FilterKind.None, null);
        }

        private void LongListSelector_GroupViewClosing(object sender, GroupViewClosingEventArgs e)
        {
            e.Cancel = true;
            if (e.SelectedGroup != null)
            {
                this.currentSelector.ScrollToGroup(e.SelectedGroup);
            }
            base.Dispatcher.BeginInvoke(delegate
            {
                QuadraticEase ease = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseOut
                };
                IEasingFunction function = ease;
                Storyboard storyboard = new Storyboard();
                ItemsControl itemsControl = e.ItemsControl;
                foreach (object obj2 in itemsControl.Items)
                {
                    UIElement reference = itemsControl.ItemContainerGenerator.ContainerFromItem(obj2) as UIElement;
                    if (reference != null)
                    {
                        Border child = VisualTreeHelper.GetChild(reference, 0) as Border;
                        if (child != null)
                        {
                            DoubleAnimationUsingKeyFrames element = new DoubleAnimationUsingKeyFrames();
                            EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame
                            {
                                KeyTime = System.TimeSpan.FromMilliseconds(0.0),
                                Value = 0.0,
                                EasingFunction = function
                            };
                            EasingDoubleKeyFrame frame2 = new EasingDoubleKeyFrame
                            {
                                KeyTime = System.TimeSpan.FromMilliseconds(125.0),
                                Value = 90.0,
                                EasingFunction = function
                            };
                            element.KeyFrames.Add(frame);
                            element.KeyFrames.Add(frame2);
                            Storyboard.SetTargetProperty(element, new PropertyPath(PlaneProjection.RotationXProperty));
                            Storyboard.SetTarget(element, child.Projection);
                            storyboard.Children.Add(element);
                        }
                    }
                }
                storyboard.Completed += new System.EventHandler(this._swivelHide_Completed);
                storyboard.Begin();
            });
        }

        private void LongListSelector_GroupViewOpened(object sender, GroupViewOpenedEventArgs e)
        {
            this.currentSelector = sender as LongListSelector;
            QuadraticEase ease = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            };
            IEasingFunction function = ease;
            Storyboard storyboard = new Storyboard();
            ItemsControl itemsControl = e.ItemsControl;
            foreach (object obj2 in itemsControl.Items)
            {
                UIElement reference = itemsControl.ItemContainerGenerator.ContainerFromItem(obj2) as UIElement;
                if (reference != null)
                {
                    Border child = VisualTreeHelper.GetChild(reference, 0) as Border;
                    if (child != null)
                    {
                        DoubleAnimationUsingKeyFrames element = new DoubleAnimationUsingKeyFrames();
                        EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame
                        {
                            KeyTime = System.TimeSpan.FromMilliseconds(0.0),
                            Value = -60.0,
                            EasingFunction = function
                        };
                        EasingDoubleKeyFrame frame2 = new EasingDoubleKeyFrame
                        {
                            KeyTime = System.TimeSpan.FromMilliseconds(85.0),
                            Value = 0.0,
                            EasingFunction = function
                        };
                        element.KeyFrames.Add(frame);
                        element.KeyFrames.Add(frame2);
                        Storyboard.SetTargetProperty(element, new PropertyPath(PlaneProjection.RotationXProperty));
                        Storyboard.SetTarget(element, child.Projection);
                        storyboard.Children.Add(element);
                    }
                }
            }
            storyboard.Begin();
        }

        private void PeopleImporter_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.allPeopleLoaded)
            {
                this.LoadAllPeople();
                this.allPeopleLoaded = true;
                base.ApplicationBar.GetIconButtonFrom(0).IsEnabled = true;
            }
        }

        private void showGroups_Click(object sender, System.EventArgs e)
        {
            this.currentSelector = this.buddies;
            this.currentSelector.DisplayGroupView();
        }

        public PageActionType PageAction { get; set; }

        public ObservableCollection<PeopleProfile> PeopleFliterOut { get; set; }

        public PeopleByFirstName PeopleList { get; set; }

        public ObservableCollection<PeopleProfile> Peoples { get; set; }
    }
}

