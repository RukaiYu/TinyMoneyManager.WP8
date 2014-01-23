using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMoneyManager.Component;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace TinyMoneyManager.Controls
{
    public interface ISummaryChart
    {
        SummaryDetailsCollection ExpensesSummaryDetailsCollection { get; set; }

        bool HasLoadData { get; set; }
        bool NeedRefreshData { get; set; }

        void LoadData(DetailsCondition dc, IEnumerable<SummaryDetails> source);

        string SummaryTitle { get; set; }

        void DetectImageSize(PageOrientation pageOrientation);

        Action<UserControl> ToShowChartInFullScreen { get; set; }
    }

    public static class ISummaryChartExtensions
    {
        public static void SetNeedRefreshData(this ISummaryChart chart)
        {
            if (chart != null)
                chart.NeedRefreshData = true;
        }
    }
}
