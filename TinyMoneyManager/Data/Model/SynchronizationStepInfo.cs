namespace TinyMoneyManager.Data.Model
{
    using NkjSoft.Extensions;
    using System;
    using TinyMoneyManager.Component;

    public class SynchronizationStepInfo : NotionObject
    {
        private static string defImage = "/TinyMoneyManager;component/images/{0}.png";
        private bool isStart;
        public static readonly string IsStartProperty = "IsStart";
        private string stepInfo = string.Empty;
        private TinyMoneyManager.Component.StepStatus stepStatus = TinyMoneyManager.Component.StepStatus.Processing;
        public static readonly string StepStatusProperty = "StepStatus";

        public bool IsStart
        {
            get
            {
                return this.isStart;
            }
            set
            {
                this.isStart = value;
                this.OnNotifyPropertyChanged(IsStartProperty);
            }
        }

        public string StepInfo
        {
            get
            {
                return this.stepInfo;
            }
            set
            {
                this.stepInfo = value;
                this.OnNotifyPropertyChanged("StepInfo");
            }
        }

        public TinyMoneyManager.Component.StepStatus StepStatus
        {
            get
            {
                return this.stepStatus;
            }
            set
            {
                this.stepStatus = value;
                this.OnNotifyPropertyChanged(StepStatusProperty);
                this.OnNotifyPropertyChanged("StepStatusImage");
            }
        }

        public string StepStatusImage
        {
            get
            {
                return defImage.FormatWith(new object[] { this.StepStatus.ToString() });
            }
        }
    }
}

