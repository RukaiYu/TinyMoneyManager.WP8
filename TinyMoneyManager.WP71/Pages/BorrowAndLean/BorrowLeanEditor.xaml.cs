namespace TinyMoneyManager.Pages.BorrowAndLean
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls.BorrowAndLean;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels.BorrowLeanManager;

    public partial class BorrowLeanEditor : PhoneApplicationPage, INotifyPropertyChanged
    {
        private PageActionType actionMode;

        private BorrowAndLeanEditorControl borrowLeanEditor;
        public BorrowLeanViewModel borrowLeanViewModel = ViewModelLocator.BorrowLeanViewModel;

        public static System.Func<Repayment> GetEditObject = () => new Repayment();

        private decimal oldAmountValue = 0.0M;

        public event PropertyChangedEventHandler PropertyChanged;

        public BorrowLeanEditor()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.borrowLeanEditor = new BorrowAndLeanEditorControl(this);
            this.InitializeApplicationBar();
        }



        private void borrowLeanEditor_Confirmed(object sender, System.EventArgs e)
        {
            this.Save_Click(sender, e);
        }

        private void Close_Click(object sender, System.EventArgs e)
        {
            this.SafeGoBack();
        }

        private void DeleteButton_Click(object sender, System.EventArgs e)
        {
            if (ViewModelLocator.BorrowLeanViewModel.DeletingObjectService<Repayment>(this.borrowLeanEditor.CurrentObject, i => LocalizedObjectHelper.GetLocalizedStringFrom(i.BorrowLoanTypeName), null))
            {
                this.SafeGoBack();
            }
        }

        private void InitializeApplicationBar()
        {
            base.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.Save;
            base.ApplicationBar.GetIconButtonFrom(1).Text = AppResources.Cancel;
            base.ApplicationBar.GetMenuItemButtonFrom(0).Text = AppResources.Delete;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                if (!ViewModelLocator.AccountViewModel.IsDataLoaded)
                {
                    ViewModelLocator.AccountViewModel.QuickLoadData();
                }
                Repayment repayment = (GetEditObject == null) ? null : GetEditObject();
                this.PageTitle.Text = AppResources.BorrowAndLean.ToUpperInvariant();
                if ((repayment == null) || (repayment.Id == System.Guid.Empty))
                {
                    repayment = this.borrowLeanViewModel.CreateBorrowLeanEntry();
                    this.ActionMode = PageActionType.Add;
                    base.ApplicationBar.GetMenuItemButtonFrom(0).IsEnabled = false;
                }
                else
                {
                    this.ActionMode = PageActionType.Edit;
                    this.oldAmountValue = repayment.Amount;
                }
                this.borrowLeanEditor.CurrentObject = repayment;
                this.ContentPanel.Children.Add(this.borrowLeanEditor);
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, e);
            }
        }

        private void Save_Click(object sender, System.EventArgs e)
        {
            Repayment updatedObject = this.borrowLeanEditor.GetUpdatedObject();
            if (updatedObject != null)
            {
                ViewModelLocator.BorrowLeanViewModel.SaveOrInsert(updatedObject, this.oldAmountValue);
                this.Close_Click(sender, e);
            }
        }

        public PageActionType ActionMode
        {
            get
            {
                return this.actionMode;
            }
            set
            {
                if (this.actionMode != value)
                {
                    this.actionMode = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs("ActionMode"));
                }
            }
        }
    }
}

