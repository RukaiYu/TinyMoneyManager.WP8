using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Language;

using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component.Common;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;

using NkjSoft.Extensions;
using System.Data.Linq;
using TinyMoneyManager.Data;
using System.Threading;
namespace TinyMoneyManager.ViewModels.BudgetManagement
{
    public class BudgetProjectManagementViewModel : TinyMoneyManager.Component.NkjSoftViewModelBase
    {
        public GroupedBudgetProjectCollection BudgetProjectList { get; set; }
        /// <summary>
        /// Gets or sets the buget items for add.
        /// </summary>
        /// <value>
        /// The buget items for add.
        /// </value>
        public ObservableCollection<BudgetItem> BugetItemsForAdd { get; set; }

        public ObservableCollection<BudgetItem> BugetItemsForEditToAdd { get; set; }

        private BudgetProject currentEditOrViewBudgetProject;

        public BudgetProject CurrentEditOrViewProject
        {
            get { return currentEditOrViewBudgetProject; }
            set
            {
                if (currentEditOrViewBudgetProject != value)
                {
                    OnNotifyPropertyChanging("CurrentEditOrViewProject");
                    currentEditOrViewBudgetProject = value;
                    this.BugetItemsForAdd = new ObservableCollection<BudgetItem>(value.BudgetItems);
                    OnNotifyPropertyChanged("CurrentEditOrViewProject");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetProjectManagementViewModel"/> class.
        /// </summary>
        public BudgetProjectManagementViewModel()
        {
            BudgetProjectList = new GroupedBudgetProjectCollection();
            BugetItemsForAdd = new ObservableCollection<BudgetItem>();
            BugetItemsForEditToAdd = new ObservableCollection<BudgetItem>();
        }

        public override void LoadDataIfNot()
        {
            if (IsDataLoaded)
                return;

            LoadBudgetProjectsCollection();
            IsDataLoaded = true;
        }

        /// <summary>
        /// Loads the budget projects collection.
        /// </summary>
        public void LoadBudgetProjectsCollection()
        {
            BudgetProjectList.Clear();

            var data = from proj in AccountBookDataContext.BudgetProjects
                       select proj;

            foreach (var item in data)
            {
                item.InitAssociatedBudgetItemsSummary();
                BudgetProjectList.Add(item);
            }
        }

        /// <summary>
        /// Toggles the pivot selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="selectedIndex">Index of the selected.</param>
        public void TogglePivotSelectionChanged(object sender, int selectedIndex)
        {
            //Load data for budgetProjects list.
            if (selectedIndex == 0)
            {
                LoadDataIfNot();
            }
            else if (selectedIndex == 1)
            {
                //

            }
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            if (CurrentEditOrViewProject.Id == Guid.Empty)
            {
                CurrentEditOrViewProject.Id = GuidForAdd;
                CurrentEditOrViewProject.CreateAt = DateTime.Now;
                AccountBookDataContext.BudgetProjects.InsertOnSubmit(CurrentEditOrViewProject);

                AccountBookDataContext.BudgetItems.InsertAllOnSubmit(BugetItemsForAdd);

                this.BudgetProjectList.Add(CurrentEditOrViewProject);

                CurrentEditOrViewProject.AssociatedBudgetItemsSummary = CurrentEditOrViewProject.GetNotesForProject(BugetItemsForAdd);
                //CurrentEditOrViewProject.UpdateTotalAmount();

            }

            AccountBookDataContext.SubmitChanges();

            UpdateCurrentMonthBudgetSummary();
        }

        public Guid GuidForAdd;
        /// <summary>
        /// Resets the current.
        /// </summary>
        public void ResetCurrent()
        {
            CurrentEditOrViewProject = new BudgetProject();
            CurrentEditOrViewProject.Id = Guid.Empty;
            CurrentEditOrViewProject.CreateAt = null;
            GuidForAdd = Guid.NewGuid();
            BugetItemsForAdd.Clear();
        }

        public void PrepareForEdit(BudgetProject budgetProject)
        {
            CurrentEditOrViewProject = budgetProject;
            GuidForAdd = budgetProject.Id;

            BugetItemsForEditToAdd.Clear();
        }

        /// <summary>
        /// Deletes the projects.
        /// </summary>
        /// <param name="items">The items.</param>
        public bool DeleteMultipleProjects(params BudgetProject[] items)
        {
            return DeleteProjects(items);
        }

        public bool DeleteProjects(IEnumerable<BudgetProject> items)
        {
            var count = items.Count();

            if (count == 0)
            {
                return false;
            }

            var confirmMsg = string.Empty;
            if (count == 1)
            {
                confirmMsg = AppResources.DeleteAccountItemMessage;
            }
            else
            {
                confirmMsg = AppResources.DeleteSelectedItems;
            }

            return CommonExtensions.AlertConfirm(null, confirmMsg, () =>
               {
                   try
                   {
                       var temp = items.ToList();
                       for (int i = 0; i < count; i++)
                       {
                           var item = temp[i];
                           this.BudgetProjectList.Remove(item);

                           AccountBookDataContext.BudgetItems.DeleteAllOnSubmit(item.BudgetItems);
                           AccountBookDataContext.BudgetProjects.DeleteOnSubmit(item);
                       }

                       AccountBookDataContext.SubmitChanges();

                       UpdateCurrentMonthBudgetSummary();

                   }
                   catch (Exception ex)
                   {
                       CommonExtensions.Alert(null, ex.Message);
                   }
               }) == MessageBoxResult.OK;
        }

        /// <summary>
        /// Updates the current month budget summary.
        /// </summary>
        public void UpdateCurrentMonthBudgetSummary()
        {
            ViewModelLocator.MainPageViewModel.LoadSummary();
        }

        /// <summary>
        /// Updatings the associated categories for current edit instance.
        /// </summary>
        /// <param name="category">The category.</param>
        public void UpdatingAssociatedCategoriesForCurrentEditInstance(params Category[] categories)
        {
            var currentCategories = CurrentEditOrViewProject.BudgetItems.Select(p => p.AssociatedCategory.Id).ToList();

            var targetNews = categories.Where(p => !currentCategories.Contains(p.Id)).ToList();

            if (targetNews.Count == 0)
                return;

            if (CurrentEditOrViewProject.Id == Guid.Empty)
            {
                foreach (var item in targetNews)
                {
                    BugetItemsForAdd.Add(new BudgetItem()
                    {
                        Amount = 0.0m,
                        ProjectId = GuidForAdd,
                        BudgetItemType = item.IsParent ? BudgetType.ParentCategory : BudgetType.Category,
                        AssociatedCategory = item,
                    });
                }
            }
            else
            {
                foreach (var item in targetNews)
                {
                    var budgetItem = new BudgetItem()
                    {
                        Amount = 0.0m,
                        BudgetProject = CurrentEditOrViewProject,
                        BudgetItemType = item.IsParent ? BudgetType.ParentCategory : BudgetType.Category,
                        AssociatedCategory = item,
                    };

                    BugetItemsForAdd.Add(budgetItem);
                    BugetItemsForEditToAdd.Add(budgetItem);
                    CurrentEditOrViewProject.BudgetItems.Add(budgetItem);
                }

                if (BugetItemsForEditToAdd.Count > 0)
                {
                    AccountBookDataContext.BudgetItems.InsertAllOnSubmit(BugetItemsForEditToAdd);
                    AccountBookDataContext.SubmitChanges();

                    BugetItemsForEditToAdd.Clear();
                    CurrentEditOrViewProject.AssociatedBudgetItemsSummary = CurrentEditOrViewProject.GetNotesForProject(BugetItemsForAdd);
                }
            }

            UpdateCurrentMonthBudgetSummary();
        }

        /// <summary>
        /// Removes the associated budget items.
        /// </summary>
        /// <param name="iEnumerable">The i enumerable.</param>
        public void RemoveAssociatedBudgetItems(IEnumerable<BudgetItem> budgetItemsToRemove)
        {
            var temp = budgetItemsToRemove.ToList();

            if (CurrentEditOrViewProject.Id != Guid.Empty)
                AccountBookDataContext.BudgetItems.DeleteAllOnSubmit(temp);

            var count = temp.Count;

            for (int i = 0; i < count; i++)
            {
                var item = temp[i];
                BugetItemsForAdd.Remove(item);
                BugetItemsForEditToAdd.Remove(item);
                CurrentEditOrViewProject.BudgetItems.Remove(item);
            }

            CurrentEditOrViewProject.AssociatedBudgetItemsSummary = CurrentEditOrViewProject.GetNotesForProject(BugetItemsForAdd);
            CurrentEditOrViewProject.UpdateTotalAmount();
            AccountBookDataContext.SubmitChanges();

            ViewModelLocator.MainPageViewModel.LoadSummary();
        }

        /// <summary>
        /// Counts for last settle amount for category. The value is represent in global currency rate by the default currency type.
        /// </summary>
        /// <param name="budgetItem">The budget item.</param>
        /// <returns></returns>
        public decimal CountForLastSettleAmountForCategory(BudgetItem budgetItem, SearchingScope scope = SearchingScope.LastMonth)
        {
            DetailsCondition dc = new DetailsCondition();
            dc.SearchingScope = scope;
            return CountSumOfBudgetItems(budgetItem, dc);
        }

        /// <summary>
        /// Counts the sum of budget items.
        /// </summary>
        /// <param name="budgetItem">The budget item.</param>
        /// <param name="dc">The dc.</param>
        /// <returns></returns>
        public decimal CountSumOfBudgetItems(BudgetItem budgetItem, DetailsCondition dc)
        {
            var query = AccountBookDataContext.AccountItems.AsQueryable();

            query = query.Where(p => p.CreateTime.Date >= dc.StartDate.Value.Date
                && p.CreateTime.Date <= dc.EndDate.Value.Date);

            if (budgetItem.BudgetItemType == BudgetType.ParentCategory)
            {
                var childIds = AccountBookDataContext.Categories
                    .Where(p => p.ParentCategoryId == budgetItem.AssociatedCategory.Id)
                    .Select(p => p.Id)
                        .ToList();

                query = query.Where(p => childIds.Contains(p.CategoryId));
            }
            else
            {
                query = query.Where(p => p.CategoryId == budgetItem.AssociatedCategory.Id);
            }

            var result = query.AsEnumerable().Sum(p => p.GetMoney());

            return result.GetValueOrDefault();
        }

        /// <summary>
        /// Counts for settle amount for budget project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        public decimal CountForSettleAmountForBudgetProject(BudgetProject project, SearchingScope scope = SearchingScope.CurrentMonth)
        {
            if (project.BudgetItems == null || project.BudgetItems.Count == 0)
            {
                return 0.0m;
            }

            DetailsCondition dc = new DetailsCondition();
            dc.SearchingScope = scope;

            var result = 0.0m;
            foreach (var category in project.BudgetItems)
            {
                result += CountSumOfBudgetItems(category, dc);
            }

            return result;
        }

        /// <summary>
        /// Updates the total amount info by budget item amount changed.
        /// </summary>
        /// <param name="budgetItemObject">The budget item object.</param>
        public void UpdateTotalAmountInfoByBudgetItemAmountChanged(object budgetItemObject)
        {
            var budgetItem = budgetItemObject as BudgetItem;

            if (CurrentEditOrViewProject.Id != Guid.Empty)
            {
                CurrentEditOrViewProject.UpdateTotalAmount(BugetItemsForAdd);

                AccountBookDataContext.SubmitChanges();
            }
            else
            {
                CurrentEditOrViewProject.UpdateTotalAmount(BugetItemsForAdd);
            }

            UpdateCurrentMonthBudgetSummary();
        }

        /// <summary>
        /// Checks the save.
        /// </summary>
        /// <returns></returns>
        public bool CheckSave()
        {
            var query = AccountBookDataContext.BudgetProjects
                .Where(p => p.Name == CurrentEditOrViewProject.Name);

            if (CurrentEditOrViewProject.Id != Guid.Empty)
            {
                query = query.Where(p => p.Id != CurrentEditOrViewProject.Id);
            }

            return query.Count() > 0;
        }

        public bool HasLoadedFirstTime = false;
        /// <summary>
        /// Loads the monthly budget info.
        /// </summary>
        public void LoadMonthlyBudgetInfo()
        {
            if (HasLoadedFirstTime) { return; }
            HasLoadedFirstTime = true;
            Thread th = new Thread(() =>
            {
                var MonthlyData = ViewModelLocator.AccountItemViewModel.GetOneMonthData(DateTime.Now.Date);
                var expense = ViewModelLocator.MainPageViewModel.CountThisMonth(MonthlyData, ItemType.Expense);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    BudgetManager.Current.UpdateCurrentMonthBudgetSummary(expense);
                });

            });

            th.Start();

        }

        /// <summary>
        /// Gets the budget amount for category.
        /// </summary>
        /// <param name="budgetItem">The budget item.</param>
        /// <param name="detailsCondition">The details condition.</param>
        /// <returns></returns>
        public decimal GetBudgetAmountForCategory(BudgetItem budgetItem, DetailsCondition detailsCondition)
        {
            var query = AccountBookDataContext.BudgetItems.Where(p => p.BudgetTargetId == budgetItem.AssociatedCategory.Id)
                .ToList().Sum(p => p.GetMoney());

            return query.GetValueOrDefault();
        }
    }
}
