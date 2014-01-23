namespace TinyMoneyManager.Component
{
    using System;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component.Converter;

    public class RepaymentStatusWapper
    {
        public static System.Func<String,String> NameGetter;

        public RepaymentStatusWapper(RepaymentStatus status)
        {
            this.Status = status;
            this.Name = NameGetter(status.ToString());
        }

        public string Color
        {
            get
            {
                return TinyMoneyManager.Component.Converter.RepaymentStatusToColorConverter.GetColorFromStatus(this.Status);
            }
        }

        public string Name { get; set; }

        public RepaymentStatus Status { get; set; }
    }
}

