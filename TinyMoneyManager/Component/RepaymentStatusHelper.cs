namespace TinyMoneyManager.Component
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Windows;

    public class RepaymentStatusHelper
    {
        private System.Collections.Generic.List<RepaymentStatusWapper> list;

        public static RepaymentStatusHelper Instance
        {
            get
            {
                return (Application.Current.Resources["repaymentStatusList"] as RepaymentStatusHelper);
            }
        }

        public bool NeedReLoad { get; set; }

        public System.Collections.Generic.List<RepaymentStatusWapper> RepaymentStatusList
        {
            get
            {
                if ((this.list == null) || this.NeedReLoad)
                {
                    RepaymentStatusWapper.NameGetter = new System.Func<String, String>(LocalizedStrings.GetLanguageInfoByKey);
                    this.list = new System.Collections.Generic.List<RepaymentStatusWapper> { new RepaymentStatusWapper(0), new RepaymentStatusWapper(RepaymentStatus.OnGoing) };
                    this.NeedReLoad = false;
                }
                return this.list;
            }
        }
    }
}

