using System;
using System.Windows.Media.Imaging;

namespace SilverlightWP7MultiTouchSample.ViewModels
{
    /// <summary>
    /// ViewModel for the sample ImagePage
    /// </summary>
    public class ImageViewModel : ViewModelBase
    {
        public ImageViewModel()
        {
            this.SelectedPicture = new BitmapImage();
        }

        /// <summary>
        /// The Selected picture
        /// </summary>
        private BitmapImage _selectedPicture;
        public BitmapImage SelectedPicture
        {
            get { return _selectedPicture; }
            set
            {
                _selectedPicture = value;
                RaisePropertyChanged("SelectedPicture");
            }
        }
    }
}
