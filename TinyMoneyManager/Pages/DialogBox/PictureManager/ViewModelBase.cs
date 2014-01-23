using System.ComponentModel;

namespace SilverlightWP7MultiTouchSample.ViewModels
{
    /// <summary>
    /// Base class for ViewModels
    /// </summary>
    public class ViewModelBase:INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation        
            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged(string name)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        #endregion
    }
}
