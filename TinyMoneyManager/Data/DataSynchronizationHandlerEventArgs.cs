using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMoneyManager.ViewModels;

namespace TinyMoneyManager.Data
{

    public class DataSynchronizationHandlerEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        private ViewModels.SynchronizationStepViewModel stepViewModel;

        public DataSynchronizationInfo HandlingData { get; set; }

        /// <summary>
        /// Gets the step view model.
        /// </summary>
        public ViewModels.SynchronizationStepViewModel StepViewModel
        {
            get { return stepViewModel; }
            private set { stepViewModel = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSynchronizationHandlerEventArgs"/> class.
        /// </summary>
        /// <param name="step">The step.</param>
        public DataSynchronizationHandlerEventArgs(SynchronizationStepViewModel step, DataSynchronizationInfo data)
        {
            this.HandlingData = new DataSynchronizationInfo();
            this.HandlingData.Action = data.Action;
            this.HandlingData.HandlingInfo = data.HandlingInfo;

            this.stepViewModel = step;
        }


    }

}
