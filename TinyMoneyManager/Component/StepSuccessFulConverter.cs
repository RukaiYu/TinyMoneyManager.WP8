namespace TinyMoneyManager.Component
{
    using NkjSoft.Extensions;
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using TinyMoneyManager.ViewModels;

    public class StepSuccessFulConverter : IValueConverter
    {
        private static string defImage = "/TinyMoneyManager;component/images/{0}.png";

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string defImage = StepSuccessFulConverter.defImage;
            SynchronizationStepViewModel model = value as SynchronizationStepViewModel;
            if (model != null)
            {
                defImage = StepSuccessFulConverter.defImage.FormatWith(new object[] { model.Step.StepStatus.ToString() });
            }
            return defImage;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

