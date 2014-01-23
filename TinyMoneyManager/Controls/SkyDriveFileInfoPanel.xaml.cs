namespace TinyMoneyManager.Controls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using TinyMoneyManager;
    using TinyMoneyManager.Controls.SkyDriveDataSyncing;

    public partial class SkyDriveFileInfoPanel : UserControl, INotifyPropertyChanged
    {
        private string dataFile;

        private string file;

        private ObjectFromSkyDrive objectForShow;

        public event PropertyChangedEventHandler PropertyChanged;

        public SkyDriveFileInfoPanel()
        {
            this.InitializeComponent();
            base.DataContext = this;
            this.objectForShow = new ObjectFromSkyDrive();
            this.file = LocalizedStrings.GetLanguageInfoByKey("File");
            this.dataFile = App.AppName + " " + LocalizedStrings.GetLanguageInfoByKey("DataFile");
        }


        protected virtual void OnNotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Update(ObjectFromSkyDrive menuItem)
        {
            this.ObjectTypeIconImagePath.Source = new BitmapImage(new Uri(menuItem.ObjectTypeIconImagePath, UriKind.RelativeOrAbsolute));
            this.ObjectName.Text = menuItem.Name;
            this.Size.Text = menuItem.Size;
            this.From.Text = menuItem.From;
            this.SharedWith.Text = menuItem.ShareWith;
            this.ModifiedDate.Text = menuItem.UpdateTimeString;
            this.Description.Text = menuItem.Description;
        }

        public ObjectFromSkyDrive ObjectForShow
        {
            get
            {
                return this.objectForShow;
            }
            set
            {
                if (this.objectForShow != value)
                {
                    this.objectForShow = value;
                    this.OnNotifyPropertyChanged("ObjectForShow");
                }
            }
        }

        public string Title
        {
            get
            {
                return this.TitleLabel.Text;
            }
            set
            {
                this.TitleLabel.Text = value;
                this.OnNotifyPropertyChanged("Title");
            }
        }
    }
}

