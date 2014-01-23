namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class PictureViewModel : NkjSoftViewModelBase
    {
        public override void Delete<T>(T obj)
        {
            PictureInfo info = obj as PictureInfo;
            this.DeletePicture(info.FileName, info.Tag);
        }

        public void DeletePicture(PictureInfo picinfo)
        {
            base.Delete<PictureInfo>(picinfo);
            this.SubmitChanges();
        }

        public void DeletePicture(string fileName, string tag)
        {
            string path = System.IO.Path.Combine(this.GetFolderInPictures(tag), fileName);
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists(path))
                {
                    file.DeleteFile(path);
                }
            }
        }

        public void DeletePictures(System.Collections.Generic.IEnumerable<PictureInfo> entitySet)
        {
            this.AccountBookDataContext.PictureInfos.DeleteAllOnSubmit<PictureInfo>(entitySet);
            foreach (PictureInfo info in entitySet)
            {
                this.Delete<PictureInfo>(info);
            }
        }

        public string GetFolderInPictures(string tagName)
        {
            return System.IO.Path.Combine(AccountBookDataFolderStructure.CombineFromRoot("Pictures"), tagName);
        }

        public string GetFullPath(PictureInfo picInfo)
        {
            return System.IO.Path.Combine(this.GetFolderInPictures(picInfo.Tag), picInfo.FileName);
        }

        public void LoadContent(System.Collections.Generic.IEnumerable<PictureInfo> list)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (PictureInfo info in list)
                {
                    string path = System.IO.Path.Combine(this.GetFolderInPictures(info.Tag), info.FileName);
                    System.IO.Stream streamSource = null;
                    BitmapImage content = null;
                    if (info.Content != null)
                    {
                        content = info.Content;
                        continue;
                    }
                    content = new BitmapImage();
                    if (info.ContentSource != null)
                    {
                        streamSource = info.ContentSource;
                    }
                    else
                    {
                        if (file.FileExists(path))
                        {
                            streamSource = file.OpenFile(path, System.IO.FileMode.Open);
                        }
                        info.ContentSource = streamSource;
                        if (streamSource != null)
                        {
                            content.SetSource(streamSource);
                            streamSource.Close();
                        }
                    }
                    info.Content = content;
                }
            }
        }

        public void LoadContent(string folder, System.Collections.Generic.IEnumerable<PictureInfo> list, System.Action<PictureInfo> callBack = null)
        {
            if (list == null) return;

            if (callBack == null)
            {
                callBack = delegate(PictureInfo i)
                {
                };
            }
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists(folder))
                {
                    file.CreateDirectory(folder);
                }
                foreach (PictureInfo info in list)
                {
                    string path = System.IO.Path.Combine(folder, info.FileName);
                    System.IO.Stream streamSource = null;
                    BitmapImage content = null;
                    if (info.Content != null)
                    {
                        content = info.Content;
                    }
                    else
                    {
                        content = new BitmapImage();
                        if (info.ContentSource != null)
                        {
                            streamSource = info.ContentSource;
                        }
                        else
                        {
                            if (!file.FileExists(path))
                            {
                                path = info.FullPath;
                            }

                            if (file.FileExists(path))
                            {
                                streamSource = file.OpenFile(path, System.IO.FileMode.Open);
                            }

                            info.ContentSource = streamSource;
                            if (streamSource != null)
                            {
                                content.SetSource(streamSource);
                                streamSource.Close();
                            }
                        }
                        info.Content = content;
                    }
                    callBack(info);
                }
            }
        }

        public void LoadData(System.Guid attachedId, string tagName, ObservableCollection<PictureInfo> Pictures)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                var db = AccountBookDataContext;

                System.Collections.Generic.List<PictureInfo> list = (from p in this.AccountBookDataContext.PictureInfos
                                                                     where p.AttachedId == attachedId
                                                                     select p).ToList<PictureInfo>();

                string folder = this.GetFolderInPictures(tagName);

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    LoadContent(folder, list, delegate(PictureInfo item)
                    {
                        Pictures.Add(item);
                    });
                });
            });
        }

        public void SavePicture(params PictureInfo[] picInfos)
        {
            this.SavePicture(picInfos.AsEnumerable<PictureInfo>());
        }

        public void SavePicture(System.Collections.Generic.IEnumerable<PictureInfo> picInfos)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (PictureInfo info in picInfos)
                {
                    string fileName = info.FileName;
                    string folderInPictures = this.GetFolderInPictures(info.Tag);
                    if (!file.DirectoryExists(folderInPictures))
                    {
                        file.CreateDirectory(folderInPictures);
                    }
                    string path = System.IO.Path.Combine(folderInPictures, info.FileName);
                    System.IO.Stream contentSource = info.ContentSource;
                    info.FullPath = path;
                    if (contentSource != null)
                    {
                        contentSource.Position = 0;

                        if (file.FileExists(path))
                        {
                            file.DeleteFile(path);
                        }

                        using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream2 = file.CreateFile(path))
                        {
                            contentSource.CopyTo(stream2);
                        }
                    }
                    info.FullPath = path;
                }
            }
        }

        public void SavePictures(System.Collections.Generic.IEnumerable<PictureInfo> collection)
        {
            try
            {
                this.SavePicture(collection);
                this.SubmitChanges();
            }
            catch (System.Exception exception)
            {
                throw exception;
            }
        }

        public void ShowPicture()
        {
        }

        /// <summary>
        /// Inserts the pictures.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void InsertPicturesOnSubmit(IEnumerable<PictureInfo> pictures)
        {
            this.AccountBookDataContext.PictureInfos.InsertAllOnSubmit(pictures);
        }

        public void SavePicturesFrom(IEnumerable<PictureInfo> picInfos, string replaceOldFolderName)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (PictureInfo info in picInfos)
                {
                    string fileName = info.FileName;
                    string folderInPictures = this.GetFolderInPictures(info.Tag);
                    if (!file.DirectoryExists(folderInPictures))
                    {
                        file.CreateDirectory(folderInPictures);
                    }
                    string path = System.IO.Path.Combine(folderInPictures, info.FileName);

                    var newFileName = info.FullPath.Replace(replaceOldFolderName, info.Tag);

                    var oldFile = info.FullPath;

                    if (file.FileExists(path))
                    {
                        file.DeleteFile(path);
                    }

                    if (file.FileExists(oldFile))
                    {
                        file.CopyFile(oldFile, newFileName, true);

                        info.FullPath = newFileName;
                    }
                }
            }
        }
    }
}

