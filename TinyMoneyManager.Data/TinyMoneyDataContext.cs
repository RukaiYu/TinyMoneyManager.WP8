namespace TinyMoneyManager.Data
{
    using System;
    using System.Data.Linq;
    using TinyMoneyManager.Data.Model;

    public class TinyMoneyDataContext : DataContext
    {
        public Table<AccountItem> AccountItems;
        public Table<Account> Accounts;
        public Table<BudgetItem> BudgetItems;
        public Table<BudgetMonthlyReport> BudgetMonthlyReports;
        public Table<BudgetProject> BudgetProjects;
        public Table<Category> Categories;
        public const string DBConnectionString = (DbConnectionStringDataSourcePath + DbFileName);
        public const string DbConnectionStringDataSourcePath = "Data Source=isostore:/";
        public const string DbFileName = "TinyAccountBook.sdf";
        public Table<PeopleAssociationData> PeopleAssociationDatas;
        public Table<PeopleGroup> PeopleGroups;
        public Table<PeopleProfile> Peoples;
        public Table<PictureInfo> PictureInfos;
        public Table<Repayment> Repayments;
        public Table<TinyMoneyManager.Data.Model.SchedulePlanningTable> SchedulePlanningTable;
        public Table<TallySchedule> TallyScheduleTable;
        public Table<TransferingItem> TransferingItems;

        public TinyMoneyDataContext()
            : this(DBConnectionString)
        {
        }

        public TinyMoneyDataContext(string connectionString)
            : base(connectionString)
        {
        }

        public static TinyMoneyDataContext CreateDbByFile(string fileName)
        {
            return new TinyMoneyDataContext(DbConnectionStringDataSourcePath + fileName);
        }
    }
}

