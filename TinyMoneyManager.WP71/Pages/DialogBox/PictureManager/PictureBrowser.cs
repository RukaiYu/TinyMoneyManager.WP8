namespace TinyMoneyManager.Pages.DialogBox.PictureManager
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Tasks;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.ViewModels;

    public class PictureBrowser : PhoneApplicationPage
    {
        private bool _contentLoaded;
        internal ApplicationBarIconButton BrowserLib;
        internal Grid LayoutRoot;
        internal ListBox menuList;
        private static PictureActionHandler pictureHandler;
        public PictureViewModel pictureViewModel;
        internal ApplicationBarIconButton TakePicture;

        public PictureBrowser()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.Pictures = new ObservableCollection<PictureInfo>();
            this.initAppBar();
            this.pictureViewModel = ViewModelLocator.PicturesViewModel;
            base.DataContext = this;
        }

        private void AddPictureInPhone(System.IO.Stream stream)
        {
            PictureInfo info2 = new PictureInfo {
                AttachedId = this.CurrentAttachedObjectId
            };
            PictureInfo item = info2;
            BitmapImage image = new BitmapImage();
            item.Tag = this.TagName;
            image.SetSource(stream);
            item.PictureId = System.Guid.NewGuid();
            item.Content = image;
            item.SetFileName();
            item.ContentSource = stream;
            this.Pictures.Add(item);
            PictureHandler.OnAfterAdd(item);
        }

        private void BrowserLib_Click(object sender, System.EventArgs e)
        {
            PhotoChooserTask task = new PhotoChooserTask();
            task.Completed += new System.EventHandler<PhotoResult><PhotoResult>(this.pct_Completed);
            task.Show();
        }

        private void cameraCapture_Completed(object sender, PhotoResult e)
        {
            if (e.Error != null)
            {
                this.AlertNotification(e.Error.Message, null);
            }
            else
            {
                this.AddPictureInPhone(e.ChosenPhoto);
            }
        }

        private void DeleteBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            System.Action callBack = null;
            PictureInfo item = this.GetPictureInfo(sender);
            try
            {
                if (callBack == null)
                {
                    callBack = delegate {
                        this.AlertNotification(AppResources.OperationSuccessfullyMessage, null);
                        this.Pictures.Remove(item);
                        PictureHandler.OnAfterDelete(item);
                    };
                }
                this.pictureViewModel.DeletingObjectService<PictureInfo>(item, i => AppResources.Picture, callBack);
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void EditBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            System.Action<String><string> resultSetter = null;
            PictureInfo item = this.GetPictureInfo(sender);
            if (item != null)
            {
                if (resultSetter == null)
                {
                    resultSetter = delegate (string s) {
                        item.Comments = s;
                        pictureHandler.OnUpdatePicture(item);
                    };
                }
                this.NavigateToEditValueInTextBoxEditorPage(AppResources.Comments, item.Comments, delegate (TextBox t) {
                    t.SelectAll();
                }, null, resultSetter);
            }
        }

        public PictureInfo GetPictureInfo(object sender)
        {
            return ((sender as MenuItem).Tag as PictureInfo);
        }

        public static void Go(System.Guid attachedId, string tagName, PhoneApplicationPage fromPage)
        {
            fromPage.NavigateTo("/pages/DialogBox/PictureManager/PictureBrowser.xaml?id={0}&tagName={1}", new object[] { attachedId, tagName });
        }

        private void initAppBar()
        {
            base.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.Browser.ToLowerInvariant();
            base.ApplicationBar.GetIconButtonFrom(1).Text = AppResources.TakePicture.ToLowerInvariant();
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Application.LoadComponent(this, new Uri("/TinyMoneyManager;component/Pages/DialogBox/PictureManager/PictureBrowser.xaml", UriKind.Relative));
                this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
                this.menuList = (ListBox) base.FindName("menuList");
                this.BrowserLib = (ApplicationBarIconButton) base.FindName("BrowserLib");
                this.TakePicture = (ApplicationBarIconButton) base.FindName("TakePicture");
            }
        }

        public void LoadPictures(System.Guid attachedId, string tagName)
        {
            System.Action a = null;
            this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.Picture }));
            this.Pictures.Clear();
            if (pictureHandler != null)
            {
                System.Collections.Generic.IEnumerable<PictureInfo><PictureInfo> list = pictureHandler.OnGetItems();
                string folderInPictures = this.pictureViewModel.GetFolderInPictures(tagName);
                this.pictureViewModel.LoadContent(folderInPictures, list, new System.Action<PictureInfo><PictureInfo>(this.Pictures.Add));
                this.WorkDone();
            }
            else
            {
                this.pictureViewModel.LoadData(attachedId, tagName, this.Pictures);
                if (a == null)
                {
                    a = delegate {
                        this.WorkDone();
                    };
                }
                base.Dispatcher.BeginInvoke(a);
            }
        }

        private void menuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Func<PictureInfo><PictureInfo> func = null;
            PictureInfo item = this.menuList.SelectedItem as PictureInfo;
            if (item != null)
            {
                if (func == null)
                {
                    func = () => item;
                }
                PictureViewer.bitmapGetter = func;
                this.NavigateTo("/pages/DialogBox/PictureManager/PictureViewer.xaml?id={0}", new object[] { item.Id });
                this.menuList.SelectedItem = null;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string source = this.GetNavigatingParameter("id", null);
                this.TagName = this.GetNavigatingParameter("tagName", null);
                this.LoadPictures(source.ToGuid(), this.TagName);
            }
        }

        private void pct_Completed(object sender, PhotoResult e)
        {
            if (e.Error != null)
            {
                this.AlertNotification(e.Error.Message, null);
            }
            else if (e.ChosenPhoto != null)
            {
                this.AddPictureInPhone(e.ChosenPhoto);
            }
        }

        private void TakePicture_Click(object sender, System.EventArgs e)
        {
            CameraCaptureTask task = new CameraCaptureTask();
            task.Completed += new System.EventHandler<PhotoResult><PhotoResult>(this.cameraCapture_Completed);
            task.Show();
        }

        private void ViewFullMenuItem(object sender, RoutedEventArgs e)
        {
            PictureInfo item = this.GetPictureInfo(sender);
            PictureViewer.bitmapGetter = () => item;
            this.NavigateTo("/pages/DialogBox/PictureManager/PictureViewer.xaml?id={0}", new object[] { item.Id });
        }

        public System.Guid CurrentAttachedObjectId { get; set; }

        public static PictureActionHandler PictureHandler
        {
            get
            {
                if (pictureHandler == null)
                {
                    pictureHandler = new PictureActionHandler();
                }
                return pictureHandler;
            }
            set
            {
                pictureHandler = value;
            }
        }

        public ObservableCollection<PictureInfo> Pictures { get; set; }

        public string TagName { get; set; }
    }
}

