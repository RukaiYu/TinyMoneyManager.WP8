namespace TinyMoneyManager.Pages.BorrowAndLean
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.ViewModels.BorrowLeanManager;

    public partial class BorrowOrLoanRepayReceiveInfoViewerPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private ApplicationBar applicationBarForViewPivot;
        private BorrowLeanViewModel borrowLoanViewModel;
        private Repayment current;

        private bool needReloadRelatedItems = true;

        private decimal oldAmount = 0.0M;

        public event PropertyChangedEventHandler PropertyChanged;

        public BorrowOrLoanRepayReceiveInfoViewerPage()
        {
            this.InitializeComponent();
            base.DataContext = this;
            TiltEffect.SetIsTiltEnabled(this, true);
            this.borrowLoanViewModel = ViewModelLocator.BorrowLeanViewModel;
            this.MainPivot.SelectionChanged += new SelectionChangedEventHandler(this.MainPivot_SelectionChanged);
            this.InitializeApplicationBar();
        }

        private void AccountNameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            AccountInfoViewer.CurrentAccountGetter = this.Current.PayFromAccount;
            this.NavigateTo("/Pages/DialogBox/AccountInfoViewer.xaml?action=view&id={0}&to=1&fromSelf=false", new object[] { this.Current.FromAccountId });
        }

        private void AmountOfTHisTimeValueEditor_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(this.TotalOfThisTimeMoneyTitle.Text, this.Current.BorrowLoanAmountString, delegate(TextBox t)
            {
                t.SelectAll();
            }, delegate(string s)
            {
                if (s.ToDecimal() <= 0M)
                {
                    this.AlertNotification(AppResources.AmountShouldBeGreatThanZeroMessage, null);
                    return false;
                }
                return true;
            }, delegate(string s)
            {
                this.Current.Amount = s.ToDecimal();
                this.oldAmount = this.current.Amount;
                this.borrowLoanViewModel.SaveOrInsert(this.Current, this.oldAmount);
            });
        }

        private void DebetorPanel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo("/Pages/People/PeopleProfileViewerPage.xaml?id={0}", new object[] { this.Current.ToPeople.Id });
        }

        private void deleteButton_Click(object sender, System.EventArgs e)
        {
            if (ViewModelLocator.BorrowLeanViewModel.DeletingObjectService<Repayment>(this.Current, i => LocalizedObjectHelper.GetLocalizedStringFrom(i.BorrowLoanTypeName), null))
            {
                this.SafeGoBack();
            }
        }

        private void DeleteRepayItem_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = (sender as MenuItem).Tag as Repayment;
            if (ViewModelLocator.BorrowLeanViewModel.DeletingObjectService<Repayment>(tag, i => LocalizedObjectHelper.GetLocalizedStringFrom(i.BorrowLoanTypeName), null))
            {
                this.needReloadRelatedItems = true;
                this.LoadRelatedItems();
            }
        }

        private void editButton_Click(object sender, System.EventArgs e)
        {
            RepayOrReceiveEditorPage.Go(this.Current, this, PageActionType.Edit);
        }

        private void EditRepayItem_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = (sender as MenuItem).Tag as Repayment;
            RepayOrReceiveEditorPage.Go(tag, this, PageActionType.Edit);
        }

        private void InitializeApplicationBar()
        {
            this.applicationBarForViewPivot = new ApplicationBar();
            ApplicationBarIconButton button = IconUirs.CreateDeleteButton();
            ApplicationBarIconButton button2 = IconUirs.CreateEditButton();
            this.applicationBarForViewPivot.Buttons.Add(button2);
            this.applicationBarForViewPivot.Buttons.Add(button);
            button2.Click += new System.EventHandler(this.editButton_Click);
            button.Click += new System.EventHandler(this.deleteButton_Click);
            base.ApplicationBar = this.applicationBarForViewPivot;
        }

        private void InitializeView(System.Guid id)
        {
            this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.Details }));
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                Repayment tempObject = this.borrowLoanViewModel.QueryBorrowLoan(p => p.Id == id).FirstOrDefault<Repayment>();
                if (tempObject != null)
                {
                    this.Dispatcher.BeginInvoke(delegate
                    {
                        this.Current = tempObject;
                        string str = this.Current.BorrowLoanTypeName.ToLowerInvariant();
                        this.RelatedItemsPivot.Header = "{0}{1}".FormatWith(new object[] { AppResources.Related, str });
                        this.DetailsPivot.Header = "{0}{1}".FormatWith(new object[] { str, AppResources.Details });
                        this.TotalOfThisTimeMoneyTitle.Text = AppResources.AmountOfThisTimeFormatter.FormatWith(new object[] { str }).ToLowerInvariant();
                        this.oldAmount = this.current.Amount;
                        this.WorkDone();
                    });
                }
            });
        }

        private void LoadRelatedItems()
        {
            if ((this.current != null) && this.needReloadRelatedItems)
            {
                this.needReloadRelatedItems = false;
                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { this.RelatedItemsPivot.Header }));
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    System.Collections.Generic.List<Repayment> data = (from p in this.current.RepayToOrGetBackFrom.RepayToOrGetBackFromItems
                                                                       where p.Id != this.current.Id
                                                                       select p).ToList<Repayment>();
                    ObservableCollection<BorrowLeanGroupByTimeViewModel> groupData = this.borrowLoanViewModel.LoadHistoryList(data);
                    base.Dispatcher.BeginInvoke(delegate
                    {
                        this.BorrowOrLeanList.ItemsSource = groupData;
                        this.WorkDone();
                    });
                });
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MainPivot != null)
            {
                if (this.MainPivot.SelectedIndex == 1)
                {
                    base.ApplicationBar.IsVisible = false;
                    this.LoadRelatedItems();
                }
                else
                {
                    base.ApplicationBar.IsVisible = true;
                }
            }
        }

        private void NotesValueEditor_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.Notes, this.Current.Notes, delegate(TextBox t)
            {
                t.SelectAll();
            }, null, delegate(string s)
            {
                this.Current.Notes = s;
                this.borrowLoanViewModel.SaveOrInsert(this.Current, 0.0M);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.InitializeView(this.GetNavigatingParameter("id", null).ToGuid());
            }
        }

        public Repayment Current
        {
            get
            {
                return this.current;
            }
            set
            {
                if (this.current != value)
                {
                    this.current = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Current"));
                    }
                }
            }
        }

        /// <summary>
        /// Views the specified from page.
        /// </summary>
        /// <param name="fromPage">From page.</param>
        /// <param name="item">The item.</param>
        public static void View(PhoneApplicationPage fromPage, Repayment item)
        {
            if (item != null)
            {
                fromPage.NavigateTo("/Pages/BorrowAndLean/BorrowOrLoanRepayReceiveInfoViewerPage.xaml?id={0}", item.Id);
            }
        }
    }
}

