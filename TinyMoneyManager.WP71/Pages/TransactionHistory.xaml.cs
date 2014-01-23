using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Component;
using TinyMoneyManager.ViewModels;

using NkjSoft.WPhone.Extensions;
namespace TinyMoneyManager.Pages
{
    public partial class TransactionHistory : PhoneApplicationPage
    {
        private ApplicationBarMenuItem openCategoryPageMenuButton;
        protected bool preventListSelectionChanged;
        private string selectAllText = string.Empty;
        private string SelectInvertText = string.Empty;
        private ApplicationBarIconButton sendingSummaryIconButton;
        private string unSelectAllText = string.Empty;
        private ViewModeConfig vmc;

        private AccountItemListViewModel accountItemLisViewModel;
        public TransactionHistory()
        {
            InitializeComponent();

            this.DataContext = this;

            this.MainPivotTitle.SelectionChanged += MainPivotTitle_SelectionChanged;
            this.ExpensesListBox.DataContext = ViewModelLocator.ExpensesViewModel;
        }

        void MainPivotTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = this.MainPivotTitle.SelectedIndex;
            GroupViewModel expensesViewModel = ViewModelLocator.ExpensesViewModel;
            switch (selectedIndex)
            {
                case 0:
                    expensesViewModel = ViewModelLocator.ExpensesViewModel;
                    this.vmc = ViewModelLocator.ExpensesViewModel.ViewModeInfo;
                    //   this.openCategoryPageMenuButton.Text = LocalizedStrings.GetCombinedText(this.ExpensesPivot.Header.ToString(), this.currentDialogItemTypeTitle, false);
                    break;
            }
            if (!expensesViewModel.IsDataLoaded)
            {
                new System.Threading.Thread(delegate(object o)
                {
                    (o as GroupViewModel).Load();
                }).Start(expensesViewModel);
            }
        }

        public static void Go(PhoneApplicationPage fromPage)
        {
            fromPage.NavigateTo("/Pages/TransactionHistory.xaml");
        }


    }
}