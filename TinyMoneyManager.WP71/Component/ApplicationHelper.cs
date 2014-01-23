using System;
using System.Net;
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Coding4Fun.Phone.Controls;
using TinyMoneyManager.Controls;
using TinyMoneyManager.Data.Model;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using TinyMoneyManager.Data;
using TinyMoneyManager.Language;

using NkjSoft.Extensions;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using System.Windows.Controls;
namespace TinyMoneyManager.Component
{
    public static class ApplicationHelper
    {
        private const string _HasLoadDefaultCategorys = "_HasLoadDefaultCategorys";
        private static readonly string CurrencyConversionDataKey = "CurrencyConversionData";
        private static System.DateTime defaultSyncAt = new System.DateTime(2012, 12, 1, 23, 0, 0);
        private static readonly string LastSyncAtKey = "LastSyncAtKey";

        public static System.Collections.Generic.IEnumerable<T> FindChildOfType<T>(this DependencyObject root) where T : class
        {
            Queue<DependencyObject> iteratorVariable0 = new Queue<DependencyObject>();
            iteratorVariable0.Enqueue(root);
            while (iteratorVariable0.Count > 0)
            {
                DependencyObject reference = iteratorVariable0.Dequeue();
                for (int i = VisualTreeHelper.GetChildrenCount(reference) - 1; 0 <= i; i--)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(reference, i);
                    T iteratorVariable4 = child as T;
                    if (iteratorVariable4 != null)
                    {
                        yield return iteratorVariable4;
                    }
                    iteratorVariable0.Enqueue(child);
                }
            }
            yield return default(T);
        }

        public static string GetApplicationFileContentFrom(string filePath)
        {
            string str = string.Empty;
            using (System.IO.Stream stream = Application.GetResourceStream(GetResourceUriFromApplication(filePath)).Stream)
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    str = reader.ReadToEnd();
                }
            }
            return str;
        }

        public static string GetDefaultCurrencyTableContent()
        {
            return GetApplicationFileContentFrom("Resources/currencyRateTableContent.txt");
        }

        public static BitmapImage GetPictureFromApplication(string fileName)
        {
            BitmapImage image = null;
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.FileExists(fileName))
                {
                    return image;
                }
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(fileName, System.IO.FileMode.Open))
                {
                    image = new BitmapImage
                    {
                        CreateOptions = BitmapCreateOptions.None
                    };
                    image.SetSource(stream);
                }
            }
            return image;
        }

        public static Uri GetResourceUriFromApplication(string filename)
        {
            return new Uri(string.Format("/TinyMoneyManager;component/{0}", filename), UriKind.RelativeOrAbsolute);
        }

        public static void LoadCurrencyConversionData()
        {
            string currencyConversionRateTable = CurrencyConversionRateTable;
            if ((currencyConversionRateTable.Length == 0) || (currencyConversionRateTable.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries).Length < 0x11))
            {
                ConversionRateHelper.ConversionRateTable = ConversionRateHelper.LoadDefaultConversionTable(GetDefaultCurrencyTableContent());
                CurrencyConversionRateTable = ConversionRateHelper.Save();
            }
            else
            {
                ConversionRateHelper.ConversionRateTable = ConversionRateHelper.LoadTable(currencyConversionRateTable, () => GetDefaultCurrencyTableContent());
            }
        }

        public static void Pin(Uri uri, ShellTileData initialData)
        {
            ShellTile.Create(uri, initialData);
        }

        public static ApplicationSafetyService RegisterNeedInputPasswordService(this PhoneApplicationPage serviceHost, System.Action beforePopupPasswordInputDialog, System.Action<String> afterPasswordWrong, System.Action afterSuccessPassed)
        {
            return new ApplicationSafetyService(serviceHost) { BeforePopupPasswordInputDialog = beforePopupPasswordInputDialog, AfterPasswordWrong = afterPasswordWrong, AfterPasswordPassed = afterSuccessPassed };
        }

        public static void SaveConversionRateTable()
        {
            IsolatedStorageSettings.ApplicationSettings[CurrencyConversionDataKey] = ConversionRateHelper.Save();
        }

        public static void SavePictureToIsolateStorage(System.IO.Stream stream, string fileName)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream2 = file.OpenFile(fileName, System.IO.FileMode.OpenOrCreate))
                {
                    BitmapImage source = new BitmapImage
                    {
                        CreateOptions = BitmapCreateOptions.None
                    };
                    source.SetSource(stream);
                    WriteableBitmap bitmap = new WriteableBitmap(source);
                    bitmap.SaveJpeg(stream2, bitmap.PixelWidth, bitmap.PixelHeight, 0, 0x55);
                    stream2.Close();
                }
            }
        }

        public static string VerifyTableFromSource(string source)
        {
            System.Func<Decimal> callBackWhenFailed = null;
            if (string.IsNullOrEmpty(source) || (source.Trim().Length == 0))
            {
                return ConversionRateHelper.Save();
            }
            string newLine = System.Environment.NewLine;
            if (!source.Contains(newLine))
            {
                newLine = "\n";
            }
            string[] strArray = source.Split(new string[] { newLine }, System.StringSplitOptions.RemoveEmptyEntries);
            char[] separator = new char[] { ',' };
            int num = 0;
            bool hasError = false;
            foreach (string str2 in strArray)
            {
                string[] strArray2 = str2.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                int length = strArray2.Length;
                for (int i = 0; i < length; i++)
                {
                    if (callBackWhenFailed == null)
                    {
                        callBackWhenFailed = delegate
                        {
                            hasError = true;
                            return -1M;
                        };
                    }
                    strArray2[i].ToDecimal(callBackWhenFailed);
                    if (hasError)
                    {
                        break;
                    }
                }
                if (hasError)
                {
                    break;
                }
                num++;
            }
            if (hasError)
            {
                MessageBox.Show(AppResources.CurrencyTableUpdatingNotes, App.AlertBoxTitle, MessageBoxButton.OK);
                return ConversionRateHelper.Save();
            }
            return source;
        }

        public static string CurrencyConversionRateTable
        {
            get
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains(CurrencyConversionDataKey))
                {
                    return string.Empty;
                }
                return IsolatedStorageSettings.ApplicationSettings[CurrencyConversionDataKey].ToString();
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[CurrencyConversionDataKey] = value;
            }
        }

        public static bool HasLoadDefaultCategorys
        {
            get
            {
                bool flag = false;
                IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>("_HasLoadDefaultCategorys", out flag);
                return flag;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["_HasLoadDefaultCategorys"] = value;
            }
        }

        public static System.DateTime LastSyncAt
        {
            get
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains(LastSyncAtKey))
                {
                    return defaultSyncAt;
                }
                return (System.DateTime)IsolatedStorageSettings.ApplicationSettings[LastSyncAtKey];
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[LastSyncAtKey] = value;
            }
        }

        public static string LastSyncAtString
        {
            get
            {
                return LastSyncAt.ToString(LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.ShortDatePattern);
            }
        }

    }

    public partial class ApplicationSafetyService
    {
        private PhoneApplicationPage pageHock;

        public Action BeforePopupPasswordInputDialog;

        public Action<string> AfterPasswordWrong;

        public Action AfterPasswordPassed;

        private string passwordWrongMessage = string.Empty;
        private string passwordErrorMessage = string.Empty;
        private string inputDialogTitle = string.Empty;
        public bool HasLogon { get; set; }

        public ApplicationSafetyService(PhoneApplicationPage page)
        {
            pageHock = page;
            passwordWrongMessage = AppResources.PasswordWrongText;

            inputDialogTitle = AppResources.PasswordToLoginText;
            passwordErrorMessage = inputDialogTitle;
            this.HeightOfContent = page
                .ActualHeight;
            this.HasLogon = false;
        }

        CustomMessageBox cmb = new CustomMessageBox();

        public double HeightOfContent { get; set; }

        private string _userInput;
        public void ShowPasswordInputPopup(double height = 0)
        {
            height = HeightOfContent;

            if (HasLogon || !AppSetting.Instance.EnablePoketLock)
                return;

            if (BeforePopupPasswordInputDialog != null)
                BeforePopupPasswordInputDialog();

            var tb = new TextBox();
            tb.InputScope = MoneyInputTextBox.NumberInputScope;
            cmb.IsLeftButtonEnabled = true;

            var lbl = new TextBlock();
            lbl.Text = passwordErrorMessage;
            lbl.Style = (Style)Application.Current.Resources["PhoneTextAccentStyle"];

            cmb.LeftButtonContent = AppResources.Cancel.ToUpper();

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(0, 300, 12, 10);
            sp.VerticalAlignment = VerticalAlignment.Bottom;
            if (height != 0)
            {
                cmb.Height = height;
            }

            sp.Children.Add(lbl);
            sp.Children.Add(tb);
            cmb.Opacity = 0.99;


            tb.TextChanged += tb_TextChanged;
            cmb.Content = sp;

            cmb.Dismissed += cmb_Dismissed;
            cmb.Show();
        }

        void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            this._userInput = (sender as TextBox).Text;

            if (_userInput == AppSetting.Instance.Password)
            {
                cmb.Dismiss();
            }
        }

        void cmb_Dismissed(object sender, DismissedEventArgs e)
        {
            cmb.Dismissed -= cmb_Dismissed;
            if (_userInput == AppSetting.Instance.Password)
            {
                AfterPasswordPassed();
                HasLogon = true;
            }
            else
            {
                if (_userInput != AppSetting.Instance.Password)
                {
                    passwordErrorMessage = AppResources.CurrentPasswordError;

                    AfterPasswordWrong(passwordWrongMessage);
                }
            }
        }

        void pip_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult == PopUpResult.Ok && e.Result == AppSetting.Instance.Password)
            {
                AfterPasswordPassed();
                HasLogon = true;
            }
            else if (e.PopUpResult == PopUpResult.Cancelled)
            {
                if (pageHock.NavigationService.CanGoBack)
                    pageHock.NavigationService.GoBack();
                else
                    AfterPasswordWrong(passwordWrongMessage);
            }
            else
            {
                passwordErrorMessage = AppResources.CurrentPasswordError;
                ShowPasswordInputPopup();
            }
        }

    }

    public partial class ApplicationSafetyService
    {
        public const string ApplicationGlobalLogFileName = "";

        //static members
        public static void TrackToRun(Action actionToRun)
        {
            if (actionToRun == null)
                return;

            try
            {
                actionToRun();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

