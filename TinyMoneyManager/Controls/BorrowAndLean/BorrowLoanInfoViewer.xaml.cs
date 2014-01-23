namespace TinyMoneyManager.Controls.BorrowAndLean
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.BorrowAndLean;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.Pages.People;
    using TinyMoneyManager.ViewModels.BorrowLeanManager;

    public partial class BorrowLoanInfoViewer : UserControl, INotifyPropertyChanged
    {

        private PhoneApplicationPage associatedPage;
        public BorrowLeanViewModel borrowLoanViewModel;

        private Repayment current;

        public bool needReloadHistory = true;

        private decimal oldAmount = 0.0M;

        private string stasticItemsTips;

        private string titalOfAction = string.Empty;
        public event PropertyChangedEventHandler PropertyChanged;

        public BorrowLoanInfoViewer(PhoneApplicationPage page)
        {
            this.InitializeComponent();
            base.DataContext = this;
            this.associatedPage = page;
            this.borrowLoanViewModel = ViewModelLocator.BorrowLeanViewModel;
            this.DetailsPivot.Header = AppResources.Details.ToLowerInvariant().Trim();
        }

        private void AccountNameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            AccountInfoViewer.CurrentAccountGetter = this.Current.PayFromAccount;
            this.associatedPage.NavigateTo("/Pages/DialogBox/AccountInfoViewer.xaml?action=view&id={0}&to=1&fromSelf=false", new object[] { this.Current.FromAccountId });
        }

        private void AmountValueEditor_Click(object sender, RoutedEventArgs e)
        {
            this.associatedPage.NavigateToEditValueInTextBoxEditorPage(this.TotalMoneyTitle.Text, this.Current.BorrowLoanAmountString, delegate(TextBox t)
            {
                t.SelectAll();
            }, delegate(string s)
            {
                if (s.ToDecimal() <= 0M)
                {
                    this.associatedPage.AlertNotification(AppResources.AmountShouldBeGreatThanZeroMessage, null);
                    return false;
                }
                return true;
            }, delegate(string s)
            {
                this.Current.Amount = s.ToDecimal();
                this.borrowLoanViewModel.SaveOrInsert(this.Current, this.oldAmount);
                this.LoadHistoryData(true);
                this.oldAmount = this.current.Amount;
            });
        }

        private void BorrowOrLeanList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = this.BorrowOrLeanList.SelectedItem as Repayment;
            if (item != null)
            {
                BorrowOrLoanRepayReceiveInfoViewerPage.View(this.associatedPage, item);

                this.BorrowOrLeanList.SelectedItem = null;
            }
        }

        private void DebetorPanel_Click(object sender, RoutedEventArgs e)
        {
            PeopleProfileViewerPage.PeopleProfileGetter = () => this.Current.ToPeople;
            this.associatedPage.NavigateTo("/Pages/People/PeopleProfileViewerPage.xaml?id={0}", new object[] { this.current.ToPeopleId });
        }

        private void DeleteRepayItem_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = (sender as MenuItem).Tag as Repayment;
            if (ViewModelLocator.BorrowLeanViewModel.DeletingObjectService<Repayment>(tag, i => LocalizedObjectHelper.GetLocalizedStringFrom(i.BorrowLoanTypeName), null))
            {
                this.LoadHistoryData(true);
            }
        }

        private void EditRepayItem_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = (sender as MenuItem).Tag as Repayment;
            if (tag != null)
            {
                this.goToEditRepay(tag);
            }
        }

        private void goToEditRepay(Repayment item)
        {
            RepayOrReceiveEditorPage.Go(item, this.associatedPage, PageActionType.Edit);
            RepayOrReceiveEditorPage.CallBackIfHasEdit = new System.Action<bool>(this.RepayOrReceiveEditorPage_CallBackIfHasEdit);
        }

        public void LoadHistoryData(bool forceToLoad = false)
        {
            System.Threading.WaitCallback callBack = null;
            if ((this.needReloadHistory || forceToLoad) && (this.RepayOrReceieveHistoryPivot.Header != null))
            {
                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { this.RepayOrReceieveHistoryPivot.Header.ToString() }));
                if (callBack == null)
                {
                    callBack = delegate(object o)
                    {
                        ObservableCollection<BorrowLeanGroupByTimeViewModel> data = this.borrowLoanViewModel.LoadHistoryList(this.Current.RepayToOrGetBackFromItems);
                        decimal num = ((System.Collections.Generic.IEnumerable<decimal>)(from p in this.Current.RepayToOrGetBackFromItems.AsEnumerable<Repayment>().ToList<Repayment>() select p.GetMoneyForRepayOrReceive(null))).Sum();
                        string amountInfo = this.current.BorrowLoanCurrency.GetCurrencySymbolWithMoney(new decimal?(num));
                        base.Dispatcher.BeginInvoke(delegate
                        {
                            this.BorrowOrLeanList.ItemsSource = data;
                            this.needReloadHistory = false;
                            this.AlreadyPayPanelBlock.Text = amountInfo;

                            this.StatusinfoBlock.Text = num >= this.Current.Amount ? AppResources.Completed : AppResources.InCompleted;
                            if (this.Current.RepayToOrGetBackFromItems.Count<Repayment>() > 0)
                            {
                                this.StasticItemsTips = "{0}: {1}".FormatWith(new object[] { this.AlreadyPayPanelTitle.Text, amountInfo });
                            }
                            else
                            {
                                this.StasticItemsTips = string.Empty;
                            }
                            this.WorkDone();
                        });
                    };
                }
                System.Threading.ThreadPool.QueueUserWorkItem(callBack);
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MainPivot.SelectedIndex == 1)
            {
                this.LoadHistoryData(false);
            }
        }

        private void NotesValueEditor_Click(object sender, RoutedEventArgs e)
        {
            this.associatedPage.NavigateToEditValueInTextBoxEditorPage(AppResources.Notes, this.Current.Notes, delegate(TextBox t)
            {
                t.SelectAll();
            }, null, delegate(string s)
            {
                this.Current.Notes = s;
                this.borrowLoanViewModel.SaveOrInsert(this.Current, 0.0M);
            });
        }

        private void RepayOrReceiveEditorPage_CallBackIfHasEdit(bool obj)
        {
            this.needReloadHistory = true;
        }

        private void RepayToOrGetBackFromItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.LoadHistoryData(true);
        }

        public void ViewRepayment(Repayment instance)
        {
            if (instance != null)
            {
                this.Current = instance;
                this.oldAmount = instance.Amount;
                this.RepayOrReceieveHistoryPivot.Header = AppResources.HistoryOf.FormatWith(new object[] { this.current.ReverseRepaymentTypeName }).ToLowerInvariant();
                if (((LeanType)this.current.BorrowOrLean) == LeanType.BorrowIn)
                {
                    this.titalOfAction = AppResources.Repayed.ToLowerInvariant();
                }
                else
                {
                    this.titalOfAction = AppResources.Receieved.ToLowerInvariant();
                }
                if (this.current.RepayToOrGetBackFromItems != null)
                {
                    this.current.RepayToOrGetBackFromItems.CollectionChanged += new NotifyCollectionChangedEventHandler(this.RepayToOrGetBackFromItems_CollectionChanged);
                }

                this.AlreadyPayPanelTitle.Text = AppResources.AlreadyRepayOrReceieveTitle.FormatWith(new object[] { this.titalOfAction }).ToLowerInvariant();
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

        public string StasticItemsTips
        {
            get
            {
                return this.stasticItemsTips;
            }
            set
            {
                this.stasticItemsTips = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("StasticItemsTips"));
                }
            }
        }
    }
}

