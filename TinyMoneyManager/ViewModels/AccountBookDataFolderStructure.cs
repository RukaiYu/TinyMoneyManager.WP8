namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.IO;

    public class AccountBookDataFolderStructure
    {
        public const string AccountItemsFolder = "AccountItmes";
        public const string PictureFolder = "Pictures";
        public const string RootFolder = "AccountBookDataFolder";
        public const string XmlDataFileName = "AccountBook.SyncData.xml.txt";
        public const string XmlDataFolder = "XmlSyncingData";

        public static string CombineFromRoot(string secondPath)
        {
            return System.IO.Path.Combine("AccountBookDataFolder", secondPath);
        }

        public static string AccountItemsPicturesFolder
        {
            get
            {
                return string.Format(@"{0}\{1}\{2}", "AccountBookDataFolder", "Pictures", "AccountItmes");
            }
        }
    }
}

