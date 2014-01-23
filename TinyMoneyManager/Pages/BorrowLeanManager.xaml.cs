namespace TinyMoneyManager.Pages
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.BorrowAndLean;
    using TinyMoneyManager.Pages.People;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.ViewModels.BorrowLeanManager;
    using System.Collections.Generic;

    public partial class BorrowLeanManager : PhoneApplicationPage
    {
        private IApplicationBar applicationBarForBorrowAndLean;
        private IApplicationBar applicationBarForPeople;
        public BorrowLeanViewModel borrowLeanViewModel { get; set; }

        public PeopleViewModel peopleViewModel { get; set; }

        public BorrowLeanManager()
        {
            this.InitializeComponent();
            this.InitializeApplicationBar();
            base.DataContext = this;
            TiltEffect.SetIsTiltEnabled(this, true);
            this.borrowLeanViewModel = ViewModelLocator.BorrowLeanViewModel;
            this.peopleViewModel = ViewModelLocator.PeopleViewModel;
            this.BorrowOrLeanList.DataContext = this.borrowLeanViewModel;

            loadStatus();

            ToggleCategoryTypeButtonTitle.DataContext = ViewModelLocator.BorrowLeanViewModel;

        }

        private void loadStatus()
        {
            ff.ItemsSource = new Dictionary<RepaymentStatus, string>()
            {
                {RepaymentStatus.All,AppResources.All},
                {RepaymentStatus.Completed,AppResources.Completed},
                {RepaymentStatus.InCompleted, AppResources.InCompleted} 
            };

            ff.SelectionChanged += new SelectionChangedEventHandler(ff_SelectionChanged);
        }

        private void BorrowOrLeanList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Repayment selectedItem = this.BorrowOrLeanList.SelectedItem as Repayment;
            if (selectedItem != null)
            {
                if ((((LeanType)selectedItem.BorrowOrLean.Value) == LeanType.Receipt) || (((LeanType)selectedItem.BorrowOrLean.Value) == LeanType.Repayment))
                {
                    this.NavigateTo(ViewPath.RepaymentOrReceiptViewerPage, new object[] { selectedItem.Id });
                }
                else
                {
                    this.NavigateTo(ViewPath.BorrowLeanInfoViewerPage, new object[] { selectedItem.Id });
                }
                this.BorrowOrLeanList.SelectedItem = null;
            }
        }

        private void createNewBorrowOrLeanItemButton_Click(object sender, System.EventArgs e)
        {
            BorrowLeanEditor.GetEditObject = null;
            this.NavigateTo("/Pages/BorrowAndLean/BorrowLeanEditor.xaml");
        }

        private void createNewPersonItemButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/People/PeopleInfoEditorPage.xaml");
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.BorrowLeanViewModel.DeletingObjectService<Repayment>((sender as MenuItem).Tag as Repayment, i => LocalizedObjectHelper.GetLocalizedStringFrom(i.BorrowLoanTypeName), null);
        }

        private void DeletePersonMenuItem(object sender, RoutedEventArgs e)
        {
            this.AlertConfirm(AppResources.DeleteAccountItemMessage, delegate
            {
                PeopleProfile tag = (sender as MenuItem).Tag as PeopleProfile;
                this.peopleViewModel.Delete<PeopleProfile>(tag);
            }, AppResources.DeletingObject.FormatWith(new object[] { AppResources.People }));
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = (sender as MenuItem).Tag as Repayment;
            if (tag != null)
            {
                this.GoToEditRepayment(tag);
            }
        }

        private void EditPersonMenuItem(object sender, RoutedEventArgs e)
        {
            System.Func<PeopleProfile> func = null;
            PeopleProfile peopleProfile = (sender as MenuItem).Tag as PeopleProfile;
            if (peopleProfile != null)
            {
                if (func == null)
                {
                    func = () => peopleProfile;
                }
                PeopleInfoEditorPage.PeopleProfileGetter = func;
                this.NavigateTo("/Pages/People/PeopleInfoEditorPage.xaml?action=view");
            }
        }

        private void GoToEditRepayment(Repayment entry)
        {
            System.Func<Repayment> func = null;
            if (entry.IsRepaymentOrReceieve)
            {
                RepayOrReceiveEditorPage.Go(entry, this, PageActionType.Edit);
            }
            else
            {
                if (func == null)
                {
                    func = () => entry;
                }
                BorrowLeanEditor.GetEditObject = func;
                this.NavigateTo("/Pages/BorrowAndLean/BorrowLeanEditor.xaml");
            }
        }

        private void InitializeApplicationBar()
        {
            this.applicationBarForBorrowAndLean = new ApplicationBar();
            ApplicationBarIconButton button = new ApplicationBarIconButton
            {
                IconUri = IconUirs.AddPlusIconButton,
                Text = AppResources.CreateNewItem
            };
            button.Click += new System.EventHandler(this.createNewBorrowOrLeanItemButton_Click);
            this.applicationBarForBorrowAndLean.Buttons.Add(button);
            this.applicationBarForPeople = new ApplicationBar();
            ApplicationBarIconButton button2 = new ApplicationBarIconButton
            {
                IconUri = IconUirs.AddPlusIconButton,
                Text = AppResources.CreateNewItem
            };
            button2.Click += new System.EventHandler(this.createNewPersonItemButton_Click);
            this.applicationBarForPeople.Buttons.Add(button2);
        }

        private void PeopleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Func<PeopleProfile> func = null;
            PeopleProfile item;
            if (this.PeopleListBox != null)
            {
                item = this.PeopleListBox.SelectedItem as PeopleProfile;
                if (item != null)
                {
                    if (func == null)
                    {
                        func = () => item;
                    }
                    PeopleProfileViewerPage.PeopleProfileGetter = func;
                    this.NavigateTo("/Pages/People/PeopleProfileViewerPage.xaml?action=view&id={0}", new object[] { item.Id });
                    this.PeopleListBox.SelectedItem = null;
                }
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.BorrowLeanManagerPivots.SelectedIndex == 0)
            {
                base.ApplicationBar = this.applicationBarForBorrowAndLean;
                loadBorrowLoanData();
            }
            else
            {
                base.ApplicationBar = this.applicationBarForPeople;
                if (!this.peopleViewModel.IsDataLoaded)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                           this.peopleViewModel.LoadDataIfNot());
                }
            }
        }

        private void loadBorrowLoanData()
        {
            if (!this.borrowLeanViewModel.IsDataLoaded)
            {
                try
                {
                    mangoProgressIndicator.GlobalIndicator.Instance.BusyForWork(
                        AppResources.NowLoadingFormatter.FormatWith(string.Empty));

                    ThreadPool.QueueUserWorkItem((o) =>
                    {
                        this.borrowLeanViewModel.LoadDataIfNot();

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            this.ToggleCategoryTypeButtonTitle.Text = LocalizedStrings.GetLanguageInfoByKey(borrowLeanViewModel.SearchingCondition.Status.ToString());
                            this.WorkDone();
                        });
                    });
                }
                catch (System.Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                    this.WorkDone();
                }
            }
        }

        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RepaymentButton_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = (sender as MenuItem).Tag as Repayment;
            RepayOrReceiveEditorPage.Go(tag, this, PageActionType.Add);
        }

        private void ff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var statusData = ff.SelectedItem;

                if (statusData != null)
                {
                    var status = (KeyValuePair<RepaymentStatus, string>)statusData;
                    this.borrowLeanViewModel.SearchingCondition.Status = status.Key;
                    this.borrowLeanViewModel.IsDataLoaded = false;
                    loadBorrowLoanData();
                }
            }
        }

        private void ToggleCategoryTypeButtonTitle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ff.Open();
        }
    }
}

