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
using TinyMoneyManager.Data.Model;
using System.Collections.ObjectModel;

namespace TinyMoneyManager.ViewModels.BorrowLeanManager
{
    public class BorrowLeanSeachingCondition : DetailsCondition
    {
        private Collection<Guid> fromPeoples;

        public Collection<Guid> FromPeoples
        {
            get { return fromPeoples; }
            set
            {
                if (fromPeoples != value)
                {
                    OnNotifyPropertyChanging("FromPeoples");
                    fromPeoples = value;
                    OnNotifyPropertyChanged("FromPeoples");
                }
            }
        }

        private RepaymentStatus _status;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public RepaymentStatus Status
        {
            get { return _status; }
            set
            {
                if (value != _status)
                {
                    OnNotifyPropertyChanging("Status");
                    _status = value;
                    OnNotifyPropertyChanged("Status");
                }
            }
        }


    }
}
