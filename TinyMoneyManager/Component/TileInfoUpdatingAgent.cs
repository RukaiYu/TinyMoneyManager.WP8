namespace TinyMoneyManager.Component
{
    using ImageTools;
    using Microsoft.Phone.Scheduler;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager.ScheduledAgentLib;

    public class TileInfoUpdatingAgent
    {
        public const string fullPath = "Shared/ShellContent/tiles/LiveTile.png";
        public const string isoFullPath = "isostore:/Shared/ShellContent/tiles/LiveTile.png";
        public const string ShellTilePicturePathRoot = "Shared/ShellContent/tiles";
        public const string tiledirectory = "Shared/ShellContent/tiles";
        private static readonly string TileInfoUpdatingAgentIsScheduledKey = "TileInfoUpdatingAgentIsScheduledKey";

        private static void addTask(string name)
        {
            try
            {
                PeriodicTask action = new PeriodicTask(name) {
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
                    flag = (bool) IsolatedStorageSettings.ApplicationSettings[TileInfoUpdatingAgentIsScheduledKey];
                }
                return flag;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[TileInfoUpdatingAgentIsScheduledKey] = value;
            }
        }
    }
}

