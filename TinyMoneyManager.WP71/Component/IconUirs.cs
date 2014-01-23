namespace TinyMoneyManager
{
    using Microsoft.Phone.Shell;
    using System;
    using TinyMoneyManager.Language;

    public class IconUirs
    {
        public static readonly Uri AddPlusIconButton = CreateUri("/icons/appbar.add.rest.png");
        public static readonly Uri ArrowDown = CreateUri("/images/arrow_down.png");
        public static readonly Uri ArrowRight = CreateUri("/images/arrow_Right.png");
        public static readonly Uri ArrowUp = CreateUri("/images/arrow_up.png");
        public static Uri CalcutorIconButton = CreateUri("/icons/appbar.calcutor.rest.png");
        public static readonly Uri CancelIconButton = CreateUri("/icons/appbar.cancel.rest.png");
        public static Uri CheckIcon = CreateUri("/icons/appbar.check.rest.png");
        public static readonly Uri ChooseMonthIconButton = CreateUri("/icons/Months/appbar.chooseMonth9.png");
        public static readonly Uri CloudUploadIcon = CreateUri("/icons/appbar/appbar.cloudupload.rest.png");
        public static readonly Uri ColoudIconButton = CreateUri("/icons/appbar/appbar.feature.coloud.rest.png");
        public static Uri DeleteIcon = CreateUri("/icons/appbar.delete.rest.png");
        public static Uri EditIcon = CreateUri("/icons/appbar.edit.rest.png");
        public static Uri LikeToIcon = CreateUri("/icons/appbar.linkto.rest.png");
        public static Uri NextIcon = CreateUri("/icons/appBar/appbar.next.rest.png");
        public static Uri PreviousIcon = CreateUri("/icons/appBar/appbar.back.rest.png");
        public static Uri SaveIcon = CreateUri("/icons/appbar.save.rest.png");
        public static readonly Uri SearchingButtonIcon = CreateUri("/icons/appBar/appbar.feature.search.rest.png");
        public static readonly Uri SearchRefineIconButton = CreateUri("/icons/appbar/appbar.search.refine.rest.png");
        public static readonly Uri SelectIconButton = CreateUri("/Toolkit.Content/ApplicationBar.Select.png");
        public static readonly Uri SendIconButton = CreateUri("/icons/appbar.feature.email.rest.png");
        public static readonly Uri StaticsIconButton = CreateUri("/icons/appbar.summary.rest.png");

        internal static ApplicationBarIconButton CreateDeleteButton()
        {
            return new ApplicationBarIconButton(DeleteIcon) { Text = AppResources.Delete };
        }

        internal static ApplicationBarIconButton CreateEditButton()
        {
            return new ApplicationBarIconButton(EditIcon) { Text = AppResources.Edit };
        }

        internal static ApplicationBarIconButton CreateIconButton(string text, Uri iconUri)
        {
            return new ApplicationBarIconButton(iconUri) { Text = text };
        }

        internal static ApplicationBarMenuItem CreateMenuButton(string text)
        {
            return new ApplicationBarMenuItem { Text = text };
        }

        public static Uri CreateUri(string file)
        {
            return new Uri(file, UriKind.Relative);
        }
    }
}

