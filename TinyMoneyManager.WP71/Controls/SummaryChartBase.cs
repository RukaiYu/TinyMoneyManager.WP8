using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TinyMoneyManager.Language;
using TinyMoneyManager.Component;

using NkjSoft.Extensions;
namespace TinyMoneyManager.Controls
{
    public class SummaryChartBase
    {

        public const string StasticChartImageCacheFolder = "/StasticChartImageCacheFolder";
        /// <summary>
        /// Saves the image to picture album.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="title">The title.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public static void SaveImageToPictureAlbum(UIElement content, string title, int width, int height)
        {
            if (MessageBox.Show(AppResources.ConfirmToSavePictureToPhone, AppResources.AppName, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            try
            {
                content.SaveToPictureLiabray(StasticChartImageCacheFolder, "{0}.jpg".FormatWith(DateTime.Now.ToString("yyyyMMddHHmmss")), width, height);

                CommonExtensions.AlertNotification(null, AppResources.SaveImageToPictureHubSuccessMessage);
            }
            catch (Exception ex)
            {
                ex.AlertErrorMessage(AppResources.SaveStasticChartToLiabraryError);
            }
        }

    }
}
