using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Linq;
namespace TinyMoneyManager.Pages.BudgetManagement
{
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels.BudgetManagement;
    using TinyMoneyManager.Data.Model;
    using System.Threading;

    public partial class BudgetProjectListPage : PhoneApplicationPage, INkjSoftPhonePageContract
    {
        public ItemType ItemType { get; set; }

        public BudgetProjectManagementViewModel budgetProjectManagementViewModel;

        public ApplicationBar ApplicationBarForBudgetProject;

        public ApplicationBar ApplicationBarForBudgetMonthlyReport;

        public ApplicationBar applicationBarForProjectlistSelectorMode;
        public ApplicationBar tempApplicationBar;

        public BudgetProjectListPage()
        {
            InitializeComponent();
            budgetProjectManagementViewModel = ViewModelLocator.BudgetProjectViewModel;
            InitializeApplicationBar();
            TiltEffect.SetIsTiltEnabled(this, true);

            this.DataContext = budgetProjectManagementViewModel;

            this.BudgetProjectList.IsSelectionEnabledChanged += new System.Windows.DependencyPropertyChangedEventHandler(BudgetProjectList_IsSelectionEnabledChanged);
        }

        void BudgetProjectList_IsSelectionEnabledChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.MainPivot.IsLocked = true;
                ApplicationBar = applicationBarForProjectlistSelectorMode;
            }
            else
            {
                this.MainPivot.IsLocked = false;
                ApplicationBar = ApplicationBarForBudgetProject;
            }
        }

        public void InitializeApplicationBar()
        {
            ApplicationBarForBudgetProject = new ApplicationBar();
            ApplicationBarForBudgetMonthlyReport = new ApplicationBar();

            // Add budgetProject button.

            ApplicationBarIconButton addBudgetProjectButton =
                new ApplicationBarIconButton(IconUirs.AddPlusIconButton);
            addBudgetProjectButton.Text = AppResources.AddButtonText;
            addBudgetProjectButton.Click += new System.EventHandler(addBudgetProjectButton_Click);

            ApplicationBarIconButton exportBudgetProjectsButton =
                new ApplicationBarIconButton(IconUirs.ColoudIconButton);
            exportBudgetProjectsButton.Text = AppResources.ExportReport;

            ApplicationBarForBudgetProject.Buttons.Add(addBudgetProjectButton);
            //ApplicationBarForBudgetProject.Buttons.Add(exportBudgetProjectsButton);

            ApplicationBarIconButton exportBudgetMonthlyReportButton =
              new ApplicationBarIconButton(IconUirs.ColoudIconButton);
            exportBudgetMonthlyReportButton.Text = AppResources.ExportReport;
            exportBudgetMonthlyReportButton.IsEnabled = false;

            ApplicationBarForBudgetMonthlyReport.Buttons.Add(exportBudgetMonthlyReportButton);

            applicationBarForProjectlistSelectorMode = new ApplicationBar();
            ApplicationBarIconButton deleteMenuItem = new ApplicationBarIconButton(IconUirs.DeleteIcon);
            deleteMenuItem.Text = AppResources.Delete;
            deleteMenuItem.Click += new System.EventHandler(deleteMenuItem_Click);

            applicationBarForProjectlistSelectorMode.Buttons.Add(deleteMenuItem);
        }

        public void deleteMenuItem_Click(object sender, System.EventArgs e)
        {
            var items = BudgetProjectList.SelectedItems.OfType<BudgetProject>();

            budgetProjectManagementViewModel.DeleteProjects(items);
        }

        void addBudgetProjectButton_Click(object sender, System.EventArgs e)
        {
            budgetProjectManagementViewModel.ResetCurrent();
            this.NavigateTo(ViewPath.BudgetManagement.BudgetProjectEditPage, 1);
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = MainPivot.SelectedIndex;
            if (index == -1)
            {
                return;
            }

            if (index == 0)
            {
                this.ApplicationBar = ApplicationBarForBudgetProject;
            }
            if (index == 1)
            {
                this.ApplicationBar = ApplicationBarForBudgetMonthlyReport;
                LoadChart();
            }

            budgetProjectManagementViewModel.TogglePivotSelectionChanged(sender, MainPivot.SelectedIndex);

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                return;

            var targetIndex = this.GetNavigatingParameter("pivotIndex").ToInt32();

            ItemType = (ItemType)this.GetNavigatingParameter("itemType").ToInt32();

            this.MainPivot.SelectedIndex = targetIndex;
        }

        private void BudgetProjectItemButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.BudgetProjectList.IsSelectionEnabled)
            {
                return;
            } 

            var item = (sender as HyperlinkButton).Tag as BudgetProject;

            ViewModelLocator.BudgetProjectViewModel.PrepareForEdit(item);
            this.NavigateTo(ViewPath.BudgetManagement.BudgetProjectEditPage, 4);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (MainPivot.SelectedIndex == 0 && BudgetProjectList.IsSelectionEnabled)
            {
                BudgetProjectList.IsSelectionEnabled = false;
                e.Cancel = true;
                return;
            }


            base.OnBackKeyPress(e);
        }

        Controls.BudgetSettleMonthlyReportChart budgetSettleMonthlyReportChart;

        DetailsCondition seachingCondtion;

        public void LoadChart()
        {

            if (budgetSettleMonthlyReportChart == null || budgetSettleMonthlyReportChart.NeedRefreshData)
            {
                this.BusyForWork(AppResources.Loading);
                Thread th = new Thread(() =>
                {
                    this.InvokeInThread(() =>
                    {
                        if (budgetSettleMonthlyReportChart == null)
                        {
                            seachingCondtion = new DetailsCondition();
                            seachingCondtion.IncomeOrExpenses = Component.ItemType.Expense;
                            seachingCondtion.SearchingScope = SearchingScope.CurrentYear;

                            budgetSettleMonthlyReportChart = new Controls.BudgetSettleMonthlyReportChart();

                            this.ImagePivot.Content = budgetSettleMonthlyReportChart;
                        }

                        budgetSettleMonthlyReportChart.LoadData(seachingCondtion, null);

                        this.WorkDone();
                    });
                });

                th.Start();
            }
        }
    }
}