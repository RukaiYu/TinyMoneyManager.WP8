namespace TinyMoneyManager.Component
{
    using mangoProgressIndicator;
    using Microsoft.Phone.Data.Linq;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Windows;
    using TinyMoneyManager;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public class AppUpdater
    {
        public const string BackupDataBaseFileFolder = "dbFileCacheFolder";
        private static bool failedToUpdate;
        private const string LastTimeCrashedCauseOfDbKey = "LastTimeCrashedCauseOfDbKey";
        private static int oldVersion;
        private const string sqlCeException = "SqlCeException";

        public static void AddErrorLog(string block, string msg, params string[] args)
        {
        }

        public static void AfterDataBaseOperationFailed()
        {
            if (failedToUpdate)
            {
                MessageBox.Show(LocalizedStrings.GetLanguageInfoByKey("FailedWhenUpdatingDatabaseMesssage"), "Account Book(手机账本)", MessageBoxButton.OK);
                BackupDatabase(TinyMoneyDataContext.DbFileName, oldVersion.ToString());
            }
        }

        private static void AfterRebuild(DatabaseSchemaUpdater dataBaseUpdater)
        {
            UpdateStructure(dataBaseUpdater);
        }

        private static void BackupCacheFile(System.IO.IsolatedStorage.IsolatedStorageFile store)
        {
            if (store.FileExists(DataContextDataHandler.LocalFileName))
            {
                store.CopyFile(DataContextDataHandler.LocalFileName, DataContextDataHandler.LocalFileNameWhenCrashed, true);
            }
        }

        private static string BackupDatabase(string filePath, string version)
        {
            string path = string.Format("account_book_database_v{0}.sdf", version);
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                System.Console.WriteLine(file.GetFileNames("*").ToList<string>().Count.ToString());
                ViewModelLocator.AccountBookDataContext.Dispose();
                if (!file.DirectoryExists("dbFileCacheFolder"))
                {
                    file.CreateDirectory("dbFileCacheFolder");
                }
                path = "dbFileCacheFolder/" + path;
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(filePath, System.IO.FileMode.Open))
                {
                    using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream2 = file.CreateFile(path))
                    {
                        byte[] buffer = new byte[0x1000];
                        int count = -1;
                        while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream2.Write(buffer, 0, count);
                        }
                        stream2.Close();
                        stream.Close();
                    }
                }
                BackupCacheFile(file);
            }
            return path;
        }

        public static bool EnsureItsADatabaseIssue(System.Exception exception)
        {
            string name = exception.GetType().Name;
            System.Console.WriteLine(name);
            return (name == "SqlCeException");
        }

        public static string EnsureWhetherNeedToRebuildDataBase(System.Exception exception)
        {
            if (!EnsureItsADatabaseIssue(exception))
            {
                return string.Empty;
            }
            MessageBox.Show(AppResources.FailedWhenUpdatingDatabaseMesssage.FormatWith(new object[] { "http://yurukai.wordpress.com" }));
            BackupDatabase(TinyMoneyDataContext.DbFileName, oldVersion.ToString());
            ForceRebuildDatabase();
            LastTimeCrashedCauseOfDb = true;
            return AppResources.RebuildDatabaseDoneMessage;
        }

        public static void EveryDayBackup()
        {
        }

        public static void ForceRebuildDatabase()
        {
            GlobalIndicator.Instance.BusyForWork(LocalizedStrings.GetLanguageInfoByKey("RebuildingDatabase"), new object[0]);
            using (TinyMoneyDataContext context = new TinyMoneyDataContext())
            {
                DatabaseSchemaUpdater updater = null;
                if (context.DatabaseExists())
                {
                    context.DeleteDatabase();
                    context.CreateDatabase();
                    updater = context.CreateDatabaseSchemaUpdater();
                    updater.DatabaseSchemaVersion = 8;
                    updater.Execute();
                }
            }
            ApplicationHelper.HasLoadDefaultCategorys = false;
            ViewModelLocator.instanceLoader.Reset("DataContext_AccountBookDataContext", new TinyMoneyDataContext());
            GlobalIndicator.Instance.WorkDone();
        }

        public static bool HandleDatabaseUpdatingAndDataPreload()
        {
            bool flag = false;
            try
            {
                UpdateIfNeed();
                EveryDayBackup();
                LoadDefaultCategorys();
                flag = true;
            }
            catch (System.Exception exception)
            {
                EnsureWhetherNeedToRebuildDataBase(exception);
                flag = false;
                MessageBox.Show(exception.Message);
            }
            return flag;
        }

        public static void LoadDefaultCategorys()
        {
            if (!ApplicationHelper.HasLoadDefaultCategorys)
            {
                System.Collections.Generic.List<Category> categories = LocalizedFileHelper.LoadDataSourceFromLocalizedFile<Category>(AppSetting.Instance.DisplayLanguage, "DefaultCategories.xml");
                ViewModelLocator.CategoryViewModel.AddRange(categories);
                LoadDefaultPeopleGroups(ViewModelLocator.CategoryViewModel.AccountBookDataContext);
                ViewModelLocator.AccountViewModel.AddAccount(AccountViewModel.DefaultAccounts);
                ApplicationHelper.HasLoadDefaultCategorys = true;
            }
        }

        private static void LoadDefaultPeopleGroups(TinyMoneyDataContext db)
        {
            string displayLanguage = AppSetting.Instance.DisplayLanguage;
            System.Collections.Generic.List<PeopleGroup> peopleGroups = LocalizedFileHelper.LoadDataSourceFromLocalizedFile<PeopleGroup>(displayLanguage, "DefaultPeopleGroup.xml");
            ApplicationSafetyService.TrackToRun(delegate
            {
                System.Collections.Generic.List<Guid> idsAlreadyExist = (from p in db.PeopleGroups select p.Id).ToList<System.Guid>();
                (from p in peopleGroups
                 where idsAlreadyExist.Contains(p.Id)
                 select p).ToList<PeopleGroup>().ForEach(delegate(PeopleGroup p)
                {
                    peopleGroups.Remove(p);
                });
                if (peopleGroups.Count > 0)
                {
                    db.PeopleGroups.InsertAllOnSubmit<PeopleGroup>(peopleGroups);
                    db.SubmitChanges();
                }
            });
        }

        public static void RebuildDatabase()
        {
            using (TinyMoneyDataContext context = new TinyMoneyDataContext())
            {
                DatabaseSchemaUpdater dataBaseUpdater = null;
                if (!context.DatabaseExists())
                {
                    context.CreateDatabase();
                    dataBaseUpdater = context.CreateDatabaseSchemaUpdater();
                    dataBaseUpdater.DatabaseSchemaVersion = DatabaseVersion.LatestVersion;
                    dataBaseUpdater.Execute();
                }
                else
                {
                    dataBaseUpdater = context.CreateDatabaseSchemaUpdater();
                    int databaseSchemaVersion = dataBaseUpdater.DatabaseSchemaVersion;
                    if (dataBaseUpdater.DatabaseSchemaVersion != DatabaseVersion.LatestVersion)
                    {
                        try
                        {
                            AfterRebuild(dataBaseUpdater);
                            failedToUpdate = false;
                        }
                        catch (System.Exception exception)
                        {
                            oldVersion = databaseSchemaVersion;
                            failedToUpdate = true;
                            throw exception;
                        }
                    }
                }
                BudgetManager.Current.SaveCurrentMonthBudgetSettleReport(context, System.DateTime.Now);
            }
        }

        private void RepairDatabase(string errorMessage)
        {
        }

        private static bool ReplaceCurrentDatabasFileFrom(string newFileName, System.IO.IsolatedStorage.IsolatedStorageFile store)
        {
            store.DeleteFile(TinyMoneyDataContext.DbFileName);
            store.CopyFile(newFileName, TinyMoneyDataContext.DbFileName, true);
            return true;
        }

        public static bool RestoreDatabase(byte[] sdfFileContent)
        {
            bool flag;
            string path = "account_book_database_Restore_Temp.sdf";
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists("dbFileCacheFolder"))
                {
                    file.CreateDirectory("dbFileCacheFolder");
                }
                path = "dbFileCacheFolder/" + path;
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.CreateFile(path))
                {
                    byte[] buffer = sdfFileContent;
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                }
                try
                {
                    GlobalIndicator.Instance.BusyForWork("Checking database file version ...", new object[0]);
                    TinyMoneyDataContext dataContext = TinyMoneyDataContext.CreateDbByFile(path);
                    if (!dataContext.DatabaseExists())
                    {
                        dataContext.CreateDatabase();
                    }
                    DatabaseSchemaUpdater updater = dataContext.CreateDatabaseSchemaUpdater();
                    if (updater.DatabaseSchemaVersion != 8)
                    {
                        if (CommonExtensions.AlertConfirm(null, AppResources.RestoreDatabase_UnsupportDatabaseVerionAgainstCurrentVersion.FormatWith(new object[] { updater.DatabaseSchemaVersion, 8 }), null, null) == MessageBoxResult.Cancel)
                        {
                            return false;
                        }
                    }
                    else if (CommonExtensions.AlertConfirm(null, AppResources.RestoreDatabase_Confirm.FormatWith(new object[] { updater.DatabaseSchemaVersion }), null, null) == MessageBoxResult.Cancel)
                    {
                        return false;
                    }
                    GlobalIndicator.Instance.BusyForWork("Backuping current old database file ...", new object[0]);
                    string newFileName = BackupDatabase(TinyMoneyDataContext.DbFileName, 8 + "_backup");
                    GlobalIndicator.Instance.BusyForWork("Replacing current old database file with the one from PC client ...", new object[0]);
                    try
                    {
                        ReplaceCurrentDatabasFileFrom(path, file);
                    }
                    catch (System.Exception exception)
                    {
                        MessageBox.Show("Failed to repalce database with the file from PC client. Will roll back operating ...\r\nError:\r\n\t{0}".FormatWith(new object[] { exception.Message }));
                        ReplaceCurrentDatabasFileFrom(newFileName, file);
                        return false;
                    }
                    return true;
                }
                catch (System.Exception exception2)
                {
                    GlobalIndicator.Instance.WorkDone();
                    CommonExtensions.Alert(null, exception2.Message, null);
                    return false;
                }
                finally
                {
                    GlobalIndicator.Instance.WorkDone();
                }
            }
            return flag;
        }

        public static void UpdateIfNeed()
        {
            RebuildDatabase();
        }

        public static void UpdateStructure(DatabaseSchemaUpdater dataBaseUpdater)
        {
            if (dataBaseUpdater.DatabaseSchemaVersion == 2)
            {
                dataBaseUpdater.AddColumn<Category>("Favourite");
                dataBaseUpdater.DatabaseSchemaVersion = 3;
                dataBaseUpdater.Execute();
            }
            if (dataBaseUpdater.DatabaseSchemaVersion == 3)
            {
                BudgetItem.UpdateDataContext(dataBaseUpdater);
                BudgetProject.UpdateDataContext(dataBaseUpdater);
                BudgetMonthlyReport.UpdateUpdateDataContext(dataBaseUpdater);
                dataBaseUpdater.DatabaseSchemaVersion = 4;
                dataBaseUpdater.Execute();
            }
            if (dataBaseUpdater.DatabaseSchemaVersion == 4)
            {
                BudgetProject.UpdateDataContext_At_v190(dataBaseUpdater);
                Category.UpdateDataContext_At_v190(dataBaseUpdater);
                BudgetItem.UpdateDataContextAt_190(dataBaseUpdater);
                dataBaseUpdater.AddTable<TallySchedule>();
                dataBaseUpdater.AddTable<PeopleProfile>();
                Repayment.UpdateTableStructureAtV1_9_1(dataBaseUpdater);
                dataBaseUpdater.DatabaseSchemaVersion = 5;
                dataBaseUpdater.Execute();
                dataBaseUpdater.Context.GetTable<BudgetProject>().ToList<BudgetProject>().ForEach(delegate(BudgetProject p)
                {
                    p.Amount = p.GetMoney();
                });
                dataBaseUpdater.Context.SubmitChanges();
            }
            if (dataBaseUpdater.DatabaseSchemaVersion == 5)
            {
                AccountItem.UpdateStructureAt1_9_1(dataBaseUpdater);
                SchedulePlanningTable.UpdateStructureAt_1_9_1(dataBaseUpdater);
                TallySchedule.UpdateStructureAt_1_9_1(dataBaseUpdater);
                dataBaseUpdater.DatabaseSchemaVersion = 6;
                dataBaseUpdater.Execute();
            }
            if (dataBaseUpdater.DatabaseSchemaVersion == 6)
            {
                PeopleGroup.HandleUpdatingAt_1_9_3(dataBaseUpdater);
                PeopleProfile.UpdateStructureAt_1_9_3(dataBaseUpdater);
                Repayment.UpdateTableStructureAtV1_9_3(dataBaseUpdater);
                dataBaseUpdater.DatabaseSchemaVersion = 7;
                dataBaseUpdater.Execute();
                TinyMoneyDataContext db = dataBaseUpdater.Context as TinyMoneyDataContext;
                LoadDefaultPeopleGroups(db);
                Repayment.UpdateOldData(db);
            }
            if (dataBaseUpdater.DatabaseSchemaVersion == 7)
            {
                PictureInfo.UpdateStructureAt_196(dataBaseUpdater);
                PeopleAssociationData.UpdateStructureAt_196(dataBaseUpdater);
                AccountItem.UpdateStructureAt1_9_6(dataBaseUpdater);
                dataBaseUpdater.DatabaseSchemaVersion = 8;
                dataBaseUpdater.Execute();
            }

            if (dataBaseUpdater.DatabaseSchemaVersion == 8)
            {
                Account.UpdateDataContext_At_v1972(dataBaseUpdater);
                dataBaseUpdater.DatabaseSchemaVersion = DatabaseVersion.LatestVersion;
                dataBaseUpdater.Execute();

                Repayment.UpdateTableStructureAtV1_9_8(dataBaseUpdater, ViewModels.BorrowLeanManager.BorrowLeanViewModel.EnsureStatus);
            }

        }

        public static bool LastTimeCrashedCauseOfDb
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue<bool>("LastTimeCrashedCauseOfDbKey", false);
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LastTimeCrashedCauseOfDbKey"] = value;
            }
        }
    }

}


