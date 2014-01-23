using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TinyMoneyManager.Pages.BorrowAndLean
{
    using System.Threading;
    using NkjSoft.Extensions;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.ViewModels;

    using NkjSoft.WPhone.Extensions;

    public partial class BorrowLoanStasticsPage : PhoneApplicationPage
    {
        private bool hasLoadRelationShip;

        ViewModels.PeopleViewModel peopleProfileViewModel;
        private string _loaningShortTitle;

        public LeanType _loaningTpyeToSearch;

        public BorrowLoanStasticsPage()
        {
            InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);

            peopleProfileViewModel = ViewModelLocator.PeopleViewModel;

            _loaningTpyeToSearch = LeanType.BorrowIn;
            this.Loaded += BorrowLoanStasticsPage_Loaded;
        }

        void BorrowLoanStasticsPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadRelationShip();
        }

        private void LoadRelationShip()
        {
            if (!hasLoadRelationShip)
            {
                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(AppResources.LoaningRelationShipInformation.ToLower()));
                this.hasLoadRelationShip = true;
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    _loaningShortTitle = _loaningTpyeToSearch == LeanType.BorrowIn ? AppResources.AllIOwnHimBlockTitleShortFormatter
                        : AppResources.AllHeOwnMeBlockTitleShortFormatter;

                    var allPeoplesThatHasRelationShip = new List<PeopleProfile>();
                    TinyMoneyDataContext db = peopleProfileViewModel.AccountBookDataContext;

                    var allPeoples =
                         (from r in db.Repayments
                          where r.RepaymentRecordType == RepaymentType.MoneyBorrowOrLeanRepayment
                          join p in db.Peoples on r.ToPeopleId equals p.Id
                          select p).Distinct(p => p.Id);

                    foreach (var people in allPeoples)
                    {
                        if (peopleProfileViewModel.CalculateLoaningRelationShip(people, _loaningTpyeToSearch))
                        {
                            people.LoaningShortTitle = this._loaningShortTitle;
                            allPeoplesThatHasRelationShip.Add(people);
                        }
                    }

                    Dispatcher.BeginInvoke(() =>
                    {
                        this.PeopleListBox.ItemsSource = allPeoplesThatHasRelationShip;
                        this.WorkDone();

                    });
                });
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                this._loaningTpyeToSearch = e.GetNavigatingParameter("type").ToInt32().ToEnum<LeanType>(); 
            }
        }

        private void PeopleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PeopleListBox.SelectedItem != null
                && e.AddedItems.Count > 0)
            {
                PeopleListBox.SelectedItem = null;

                var peopleInfo = e.AddedItems[0] as PeopleProfile;

                Pages.People.PeopleProfileViewerPage.View(peopleInfo.Id, this, 1);
            }
        }
    }
}