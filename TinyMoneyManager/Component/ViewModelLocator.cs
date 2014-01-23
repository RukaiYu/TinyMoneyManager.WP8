namespace TinyMoneyManager.Component
{
    using System;
    using TinyMoneyManager;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.ViewModels.BorrowLeanManager;
    using TinyMoneyManager.ViewModels.BudgetManagement;
    using TinyMoneyManager.ViewModels.CustomizedTallyManager;
    using TinyMoneyManager.ViewModels.ScheduleManager;

    public class ViewModelLocator
    {
        public const string DataContext_AccountBookDataContextKey = "DataContext_AccountBookDataContext";
        public static InstanceLoader instanceLoader = new InstanceLoader();

        public static TinyMoneyDataContext AccountBookDataContext
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyDataContext>("DataContext_AccountBookDataContext");
            }
            set
            {
                instanceLoader.Reset("DataContext_AccountBookDataContext", value);
            }
        }

        public static AccountItemListViewModel AccountItemViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<AccountItemListViewModel>("AccountItemListViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.AccountViewModel AccountViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.AccountViewModel>("AccountViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.BorrowLeanManager.BorrowLeanViewModel BorrowLeanViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.BorrowLeanManager.BorrowLeanViewModel>("____BorrowLeanViewModel");
            }
        }

        public static BudgetProjectManagementViewModel BudgetProjectViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<BudgetProjectManagementViewModel>("BudgetProjectManagementViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.CategoryViewModel CategoryViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.CategoryViewModel>("CategoryViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.CustomizedTallyManager.CustomizedTallyViewModel CustomizedTallyViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.CustomizedTallyManager.CustomizedTallyViewModel>("____CustomizedTallyViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.ExpensesViewModel ExpensesViewModel
        {
            get
            {
                return AccountItemViewModel.ExpenseItemListViewModel;
            }
        }

        public static TinyMoneyManager.ViewModels.IncomeViewModel IncomeViewModel
        {
            get
            {
                return AccountItemViewModel.IncomeItemListViewModel;
            }
        }

        public static MenuItemViewModel MainPageViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<MenuItemViewModel>("MenuItemViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.PeopleViewModel PeopleViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.PeopleViewModel>("PeopleViewModel");
            }
        }

        public static PictureViewModel PicturesViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<PictureViewModel>("____PicturesViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.RepaymentManagerViewModel RepaymentManagerViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.RepaymentManagerViewModel>("RepaymentManagerViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.ScheduleManager.ScheduleManagerViewModel ScheduleManagerViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.ScheduleManager.ScheduleManagerViewModel>("____ScheduleManagerViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.SynchronizationManagerViewModel SynchronizationManagerViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.SynchronizationManagerViewModel>("SynchronizationManagerViewModel");
            }
        }

        public static TinyMoneyManager.ViewModels.TransferingHistoryViewModel TransferingHistoryViewModel
        {
            get
            {
                return instanceLoader.LoadSingelton<TinyMoneyManager.ViewModels.TransferingHistoryViewModel>("TransferingHistoryViewModel");
            }
        }
    }
}

