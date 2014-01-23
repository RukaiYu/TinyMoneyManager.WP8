using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using MultiTouch.Behaviors.Silverlight4;
using System.Windows.Interactivity;
using SilverlightWP7MultiTouchSample.ViewModels;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager.Pages.DialogBox.PictureManager
{
    public partial class PictureViewer : PhoneApplicationPage
    {
        private MultiTouchBehavior _multiTouchBehavior;

        public ImageViewModel ImageViewModel;

        public PictureViewer()
        {
            InitializeComponent();

            ImageViewModel = new SilverlightWP7MultiTouchSample.ViewModels.ImageViewModel();
            DataContext = ImageViewModel;

            this.Loaded += ImagePage_Loaded;
        }

        void ImagePage_Loaded(object sender, RoutedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(_image).OfType<MultiTouchBehavior>();
            if (behaviors.ToList().Count > 0)
            {
                _multiTouchBehavior = behaviors.First();
            }

            _multiTouchBehavior.Move(new Point(230, 250), 0, 200);
        }

        public static Func<PictureInfo> bitmapGetter;

        public PictureInfo Current { get; set; }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                Current = bitmapGetter == null ? new PictureInfo() : bitmapGetter();
                ImageViewModel.SelectedPicture = Current.Content;
               
                //HeightTextbox.Text = bitMap.PixelHeight.ToString();
                //WidthTextbox.Text = bitMap.PixelWidth.ToString();
                PageTitle.Text = Current.Comments;
            }
        }

        private void DoubleTapBehavior_DoubleTap(object sender, EventArgs e)
        {

        }
    }
}