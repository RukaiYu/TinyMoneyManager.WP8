using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.Data.Model
{
    using NkjSoft.Extensions;
    /// <summary>
    /// 
    /// </summary>
    public class BudgetStatsicSetting : NotionObject
    {
        private DateTime _startDate;

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                if (value != _startDate)
                {
                    OnNotifyPropertyChanging("StartDate");
                    _startDate = value;
                    OnNotifyPropertyChanged("StartDate");
                }
            }
        }

        private DateTime _endDate;

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                if (value != _endDate)
                {
                    OnNotifyPropertyChanging("EndDate");
                    _endDate = value;
                    OnNotifyPropertyChanged("EndDate");
                }
            }
        }

        /// <summary>
        /// Gets the start day.
        /// </summary>
        /// <value>
        /// The start day.
        /// </value>
        public int StartDay
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the end day.
        /// </summary>
        /// <value>
        /// The end day.
        /// </value>
        public int EndDay
        {
            get;
            set;
        }

        private BudgetStatsicMode _budgetStatsicMode;
        public static string BudgetStatsicModeProperty = "BudgetStatsicMode";
        public BudgetStatsicMode BudgetStatsicMode
        {
            get { return _budgetStatsicMode; }
            set
            {
                if (value != _budgetStatsicMode)
                {
                    OnNotifyPropertyChanging(BudgetStatsicModeProperty);
                    _budgetStatsicMode = value;
                    OnNotifyPropertyChanged(BudgetStatsicModeProperty);
                }
            }
        }

        private int _BudgetStatsicModeIndex;
        public static string BudgetStatsicModeIndexProperty = "BudgetStatsicModeIndex";
        /// <summary>
        /// Gets or sets the index of the budget statsic mode.
        /// </summary>
        /// <value>
        /// The index of the budget statsic mode.
        /// </value>
        public int BudgetStatsicModeIndex
        {
            get { return (int)BudgetStatsicMode - 1; }
            set
            {
                if (value != _BudgetStatsicModeIndex - 1)
                {
                    OnNotifyPropertyChanging(BudgetStatsicModeIndexProperty);
                    _BudgetStatsicModeIndex = value + 1;
                    this.BudgetStatsicMode = this._BudgetStatsicModeIndex.ToEnum<BudgetStatsicMode>();
                    OnNotifyPropertyChanged(BudgetStatsicModeIndexProperty);
                }
            }
        }

        private bool _hasCalculateDates;

        /// <summary>
        /// Gets or sets a value indicating whether this instance has calculate dates.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has calculate dates; otherwise, <c>false</c>.
        /// </value>
        public bool HasCalculateDates
        {
            get { return _hasCalculateDates; }
            set
            {
                if (value != _hasCalculateDates)
                {
                    OnNotifyPropertyChanging("HasCalculateDates");
                    _hasCalculateDates = value;
                    OnNotifyPropertyChanged("HasCalculateDates");
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetStatsicSetting" /> class.
        /// </summary>
        public BudgetStatsicSetting()
        {
            this._budgetStatsicMode = Model.BudgetStatsicMode.BudgetStaticModeOfByMonth;
            this.StartDay = 1;
            this.EndDay = 30;
        }



        /// <summary>
        /// Calculates this instance.
        /// </summary>
        /// <param name="dateOffset">The date offset.</param>
        public bool Calculate(DateTime? dateOffset = null)
        {
            HasCalculateDates = true;

            if (dateOffset == null)
            {
                dateOffset = DateTime.Now;
            }

            if (this.BudgetStatsicMode == Model.BudgetStatsicMode.BudgetStaticModeOfByMonth)
            {
                this.StartDay = 1;
                this.StartDate = new DateTime(dateOffset.Value.Year, dateOffset.Value.Month, 1, 0, 0, 0);

                this.EndDate = this.StartDate.AddMonths(1).AddDays(-1);
                this.EndDay = DateTime.Now.GetLastDayOfMonth().Day;
                return false;
            }
            else
            {
                var month = dateOffset.Value.Month;
                var year = dateOffset.Value.Year;
                var startDay = StartDay;

                var s_month = month;
                var e_month = month;
                var s_year = year;
                var dayOfThisMonth = DateTime.Now.Day;

                // 20
                if (dayOfThisMonth >= startDay)
                {
                    s_month = DateTime.Now.Month;
                    e_month = s_month + 1;

                    if (e_month == 13)
                    {
                        e_month = 1;
                        year = s_year + 1;
                    }
                }
                else
                {
                    s_month = DateTime.Now.Month - 1;

                    if (s_month == 0)
                    {
                        s_month = 12;
                        s_year = year - 1;
                    }
                }

                var lastDay = new DateTime(year, e_month, 1)
                 .GetLastDayOfMonth();

                if (EndDay > lastDay.Day)
                {
                    EndDay = lastDay.Day;
                }

                this.StartDate = new DateTime(s_year, s_month, StartDay);
                this.EndDate = new DateTime(year, e_month, EndDay);
            }

            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum BudgetStatsicMode
    {
        BudgetStaticModeOfByMonth = 1,
        BudgetStaticModeOfCustomized = 2,
    }
}
