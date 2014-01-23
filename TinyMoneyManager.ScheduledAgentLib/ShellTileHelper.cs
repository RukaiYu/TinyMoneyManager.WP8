namespace TinyMoneyManager.ScheduledAgentLib
{
    using Microsoft.Phone.Shell;
    using System;
    using System.Linq;

    public static class ShellTileHelper
    {
        public static ShellTile GetAppTile(Uri uri)
        {
            return ShellTile.ActiveTiles.FirstOrDefault<ShellTile>(x => (x.NavigationUri == uri));
        }

        public static bool IsPinned(string uniqueId)
        {
            return (ShellTile.ActiveTiles.FirstOrDefault<ShellTile>(x => x.NavigationUri.ToString().Contains(uniqueId)) != null);
        }

        public static bool IsPinned(Uri uri)
        {
            return (ShellTile.ActiveTiles.FirstOrDefault<ShellTile>(x => (x.NavigationUri == uri)) != null);
        }

        public static void UnPin(string id)
        {
            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault<ShellTile>(x => x.NavigationUri.ToString().Contains(id));
            if (tile != null)
            {
                tile.Delete();
            }
        }

        public static void UnPin(Uri uri)
        {
            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault<ShellTile>(x => x.NavigationUri == uri);
            if (tile != null)
            {
                tile.Delete();
            }
        }
    }
}

