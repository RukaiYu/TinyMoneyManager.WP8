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
using TinyMoneyManager.Component;
using System.ComponentModel;

namespace TinyMoneyManager.Controls
{
    public class ExpenseAndIncomeListSelectModeManager : NotionObject// DependencyObject, INotifyPropertyChanged
    {
        private bool isExpenseListAtSelectMode;
        public bool IsExpenseListAtSelectMode
        {
            get { return isExpenseListAtSelectMode; }
            set
            {
                if (isExpenseListAtSelectMode != value)
                {
                    isExpenseListAtSelectMode = value;
                    OnNotifyPropertyChanged("IsExpenseListAtSelectMode");
                }
            }
        }

        private bool isIncomeListAtSelectMode;
        public bool IsIncomeListAtSelectMode
        {
            get { return isIncomeListAtSelectMode; }
            set
            {
                if (isIncomeListAtSelectMode != value)
                {
                    isIncomeListAtSelectMode = value;
                    OnNotifyPropertyChanged("IsIncomeListAtSelectMode");
                }
            }
        }

        private bool isExpenseListNoAtSelectMode;
        public bool IsExpenseListNoAtSelectMode
        {
            get { return isExpenseListNoAtSelectMode; }
            set
            {
                if (isExpenseListNoAtSelectMode != value)
                {
                    isExpenseListNoAtSelectMode = value;
                    OnNotifyPropertyChanged("IsExpenseListNoAtSelectMode");
                }
            }
        }

        private bool isIncomeListNoAtSelectMode;
        public bool IsIncomeListNoAtSelectMode
        {
            get { return isIncomeListNoAtSelectMode; }
            set
            {
                if (isIncomeListNoAtSelectMode != value)
                {
                    isIncomeListNoAtSelectMode = value;
                    OnNotifyPropertyChanged("IsIncomeListNoAtSelectMode");
                }
            }
        }

        public void ChangeExpenseListSelectMode()
        {
            bool isSelectMode = !isExpenseListAtSelectMode;

            this.IsExpenseListAtSelectMode = isSelectMode;
            this.IsExpenseListNoAtSelectMode = !isSelectMode;

        }


        public void ChangeIncomeListSelectMode()
        {
            bool isSelectMode = !isIncomeListAtSelectMode;

            this.IsIncomeListAtSelectMode = isSelectMode;
            this.IsIncomeListNoAtSelectMode = !isSelectMode;
        }

        public ExpenseAndIncomeListSelectModeManager()
        {
            this.IsExpenseListAtSelectMode = false;
            this.IsIncomeListAtSelectMode = false;

            this.IsExpenseListNoAtSelectMode = true;
            this.IsIncomeListNoAtSelectMode = true;
        }

        public bool Status
        {
            get { return false; } 
        }

        internal void ResetCheckboxStatus(bool isSelectModeOn)
        {
            if (!isSelectModeOn)
            {
                OnNotifyPropertyChanged("Status");
            }
        }
    }
}
