using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Windows.Phone.Speech.VoiceCommands;

using NkjSoft.Extensions;
using TinyMoneyManager.Data.Model;
using SpeechLabs.Common.Extensions;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using TinyMoneyManager.Component;
using Windows.Foundation;
namespace AccountBook.VoiceCommandData
{
    public class Manager : IDisposable
    {
        public readonly static string PlaceHolder = "<!--PlaceHolderOfPhraseList-->";
        public readonly static string PlaceHolderOfPrefix = "$placeHolderOfPrefix";
        public readonly static string LangPlaceHolder = "lang-Place";
        public readonly static string PriceUnitTag = "$unit";

        private TinyMoneyManager.Data.ScheduleManager.SchedulePlanningManager _scheduleManager;
        public static Manager Instance { get; set; }

        private string _defaultUnit = string.Empty;

        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        /// <value>
        /// The data context.
        /// </value>
        public TinyMoneyManager.Data.TinyMoneyDataContext DataContext { get; set; }

        /// <summary>
        /// Initializes the <see cref="Manager" /> class.
        /// </summary>
        static Manager()
        {
            Instance = new Manager();
        }

        public Manager()
        {
            DataContext = new TinyMoneyManager.Data.TinyMoneyDataContext();
            this._scheduleManager = new TinyMoneyManager.Data.ScheduleManager.SchedulePlanningManager(DataContext);
        }

        /// <summary>
        /// Inits the command.
        /// </summary>
        public async Task<InitlaizingStatus> InitCommand(string lang, bool enabled = true)
        {

            var uri = GetVoiceCommandFileUri(lang);

            if (uri == null)
            {
                return InitlaizingStatus.NoFoundVCDFile;
            }

            try
            {
                await VoiceCommandService.InstallCommandSetsFromFileAsync(uri);
                return InitlaizingStatus.Completed;
            }
            catch (Exception ex)
            {
#if DEBUG
                // throw;
#endif
                return InitlaizingStatus.Failed;
            }
        }

        /// <summary>
        /// Gets the voice command file URI.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <param name="needUriTemplate">if set to <c>true</c> [need URI template].</param>
        /// <returns></returns>
        private Uri GetVoiceCommandFileUri(string lang, bool needUriTemplate = true)
        {
            lang = PredicateLang(lang);

            var fileName = "VoiceCommandDefinition/VoiceCommandDefinition.{0}.xml"
                .FormatWith(lang);

            if (!needUriTemplate)
            {
                return new Uri(fileName, UriKind.RelativeOrAbsolute);
            }

            Uri uri = null;

            if (SpeechLabs.Common.Extensions.IsolatedStorageSettingExtensions.ItemExists(fileName))
            {
                uri = new Uri("ms-appdata:///local/{0}".FormatWith(fileName));
            }

            return uri;
        }

        /// <summary>
        /// Predicates the lang.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <returns></returns>
        private string PredicateLang(string lang)
        {
            lang = lang.ToLower();

            // by default, to use en-us voice conmmand.
            if (lang.StartsWith("en"))
            {
                lang = "en";
            }

            switch (lang)
            {
                // by default, to use en-us voice conmmand.
                default:
                case "en":
                    _defaultUnit = "dollars";
                    break;
                case "zh-cn":
                case "zh-sg":
                    lang = "zhs";
                    _defaultUnit = "元";
                    break;
                case "zh-tw":
                case "zh-hk":
                    lang = "zht";
                    _defaultUnit = "元";
                    break;
            }

            return lang;
        }

        /// <summary>
        /// Loads the SGRM.
        /// </summary>
        /// <param name="_recognizer">The _recognizer.</param>
        public async void LoadSGRM(Windows.Phone.Speech.Recognition.SpeechRecognizerUI _recognizer, string lang)
        {
            lang = PredicateLang(lang);

            _recognizer.Recognizer.Grammars.AddGrammarFromUri("confirmation", new Uri("ms-appx:///SRGSGrammar/SRGSGrammar.{0}.xml"
                .FormatWith(lang)));

            try
            {
                await _recognizer.Recognizer.PreloadGrammarsAsync();
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        /// Generates the voice command definition.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="prefix">The prefix.</param>
        public void GenerateVoiceCommandDefinition(string prefix, string lang, string unitOfPrice = "")
        {
            using (TinyMoneyManager.Data.TinyMoneyDataContext db = new TinyMoneyManager.Data.TinyMoneyDataContext())
            {
                var categories = db.Categories
                    .Where(p => p.ParentCategoryId != Guid.Empty)
                    .ToList();

                var accounts = db.Accounts
                    .ToList();

                var fileName = GetVoiceCommandFileUri(lang, false);

                var template = GetTemplate(fileName.OriginalString);

                // replace the place holder with following contents.

                var leftPhraseList = new StringBuilder(PlaceHolder);

                var categoriesPhraseList = GeneratePharseList<Category>("category", categories, p => p.Name);
                var accountsPhraseList = GeneratePharseList("account", accounts, p => p.Name);
                var moneyPhraseList = GeneratePharseList("number", Enumerable.Range(1, 1900), p => p.ToString());

                leftPhraseList
                    .AppendLine()
                    .AppendLine(categoriesPhraseList)
                    .AppendLine(accountsPhraseList)
                    .AppendLine(moneyPhraseList);

                template = template.Replace(PlaceHolder, leftPhraseList.ToString())
                    .Replace(PlaceHolderOfPrefix, prefix)
                    .Replace(LangPlaceHolder, lang)
                    .Replace(PriceUnitTag, unitOfPrice.IsNullOrEmpty() ? _defaultUnit : unitOfPrice);

                IsolatedStorageSettingExtensions.SaveToIsoStore(fileName.OriginalString, Encoding.UTF8.GetBytes(template));
            }
        }

        /// <summary>
        /// Generates the pharse list.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="elements">The elements.</param>
        /// <param name="innerContentGetter">The inner content getter.</param>
        /// <returns></returns>
        private string GeneratePharseList<T1>(string labelName, IEnumerable<T1> elements, Func<T1, string> innerContentGetter)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("    <PhraseList Label=\"{0}\">", labelName)
                .AppendLine();

            foreach (var item in elements)
            {
                sb.AppendFormat("      <Item> {0} </Item>", innerContentGetter(item)).AppendLine();
            }

            sb.AppendLine("    </PhraseList>");

            return sb.ToString();
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private static string GetTemplate(string fileName)
        {
            var uri = new Uri("/AccountBook.VoiceCommandData;component/" + fileName, UriKind.RelativeOrAbsolute);

            using (StreamReader sr = new StreamReader(Application.GetResourceStream(uri).Stream))
            {
                var content = sr.ReadToEnd();

                return content;
            }
        }

        /// <summary>
        /// Schedules items to be created by Schedule Planing Manager.
        /// </summary>
        /// <param name="accountItems">The account item.</param>
        /// <returns></returns>
        public bool ScheduleCreate(params AccountItem[] accountItems)
        {
            return this._scheduleManager.SaveTransactionRecords(accountItems);
        }

        /// <summary>
        /// Schedules the create from.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="money">The money.</param>
        /// <param name="notes">The notes.</param>
        /// <param name="people">The people.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<bool> ScheduleCreateFrom(string accountName, string categoryName, ItemType itemType, decimal money, string notes, string people)
        {
            var item = await DetectAccountItem(accountName, categoryName, itemType, money, notes, people);
            if (item == null)
            {
                return false;
            }

            return ScheduleCreate(item);
        }

        /// <summary>
        /// Detects the account item.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="money">The money.</param>
        /// <param name="notes">The notes.</param>
        /// <param name="peopleName">The people.</param>
        /// <returns></returns>
        public async Task<AccountItem> DetectAccountItem(string accountName, string categoryName, ItemType itemType, decimal money, string notes, string peopleName)
        {
            var ret = await Task.Run<AccountItem>(() =>
                {
                    var account = DataContext.Accounts.FirstOrDefault(p => p.Name == accountName);

                    var category = DataContext.Categories.FirstOrDefault(p => p.Name == categoryName);

                    //  var people = DataContext.Peoples.FirstOrDefault(p => p.Name == peopleName);

                    if (account == null || category == null)
                    {
                        return null;
                    }

                    var result = new AccountItem();

                    result.Account = account;
                    result.Category = category;
                    result.Description = notes;
                    result.Money = money;
                    result.Type = category.CategoryType;

                    return result;
                });

            return ret;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._scheduleManager != null)
            {
                this._scheduleManager.Dispose();
            }
        }
    }

    public enum InitlaizingStatus
    {
        Completed,
        NoFoundVCDFile,
        Failed,
        Disabled
    }
}
