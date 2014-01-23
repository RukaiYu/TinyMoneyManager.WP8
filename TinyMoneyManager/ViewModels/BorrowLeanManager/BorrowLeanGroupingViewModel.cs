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
using TinyMoneyManager.Data.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

using System.Linq;
using TinyMoneyManager.Component;
using TinyMoneyManager.ViewModels.Common;
namespace TinyMoneyManager.ViewModels.BorrowLeanManager
{
    public class BorrowLeanGroupByTimeViewModel : ObjectGroupingViewModel<DateTime, Repayment>
    {
        public BorrowLeanGroupByTimeViewModel(DateTime createTime)
            : base(createTime)
        {

        }

        public override string HeaderInfo
        {
            get
            {
                return
                    Key.Date.ToString(Key.Year == DateTime.Now.Year ? ConstString.formatWithoutYear : ConstString.FormatWithShortDateAndWeekWithYear, LocalizedStrings.CultureName);
            }
        }

        public override string TotalAmount
        {
            get
            {
                return string.Empty;
            }
        }
    }



}
