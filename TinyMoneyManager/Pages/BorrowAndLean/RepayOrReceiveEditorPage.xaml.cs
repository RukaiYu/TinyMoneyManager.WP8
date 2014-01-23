using System;
using System.ComponentModel;
using Microsoft.Phone.Controls;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using Microsoft.Phone.Shell;

namespace TinyMoneyManager.Pages.BorrowAndLean
{
    using NkjSoft.Extensions;
    public partial class RepayOrReceiveEditorPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        Controls.BorrowAndLean.RepaymentOrReceiptEditor repayOrReceieveEditor;
        public static Func<Repayment> GetEditObject;

        public ViewModels.BorrowLeanManager.BorrowLeanViewModel borrowLeanViewModel = ViewModelLocator.BorrowLeanViewModel;

        public static Action<bool> CallBackIfHasEdit;

        public event PropertyChangedEventHandler PropertyChanged;
        private decimal oldAmountValue = 0.0m;
        private PageActionType actionMode;
        /// <summary>
        /// Gets or sets the action mode.
        /// </summary>
        /// <value>
        /// The action mode.
        /// </value>
        public TinyMoneyManager.Component.PageActionType ActionMode
        {
            get { return actionMode; }
            set
            {
                if (actionMode != value)
                {
                    actionMode = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ActionMode"));
                }
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, e);
        }

        ApplicationBarMenuItem deleteButton;

        public RepayOrReceiveEditorPage()
        {
            InitializeComponent();

            TiltEffect.SetIsTiltEnabled(this, true);

            InitializeApplicationBar();
            this.DataContext = this;
            repayOrReceieveEditor = new Controls.BorrowAndLean.RepaymentOrReceiptEditor(this);
            repayOrReceieveEditor.aph.OriginalBar = appBar;

            this.Loaded += new System.Windows.RoutedEventHandler(RepayOrReceiveEditorPage_Loaded);
        }

        bool dataLoaded = false;
        void RepayOrReceiveEditorPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dataLoaded)
                return;

            dataLoaded = true;

            if (actionMode == PageActionType.Edit)
            {

            }

        }
        ApplicationBar appBar = null;
        ApplicationBarMenuItem scheduledRepayOrReceiveButton = null;
        /// <summary>
        /// Initializes the application bar.
        /// </summary>
        private void InitializeApplicationBar()
        {
            var saveButton = IconUirs.CreateIconButton(AppResources.Save, IconUirs.SaveIcon);

            saveButton.Click += new EventHandler(saveButton_Click);

            var cancelButton =
                IconUirs.CreateIconButton(AppResources.Cancel, IconUirs.CancelIconButton);

            cancelButton.Click += new EventHandler(cancelButton_Click);

            deleteButton = IconUirs.CreateMenuButton(AppResources.Delete);
            deleteButton.Click += new EventHandler(deleteButton_Click);

            appBar = new ApplicationBar();

            appBar.Buttons.Add(saveButton);
            appBar.Buttons.Add(cancelButton);
            appBar.MenuItems.Add(deleteButton);


            scheduledRepayOrReceiveButton = IconUirs.CreateMenuButton(AppResources.ScheduledRepayOrReceive.FormatWith(string.Empty));
            scheduledRepayOrReceiveButton.IsEnabled = false;
            appBar.MenuItems.Add(scheduledRepayOrReceiveButton);

            this.ApplicationBar = appBar;
        }

        void deleteButton_Click(object sender, EventArgs e)
        {
            if (ViewModelLocator.BorrowLeanViewModel
                   .DeletingObjectService<Repayment>(repayOrReceieveEditor.CurrentObject,
                   (i) => LocalizedObjectHelper.GetLocalizedStringFrom(i.BorrowLoanTypeName)))
            {
                this.SafeGoBack();
            }
        }

        void cancelButton_Click(object sender, EventArgs e)
        {
            this.SafeGoBack();
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            var repayment = repayOrReceieveEditor.GetUpdatedObject();
            if (repayment == null)
                return;

            if (CallBackIfHasEdit != null)
                CallBackIfHasEdit(true);
            CallBackIfHasEdit = null;
            ViewModelLocator.BorrowLeanViewModel.SaveOrInsert(repayment, oldAmountValue);

            cancelButton_Click(sender, e);
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
                if (!ViewModelLocator.AccountViewModel.IsDataLoaded)
                {
                    ViewModelLocator.AccountViewModel.QuickLoadData();
                }

                var action = this.GetNavigatingParameter("action", PageActionType.Add);

                actionMode = (PageActionType)(Enum.Parse(typeof(PageActionType), action, true));

                Repayment editObject = GetEditObject == null ? null : GetEditObject();

                var actionForTitle = string.Empty;

                if (actionMode == PageActionType.Add)
                {
                    actionForTitle = AppResources.Add;
                    deleteButton.IsEnabled = false;
                }
                else
                {
                    actionForTitle = AppResources.Edit;
                    oldAmountValue = editObject.Amount;
                }

                repayOrReceieveEditor.CurrentObject = editObject;
                var actionName = editObject.BorrowLoanTypeName;
                PageTitle.Text = "{0} {1}".FormatWith(actionForTitle, actionName)
                    .ToUpperInvariant();
                scheduledRepayOrReceiveButton.Text = AppResources.ScheduledRepayOrReceive.FormatWith(actionName);
                this.ContentPanel.Children.Add(repayOrReceieveEditor);
            }
        }

        /// <summary>
        /// Goes the specified repayment.
        /// </summary>
        /// <param name="repayment">The repayment.</param>
        public static void Go(Repayment repayment, PhoneApplicationPage pageFrom, PageActionType action)
        {
            var warpped = repayment;

            if (action == PageActionType.Add)
            {
                warpped = ViewModelLocator.BorrowLeanViewModel.CreateRepayOrReceieveEntry(repayment);
                warpped.RepayToOrGetBackFrom = repayment;
            }

            GetEditObject = () => warpped;

            pageFrom.NavigateTo("/Pages/BorrowAndLean/RepayOrReceiveEditorPage.xaml?action={0}", action);
        }

        /// <summary>
        /// Initializes the <see cref="RepayOrReceiveEditorPage"/> class.
        /// </summary>
        static RepayOrReceiveEditorPage()
        {
            GetEditObject = new Func<Repayment>(() => new Repayment());
        }
    }
}