namespace TinyMoneyManager.Pages.DialogBox.PictureManager
{
    using Microsoft.Phone.Controls;
    using MultiTouch.Behaviors.Silverlight4;
    using SilverlightWP7MultiTouchSample.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interactivity;
    using System.Windows.Navigation;
    using TinyMoneyManager.Data.Model;

    public class PictureViewer : PhoneApplicationPage
    {
        private bool _contentLoaded;
        internal Image _image;
        private MultiTouchBehavior _multiTouchBehavior;
        internal TextBlock ApplicationTitle;
        public static System.Func<PictureInfo><PictureInfo> bitmapGetter;
        internal Grid ContentPanel;
        public SilverlightWP7MultiTouchSample.ViewModels.ImageViewModel ImageViewModel;
        internal Grid LayoutRoot;
        internal TextBlock PageTitle;
        internal StackPanel TitlePanel;

        public PictureViewer()
        {
            this.InitializeComponent();
            this.ImageViewModel = new SilverlightWP7MultiTouchSample.ViewModels.ImageViewModel();
            base.DataContext = this.ImageViewModel;
            base.Loaded += new RoutedEventHandler(this.ImagePage_Loaded);
        }

        private void DoubleTapBehavior_DoubleTap(object sender, System.EventArgs e)
        {
        }

        private void ImagePage_Loaded(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.IEnumerable<MultiTouchBehavior><MultiTouchBehavior> source = Interaction.GetBehaviors(this._image).OfType<MultiTouchBehavior>();
            if (source.ToList<MultiTouchBehavior>().Count > 0)
            {
                this._multiTouchBehavior = source.First<MultiTouchBehavior>();
            }
            this._multiTouchBehavior.Move(new Point(230.0, 250.0), 0.0, 200.0);
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Application.LoadComponent(this, new Uri("/TinyMoneyManager;component/Pages/DialogBox/PictureManager/PictureViewer.xaml", UriKind.Relative));
                this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
                this.TitlePanel = (StackPanel) base.FindName("TitlePanel");
                this.ApplicationTitle = (TextBlock) base.FindName("ApplicationTitle");
                this.PageTitle = (TextBlock) base.FindName("PageTitle");
                this.ContentPanel = (Grid) base.FindName("ContentPanel");
                this._image = (Image) base.FindName("_image");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.Current = (bitmapGetter == null) ? new PictureInfo() : bitmapGetter();
                this.ImageViewModel.SelectedPicture = this.Current.Content;
                this.PageTitle.Text = this.Current.Comments;
            }
        }

        public PictureInfo Current { get; set; }
    }
}

