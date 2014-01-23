namespace TinyMoneyManager.Controls
{
    using Microsoft.Live;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Markup;
    using System.Windows.Media;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls.SkyDriveDataSyncing;

    public partial class SkyDriveFolderAndFileBrowser : UserControl
    {
        private static bool _reshowAppBar;

        private Color beforeColor;
        private double beforeOpacity = 1.0;

        private Stack<string> folderIds = new Stack<string>();
        private bool hasRootDirFolder;
        public static string JustMeTag = string.Empty;


        private string ReportTotalCountOfObjectsFromSkyDriveText;
        public System.Collections.Generic.Dictionary<String, String> shareWithKeyValue;
        private SkyDriveFileInfoPanel skyDriveFileInfoPanel;

        private string title;
        public event System.EventHandler Closed;

        public event System.EventHandler<ObjectBrowseringChangedHandlerEventArgs> ObjectBrowseringChanged;

        public event System.EventHandler<ObjectBrowseringChangedHandlerEventArgs> ObjectBrowseringChanging;

        public event System.EventHandler<ReportStatusHandlerEventArgs> ReportingSyncingStatus;

        public SkyDriveFolderAndFileBrowser(Microsoft.Live.LiveConnectClient liveConnectClient)
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.LiveConnectClient = liveConnectClient;
            this.ObjectsFromSkyDrive = new ObservableCollection<ObjectFromSkyDrive>();
            this.SkyDriveObjectList.ItemsSource = this.ObjectsFromSkyDrive;
            this.beforeOpacity = SystemTray.Opacity;
            this.beforeColor = SystemTray.ForegroundColor;
            this.skyDriveFileInfoPanel = new SkyDriveFileInfoPanel();
            this.ReportTotalCountOfObjectsFromSkyDriveText = LocalizedStrings.GetLanguageInfoByKey("ReportTotalCountOfObjectsFromSkyDriveText");
            this.ObjectOpreationPanel.Children.Add(this.skyDriveFileInfoPanel);
            this.InitializeShareWithKeyValue();
        }

        private void Close()
        {
            this.Close(this, null);
        }

        public void Close(object sender, System.EventArgs e)
        {
            SystemTray.Opacity = this.beforeOpacity;
            SystemTray.ForegroundColor = this.beforeColor;
            this.WorkDone();
            this.OnDialogClosed(e);
            ((PhoneApplicationPage)RootVisual.Content).BackKeyPress -= new System.EventHandler<CancelEventArgs>(this.page_BackKeyPress);
            this.ChildWindowPopup.IsOpen = false;
            if (_reshowAppBar)
            {
                if (((PhoneApplicationPage)RootVisual.Content).ApplicationBar != null)
                {
                    ((PhoneApplicationPage)RootVisual.Content).ApplicationBar.IsVisible = true;
                }
            }
        }

        private void createNoteListEntry(string name, string fileId, string fileExtension, System.Collections.Generic.IDictionary<String, Object> datum)
        {
            ObjectFromSkyDrive item = new ObjectFromSkyDrive(fileId, fileExtension.Replace(".", null))
            {
                Name = name
            };
            if (datum != null)
            {
                item.Size = this.getValueFromDict<double>(datum, "size", v => System.Convert.ToDouble(v) / 1024.0, 0.0).ToString("F2") + " KB";
                item.From = this.getValueFromDict<System.Collections.Generic.Dictionary<string, object>>(datum, "from", v => v as System.Collections.Generic.Dictionary<String, Object>, null)["name"].ToString();
                item.CreateTime = new System.DateTime?(this.getValueFromDict<System.DateTime>(datum, "created_time", v => System.Convert.ToDateTime(v), System.DateTime.Now));
                item.UpdateTime = new System.DateTime?(this.getValueFromDict<System.DateTime>(datum, "updated_time", v => System.Convert.ToDateTime(v), System.DateTime.Now));
                item.Description = this.getValueFromDict<string>(datum, "description", delegate(object v)
                {
                    if (v != null)
                    {
                        return v.ToString();
                    }
                    return string.Empty;
                }, "..");
                System.Collections.Generic.Dictionary<string, object> dictionary = datum["shared_with"] as System.Collections.Generic.Dictionary<String, Object>;
                if (dictionary == null)
                {
                    item.ShareWith = "unknown";
                }
                else
                {
                    item.ShareWith = this.GetShareWithByKey(dictionary["access"].ToString());
                }
                item.ParentId = this.getValueFromDict<string>(datum, "parent_id", v => v.ToString(), string.Empty);
            }
            this.ObjectsFromSkyDrive.Add(item);
        }

        private void DoSelectedAndCloseDialog(ObjectFromSkyDrive sp)
        {
            ObjectBrowseringChangedHandlerEventArgs e = new ObjectBrowseringChangedHandlerEventArgs(sp.FileType, sp.ShareWith, sp.Name);
            this.OnObjectBrowseringChanging(e);
            if (!e.SelectAsResult)
            {
                this.IsSelectAFolderAsResult = false;
            }
            else
            {
                this.SelectedItem = sp;
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
        }

        public void Find(string root)
        {
            this.findNoteList(root);
        }

        private void findNoteList(string newFolderId)
        {
            this.BusyForWork("loading...");
            if ((this.folderIds.Count == 0) || (this.folderIds.Peek() != newFolderId))
            {
                this.folderIds.Push(newFolderId);
            }
            this.SkyDriveObjectList.SelectionChanged -= new SelectionChangedEventHandler(this.SkyDriveObjectList_SelectionChanged);
            this.LiveConnectClient.GetCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.findNoteList_Callback);
            this.LiveConnectClient.GetAsync(newFolderId + "/files");
        }

        private void findNoteList_Callback(object sender, LiveOperationCompletedEventArgs e)
        {
            this.LiveConnectClient.GetCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.findNoteList_Callback);
            if (e.Error != null)
            {
                this.ReportError("ErrorWhenSyncingObjectsFromSkyDrive", e.Error.Message, e.Error);
            }
            else
            {
                this.ObjectsFromSkyDrive.Clear();
                System.Collections.Generic.List<Object> list = (System.Collections.Generic.List<object>)e.Result["data"];
                if (this.folderIds.Peek() != "/me/skydrive")
                {
                    this.hasRootDirFolder = true;
                    this.createNoteListEntry("..", null, "folder", null);
                }
                else
                {
                    this.hasRootDirFolder = false;
                }
                foreach (System.Collections.Generic.IDictionary<string, object> dictionary in list)
                {
                    string name = dictionary["name"].ToString();
                    string fileExtension = dictionary["type"].ToString();
                    string str3 = fileExtension;
                    if (str3 != null)
                    {
                        if (!(str3 == "folder"))
                        {
                            if (str3 == "file")
                            {
                                goto Label_0121;
                            }
                            if (((str3 == "album") || (str3 == "Video")) || (str3 == "Picture"))
                            {
                            }
                        }
                        else
                        {
                            this.createNoteListEntry(name, dictionary["id"].ToString(), fileExtension, dictionary);
                        }
                    }
                    continue;
                Label_0121:
                    this.HandlerFile(name, fileExtension, dictionary);
                }
                int count = this.ObjectsFromSkyDrive.Count;
                count = this.hasRootDirFolder ? (count - 1) : count;
                this.DelayWorkFor(0xfa0, this.ReportTotalCountOfObjectsFromSkyDriveText.FormatWith(new object[] { count }));
            }
            this.SkyDriveObjectList.SelectionChanged += new SelectionChangedEventHandler(this.SkyDriveObjectList_SelectionChanged);
        }

        public string GetShareWithByKey(string key)
        {
            string str = key;
            if (!this.shareWithKeyValue.TryGetValue(key.ToLowerInvariant(), out str))
            {
                str = key;
            }
            return str;
        }

        public T getValueFromDict<T>(System.Collections.Generic.IDictionary<String, Object> data, string key, System.Func<Object, T> callBack, T defVal)
        {
            object obj2 = null;
            if (data.TryGetValue(key, out obj2))
            {
                return callBack(obj2);
            }
            return defVal;
        }

        private void HandlerFile(string name, string type, System.Collections.Generic.IDictionary<string, object> datum)
        {
            string extension = System.IO.Path.GetExtension(name);
            string fileId = datum["id"].ToString();
            bool flag = false;
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = "default";
            }
            else
            {
                flag = this.FileFliters.Contains<string>("*") || this.FileFliters.Contains<string>(extension);
            }
            if (flag)
            {
                name = name.Replace(extension, null);
                this.createNoteListEntry(name, fileId, extension, datum);
            }
        }


        private void InitializeShareWithKeyValue()
        {
            this.shareWithKeyValue = new System.Collections.Generic.Dictionary<string, string>();
            foreach (string str in LocalizedStrings.GetLanguageInfoByKey("ShareWithKeyValue").Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string[] strArray2 = str.Split(new char[] { ',' });
                this.shareWithKeyValue.Add(strArray2[0].ToLowerInvariant(), (strArray2[1].Length == 0) ? strArray2[0] : strArray2[1]);
            }
            JustMeTag = this.shareWithKeyValue["just me"];
        }

        protected void OnDialogClosed(System.EventArgs e)
        {
            if (this.Closed != null)
            {
                this.Closed(this, e);
            }
        }

        protected virtual void OnObjectBrowseringChanged(ObjectBrowseringChangedHandlerEventArgs e)
        {
            System.EventHandler<ObjectBrowseringChangedHandlerEventArgs> objectBrowseringChanged = this.ObjectBrowseringChanged;
            if (objectBrowseringChanged != null)
            {
                objectBrowseringChanged(this, e);
            }
        }

        protected virtual void OnObjectBrowseringChanging(ObjectBrowseringChangedHandlerEventArgs e)
        {
            System.EventHandler<ObjectBrowseringChangedHandlerEventArgs> objectBrowseringChanging = this.ObjectBrowseringChanging;
            if (objectBrowseringChanging != null)
            {
                objectBrowseringChanging(this, e);
            }
        }

        protected virtual void OnReportingSyncingStatus(ReportStatusHandlerEventArgs e)
        {
            System.EventHandler<ReportStatusHandlerEventArgs> reportingSyncingStatus = this.ReportingSyncingStatus;
            if (reportingSyncingStatus != null)
            {
                reportingSyncingStatus(this, e);
            }
        }

        private void page_BackKeyPress(object sender, CancelEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close(sender, e);
            e.Cancel = true;
        }

        private void ReportError(string key, string message, System.Exception exp)
        {
            this.OnReportingSyncingStatus(new ReportStatusHandlerEventArgs(key, message, exp));
        }

        private void ReportStatus(string key, string message)
        {
            this.OnReportingSyncingStatus(new ReportStatusHandlerEventArgs(key, message));
        }

        private void SelectFolderAsResult_Click(object sender, RoutedEventArgs e)
        {
            ObjectFromSkyDrive tag = ((MenuItem)sender).Tag as ObjectFromSkyDrive;
            if (tag != null)
            {
                this.IsSelectAFolderAsResult = true;
                this.DoSelectedAndCloseDialog(tag);
            }
        }

        public void Show()
        {
            SystemTray.Opacity = 0.0;
            SystemTray.ForegroundColor = Colors.Black;
            if (this.ChildWindowPopup == null)
            {
                Popup popup = new Popup
                {
                    Language = XmlLanguage.GetLanguage(LocalizedStrings.CultureName.Name)
                };
                this.ChildWindowPopup = popup;
                try
                {
                    this.ChildWindowPopup.Child = this;
                }
                catch (System.ArgumentException)
                {
                    throw new System.InvalidOperationException("The control is already shown.");
                }
            }
            if ((this.ChildWindowPopup != null) && (Application.Current.RootVisual != null))
            {
                this.ChildWindowPopup.IsOpen = true;
            }
            if (RootVisual != null)
            {
                PhoneApplicationPage content = (PhoneApplicationPage)RootVisual.Content;
                if ((content.ApplicationBar != null) && content.ApplicationBar.IsVisible)
                {
                    _reshowAppBar = true;
                    content.ApplicationBar.IsVisible = false;
                }
                content.BackKeyPress += new System.EventHandler<CancelEventArgs>(this.page_BackKeyPress);
            }
            if (this.ObjectsFromSkyDrive.Count == 0)
            {
                this.Find("/me/skydrive");
            }
        }

        private void SkyDriveObjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ObjectFromSkyDrive selectedItem = this.SkyDriveObjectList.SelectedItem as ObjectFromSkyDrive;
            if (selectedItem != null)
            {
                this.IsSelectAFolderAsResult = false;
                this.SelectedItem = selectedItem;
                ObjectBrowseringChangedHandlerEventArgs args = new ObjectBrowseringChangedHandlerEventArgs(selectedItem.FileType, this.SelectedItem.ShareWith, selectedItem.Name);
                this.OnObjectBrowseringChanged(args);
                if (args.SelectAsResult)
                {
                    this.DoSelectedAndCloseDialog(selectedItem);
                    this.SkyDriveObjectList.SelectedItem = null;
                }
                else
                {
                    if (selectedItem.FileType == "folder")
                    {
                        this.skyDriveFileInfoPanel.Update(selectedItem);
                        if (selectedItem.Name == "..")
                        {
                            this.folderIds.Pop();
                            this.findNoteList(this.folderIds.Peek());
                        }
                        else
                        {
                            this.findNoteList(selectedItem.Id);
                            this.ParentItem = selectedItem;
                        }
                    }
                    this.SkyDriveObjectList.SelectedItem = null;
                }
            }
        }

        private void ViewProperty_Click(object sender, RoutedEventArgs e)
        {
            ObjectFromSkyDrive tag = (sender as MenuItem).Tag as ObjectFromSkyDrive;
            if (tag != null)
            {
                this.skyDriveFileInfoPanel.Update(tag);
            }
        }

        public string BrowsingTipLine
        {
            get
            {
                return this.BrowsingTips.Text;
            }
            set
            {
                this.BrowsingTips.Text = value;
            }
        }

        internal Popup ChildWindowPopup { get; private set; }

        public MessageBoxResult DialogResult { get; set; }

        public string[] FileFliters { get; set; }

        public bool IsSelectAFolderAsResult { get; set; }

        public Microsoft.Live.LiveConnectClient LiveConnectClient { get; set; }

        public ObservableCollection<ObjectFromSkyDrive> ObjectsFromSkyDrive { get; set; }

        public ObjectFromSkyDrive ParentItem { get; set; }

        private static PhoneApplicationFrame RootVisual
        {
            get
            {
                if (Application.Current != null)
                {
                    return (Application.Current.RootVisual as PhoneApplicationFrame);
                }
                return null;
            }
        }

        public ObjectFromSkyDrive SelectedItem { get; set; }

        public virtual string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }
    }
}

