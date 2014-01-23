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

            var data = AccountBookDataContext.BudgetMonthlyReports
                .Where(p => p.ItemType == searchingCondition.IncomeOrExpenses
                && p.Year == searchingCondition.StartDate.Value.Year && p.Month >= searchingCondition.StartDate.Value.Month)
                .ToList();

            var query = data.GroupBy(p => new
                {
                    Date = new DateTime(p.Year, p.Month == 0 ? 1 : p.Month, 1),
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
        /// Loads the settle monthly report by customized.
        /// </summary>
        /// <param name="searchingCondition">The searching condition.</param>
        /// <returns></returns>
        public IEnumerable<SummaryDetails> LoadSettleMonthlyReportByCustomized(DetailsCondition searchingCondition)
        {
            // ensure the start year,
            var startYear = DateTime.Now.Year;
            var startYearToSearch = startYear;
            var endYear = startYear;
            var currentMonth = DateTime.Now.Month;

            int startDay = AppSetting.Instance.BudgetStatsicSettings.StartDay, endDay = AppSetting.Instance.BudgetStatsicSettings.EndDay;
            int startMonth = 1, endMonth = 1;

            var itemType = searchingCondition.IncomeOrExpenses;

            for (int i = 1; i < 13; i++)
            {
                if (i > currentMonth) { break; }

                startMonth = i;

                if (i == 12)
                {
                    endYear = startYear + 1;
                    endMonth = 1;
                }
                else
                {
                    endMonth = i + 1;
                }

                if (startDay > 3)
                {
                    if (i == 1)
                    {
                        startMonth = 12;
                        endMonth = 1;

                        startYearToSearch = startYear - 1;
                    }
                }


                // create group.
                var startDate = new DateTime(startYearToSearch, startMonth, startDay);

                var endDate = new DateTime(endYear, endMonth, endDay);

                var data =
                    AccountBookDataContext
                .AccountItems.Where(p => p.Type == searchingCondition.IncomeOrExpenses
                    && p.CreateTime.Date >= startDate && p.CreateTime.Date <= endDate)
                    .ToList();

                //groupDateList.Add(i.ToString(), new DateTime[] { startDate, endDate });

                var keyDate = new DateTime(startYear, i, 1);

                yield return new SummaryDetails
                  {
                      Count = data.Count(),
                      Date = keyDate,
                      TotalAmout = data.Sum(p => p.GetMoney()),
                      AccountItemType = itemType,
                      Name = keyDate.ToString(LocalizedStrings.CultureName.DateTimeFormat.YearMonthPattern, LocalizedStrings.CultureName)
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
