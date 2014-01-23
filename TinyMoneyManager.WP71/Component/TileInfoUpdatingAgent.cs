namespace TinyMoneyManager.Component
{
    using ImageTools;
    using Microsoft.Phone.Scheduler;
    using Microsoft.Phone.Shell;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TinyMoneyManager.ScheduledAgentLib;

    public class TileInfoUpdatingAgent
    {
        public const string fullPath = "Shared/ShellContent/tiles/LiveTile.png";
        public const string isoFullPath = "isostore:/Shared/ShellContent/tiles/LiveTile.png";
        public const string ShellTilePicturePathRoot = "Shared/ShellContent/tiles";
        public const string tiledirectory = "Shared/ShellContent/tiles";
        private static readonly string TileInfoUpdatingAgentIsScheduledKey = "TileInfoUpdatingAgentIsScheduledKey";


        private static Version TargetedVersion = new Version(7, 10, 8858);
        /// <summary>
        /// Gets a value indicating whether this instance is targeted version.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is targeted version; otherwise, <c>false</c>.
        /// </value>
        public static bool IsTargetedVersion { get { return Environment.OSVersion.Version >= TargetedVersion; } }

        private static void addTask(string name)
        {
            try
            {
                PeriodicTask action = new PeriodicTask(name)
                {
                    Description = "Account Book(手机账本) BackgroundAgent"
                };
                ScheduledActionService.Add(action);
            }
            catch (System.Exception exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    MessageBox.Show("Background agents for this application have been disabled by the user");
                }
            }
        }

        public static string CreateShellTile(string firstLineTitle, string secondLineContent, string threeLineTitle, string fourLineContent, string pictureName)
        {
            Grid content = Application.Current.Resources["TileTemplete"] as Grid;
            (content.Children[0] as TextBlock).Text = firstLineTitle;
            TextBlock block = content.Children[1] as TextBlock;
            block.Text = secondLineContent;
            block.TextWrapping = TextWrapping.Wrap;
            TextBlock block2 = content.Children[2] as TextBlock;
            block2.Text = threeLineTitle;
            block2.TextWrapping = TextWrapping.Wrap;
            (content.Children[3] as TextBlock).Text = fourLineContent;
            content.Arrange(new Rect(0.0, 0.0, 173.0, 173.0));
            return SaveTilePicture(content, pictureName);
        }

        public static void SavePicture(Grid grid, string folder, string fileName)
        {
            ExtendedImage image = grid.ToImage();
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists(folder))
                {
                    file.CreateDirectory(folder);
                }
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(fileName, System.IO.FileMode.OpenOrCreate))
                {
                    image.WriteToStream(stream, fileName);
                }
            }
        }

        public static string SaveTilePicture(Grid content, string fileName)
        {
            ExtendedImage image = content.ToImage();
            fileName = "Shared/ShellContent/tiles/" + fileName;
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists("Shared/ShellContent/tiles"))
                {
                    file.CreateDirectory("Shared/ShellContent/tiles");
                }
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(fileName, System.IO.FileMode.OpenOrCreate))
                {
                    image.WriteToStream(stream, fileName);
                }
            }
            return ("isostore:/" + fileName);
        }

        public static void StartTileInfoUpdatingAgent()
        {
            try
            {
                TileInfoUpdatingAgentIsScheduled = true;
                string name = Name;
                PeriodicTask task = ScheduledActionService.Find(name) as PeriodicTask;
                if (task == null)
                {
                    addTask(name);
                }
                else if (task.IsEnabled)
                {
                    try
                    {
                        ScheduledActionService.Remove(name);
                        addTask(name);
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }
            catch (SchedulerServiceException exception)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        public static string Name
        {
            get
            {
                return ScheduledAgent.TileInfoUpdatingAgentName;
            }
        }

        public static bool TileInfoUpdatingAgentIsScheduled
        {
            get
            {
                bool flag = false;
                if (IsolatedStorageSettings.ApplicationSettings.Contains(TileInfoUpdatingAgentIsScheduledKey))
                {
                    flag = (bool)IsolatedStorageSettings.ApplicationSettings[TileInfoUpdatingAgentIsScheduledKey];
                }
                return flag;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[TileInfoUpdatingAgentIsScheduledKey] = value;
            }
        }

        internal static void RemoveTitlePicture(string fileName)
        {
            try
            {
                System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().DeleteFile(fileName);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Updates the flip tile.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="backTitle">The back title.</param>
        /// <param name="backContent">Content of the back.</param>
        /// <param name="wideBackContent">Content of the wide back.</param>
        /// <param name="count">The count.</param>
        /// <param name="tileId">The tile id.</param>
        /// <param name="smallBackgroundImage">The small background image.</param>
        /// <param name="backgroundImage">The background image.</param>
        /// <param name="backBackgroundImage">The back background image.</param>
        /// <param name="wideBackgroundImage">The wide background image.</param>
        /// <param name="wideBackBackgroundImage">The wide back background image.</param>
        public static void UpdateFlipTile(
            string title,
            string backTitle,
            string backContent,
            string wideBackContent,
            int count,
            Uri tileId,
            Uri smallBackgroundImage,
            Uri backgroundImage,
            Uri backBackgroundImage,
            Uri wideBackgroundImage,
            Uri wideBackBackgroundImage)
        {
            if (IsTargetedVersion)
            {
                // Get the new FlipTileData type.
                Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");

                // Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

                // Loop through any existing Tiles that are pinned to Start.
                foreach (var tileToUpdate in ShellTile.ActiveTiles)
                {
                    // Look for a match based on the Tile's NavigationUri (tileId).
                    if (tileToUpdate.NavigationUri.ToString() == tileId.ToString())
                    {
                        // Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
                        var UpdateTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                        // Set the properties. 
                        SetProperty(UpdateTileData, "Title", title);
                        SetProperty(UpdateTileData, "Count", count);
                        SetProperty(UpdateTileData, "BackTitle", backTitle);
                        SetProperty(UpdateTileData, "BackContent", backContent);
                        SetProperty(UpdateTileData, "SmallBackgroundImage", smallBackgroundImage);
                        SetProperty(UpdateTileData, "BackgroundImage", backgroundImage);
                        SetProperty(UpdateTileData, "BackBackgroundImage", backBackgroundImage);
                        SetProperty(UpdateTileData, "WideBackgroundImage", wideBackgroundImage);
                        SetProperty(UpdateTileData, "WideBackBackgroundImage", wideBackBackgroundImage);
                        SetProperty(UpdateTileData, "WideBackContent", wideBackContent);

                        // Invoke the new version of ShellTile.Update.
                        shellTileType.GetMethod("Update").Invoke(tileToUpdate, new Object[] { UpdateTileData });
                        break;
                    }
                }
            }

        }

        public static void UpdateiconicFlipTile(
           string title,
           string wideBackContent1,
           string wideBackContent2,
           string wideBackContent3,
           int count,
           Uri tileId,
           Color backgroundColor,
           Uri iconImage,
           Uri smallIconImage)
        {
            if (IsTargetedVersion)
            {
                // Get the new FlipTileData type.
                Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.IconicTileData, Microsoft.Phone");

                // Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

                // Loop through any existing Tiles that are pinned to Start.
                foreach (var tileToUpdate in ShellTile.ActiveTiles)
                {
                    // Look for a match based on the Tile's NavigationUri (tileId).
                    if (tileToUpdate.NavigationUri.ToString() == tileId.ToString())
                    {
                        // Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
                        var UpdateTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                        // Set the properties. 
                        SetProperty(UpdateTileData, "Title", title);
                        SetProperty(UpdateTileData, "Count", count);
                        SetProperty(UpdateTileData, "BackgroundColor", backgroundColor);
                        SetProperty(UpdateTileData, "IconImage", iconImage);
                        SetProperty(UpdateTileData, "SmallIconImage", smallIconImage);
                        SetProperty(UpdateTileData, "WideContent1", wideBackContent1);
                        SetProperty(UpdateTileData, "WideContent2", wideBackContent2);
                        SetProperty(UpdateTileData, "WideContent3", wideBackContent3);

                        // Invoke the new version of ShellTile.Update.
                        shellTileType.GetMethod("Update").Invoke(tileToUpdate, new Object[] { UpdateTileData });
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        private static void SetProperty(object instance, string name, object value)
        {
            var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
            setMethod.Invoke(instance, new object[] { value });
        }
    }
}

