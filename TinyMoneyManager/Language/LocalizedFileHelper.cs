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
using System.Windows.Resources;
using System.Text;
using System.IO;

using NkjSoft.Extensions;

namespace TinyMoneyManager.Language
{
    public class LocalizedFileHelper
    {
        /// <summary>
        /// Loads the data source from localized file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="language">The language.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static List<T> LoadDataSourceFromLocalizedFile<T>(string language, string fileName)
        {
            if (language == "zh-SG" || language == "zh-CN")
            {
                language = "zh-CN";
            }
            else if (language.StartsWith("en"))
            {
                language = "en-US";
            }
            else if (language == "zh-TW" || language == "zh-HK")
            {
                language = "zh-TW";
            }

            var content = string.Format("/Language/LocalizedResources/{0}/{1}", language, fileName);

            content = LoadXmlFromIsolateStorage(content);

            var data = NkjSoft.WPhone.XmlHelper.DeserializeFromXmlString<List<T>>(content);


            return data;
        }

        /// <summary>
        /// Loads the XML from isolate storage.
        /// </summary>
        /// <returns></returns>
        private static string LoadXmlFromIsolateStorage(string fileName)
        {
            if (!fileName.StartsWith("/"))
            {
                fileName.Insert(0, "/");
            }

            var uriPath = "/TinyMoneyManager;component{0}".FormatWith(fileName);
            StreamResourceInfo xml = Application.GetResourceStream(new Uri(uriPath, System.UriKind.Relative));

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                System.Xml.Linq.XDocument.Load(xml.Stream).Save(sw);
                xml.Stream.Close();
                sw.Close();
            }

            return sb.ToString();
        }
    }
}
