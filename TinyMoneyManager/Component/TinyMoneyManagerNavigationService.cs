namespace TinyMoneyManager
{
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;

    public class TinyMoneyManagerNavigationService : CustomizedNavigationService<System.Guid>
    {
        public static readonly TinyMoneyManagerNavigationService Instance = new TinyMoneyManagerNavigationService();

        public override string GetPageUri(INavigationServiceInfoProvider<System.Guid> key)
        {
            string str = string.Empty;
            string pageName = key.PageName;
            if (pageName == null)
            {
                return str;
            }
            if (!(pageName == "AccountItems"))
            {
                if (pageName != "Repayments")
                {
                    if (pageName != "RepaymentsTableForReceieveOrPayBack")
                    {
                        return str;
                    }
                    return ViewPath.RepaymentOrReceiptViewerPage.FormatWith(new object[] { key.Key });
                }
            }
            else
            {
                return "/Pages/AccountItemViews/AccountItemViewer.xaml?id={0}".FormatWith(new object[] { key.Key });
            }
            return ViewPath.BorrowLeanInfoViewerPage.FormatWith(new object[] { key.Key });
        }
    }
}

