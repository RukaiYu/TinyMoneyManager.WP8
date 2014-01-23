namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.IO;

    /// <summary>
    /// 
    /// </summary>
    public class AccountBookDataFolderStructure
    {
        public const string AccountItemsFolder = "AccountItmes";
        public const string PictureFolder = "Pictures";
        public const string RootFolder = "AccountBookDataFolder";
        public const string XmlDataFileName = "AccountBook.SyncData.xml.txt";
        public const string XmlDataFolder = "XmlSyncingData";

        /// <summary>
        /// Combines from root.
        /// </summary>
        /// <param name="secondPath">The second path.</param>
        /// <returns></returns>
        public static string CombineFromRoot(string secondPath)
        {
            return System.IO.Path.Combine("AccountBookDataFolder", secondPath);
        }

        /// <summary>
        /// Gets the account items pictures folder.
        /// </summary>
        /// <value>
        /// The account items pictures folder.
        /// </value>
        public static string AccountItemsPicturesFolder
        {
            get
            {
                return string.Format(@"{0}\{1}\{2}", "AccountBookDataFolder", "Pictures", "AccountItmes");
            }
        }
    }
}

