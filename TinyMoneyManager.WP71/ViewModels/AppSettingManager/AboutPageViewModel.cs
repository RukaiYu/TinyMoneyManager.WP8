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
using System.Collections.Generic;
using TinyMoneyManager.Component;
using System.Collections.ObjectModel;

using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Data.Model;
using NkjSoft.Extensions;
namespace TinyMoneyManager.ViewModels
{
    using System.Linq;
    public class AboutPageViewModel
    {

        /// <summary>
        /// youWant : 1 : helpes
        ///                  2 : tips for sync..
        /// </summary>
        /// <param name="youWant"></param>
        public AboutPageViewModel()
        {

        }

        /// <summary>
        /// youWant : 1 : helpes
        ///                  2 : tips for sync..
        /// </summary>
        /// <param name="youWant">You want.</param>
        /// <returns></returns>
        public static IEnumerable<TipsItem> GetTips(int youWant)
        {
            var fileName = "HelpsInAbout.txt";

            switch (youWant)
            {
                default:
                    break;
                case 2:
                    fileName = "HelpsInDataSync.txt";
                    break;
                case 3:
                    fileName = "WhatsNewAndNext.txt";
                    break;
                case 4:
                    fileName = "HolidayWords.txt";
                    break;
            }

            return GetHelpTextFromFile(fileName);
        }


        /// <summary>
        /// Gets the help text from file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static IEnumerable<TipsItem> GetHelpTextFromFile(string fileName)
        {
            var currencyLanguage = AppSetting.Instance.DisplayLanguage;

            if (!LanguageType.SupportDisplayLanguages.Contains(currencyLanguage))
            {
                currencyLanguage = "en-US";
            }

            var filePath = "Language/Helps/{0}/{1}".FormatWith(currencyLanguage, fileName);

            filePath = ViewPath.LoadContentFromFile(filePath);

            var lines = filePath.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var length = lines.Length;
            int i = 0;
            return lines.Select(p => new TipsItem(++i)
            {
                Text = p.Replace("#NL#", "\r\n").Replace("#T#", "\t")
            });
        }

        /// <summary>
        /// Gets the updating logs.
        /// </summary>
        /// <returns></returns>
        public static string GetUpdatingLogs()
        {
            var g = GetHelpTextFromFile("WhatsNewAndNext.txt");

            return g.Select(p => p.Text).ToStringLine();
        }
    }

    public class TipsItem : NotionObject
    {
        public TipsItem(int id)
        {
            this.ID = id;
        }
        public TipsItem()
            : this(0)
        {
        }
        public int ID { get; set; }

        public string Text { get; set; }

    }
}
