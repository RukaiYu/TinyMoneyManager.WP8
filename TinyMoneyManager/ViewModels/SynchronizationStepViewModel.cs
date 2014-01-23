namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class SynchronizationStepViewModel : NkjSoftViewModelBase
    {
        public static string CurrentStepReplacer = string.Empty;
        public static string FailedReplacer = string.Empty;
        public static string SuccessReplacer = string.Empty;

        public SynchronizationStepViewModel(string stepInfo)
        {
            SynchronizationStepInfo info = new SynchronizationStepInfo
            {
                StepInfo = stepInfo
            };
            this.Step = info;
            this.Step.PropertyChanged += new PropertyChangedEventHandler(this.Step_PropertyChanged);
            this.stepInfoBackup = stepInfo;
        }

        public void Failed()
        {
            this.Step.StepStatus = StepStatus.Error;
        }

        public void Failed(string message)
        {
            this.Step.StepStatus = StepStatus.Error;
            this.Step.StepInfo = this.Step.StepInfo + ". " + message;
        }

        private string getFailedInfo(string stepInfo)
        {
            return (stepInfo.Replace(CurrentStepReplacer, string.Empty) + FailedReplacer);
        }

        private string getSuccessInfo(string stepInfo)
        {
            return (stepInfo.Replace(CurrentStepReplacer, string.Empty) + SuccessReplacer);
        }

        public void ResetStep()
        {
            this.Step.StepStatus = StepStatus.Processing;
            this.Step.IsStart = false;
        }

        public void Start()
        {
            this.Step.StepStatus = StepStatus.Processing;
            this.Step.IsStart = true;
        }

        private void Step_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SynchronizationStepInfo.StepStatusProperty)
            {
                if (this.Step.StepStatus == StepStatus.Error)
                {
                    this.Step.StepInfo = this.getFailedInfo(this.Step.StepInfo);
                }
                if (this.Step.StepStatus == StepStatus.Success)
                {
                    this.Step.StepInfo = this.getSuccessInfo(this.Step.StepInfo);
                }
                if (this.Step.StepStatus == StepStatus.Processing)
                {
                    this.Step.StepInfo = this.stepInfoBackup;
                }
            }
        }

        public void Success()
        {
            this.Step.StepStatus = StepStatus.Success;
        }

        public System.Func<SynchronizationStepViewModel, Boolean> ExecuteAction { get; set; }

        public System.Func<SynchronizationStepViewModel, String> HandleError { get; set; }

        public SynchronizationStepInfo Step { get; set; }

        private string stepInfoBackup { get; set; }
    }
}

