namespace TinyMoneyManager.ScheduledAgentLib
{
    using Microsoft.Phone.Shell;
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class ScheduledAgentConfiguration
    {
        public const string fullPath = "Shared/ShellContent/tiles/LiveTile.png";
        public const string isoFullPath = "isostore:/Shared/ShellContent/tiles/LiveTile.png";
        public static Uri NewRecordStartScreenLink = new Uri("/Pages/NewOrEditAccountItemPage.xaml?action=Add&fromStartScreen=true", UriKind.RelativeOrAbsolute);
        public const string NoneItemMessageKey = "NoneItemMessage";
        public const string pinToStartLink = "/Pages/NewOrEditAccountItemPage.xaml?action=Add&fromStartScreen=true";
        public const string ScheduledAgentConfigurationDataKey = "ScheduledAgentConfigurationData";
        public const string ShowTileInfoKey = "ShowTileInfoKey";
        public const string tiledirectory = "Shared/ShellContent/tiles";
        public const string TileInfoGridBackupKey = "TileInfoGridBackup";

        /// <summary>
        /// Restores the tile info from backup grid.
        /// </summary>
        /// <param name="linkName">Name of the link.</param>
        /// <param name="countOfToday">The count of today.</param>
        /// <param name="totalAmountOfToday">The total amount of today.</param>
        /// <param name="showOrNot">if set to <c>true</c> [show or not].</param>
        public static void RestoreTileInfoFromBackupGrid(string linkName = "New Record", int countOfToday = 0, string totalAmountOfToday = "0.00", bool showOrNot = true)
        {
            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault<ShellTile>(p => p.NavigationUri == NewRecordStartScreenLink);
            if (tile != null)
            {
                string str = string.Format("{0}\r\n{1}\r\n{2}", System.DateTime.Now.ToString(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.MonthDayPattern), countOfToday, totalAmountOfToday);
                StandardTileData data = new StandardTileData
                {
                    Title = linkName,
                    BackContent = showOrNot ? str : null
                };

                tile.Update(data);
            }
        }
    }
}

