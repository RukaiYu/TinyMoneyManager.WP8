using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SilverlightWP7MultiTouchSample.ViewModels
{
    /// <summary>
    /// Simple ViewModel for the MainPage
    /// </summary>
    public class MainPageViewModel:ViewModelBase
    {
        public MainPageViewModel()
        {
            //Initialize a collection of Pictures
            Pictures = new ObservableCollection<BitmapImage>()
                           {
                               new BitmapImage(new Uri("Images/JellyFish.jpg", UriKind.RelativeOrAbsolute)),
                               new BitmapImage(new Uri("Images/Desert.jpg", UriKind.RelativeOrAbsolute))
                           };
            SelectedPicture = Pictures.FirstOrDefault();
        }

        /// <summary>
        /// Pictures collection
        /// </summary>
        private ObservableCollection<BitmapImage> _pictures;
        public ObservableCollection<BitmapImage> Pictures
        {
            get { return _pictures; }
            set { _pictures = value; RaisePropertyChanged("Pictures"); }
        }

        /// <summary>
        /// The Selected picture
        /// </summary>
        private BitmapImage _selectedPicture;
        public BitmapImage SelectedPicture
        {
            get { return _selectedPicture; }
            set { _selectedPicture = value; RaisePropertyChanged("SelectedPicture"); }
        }
    }
}
