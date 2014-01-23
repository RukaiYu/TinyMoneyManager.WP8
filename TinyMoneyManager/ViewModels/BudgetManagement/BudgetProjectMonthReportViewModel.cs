using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Linq;
using NkjSoft.Extensions;
using TinyMoneyManager.Component;
using System.Collections.Generic;
using TinyMoneyManager.Language;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager.ViewModels.BudgetManagement
{
    public class BudgetProjectMonthReportViewModel : Component.NkjSoftViewModelBase
    {
        private string chartSummaryTitle;

        public string ChartTitle
        {
            get { return chartSummaryTitle; }
            set
            {
                if (chartSummaryTitle != value)
                {
                    OnNotifyPropertyChanging("ChartTitle");
                    chartSummaryTitle = value;
                    OnNotifyPropertyChanged("ChartTitle");
                }
            }
        }

        private string mainCurrencySymbol;

        public string MainCurrencySymbol
        {
            get { return mainCurrencySymbol; }
            set
            {
                if (mainCurrencySymbol != value)
                {
                    OnNotifyPropertyChanging("MainCurrencySymbol");
                    mainCurrencySymbol = value;
                    OnNotifyPropertyChanged("MainCurrencySymbol");
                }
            }
        }

        public BudgetProjectMonthReportViewModel()
        {
            mainCurrencySymbol = AppSetting.Instance.DefaultCurrency.GetCurrentString();
        }

        /// <summary>
        /// Loads the budget monthly report.
        /// </summary>
        /// <param name="seachingCondition">The seaching condition.</param>
        /// <returns></returns>
        public IEnumerable<SummaryDetails> LoadBudgetMonthlyReport(DetailsCondition searchingCondition)
        {
            var query = AccountBookDataContext.BudgetMonthlyReports
                .Where(p => p.ItemType == searchingCondition.IncomeOrExpenses
                && p.Year == searchingCondition.StartDate.Value.Year && p.Month >= searchingCondition.StartDate.Value.Month)
                .GroupBy(p => new
                {
                    Date = new DateTime(p.Year, p.Month, 1),
                    ItemType = p.ItemType
                });

            foreach (var item in query)
            {
                yield return new SummaryDetails()
                {
                    AccountItemType = item.Key.ItemType,
                    TotalAmout = item.Sum(p => p.GetMoney()),
                    Date = item.Key.Date,
                    Count = item.Count(),
                    Name = "{0}".FormatWith(item.Key.Date.ToString(LocalizedStrings.CultureName.DateTimeFormat.YearMonthPattern, LocalizedStrings.CultureName)),
                };
            }
        }

        /// <summary>
        /// Loads the settle monthly report.
        /// </summary>
        /// <param name="searchingCondition">The seaching condtion.</param>
        /// <returns></returns>
        public IEnumerable<SummaryDetails> LoadSettleMonthlyReport(DetailsCondition searchingCondition)
        {

            var settleAmount = AccountBookDataContext
                .AccountItems.Where(p => p.Type == searchingCondition.IncomeOrExpenses
                    && p.CreateTime.Year == searchingCondition.StartDate.Value.Year && p.CreateTime.Month >= searchingCondition.StartDate.Value.Month)

                    .GroupBy(p => new
                    {
                        Date = new DateTime(p.CreateTime.Year, p.CreateTime.Month, 1),
                        ItemType = p.Type
                    })
                    .ToList();

            Func<ItemType, string> incomeOrExpense = (i) => i == ItemType.Expense ? AppResources.Expense : AppResources.Income;

            foreach (var item in settleAmount)
            {
                yield return new SummaryDetails
                {
                    Count = item.Count(),
                    Date = item.Key.Date,
                    TotalAmout = item.Sum(p => p.GetMoney()),
                    AccountItemType = item.Key.ItemType,
                    Name = "{0}".FormatWith(item.Key.Date.ToString(LocalizedStrings.CultureName.DateTimeFormat.YearMonthPattern, LocalizedStrings.CultureName)),
                };
            }


        }

        /// <summary>
        /// Loads the test data.
        /// </summary>
        /// <param name="dc">The dc.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public IEnumerable<SummaryDetails> LoadTestData(DetailsCondition dc, int start, int end)
        {
            var query = Enumerable.Range(1, 5)
                .Select(p => new DateTime(DateTime.Now.Year, p, 1))
                .GroupBy(p => new
                {
                    ItemType = ItemType.Expense,
                    Date = p,
                });

            Random rd = new Random(DateTime.Now.Millisecond * new Random().Next());

            foreach (var item in query)
            {
                yield return new SummaryDetails()
                {
                    AccountItemType = item.Key.ItemType,
                    TotalAmout = (decimal)rd.Next(start, end),
                    Date = item.Key.Date,
                    Count = item.Count(),
                    Name = "{0}".FormatWith(item.Key.Date.ToString(LocalizedStrings.CultureName.DateTimeFormat.YearMonthPattern, LocalizedStrings.CultureName)),
                };
            }

        }

        /// <summary>
        /// Loads the budget monthly report.
        /// </summary>
        /// <param name="itemType">Type of the item.</param>
        /// <param name="searchingScope">The searching scope.</param>
        /// <returns></returns>
        public SummaryDetails LoadBudgetMonthlyReport(ItemType itemType, SearchingScope searchingScope)
        {
            var date = DateTime.Now.Date.GetFirstDayOfMonth().Date;
            var totalAmount = AccountBookDataContext.BudgetProjects.Where(p => p.ItemType == itemType
                ).Select(p => p.TotalAmount)
                .AsEnumerable().Sum();

            return new SummaryDetails()
             {
                 AccountItemType = itemType,
                 TotalAmout = totalAmount,
                 Date = date,
                 Count = 1,
                 Name = "{0}".FormatWith(date.ToString(LocalizedStrings.CultureName.DateTimeFormat.YearMonthPattern, LocalizedStrings.CultureName)),
             };
        }
    }
}
