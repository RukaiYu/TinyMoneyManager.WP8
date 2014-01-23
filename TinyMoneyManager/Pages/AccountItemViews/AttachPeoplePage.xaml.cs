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
using TinyMoneyManager.Data.Model;
using System.Windows.Navigation;
using TinyMoneyManager.Component;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Language;
using TinyMoneyManager.ViewModels;
using System.Collections.ObjectModel;
using TinyMoneyManager.Pages.People;
using TinyMoneyManager.Pages.DialogBox;

namespace TinyMoneyManager.Pages.AccountItemViews
{
    public partial class AttachPeoplePage : PhoneApplicationPage
    {
        private PageActionHandler<PeopleProfile> peopleChooserHandler;
        private static PageActionHandler<PeopleAssociationData> peopleSelectorHandler;
        public PeopleAssociationDataViewModel peopleViewModel;

        public AttachPeoplePage()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.Peoples = new ObservableCollection<PeopleAssociationData>();
            this.initAppBar();
            this.peopleViewModel = new PeopleAssociationDataViewModel();
            base.DataContext = this;
        }

        private void BrowserLib_Click(object sender, System.EventArgs e)
        {
            System.Action<PeopleProfile> action = null;
            if (this.peopleChooserHandler == null)
            {
                this.peopleChooserHandler = new PageActionHandler<PeopleProfile>();
                this.peopleChooserHandler.GetItems = () => ViewModelLocator.PeopleViewModel.AccountBookDataContext.Peoples.AsEnumerable<PeopleProfile>();
                if (action == null)
                {
                    action = delegate(PeopleProfile result)
                    {
                        PeopleAssociationData item = new PeopleAssociationData
                        {
                            PeopleInfo = result,
                            AttachedId = this.CurrentAttachedObjectId
                        };
                        this.Peoples.Add(item);
                        PeopleSelectorHandler.OnAfterAdd(item);
                    };
                }
                PeopleImporter.ResultGetter = action;
            }
            PeopleImporter.Go(this.peopleChooserHandler, this);
        }

        private void DeleteBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            System.Action callBack = null;
            PeopleAssociationData item = this.GetPeopleInfo(sender);
            try
            {
                if (callBack == null)
                {
                    callBack = delegate
                    {
                        this.AlertNotification(AppResources.OperationSuccessfullyMessage, null);
                        this.Peoples.Remove(item);
                        peopleSelectorHandler.OnAfterDelete(item);
                    };
                }
                this.peopleViewModel.DeletingObjectService<PeopleAssociationData>(item, i => AppResources.People, callBack);
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void EditBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            System.Action<String> resultSetter = null;
            PeopleAssociationData item = this.GetPeopleInfo(sender);
            if (item != null)
            {
                if (resultSetter == null)
                {
                    resultSetter = delegate(string s)
                    {
                        item.Comments = s;
                        peopleSelectorHandler.OnUpdate(item);
                    };
                }
                this.NavigateToEditValueInTextBoxEditorPage(AppResources.Comments, item.Comments, delegate(TextBox t)
                {
                    t.SelectAll();
                }, null, resultSetter);
            }
        }

        public PeopleAssociationData GetPeopleInfo(object sender)
        {
            return ((sender as MenuItem).Tag as PeopleAssociationData);
        }

        public static void Go(System.Guid attachedId, PhoneApplicationPage fromPage)
        {
            fromPage.NavigateTo("/pages/AccountItemViews/AttachPeoplePage.xaml?id={0}", new object[] { attachedId });
        }

        private void initAppBar()
        {
            base.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.LinkTo.ToLowerInvariant();
        }


        private void LoadPeoples(System.Guid attachedPeopleId)
        {
            System.Threading.WaitCallback callBack = null;
            System.Action a = null;
            this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.People }));
            this.Peoples.Clear();
            if (peopleSelectorHandler != null)
            {
                if (callBack == null)
                {
                    callBack = delegate(object o)
                    {
                        this.peopleViewModel.QueryPeoplesForAttachedId(attachedPeopleId, delegate(PeopleAssociationData item)
                        {
                            this.Dispatcher.BeginInvoke(delegate
                            {
                                this.Peoples.Add(item);
                            });
                        });
                        this.WorkDone();
                    };
                }
                System.Threading.ThreadPool.QueueUserWorkItem(callBack);
            }
            else
            {
                if (a == null)
                {
                    a = delegate
                    {
                        this.WorkDone();
                    };
                }
                base.Dispatcher.BeginInvoke(a);
            }
        }

        private void menuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PeopleAssociationData selectedItem = this.menuList.SelectedItem as PeopleAssociationData;
            if (selectedItem != null)
            {
                PeopleProfileViewerPage.View(selectedItem.PeopleId, this);
                this.menuList.SelectedItem = null;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.CurrentAttachedObjectId = this.GetNavigatingParameter("id", null).ToGuid();
                this.LoadPeoples(this.CurrentAttachedObjectId);
            }
        }

        public System.Guid CurrentAttachedObjectId { get; set; }

        public ObservableCollection<PeopleAssociationData> Peoples { get; set; }

        public static PageActionHandler<PeopleAssociationData> PeopleSelectorHandler
        {
            get
            {
                if (peopleSelectorHandler == null)
                {
                    peopleSelectorHandler = new PageActionHandler<PeopleAssociationData>();
                }
                return peopleSelectorHandler;
            }
            set
            {
                peopleSelectorHandler = value;
            }
        }
    }
}