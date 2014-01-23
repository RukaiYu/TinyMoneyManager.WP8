namespace TinyMoneyManager.Component
{
    using Microsoft.Xna.Framework.Media;
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Media.Imaging;

    public static class PictureExportingHelper
    {
        public static void SaveToPictureLiabray(this UIElement contentToSaveToImage, string folder, string fileName, int width, int height)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists(folder))
                {
                    file.CreateDirectory(folder);
                }
                string path = folder + "//" + fileName;
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(path, System.IO.FileMode.OpenOrCreate))
                {
                    new WriteableBitmap(contentToSaveToImage, null).SaveJpeg(stream, width, height, 0, 0x55);
                    stream.Close();
                    MediaLibrary library = new MediaLibrary();
                    using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream2 = file.OpenFile(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        library.SavePicture(fileName, stream2);
                        library.Dispose();
                    }
                }
            }
        }
    }
}

