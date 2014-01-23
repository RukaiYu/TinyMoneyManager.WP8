using System.Linq;
using System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using System.Threading;

using NkjSoft.Extensions;
namespace TinyMoneyManager.Pages.BorrowAndLean
{
    public partial class BorrowLoanInfoViewerPage : PhoneApplicationPage
    {
        Controls.BorrowAndLean.BorrowLoanInfoViewer infoViewerControl;

        public static Func<Repayment> CurrentObjectGetter;

        ApplicationBar applicationBarForViewPivot;
        ApplicationBar applicationbarForHistoryPivot;

        public Repayment Current;

        public BorrowLoanInfoViewerPage()
        {
            InitializeComponent();

            infoViewerControl = new Controls.BorrowAndLean.BorrowLoanInfoViewer(this);

            this.LayoutRoot.Children.Add(infoViewerControl);

            TiltEffect.SetIsTiltEnabled(this, true);

            InitializeApplicationBar();

            infoViewerControl.MainPivot.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(MainPivot_SelectionChanged);
        }

        void MainPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (infoViewerControl.MainPivot == null) return;

            if (infoViewerControl.MainPivot.SelectedIndex == 0)
            { this.ApplicationBar = applicationBarForViewPivot; }
            else { this.ApplicationBar = applicationbarForHistoryPivot; }
        }

        /// <summary>
        /// Initializes the application bar.
        /// </summary>
        private void InitializeApplicationBar()
        {
            applicationBarForViewPivot = new ApplicationBar();

            var deleteButton = IconUirs.CreateDeleteButton();
            var editButton = IconUirs.CreateEditButton();

            applicationBarForViewPivot.Buttons.Add(editButton);
            applicationBarForViewPivot.Buttons.Add(deleteButton);

            editButton.Click += new EventHandler(goToEditRepayment_Click);
            deleteButton.Click += new EventHandler(deleteRepayment_Click);

            applicationbarForHistoryPivot = new ApplicationBar();

            var createRepayOrReceiveButton = new ApplicationBarIconButton(IconUirs.AddPlusIconButton);

            createRepayOrReceiveButton.Text = AppResources.Add;
            createRepayOrReceiveButton.Click += new EventHandler(createRepayOrReceiveButton_Click);

            applicationbarForHistoryPivot.Buttons.Add(createRepayOrReceiveButton);

            this.ApplicationBar = applicationBarForViewPivot;
        }

        /// <summary>
        /// Handles the Click event of the createRepayOrReceiveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void createRepayOrReceiveButton_Click(object sender, EventArgs e)
        {
            Pages.BorrowAndLean.RepayOrReceiveEditorPage.Go(this.Current, this, PageActionType.Add);
        }

        /// <summary>
        /// Handles the Click event of the goToEditRepayment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void goToEditRepayment_Click(object sender, EventArgs e)
        {
            BorrowAndLean.BorrowLeanEditor.GetEditObject = () => Current;
            this.NavigateTo("/Pages/BorrowAndLean/BorrowLeanEditor.xaml");
        }

        /// <summary>
        /// Handles the Click event of the deleteRepayment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void deleteRepayment_Click(object sender, EventArgs e)
        {
            if (ViewModelLocator.BorrowLeanViewModel
                .DeletingObjectService<Repayment>(Current,
                (i) => LocalizedObjectHelper.GetLocalizedStringFrom(i.BorrowLoanTypeName)))
            {
                this.SafeGoBack();
            }
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                var id = this.GetNavigatingParameter("id").ToGuid();
                loadViewingObject(id);
            }

        }

        /// <summary>
        /// Loads the viewing object.
        /// </summary>
        /// <param name="id">The id.</param>
        private void loadViewingObject(Guid id)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                var item = ViewModelLocator.BorrowLeanViewModel
                    .QueryBorrowLoan(p => p.Id == id).FirstOrDefault();

                Dispatcher.BeginInvoke(() =>
                {
                    Current = item;
                    infoViewerControl.ViewRepayment(Current);

                    if (infoViewerControl.needReloadHistory)
                    {
                        infoViewerControl.LoadHistoryData();
                    }
                });
            });

        }
    }
}