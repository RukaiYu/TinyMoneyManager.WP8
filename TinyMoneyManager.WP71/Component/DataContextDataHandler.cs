namespace TinyMoneyManager.Component
{
    using NkjSoft.Extensions;
    using NkjSoft.WPhone;
    using NkjSoft.WPhone.Extensions;
    using RapidRepository;
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using TinyMoneyManager;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Data.ScheduleManager;
    using TinyMoneyManager.DataSynchronizationService;
    using TinyMoneyManager.DataSyncing;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public class DataContextDataHandler
    {
        private System.EventHandler<DataContextDataHandlerEventArgs> BackupDataCompleted;
        public static readonly string LocalDataFolderName = "data";
        public static readonly string LocalFileName = (LocalDataFolderName + "/localDatabaseCacheFile.data");
        public static readonly string LocalFileNameWhenCrashed = (LocalDataFolderName + "/localDatabaseCacheFileBeforeCrashed.data");
        private DataSyncingObjectManager managerForLocal;
        public event System.EventHandler<DataContextDataHandlerEventArgs> RestoreDataCompleted;
        private static System.Collections.Generic.Dictionary<String, Func<TinyMoneyDataContext, DataSynchronizationInfo, String>> tableBackupImage;
        private static System.Collections.Generic.Dictionary<String, Func<TinyMoneyDataContext, String, Int32>> tableRestoreImage;


        public DataContextDataHandler()
        {
            this.DataSynchronizationDataArg = new DataSynchronizationInfo();
            this.InitializeTableNames();
        }

        public void backupData(out string contentOfXml)
        {
            this.DataSynchronizationDataArg.Action = HandlerAction.Backup;
            this.DataSynchronizationDataArg.HandlingInfo.Clear();
            try
            {
                this.managerForLocal = new DataSyncingObjectManager();
                using (TinyMoneyDataContext context = new TinyMoneyDataContext())
                {
                    foreach (var pair in this.TableBackupImage)
                    {
                        string objects = pair.Value(context, this.DataSynchronizationDataArg);

                        this.managerForLocal.AddObject(pair.Key, objects);
                    }

                    contentOfXml = NkjSoft.WPhone.XmlHelper.SerializeToXmlString<DataSyncingObjectManager>(this.managerForLocal);
                }
            }
            catch (System.Exception exception)
            {
                throw exception;
            }
        }

        public string BackupData()
        {
            string contentOfXml = null;
            this.backupData(out contentOfXml);
            return contentOfXml;
        }

        private static string backupDataFromDb<T>(string tableName, System.Collections.Generic.List<T> list, DataSynchronizationInfo e) where T : class
        {
            e.Add(GetKeyLocalizeName(tableName), list.Count);
            return NkjSoft.WPhone.XmlHelper.SerializeToXmlString<System.Collections.Generic.List<T>>(list);
        }

        public DataSynchronizationInfo BackupDataToLocal()
        {
            string str = this.BackupData();
            using (System.IO.IsolatedStorage.IsolatedStorageFile iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                iso.CreateDirectorySafe(LocalDataFolderName);

                var fileName = Path.Combine(LocalDataFolderName, "localDataFileCacheFor_v{0}.data".FormatWith(DatabaseVersion.CurrentVersion));

                if (iso.FileExists(fileName))
                {
                    iso.DeleteFile(fileName);
                }

                using (var stream = iso.OpenFile(fileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                {
                    using (var writer = new System.IO.StreamWriter(stream, System.Text.Encoding.UTF8))
                    {
                        writer.Write(str);
                    }
                }

                return this.DataSynchronizationDataArg;
            }
        }

        public static bool CanRestoreFromLocalfile()
        {
            return System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().FileExists(LocalFileName);
        }

        /// <summary>
        /// Ensures the account amounts.
        /// </summary>
        /// <param name="db">The db.</param>
        private void EnsureAccountAmounts(TinyMoneyDataContext db)
        {
            Account accounts = db.Accounts.FirstOrDefault<Account>(p => p.Name == "现金");
            decimal num = (from x in db.AccountItems
                           where (((int)x.Type) == 1) && (x.AccountId == accounts.Id)
                           select x).Sum<AccountItem>(x => x.Money);
            decimal num2 = (from x in db.AccountItems
                            where (((int)x.Type) == 0) && (x.AccountId == accounts.Id)
                            select x).Sum<AccountItem>(x => x.Money);
            accounts.Balance = new decimal?(num - num2);
            db.SubmitChanges();
        }

        private string EnsureFileToUse()
        {
            string localFileName = LocalFileName;
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists(LocalFileNameWhenCrashed) && (MessageBox.Show(LocalizedStrings.GetLanguageInfoByKey("SelectTheDataFileCacheBeforeCrashed").FormatWith(new object[] { file.GetCreationTime(LocalFileNameWhenCrashed).DateTime.ToLocalizedDateTimeString() }), App.AlertBoxTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK))
                {
                    localFileName = LocalFileNameWhenCrashed;
                }
            }
            return localFileName;
        }

        public object GetData()
        {
            return this.BackupData();
        }

        public static string GetKeyLocalizeName(string tableName)
        {
            if (LocalizedTableName.ContainsKey(tableName))
            {
                return LocalizedTableName[tableName];
            }
            return tableName;
        }

        public static string GetMessageWithRestoreTips(DataSynchronizationInfo result, bool isFromLocal)
        {
            if (isFromLocal)
            {
                return "{0}\r\n{1}".FormatWith(new object[] { result.GetMessage(), LocalizedStrings.GetLanguageInfoByKey("{0} {1} \r\n{2}", new string[] { "OperationSuccessfullyMessage", "TheCacheFileWillBeDeletedMessage", "AfterRestoreDataMessage" }) });
            }
            return "{0}\r\n{1}".FormatWith(new object[] { result.GetMessage(), LocalizedStrings.GetLanguageInfoByKey("AfterRestoreDataMessage") });
        }

        public void HandlingDataFromSkyDrive(object data)
        {
            this.RestoreData(data.ToString());
        }

        private void InitializeTableNames()
        {
            if (LocalizedTableName == null)
            {
                string[] strArray = AppResources.LocalizedTableName.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                LocalizedTableName = new System.Collections.Generic.Dictionary<String, String>();
                foreach (string str in strArray)
                {
                    string[] strArray2 = str.Split(new char[] { ',' });
                    LocalizedTableName.Add(strArray2[0], strArray2[1]);
                }
            }
        }

        private static void instance_RestoreDataCompleted(object sender, DataContextDataHandlerEventArgs e)
        {
            SetReloadViewModelDataFlag();
        }

        protected virtual void OnBackupDataCompleted(DataContextDataHandlerEventArgs e)
        {
            System.EventHandler<DataContextDataHandlerEventArgs> backupDataCompleted = this.BackupDataCompleted;
            if (backupDataCompleted != null)
            {
                backupDataCompleted(this, e);
            }
        }

        protected virtual void OnRestoreDataCompleted(DataContextDataHandlerEventArgs e)
        {
            System.EventHandler<DataContextDataHandlerEventArgs> restoreDataCompleted = this.RestoreDataCompleted;
            if (restoreDataCompleted != null)
            {
                restoreDataCompleted(this, e);
            }
        }

        public static void RemoveData(System.Action<TinyMoneyDataContext> otherAction = null)
        {
            using (TinyMoneyDataContext context = new TinyMoneyDataContext())
            {
                context.AccountItems.DeleteAllOnSubmit<AccountItem>(context.AccountItems);
                context.Repayments.DeleteAllOnSubmit<Repayment>(context.Repayments);
                context.Accounts.DeleteAllOnSubmit<Account>(context.Accounts);
                context.Categories.DeleteAllOnSubmit<Category>(context.Categories);
                context.TransferingItems.DeleteAllOnSubmit<TransferingItem>(context.TransferingItems);
                context.BudgetItems.DeleteAllOnSubmit<BudgetItem>(context.BudgetItems);
                context.BudgetMonthlyReports.DeleteAllOnSubmit<BudgetMonthlyReport>(context.BudgetMonthlyReports);
                context.BudgetProjects.DeleteAllOnSubmit<BudgetProject>(context.BudgetProjects);
                context.TallyScheduleTable.DeleteAllOnSubmit<TallySchedule>(context.TallyScheduleTable);
                context.Peoples.DeleteAllOnSubmit<PeopleProfile>(context.Peoples);
                context.PeopleGroups.DeleteAllOnSubmit<PeopleGroup>(context.PeopleGroups);
                context.PictureInfos.DeleteAllOnSubmit<PictureInfo>(context.PictureInfos);
                context.PeopleAssociationDatas.DeleteAllOnSubmit<PeopleAssociationData>(context.PeopleAssociationDatas);
                try
                {
                    if (otherAction != null)
                    {
                        otherAction(context);
                    }
                }
                catch (System.Exception)
                {
                }
                context.SubmitChanges();
            }
        }

        public static void ResetData()
        {
            using (TinyMoneyDataContext context = new TinyMoneyDataContext())
            {
                context.AccountItems.DeleteAllOnSubmit<AccountItem>(context.AccountItems);
                context.Repayments.DeleteAllOnSubmit<Repayment>(context.Repayments);
                context.Accounts.DeleteAllOnSubmit<Account>(from p in context.Accounts
                                                            where p.CanBeDeleted
                                                            select p);
                context.Accounts.FirstOrDefault<Account>(p => !p.CanBeDeleted).Balance = 0.0M;
                context.Categories.DeleteAllOnSubmit<Category>(context.Categories);
                context.TransferingItems.DeleteAllOnSubmit<TransferingItem>(context.TransferingItems);
                context.BudgetItems.DeleteAllOnSubmit<BudgetItem>(context.BudgetItems);
                context.BudgetMonthlyReports.DeleteAllOnSubmit<BudgetMonthlyReport>(context.BudgetMonthlyReports);
                context.BudgetProjects.DeleteAllOnSubmit<BudgetProject>(context.BudgetProjects);
                context.TallyScheduleTable.DeleteAllOnSubmit<TallySchedule>(context.TallyScheduleTable);
                context.Peoples.DeleteAllOnSubmit<PeopleProfile>(context.Peoples);
                context.PeopleGroups.DeleteAllOnSubmit<PeopleGroup>(context.PeopleGroups);
                context.PictureInfos.DeleteAllOnSubmit<PictureInfo>(context.PictureInfos);
                context.PeopleAssociationDatas.DeleteAllOnSubmit<PeopleAssociationData>(context.PeopleAssociationDatas);
                try
                {
                    new SecondSchedulePlanningManager(context).Update(true);
                }
                catch (System.Exception)
                {
                }
                context.SubmitChanges();

            }
        }

        private static int restoreAccountItem(TinyMoneyDataContext db, string source)
        {
            int num3;
            try
            {
                Account entity = db.Accounts.FirstOrDefault<Account>();
                if (entity == null)
                {
                    entity = AccountViewModel.DefaultAccounts.FirstOrDefault<Account>();
                    db.Accounts.InsertOnSubmit(entity);
                    db.SubmitChanges();
                }
                int count = 0;
                System.Collections.Generic.List<AccountItem> list = NkjSoft.WPhone.XmlHelper.DeserializeFromXmlString<System.Collections.Generic.List<AccountItem>>(source.Trim());
                if (list != null)
                {
                    Table<AccountItem> table = db.GetTable<AccountItem>();
                    if (table == null)
                    {
                        return 0;
                    }
                    foreach (AccountItem item in list)
                    {
                        if (item == null)
                        {
                            continue;
                        }
                        if (item.AccountId == System.Guid.Empty)
                        {
                            item.AccountId = entity.Id;
                            decimal money = item.Money;
                            if (item.Type == ItemType.Expense)
                            {
                                money = -money;
                            }
                            decimal? balance = entity.Balance;
                            decimal num4 = money;
                            entity.Balance = balance.HasValue ? new decimal?(balance.GetValueOrDefault() + num4) : null;
                        }
                        table.InsertOnSubmit(item);
                    }
                    count = list.Count;
                    db.SubmitChanges();
                }
                num3 = count;
            }
            catch (System.Exception exception)
            {
                throw exception;
            }
            return num3;
        }

        public static void RestoreAppSettings(AppSetting appSetting)
        {
            AppSettingRepository.Instance.GetAll().ToList<AppSetting>().ForEach(delegate(AppSetting p)
            {
                AppSettingRepository.Instance.Delete(p.Id);
            });
            RapidContext.CurrentContext.SaveChanges();
            AppSetting.Instance.Id = appSetting.Id;
            AppSetting.Instance.ServerSyncIPAddress = appSetting.ServerSyncIPAddress;
            AppSetting.Instance.Password = appSetting.Password;
            AppSetting.Instance.DisplayLanguage = appSetting.DisplayLanguage ?? System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            AppSetting.Instance.ServerSyncIPAddress = appSetting.ServerSyncIPAddress;
            AppSetting.Instance.EnablePoketLock = appSetting.EnablePoketLock;
            AppSetting.Instance.Email = appSetting.Email;
            AppSetting.Instance.UseBackgroundImageForMainPage = appSetting.UseBackgroundImageForMainPage;
            AppSetting.Instance.IncludeCreditCardAmountOnAsset = appSetting.IncludeCreditCardAmountOnAsset;
            AppSetting.Instance.CurrencyConversionRateTable = ApplicationHelper.VerifyTableFromSource(appSetting.CurrencyConversionRateTable);
            AppSetting.Instance.DefaultCurrency = appSetting.DefaultCurrency;
            AppSetting.Instance.CurrencyInfo = appSetting.CurrencyInfo;
            AppSetting.Instance.ShowRepaymentInfoOnTile = appSetting.ShowRepaymentInfoOnTile;
            AppSetting.Instance.UserId = appSetting.UserId;
            AppSetting.Instance.GlobleCurrencySymbolStyle = appSetting.GlobleCurrencySymbolStyle;
            AppSetting.Instance.EnableAllAccountOverdraft = appSetting.EnableAllAccountOverdraft;
            AppSetting.Instance.Profile = appSetting.Profile ?? new UserProfile();
            AppSetting.Instance.SubscibeNotification = appSetting.SubscibeNotification;
            AppSetting.Instance.DayOfWeekForEveryDayBackup = appSetting.DayOfWeekForEveryDayBackup;
            AppSetting.Instance.AutoBackupWhenAppUp = appSetting.AutoBackupWhenAppUp;
            AppSetting.Instance.DefaultAccount = appSetting.DefaultAccount;
            AppSetting.Instance.ShowAssociatedAccountItemSummary = appSetting.ShowAssociatedAccountItemSummary;
            AppSetting.Instance.ShowCashAmountOnAsset = appSetting.ShowCashAmountOnAsset;
            AppSetting.Instance.AlertWhenBudgetIsOver = appSetting.AlertWhenBudgetIsOver;
            AppSetting.Instance.AccountCategoryWappers = appSetting.AccountCategoryWappers ?? AccountCategoryHelper.GetDefaultWrappers();
            AppSetting.Instance.FavoritesPageVisibiable = appSetting.FavoritesPageVisibiable;
            AppSetting.Instance.BudgetStatsicSettings = appSetting.BudgetStatsicSettings;
            AppSetting.Instance.VoiceCommandSettingWithDigits = appSetting.VoiceCommandSettingWithDigits;
            AppSetting.Instance.VoiceCommandSettingMininumValue = appSetting.VoiceCommandSettingMininumValue;
            AppSetting.Instance.VoiceCommandSettingMaximumValue = appSetting.VoiceCommandSettingMaximumValue;

            ApplicationHelper.HasLoadDefaultCategorys = true;
            AppSettingRepository.Instance.Add(AppSetting.Instance);
            RapidContext.CurrentContext.SaveChanges();
        }

        public void RestoreData(string dataForContext)
        {
            this.DataSynchronizationDataArg.HandlingInfo.Clear();
            this.DataSynchronizationDataArg.Action = HandlerAction.Restore;
            try
            {
                if (!string.IsNullOrEmpty(dataForContext))
                {
                    int length = dataForContext.Trim().Length;
                }
                this.managerForLocal = NkjSoft.WPhone.XmlHelper.DeserializeFromXmlString<DataSyncingObjectManager>(dataForContext);
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(LocalizedStrings.GetLanguageInfoByKey("ErrorMessageWhenRestoreEmptyDataFailed").FormatWith(new object[] { "https://skydrive.live.com/redir?resid=D9CB9D904309AE62!605", exception.Message }), App.AlertBoxTitle, MessageBoxButton.OK);
                AppUpdater.AddErrorLog("ErrorMessageWhenRestoreEmptyDataFailed", exception.Message, new string[0]);
                this.DataSynchronizationDataArg.Result = OperationResult.Successfully;
            }
            if (this.managerForLocal != null)
            {
                if (MessageBox.Show(AppResources.MakeSureToCoverCurrentData, App.AlertBoxTitle, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    this.DataSynchronizationDataArg.Result = OperationResult.Cancel;
                }
                else
                {
                    RemoveData(null);
                    string str = string.Empty;
                    int num = 0;
                    string languageInfoByKey = LocalizedStrings.GetLanguageInfoByKey("FailedToRestoreWhenObjectIs");
                    using (TinyMoneyDataContext context = new TinyMoneyDataContext())
                    {

                        foreach (var keyValue in this.TableRestoreImage)
                        {
                            System.Func<TinyMoneyDataContext, String, Int32> func = keyValue.Value;

                            DataSyncingObject obj2 = this.managerForLocal.DataSyncingObjects.FirstOrDefault<DataSyncingObject>(p => p.ObjectName == keyValue.Key);
                            int num2 = 0;
                            string keyLocalizeName = GetKeyLocalizeName(keyValue.Key);
                            if (obj2 != null)
                            {
                                try
                                {
                                    num2 = func(context, obj2.ObjectList);
                                }
                                catch (System.Exception exception2)
                                {
                                    num2 = 0;
                                    MessageBox.Show(languageInfoByKey.FormatWith(new object[] { keyLocalizeName, exception2.Message }));
                                    str = str + keyLocalizeName + "\r\n\t";
                                    num++;
                                }
                            }
                            this.DataSynchronizationDataArg.Add(keyLocalizeName, num2);
                        }


                        if (str.Length > 0)
                        {
                            this.DataSynchronizationDataArg.TotalMessage = LocalizedStrings.GetLanguageInfoByKey("FailedReplacer") + "\r\n\t" + str;
                        }
                        try
                        {
                            new SecondSchedulePlanningManager(context).Update(true);
                            ViewModelLocator.CustomizedTallyViewModel.TemplateManager.Setup(context);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    this.DataSynchronizationDataArg.Result = OperationResult.Successfully;
                    this.OnRestoreDataCompleted(new DataContextDataHandlerEventArgs(dataForContext));
                }
            }
            else
            {
                this.DataSynchronizationDataArg.Result = OperationResult.Failed;
            }
        }

        public void RestoreDatabase(DataSynchronizationArgs e)
        {
            this.DataSynchronizationDataArg.HandlingInfo.Clear();
            this.DataSynchronizationDataArg.Action = HandlerAction.Restore;
            if (e.SdfFileContent == null)
            {
                string notAvaliableFromPCClientUsedToRestoreDBMessage = AppResources.NotAvaliableFromPCClientUsedToRestoreDBMessage;
                CommonExtensions.Alert(null, notAvaliableFromPCClientUsedToRestoreDBMessage, null);
                throw new System.IO.FileNotFoundException(notAvaliableFromPCClientUsedToRestoreDBMessage);
            }
            bool flag = AppUpdater.RestoreDatabase(e.SdfFileContent);
            this.DataSynchronizationDataArg.Add("Sdf", flag ? 1 : 0);
        }

        /// <summary>
        /// Restores the data from local.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public DataSynchronizationInfo RestoreDataFromLocal(string filePath = null)
        {
            using (var iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                string path = filePath == null ? this.EnsureFileToUse() : filePath;

                if (!iso.FileExists(path))
                {
                    throw new System.IO.FileNotFoundException(path);
                }

                using (var stream = iso.OpenFile(path, System.IO.FileMode.Open))
                {
                    using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8))
                    {
                        string dataForContext = reader.ReadToEnd();

                        this.RestoreData(dataForContext);
                        if (this.DataSynchronizationDataArg.Result == OperationResult.Successfully)
                        {
                            try
                            {
                                iso.DeleteFile(LocalFileName);
                            }
                            catch (System.Exception exception)
                            {
                                AppUpdater.AddErrorLog("Restore File from Local finished, deleted file faild.", exception.ToString(), new string[0]);
                            }
                        }

                        return this.DataSynchronizationDataArg;
                    }
                }
            }
        }

        private static int restoreDataToDb<T>(TinyMoneyDataContext db, string source) where T : class
        {
            int num2;
            try
            {
                int count = 0;
                System.Collections.Generic.List<T> list = NkjSoft.WPhone.XmlHelper.DeserializeFromXmlString<System.Collections.Generic.List<T>>(source.Trim());
                if (list != null)
                {
                    Table<T> table = db.GetTable<T>();
                    if (table == null)
                    {
                        return 0;
                    }
                    foreach (T local in list)
                    {
                        if (local != null)
                        {
                            table.InsertOnSubmit(local);
                        }
                    }
                    count = list.Count;
                    db.SubmitChanges();
                }
                num2 = count;
            }
            catch (System.Exception exception)
            {
                throw exception;
            }
            return num2;
        }

        public static void SetReloadViewModelDataFlag()
        {
            bool flag = false;
            ViewModelLocator.IncomeViewModel.IsDataLoaded = flag;
            ViewModelLocator.ExpensesViewModel.IsDataLoaded = flag;
            ViewModelLocator.AccountViewModel.IsDataLoaded = flag;
            ViewModelLocator.AccountItemViewModel.IsDataLoaded = flag;
            ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = flag;
            ViewModelLocator.MainPageViewModel.HasLoadCompareInfo = flag;
            ViewModelLocator.RepaymentManagerViewModel.IsDataLoaded = flag;
            ViewModelLocator.TransferingHistoryViewModel.IsDataLoaded = flag;
            ViewModelLocator.BorrowLeanViewModel.IsDataLoaded = flag;
            ViewModelLocator.PeopleViewModel.IsDataLoaded = flag;
            ViewModelLocator.PicturesViewModel.IsDataLoaded = flag;
            ViewModelLocator.CustomizedTallyViewModel.IsDataLoaded = flag;
        }

        public DataSynchronizationInfo DataSynchronizationDataArg { get; set; }

        public static DataContextDataHandler Instance
        {
            get
            {
                DataContextDataHandler handler = new DataContextDataHandler();
                handler.RestoreDataCompleted += new System.EventHandler<DataContextDataHandlerEventArgs>(DataContextDataHandler.instance_RestoreDataCompleted);
                return handler;
            }
        }

        public static System.Collections.Generic.Dictionary<String, String> LocalizedTableName
        {
            get;
            set;
        }

        public System.Collections.Generic.Dictionary<string, System.Func<TinyMoneyDataContext, DataSynchronizationInfo, String>> TableBackupImage
        {
            get
            {
                if (tableBackupImage == null)
                {
                    System.Collections.Generic.Dictionary<string, System.Func<TinyMoneyDataContext, DataSynchronizationInfo, String>> dictionary =
                        new System.Collections.Generic.Dictionary<String, Func<TinyMoneyDataContext, DataSynchronizationInfo, String>>();
                    dictionary.Add("Categories", (db, e) => backupDataFromDb<Category>("Categories", db.Categories.ToList<Category>(), e));
                    dictionary.Add("Accounts", (db, e) => backupDataFromDb<Account>("Accounts", db.Accounts.ToList<Account>(), e));
                    dictionary.Add("AccountItems", (db, e) => backupDataFromDb<AccountItem>("AccountItems", db.AccountItems.ToList<AccountItem>(), e));
                    dictionary.Add("Repayments", (db, e) => backupDataFromDb<Repayment>("Repayments", db.Repayments.ToList<Repayment>(), e));
                    dictionary.Add("TransferingItems", (db, e) => backupDataFromDb<TransferingItem>("TransferingItems", db.TransferingItems.ToList<TransferingItem>(), e));
                    dictionary.Add("BudgetProjects", (db, e) => backupDataFromDb<BudgetProject>("BudgetProjects", db.BudgetProjects.ToList<BudgetProject>(), e));
                    dictionary.Add("BudgetItems", (db, e) => backupDataFromDb<BudgetItem>("BudgetItems", db.BudgetItems.ToList<BudgetItem>(), e));
                    dictionary.Add("BudgetMonthlyReports", (db, e) => backupDataFromDb<BudgetMonthlyReport>("BudgetMonthlyReports", db.BudgetMonthlyReports.ToList<BudgetMonthlyReport>(), e));
                    dictionary.Add("TallyScheduleTable", (db, e) => backupDataFromDb<TallySchedule>("TallyScheduleTable", db.TallyScheduleTable.ToList<TallySchedule>(), e));
                    dictionary.Add("PeopleGroups", (db, e) => backupDataFromDb<PeopleGroup>("PeopleGroups", db.PeopleGroups.ToList<PeopleGroup>(), e));
                    dictionary.Add("Peoples", (db, e) => backupDataFromDb<PeopleProfile>("Peoples", db.Peoples.ToList<PeopleProfile>(), e));
                    dictionary.Add("PictureInfos", (db, e) => backupDataFromDb<PictureInfo>("PictureInfos", db.PictureInfos.ToList<PictureInfo>(), e));
                    dictionary.Add("PeopleAssociationDatas", (db, e) => backupDataFromDb<PeopleAssociationData>("PeopleAssociationDatas", db.PeopleAssociationDatas.ToList<PeopleAssociationData>(), e));
                    dictionary.Add("AppSettings", delegate(TinyMoneyDataContext db, DataSynchronizationInfo e)
                    {
                        e.Add(GetKeyLocalizeName("AppSettings"), 1);
                        return NkjSoft.WPhone.XmlHelper.SerializeToXmlString<AppSetting>(AppSetting.Instance);
                    });
                    tableBackupImage = dictionary;
                }
                return tableBackupImage;
            }
        }

        public System.Collections.Generic.Dictionary<string, System.Func<TinyMoneyDataContext, String, Int32>> TableRestoreImage
        {
            get
            {
                if (tableRestoreImage == null)
                {
                    System.Collections.Generic.Dictionary<string, System.Func<TinyMoneyDataContext, String, Int32>> dictionary =
                        new System.Collections.Generic.Dictionary<String, Func<TinyMoneyDataContext, String, Int32>>();
                    dictionary.Add("Categories", (db, s) => restoreDataToDb<Category>(db, s));
                    dictionary.Add("Accounts", (db, s) => restoreDataToDb<Account>(db, s));
                    dictionary.Add("AccountItems", (db, s) => restoreAccountItem(db, s.Replace(">RMB<", ">CNY<")));
                    dictionary.Add("Repayments", (db, s) => restoreDataToDb<Repayment>(db, s));
                    dictionary.Add("TransferingItems", (db, s) => restoreDataToDb<TransferingItem>(db, s));
                    dictionary.Add("BudgetProjects", (db, s) => restoreDataToDb<BudgetProject>(db, s));
                    dictionary.Add("BudgetItems", (db, s) => restoreDataToDb<BudgetItem>(db, s));
                    dictionary.Add("BudgetMonthlyReports", (db, s) => restoreDataToDb<BudgetMonthlyReport>(db, s));
                    dictionary.Add("TallyScheduleTable", (db, s) => restoreDataToDb<TallySchedule>(db, s));
                    dictionary.Add("PeopleGroups", (db, s) => restoreDataToDb<PeopleGroup>(db, s));
                    dictionary.Add("Peoples", (db, s) => restoreDataToDb<PeopleProfile>(db, s));
                    dictionary.Add("PictureInfos", (db, s) => restoreDataToDb<PictureInfo>(db, s));
                    dictionary.Add("PeopleAssociationDatas", (db, s) => restoreDataToDb<PeopleAssociationData>(db, s));
                    dictionary.Add("AppSettings", delegate(TinyMoneyDataContext db, string s)
                    {
                        RestoreAppSettings(NkjSoft.WPhone.XmlHelper.DeserializeFromXmlString<AppSetting>(s));
                        return 1;
                    });
                    tableRestoreImage = dictionary;
                }
                return tableRestoreImage;
            }
            set
            {
                tableRestoreImage = value;
            }
        }


        /// <summary>
        /// Backups the current version.
        /// </summary>
        internal void BackupCurrentVersion()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        var fileName = Path.Combine(LocalDataFolderName, "localDataFileCacheFor_v{0}.beforeRebuild.data".FormatWith(DatabaseVersion.CurrentVersion));

                        var originalFile = Path.Combine(LocalDataFolderName, "localDataFileCacheFor_v{0}.data".FormatWith(DatabaseVersion.CurrentVersion));

                        if (!iso.FileExists(fileName))
                        {
                            iso.CopyFile(originalFile, fileName, true);
                        }
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    throw ex;
#endif
                }
            });
        }
    }
}

