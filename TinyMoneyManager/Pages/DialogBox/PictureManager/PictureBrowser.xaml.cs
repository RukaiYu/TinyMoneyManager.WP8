using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using TinyMoneyManager.ViewModels;
namespace TinyMoneyManager.Pages.DialogBox.PictureManager
{
    public partial class PictureBrowser : PhoneApplicationPage
    {
        private static PictureActionHandler pictureHandler;

        public static PictureActionHandler PictureHandler
        {
            get
            {
                if (pictureHandler == null)
                    pictureHandler = new PictureActionHandler();
                return pictureHandler;
            }
            set
            {
                pictureHandler = value;
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<PictureInfo> Pictures { get; set; }

        public string TagName { get; set; }

        public PictureViewModel pictureViewModel;

        public Guid CurrentAttachedObjectId { get; set; }

        public PictureBrowser()
        {
            InitializeComponent();

            TiltEffect.SetIsTiltEnabled(this, true);
            Pictures = new ObservableCollection<PictureInfo>();
            initAppBar();

            pictureViewModel = ViewModelLocator.PicturesViewModel;

            this.DataContext = this;

        }

        private void initAppBar()
        {
            this.ApplicationBar.GetIconButtonFrom(0)
                .Text = AppResources.Browser.ToLowerInvariant();

            this.ApplicationBar.GetIconButtonFrom(1)
                .Text = AppResources.TakePicture.ToLowerInvariant();
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                var attachedIdToGetPictures = this.GetNavigatingParameter("id");
                TagName = this.GetNavigatingParameter("tagName");
                LoadPictures(attachedIdToGetPictures.ToGuid(), TagName);
            }
        }

        /// <summary>
        /// Loads the pictures.
        /// </summary>
        /// <param name="attachedId">The attached id.</param>
        /// <param name="tagName">Name of the tag.</param>
        public void LoadPictures(Guid attachedId, string tagName)
        {
            this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(AppResources.Picture));

            //
            Pictures.Clear();
            if (pictureHandler != null)
            {
                var items = pictureHandler.OnGetItems();

                var folder = pictureViewModel.GetFolderInPictures(tagName);

                pictureViewModel.LoadContent(folder, items, Pictures.Add);

                this.WorkDone();
            }
            else
            {
                pictureViewModel.LoadData(attachedId, tagName, Pictures);

                Dispatcher.BeginInvoke(() =>
                {
                    this.WorkDone();
                });
            }
        }

        /// <summary>
        /// Handles the Click event of the BrowserLib control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BrowserLib_Click(object sender, EventArgs e)
        {
            Microsoft.Phone.Tasks.PhotoChooserTask pct = new Microsoft.Phone.Tasks.PhotoChooserTask();

            pct.Completed += new EventHandler<Microsoft.Phone.Tasks.PhotoResult>(pct_Completed);
            pct.Show();
        }

        /// <summary>
        /// PCT_s the completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void pct_Completed(object sender, Microsoft.Phone.Tasks.PhotoResult e)
        {
            if (e.Error != null)
            {
                this.AlertNotification(e.Error.Message);
            }
            else
            {
                if (e.ChosenPhoto == null)
                    return;

                AddPictureInPhone(e.ChosenPhoto);
            }

        }

        /// <summary>
        /// Adds the picture in phone.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void AddPictureInPhone(System.IO.Stream stream)
        {
            if (stream == null)
            {
            }
            else
            {
                var picInfo = new PictureInfo()
                {
                    AttachedId = CurrentAttachedObjectId,
                };

                BitmapImage bi = new BitmapImage();
                picInfo.Tag = TagName;
                bi.SetSource(stream);
                picInfo.PictureId = Guid.NewGuid();
                picInfo.Content = bi;
                picInfo.SetFileName();
                //picInfo.FullPath = pictureViewModel.GetFullPath(picInfo);
                picInfo.ContentSource = stream;
                Pictures.Add(picInfo);
                PictureHandler.OnAfterAdd(picInfo);
            }
        }

        private void TakePicture_Click(object sender, EventArgs e)
        {
            Microsoft.Phone.Tasks.CameraCaptureTask cameraCapture = new Microsoft.Phone.Tasks.CameraCaptureTask();

            cameraCapture.Completed += new EventHandler<Microsoft.Phone.Tasks.PhotoResult>(cameraCapture_Completed);

            cameraCapture.Show();
        }

        void cameraCapture_Completed(object sender, Microsoft.Phone.Tasks.PhotoResult e)
        {
            if (e.Error != null)
            {
                this.AlertNotification(e.Error.Message);
            }
            else
            {
                AddPictureInPhone(e.ChosenPhoto);
            }
        }

        /// <summary>
        /// Goes the specified attached id.
        /// </summary>
        /// <param name="attachedId">The attached id.</param>
        /// <param name="fromPage">From page.</param>
        public static void Go(Guid attachedId, string tagName, PhoneApplicationPage fromPage)
        {
            fromPage.NavigateTo("/pages/DialogBox/PictureManager/PictureBrowser.xaml?id={0}&tagName={1}", attachedId, tagName);
        }

        private void ViewFullMenuItem(object sender, RoutedEventArgs e)
        {
            var item = GetPictureInfo(sender);
            PictureViewer.bitmapGetter = () => item;
            this.NavigateTo("/pages/DialogBox/PictureManager/PictureViewer.xaml?id={0}", item.Id);

        }

        private void EditBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            var item = GetPictureInfo(sender);
            if (item != null)
            {
                this.NavigateToEditValueInTextBoxEditorPage(AppResources.Comments, item.Comments, (t) => t.SelectAll(),
                    null, (s) =>
                    {
                        item.Comments = s;
                        pictureHandler.OnUpdatePicture(item);
                    });
            }
        }

        /// <summary>
        /// Deletes the budget menu item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void DeleteBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            var item = GetPictureInfo(sender);

            try
            {
                pictureViewModel.DeletingObjectService(item, (i) => AppResources.Picture, () =>
                {
                    this.AlertNotification(AppResources.OperationSuccessfullyMessage);
                    Pictures.Remove(item);
                    PictureHandler.OnAfterDelete(item);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Gets the picture info.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <returns></returns>
        public PictureInfo GetPictureInfo(object sender)
        {
            return (sender as MenuItem).Tag as PictureInfo;
        }

        private void menuList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = menuList.SelectedItem as PictureInfo;
            if (item != null)
            {
                PictureViewer.bitmapGetter = () => item;
                this.NavigateTo("/pages/DialogBox/PictureManager/PictureViewer.xaml?id={0}", item.Id);
                menuList.SelectedItem = null;
            }


        }

    }

}