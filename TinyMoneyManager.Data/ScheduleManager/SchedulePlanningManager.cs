namespace TinyMoneyManager.Data.ScheduleManager
{
    using NkjSoft.Extensions;
    using NkjSoft.WPhone;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Linq;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using System.Windows;

    /// <summary>
    /// 
    /// </summary>
    public class SchedulePlanningManager : IDisposable
    {
        protected TinyMoneyDataContext dataContext;
        private int executedCount;
        public const string HandlingScheduleTaskLogFileName = "HandlingScheduleTaskErrors.log";
        public string PlanningInfoProviderFilePath = string.Format("{0}/{1}", "SchedulePlanning", "SchedulePlanningInfoProvider.xml");
        public const string PlanningInfoProviderFolder = "SchedulePlanning";
        public const string RecoveryDataErrorLogFileName = "recoveryDataError.log";
        protected System.Collections.Generic.Dictionary<RecordActionType, SchedulePlanningHandler> ScheduleHanlderCache;
        public const string SchedulePlanningFileName = "SchedulePlanningInfoProvider.xml";
        public const string SchedulePlanningResultDataFileNameFormatter = "{0}.SchedulePlanningResultData.xml";
        public string SchedulePlanningResultDataFolder = string.Format("{0}/PlanningInfoProviderFolder", "SchedulePlanning");

        private Dictionary<RecordActionType, List<IScheduledTaskItem>> _dataSavedToFileWhenExecuting;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulePlanningManager" /> class.
        /// </summary>
        /// <param name="db">The db.</param>
        public SchedulePlanningManager(TinyMoneyDataContext db)
        {
            this.dataContext = db;
            this.ScheduleHanlderCache = new System.Collections.Generic.Dictionary<RecordActionType, SchedulePlanningHandler>();
        }

        /// <summary>
        /// Adds the task.
        /// </summary>
        /// <param name="task">The task.</param>
        public virtual void AddTask(TallySchedule task)
        {
            try
            {
                XDocument providerInfo = this.Load(false);
                XElement element = providerInfo.Element("ScheduleTasks").Element("Tasks");
                System.Guid id = task.Id;
                XElement content = this.CreateScheduleDataInfoNode(task, id, null);
                element.Add(content);
                this.SaveBackPlannings(providerInfo);
            }
            catch (System.Exception exception)
            {
                this.SaveErrorsTo("HandlingScheduleTaskErrors.log", new string[] { exception.Message });
            }
        }

        /// <summary>
        /// Builds the common data node info.
        /// </summary>
        /// <param name="commonDataNode">The common data node.</param>
        protected virtual void BuildCommonDataNodeInfo(XElement commonDataNode)
        {
            commonDataNode.SetElementValue("TaskCompletedMessage", LocalizedObjectHelper.GetLocalizedStringFrom("TaskCompletedMessageFormatter"));
            commonDataNode.SetElementValue("TaskCompleteWithZeroMessage", LocalizedObjectHelper.GetLocalizedStringFrom("TaskCompleteWithZeroMessage"));
            commonDataNode.SetElementValue("DefaultCurrency", AppSetting.Instance.DefaultCurrency);
            commonDataNode.SetElementValue("AppName", AppSetting.Instance.AppName.IsNullOrEmpty() ? "ACCOUNTBOOK" : AppSetting.Instance.AppName);
            commonDataNode.SetElementValue("Weekends", AppSetting.Weekends.ToStringLine<string>(","));
        }

        /// <summary>
        /// Builds the tasks and schedule data info.
        /// </summary>
        /// <param name="taskNodeParentNode">The task node parent node.</param>
        /// <param name="scheduleDatasNode">The schedule datas node.</param>
        public virtual void BuildTasksAndScheduleDataInfo(XElement taskNodeParentNode, XElement scheduleDatasNode)
        {
            var tallySchedules = dataContext.TallyScheduleTable.Where(p => p.ProfileRecordType != ScheduleRecordType.TempleteRecord)
                .GroupBy(p => p.DayofWeek).SelectMany(p => p);

            foreach (var scheduleDataEntry in tallySchedules)
            {
                var scheduleDataInfoNode = CreateScheduleDataInfoNode(scheduleDataEntry, Guid.NewGuid());

                taskNodeParentNode.Add(scheduleDataInfoNode);
            }
        }

        /// <summary>
        /// Caches the common data.
        /// </summary>
        /// <param name="root">The root.</param>
        protected void cacheCommonData(XElement root)
        {
            XElement element = root.Element("CommonData");
            if (element != null)
            {
                try
                {
                    this.CommonDataInPlanning = element.Elements().ToDictionary<XElement, string, string>(p => p.Name.LocalName, p => p.Value);
                }
                catch (System.Exception)
                {
                }
            }
        }

        /// <summary>
        /// Calculates the plannings.
        /// </summary>
        /// <returns></returns>
        public virtual XDocument CalculatePlannings()
        {
            XDocument document = new XDocument
            {
                Declaration = new XDeclaration("1.0", "utf-8", "yes")
            };
            XElement content = new XElement("ScheduleTasks");
            content.SetAttributeValue("Language", AppSetting.Instance.DisplayLanguage);
            content.SetAttributeValue("Enabled", true);
            document.AddFirst(content);
            XElement element2 = new XElement("Tasks");
            content.Add(element2);
            this.BuildTasksAndScheduleDataInfo(element2, null);
            XElement commonDataNode = new XElement("CommonData");
            this.BuildCommonDataNodeInfo(commonDataNode);
            content.Add(commonDataNode);
            return document;
        }

        /// <summary>
        /// Checks all executed.
        /// </summary>
        /// <returns></returns>
        public bool CheckAllExecuted()
        {
            XDocument document = XDocument.Load(this.PlanningInfoProviderFilePath);
            if (document == null)
            {
                return true;
            }
            XElement element = document.Element("ScheduleTasks");
            if (element == null)
            {
                return true;
            }
            XAttribute attribute = element.Attribute("HasAllExecuted");
            return ((attribute == null) || attribute.Value.ToBoolean(true));
        }

        /// <summary>
        /// Checks the schedule task avaliable.
        /// </summary>
        /// <returns></returns>
        public bool CheckScheduleTaskAvaliable()
        {
            bool flag = false;
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.FileExists(this.PlanningInfoProviderFilePath))
                {
                    return flag;
                }
                XDocument document = this.Load(false);
                if (document == null)
                {
                    return false;
                }
                return document.Element("ScheduleTasks").TryGet<string, XAttribute>(p => p.Attribute("Enabled"), p => p.Value).ToBoolean(false);
            }
        }

        /// <summary>
        /// Creates the schedule data info node.
        /// </summary>
        /// <param name="scheduleDataEntry">The schedule data entry.</param>
        /// <param name="taskId">The task id.</param>
        /// <param name="scheduleDataNode">The schedule data node.</param>
        /// <returns></returns>
        protected virtual XElement CreateScheduleDataInfoNode(TallySchedule scheduleDataEntry, System.Guid taskId, XElement scheduleDataNode = null)
        {
            scheduleDataNode = scheduleDataNode ?? new XElement("ScheduleData");
            scheduleDataNode.SetAttributeValue("taskId", taskId);
            scheduleDataNode.SetAttributeValue("autoTokenId", scheduleDataEntry.Id);
            SchedulePlanningHandler handler = GetHandler(scheduleDataEntry.ActionType);

            handler.ParseScheduleDataToXmlNode(scheduleDataNode, scheduleDataEntry);
            return scheduleDataNode;
        }

        /// <summary>
        /// Deletes the task.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public virtual void DeleteTask(TallySchedule obj)
        {
            System.Func<XElement, Boolean> predicate = null;
            try
            {
                XDocument providerInfo = this.Load(false);
                if (predicate == null)
                {
                    predicate = p => p.Attribute("autoTokenId").Value.ToGuid() == obj.Id;
                }
                XElement element = providerInfo.Element("ScheduleTasks").Element("Tasks").Descendants("ScheduleData").Where<XElement>(predicate).FirstOrDefault<XElement>();
                if (element != null)
                {
                    element.Remove();
                    this.SaveBackPlannings(providerInfo);
                }
            }
            catch (System.Exception exception)
            {
                this.SaveErrorsTo("HandlingScheduleTaskErrors.log", new string[] { exception.Message });
            }
        }

        /// <summary>
        /// Executes the one plan.
        /// </summary>
        /// <param name="planToExecute">The plan to execute.</param>
        /// <returns></returns>
        public virtual bool ExecuteOnePlan(TallySchedule planToExecute)
        {
            TallySchedule scheduleData = planToExecute;
            if (scheduleData == null)
            {
                return false;
            }
            SchedulePlanningHandler handler = null;
            this.ScheduleHanlderCache.TryGetValue(planToExecute.ActionType, out handler);
            if (handler == null)
            {
                handler = SchedulePlanningHandler.CreatePlanningHandler(planToExecute.ActionType, this.dataContext);
                this.ScheduleHanlderCache[planToExecute.ActionType] = handler;
            }
            return handler.ProcessingExecute(scheduleData);
        }

        /// <summary>
        /// Executes the plan.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns></returns>
        public virtual XDocument ExecutePlan(XDocument dataProvider = null)
        {
            resetStatus();

            System.DateTime date = System.DateTime.Now.Date;
            this.executedCount = 0;
            this.TotalTasks = 0;
            XDocument providerInfo = dataProvider ?? this.Load(false);
            if (providerInfo == null)
            {
                return dataProvider;
            }
            try
            {
                XDocument document2 = providerInfo;
                if (document2 == null)
                {
                    return dataProvider;
                }
                XElement root = document2.Element("ScheduleTasks");
                if (root == null)
                {
                    return dataProvider;
                }
                this.cacheCommonData(root);
                root.Attribute("HasAllExecuted");
                string str = root.TryGet<string, XAttribute>(p => p.Attribute("LastExecutingDate"), p => p.Value);
                System.DateTime executingDate = date;
                if (!string.IsNullOrEmpty(str))
                {
                    executingDate = str.ToDateTime(new System.DateTime?(date));
                }

                System.Collections.Generic.IEnumerable<XElement> tasks = root.Element("Tasks").Elements("ScheduleData");

                this.executePlanForDate(date, root, tasks, null);
                while ((executingDate.Date < date.Date) && ((date.Date.Day - executingDate.Date.Day) > 1))
                {
                    executingDate = executingDate.AddDays(1.0);
                    this.executePlanForDate(executingDate, root, tasks, null);
                }

                root.SetAttributeValue("HasAllExecuted", this.TotalTasks == this.executedCount);
                root.SetAttributeValue("LastExecutingDate", date.Date);
            }
            catch (System.Exception exception)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    throw exception;
                }

                this.SaveErrorsTo("HandlingScheduleTaskErrors.log", new string[] { exception.Message });
            }
            this.SaveBackPlannings(providerInfo);
            return providerInfo;
        }

        /// <summary>
        /// Executes the plan for date.
        /// </summary>
        /// <param name="executingDate">The executing date.</param>
        /// <param name="scheduleTaksRoot">The schedule taks root.</param>
        /// <param name="tasks">The tasks.</param>
        /// <param name="scheduleDatasNode">The schedule datas node.</param>
        protected virtual void executePlanForDate(System.DateTime executingDate, XElement scheduleTaksRoot, System.Collections.Generic.IEnumerable<XElement> tasks, System.Collections.Generic.IEnumerable<XElement> scheduleDatasNode)
        {
            foreach (XElement element in tasks)
            {
                TaskInfo info = this.ParseTaskInfo(element);

                var actionHandlerType = info.TaskType;

                SchedulePlanningHandler expenseOrIncomeDataHandler = GetHandler(actionHandlerType);

                if ((info != null) && info.EnsureToRun(executingDate))
                {
                    XElement element2 = element;
                    string text1 = element2.Attribute("action").Value;
                    System.DateTime time = element2.TryGet<string, XAttribute>(p => p.Attribute("lastExecutedDate"), p => p.Value).ToDateTime(new System.DateTime?(executingDate));
                    bool flag = element2.TryGet<string, XAttribute>(p => p.Attribute("hasExecuted"), p => p.Value).ToBoolean(false);
                    if (flag && (time.Date < executingDate.Date))
                    {
                        flag = false;
                    }
                    if (!flag)
                    {
                        element2.SetAttributeValue("lastExecutedDate", executingDate.Date);
                        if (expenseOrIncomeDataHandler.ParseScheduleDataToXmlNode(element2))
                        {
                            element2.SetAttributeValue("hasExecuted", true);
                            this.executedCount++;
                        }
                    }
                    this.TotalTasks++;
                }
            }
            if (this.executedCount > 0)
            {
                if (this.SaveRecordsToISO(executingDate.ToString("yyyyMMddHHmmss"), (ex) =>
                        this.SaveErrorsTo("recoveryDataError.log", new string[] { ex.Message })
                     ))
                {
                    resetStatus();
                }
            }
        }

        /// <summary>
        /// Resets the status.
        /// </summary>
        private void resetStatus()
        {
            if (_dataSavedToFileWhenExecuting == null)
            {
                _dataSavedToFileWhenExecuting = new Dictionary<RecordActionType, List<IScheduledTaskItem>>();

                _dataSavedToFileWhenExecuting.Add(RecordActionType.CreateTranscationRecord, new List<IScheduledTaskItem>());
                _dataSavedToFileWhenExecuting.Add(RecordActionType.CreateTransferingRecord, new List<IScheduledTaskItem>());
                return;
            }

            if (this._dataSavedToFileWhenExecuting != null)
            {
                this._dataSavedToFileWhenExecuting[RecordActionType.CreateTranscationRecord].Clear();
                this._dataSavedToFileWhenExecuting[RecordActionType.CreateTransferingRecord].Clear();
            }
        }

        /// <summary>
        /// Executings the planning for DB.
        /// </summary>
        /// <returns></returns>
        public virtual int ExecutingPlanningForDB()
        {
            return 0;
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public SchedulePlanningHandler GetHandler(string action)
        {
            return this.GetHandler((RecordActionType)System.Enum.Parse(typeof(SchedulePlanningHandler), action, false));
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="recordActionType">Type of the record action.</param>
        /// <returns></returns>
        protected virtual SchedulePlanningHandler GetHandler(RecordActionType recordActionType)
        {
            SchedulePlanningHandler handler = null;
            this.ScheduleHanlderCache.TryGetValue(recordActionType, out handler);
            if (handler == null)
            {
                handler = SchedulePlanningHandler.CreatePlanningHandler(recordActionType, this.dataContext);
                this.ScheduleHanlderCache[recordActionType] = handler;
            }

            if (handler.HandlerDataParsed == null)
            {
                handler.HandlerDataParsed = this.HandlerDataParsed;
            }

            return handler;
        }

        /// <summary>
        /// Gets the scheduled data files.
        /// </summary>
        /// <returns></returns>
        public string[] GetScheduledDataFiles()
        {
            string[] strArray = new string[0];
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists("SchedulePlanning"))
                {
                    file.CreateDirectory("SchedulePlanning");
                }
                if (!file.DirectoryExists(this.SchedulePlanningResultDataFolder))
                {
                    file.CreateDirectory(this.SchedulePlanningResultDataFolder);
                }
                return file.GetFileNames(System.IO.Path.Combine(this.SchedulePlanningResultDataFolder, "*.*"));
            }
        }

        /// <summary>
        /// Handlers the data parsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        protected void HandlerDataParsed(object sender, IScheduledTaskItem data)
        {
            SchedulePlanningHandler handler = sender as SchedulePlanningHandler;
            if ((handler.HandlerType == RecordActionType.CreateIncomeRecord)
                || (handler.HandlerType == RecordActionType.CrateExpenseRecord)
                || handler.HandlerType == RecordActionType.CreateTranscationRecord)
            {
                this._dataSavedToFileWhenExecuting[RecordActionType.CreateTranscationRecord].Add(data);
                // this.AccountItemsToAdd.Add(data as AccountItem);
            }
            else if (handler.HandlerType == RecordActionType.CreateTransferingRecord)
            {
                this._dataSavedToFileWhenExecuting[RecordActionType.CreateTransferingRecord].Add(data);
            }
        }

        /// <summary>
        /// Loads the specified load from data base if empty.
        /// </summary>
        /// <param name="loadFromDataBaseIfEmpty">if set to <c>true</c> [load from data base if empty].</param>
        /// <returns></returns>
        public XDocument Load(bool loadFromDataBaseIfEmpty = false)
        {
            XDocument document = null;
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists("SchedulePlanning"))
                {
                    file.CreateDirectory("SchedulePlanning");
                }
                if (!file.DirectoryExists(this.SchedulePlanningResultDataFolder))
                {
                    file.CreateDirectory(this.SchedulePlanningResultDataFolder);
                }
                if (!file.FileExists(this.PlanningInfoProviderFilePath))
                {
                    if (loadFromDataBaseIfEmpty)
                    {
                        this.CalculatePlannings();
                    }
                    return document;
                }
                System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(this.PlanningInfoProviderFilePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    try
                    {
                        try
                        {
                            string text = reader.ReadToEnd();
                            if (((text.Length > 2) && (text[text.Length - 2] == '>')) && (text[text.Length - 1] == '>'))
                            {
                                text = text.Remove(text.Length - 1, 1);
                            }
                            document = XDocument.Parse(text);
                        }
                        catch (System.Exception exception)
                        {
                            document = null;
                            System.Console.WriteLine(exception);
                        }
                        return document;
                    }
                    finally
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                    return document;
                }
            }
        }

        /// <summary>
        /// Parses the task info.
        /// </summary>
        /// <param name="taskInfoNode">The task info node.</param>
        /// <returns></returns>
        protected TaskInfo ParseTaskInfo(XElement taskInfoNode)
        {
            TaskInfo info = new TaskInfo();
            try
            {
                info.Id = taskInfoNode.Attribute("taskId").Value.ToGuid();
                info.Frequency = taskInfoNode.Attribute("frequency").Value;
                info.Date = taskInfoNode.Attribute("date").Value.ToInt32();
                info.TaskType = taskInfoNode.Attribute("action").Value.ToEnum<RecordActionType>();
                info.DayOfWeek = taskInfoNode.Attribute("dayOfWeek").Value;
                info.Active = taskInfoNode.TryGet<string, XAttribute>(p => p.Attribute("active"), p => p.Value).ToBoolean(false);
            }
            catch (System.Exception)
            {
                info = null;
            }
            return info;
        }

        public int RecoverDataToDatabase(System.Collections.Generic.IEnumerable<String> files, System.Action<AccountItem, TallySchedule> recoverDataHandler, Action<TransferingItem> transferingItemRecover = null)
        {
            System.Collections.Generic.List<String> errorExceptionInfosource = new System.Collections.Generic.List<String>();

            var transcationData = new Dictionary<string, List<AccountItem>>();
            var transferingData = new Dictionary<string, List<TransferingItem>>();

            ExpenseOrIncomeScheduleHanlder.DataRecoverProcessor = recoverDataHandler;

            TransferingItemTaskHandler.DataRecoverProcessor = transferingItemRecover;

            int num = 0;
            int recoredRecoveredCount = 0;
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (string str in files)
                {
                    string path = System.IO.Path.Combine(this.SchedulePlanningResultDataFolder, str).Replace(@"\", "/");
                    if (file.FileExists(path))
                    {
                        using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(path, System.IO.FileMode.Open))
                        {
                            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.Unicode))
                            {
                                try
                                {
                                    var data = reader.ReadToEnd();

                                    var fileName = Path.GetFileNameWithoutExtension(path);

                                    var fileType = fileName.Substring(0, fileName.IndexOf("."));

                                    var count = 0;

                                    switch (fileType)
                                    {
                                        case "CreateTransferingRecord":
                                            var items = XmlHelper.DeserializeFromXmlString<List<TransferingItem>>(data);
                                            transferingData.Add(path, items);
                                            count = items.Count;
                                            break;
                                        default:
                                        case "CreateTranscationRecord":
                                            var itemsOfTemp = XmlHelper.DeserializeFromXmlString<List<AccountItem>>(data);
                                            transcationData.Add(path, itemsOfTemp);
                                            count = itemsOfTemp.Count;
                                            break;
                                    }

                                    recoredRecoveredCount += count;
                                }
                                catch (System.Exception exception)
                                {
                                    errorExceptionInfosource.Add(exception.ToString());
                                }
                            }
                            continue;
                        }
                    }
                }

                num = recoredRecoveredCount;

                if (recoredRecoveredCount == 0)
                {
                    num = -1;
                }

                System.Collections.Generic.List<Guid> accountIds = (from p in this.dataContext.Accounts select p.Id).ToList<System.Guid>();
                System.Collections.Generic.List<Guid> categoryIds = (from p in this.dataContext.Categories select p.Id).ToList<System.Guid>();

                Action<Action> safeRun = (a) =>
                {
                    try
                    {
                        a();
                    }
                    catch (System.Exception exception3)
                    {
                        this.SaveErrorsTo("recoveryDataError.log", "\r\nDoing Recovery error occured:\r\n" + exception3.ToString());
                    }
                };

                // recover items to database.
                safeRun(() => recoverAccountItems(file, transcationData, errorExceptionInfosource, accountIds, categoryIds));

                safeRun(() => recoverTransferingItems(file, transferingData, errorExceptionInfosource, accountIds));

                if (errorExceptionInfosource.Count > 0)
                {
                    this.SaveErrorsTo("recoveryDataError.log", errorExceptionInfosource.ToArray());
                }
            }

            return num;
        }

        private void recoverTransferingItems(IsolatedStorageFile isolatedStorage, Dictionary<string, List<TransferingItem>> transferingItems, List<string> fileMessages, List<Guid> accountIds)
        {
            var sum = 0;
            foreach (var pair in transferingItems)
            {
                var file = pair.Key;
                int num2 = 0;
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (var item in pair.Value)
                        {
                            if ((accountIds.Count(p => p == item.FromAccountId) > 0)
                                && (accountIds.Count(p => p == item.ToAccountId) > 0))
                            {
                                sum++;
                                TransferingItemTaskHandler.RecoverItemProcessor(item);
                            }
                            else
                            {
                                fileMessages.Add("Failed to add item: {0}, cause of account missed.".FormatWith(new object[] { item.Id }));
                            }
                        }
                    });
                }
                catch (System.Exception exception2)
                {
                    num2++;
                    string destinationFileName = pair.Key.Replace(".xml", ".err.xml");
                    fileMessages.Add("Failed to add items in file: {0}, cause of account or category mismatched. Have Renamed data file to {1}. details:\r\n{2}".FormatWith(new object[] { pair.Key, destinationFileName, exception2.Message }));
                    isolatedStorage.MoveFile(file, destinationFileName);
                }

                if (num2 > 0)
                {
                    this.SaveErrorsTo("recoveryDataError.log", new string[] { "Some items can't add to database.\r\n" + fileMessages.ToStringLine<string>("\r\n") });
                }
                else
                {
                    isolatedStorage.DeleteFile(file);
                }
            }

            this.dataContext.SubmitChanges();
        }

        private void recoverAccountItems(IsolatedStorageFile isolatedStorage, Dictionary<string, List<AccountItem>> transactionItems, List<string> fileMessages, List<Guid> accountIds, List<Guid> categoryIds = null)
        {
            var sum = 0;
            foreach (var pair in transactionItems)
            {
                var file = pair.Key;
                int num2 = 0;
                try
                {
                    var allTasks = this.dataContext.TallyScheduleTable.Where(p => p.ProfileRecordType == ScheduleRecordType.ScheduledRecord)
                        .ToList();

                    var taskId = Guid.Empty;

                    TallySchedule taskInfo = allTasks.Count > 0 ? allTasks[0] : null;

                    foreach (var item in pair.Value)
                    {
                        if (accountIds.Count(p => p == item.AccountId) > 0 && categoryIds.Count(p => p == item.CategoryId) > 0)
                        {
                            if (taskInfo != null && taskInfo.Id != item.AutoTokenId)
                            {
                                taskInfo = allTasks.FirstOrDefault(p => p.Id == item
                                    .AutoTokenId);
                            }

                            ExpenseOrIncomeScheduleHanlder.RecoverItemProcessor(item, taskInfo);
                            sum++;
                        }
                        else
                        {
                            fileMessages.Add("Failed to add item: {0}, cause of account or category mismatched.".FormatWith(new object[] { item.Id }));
                        }
                    }
                }
                catch (System.Exception exception2)
                {
                    num2++;
                    string destinationFileName = pair.Key.Replace(".xml", ".err.xml");
                    fileMessages.Add("Failed to add items in file: {0}, cause of account or category mismatched. Have Renamed data file to {1}. details:\r\n{2}".FormatWith(new object[] { pair.Key, destinationFileName, exception2.Message }));
                    isolatedStorage.MoveFile(file, destinationFileName);
                }

                if (num2 > 0)
                {
                    this.SaveErrorsTo("recoveryDataError.log", new string[] { "Some items can't add to database.\r\n" + fileMessages.ToStringLine<string>("\r\n") });
                }
                else
                {
                    isolatedStorage.DeleteFile(file);
                }
            }

            this.dataContext.SubmitChanges();
        }

        /// <summary>
        /// Saves the specified planning data.
        /// </summary>
        /// <param name="planningData">The planning data.</param>
        public void Save(XDocument planningData)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists("SchedulePlanning"))
                {
                    file.CreateDirectory("SchedulePlanning");
                }
                if (!file.DirectoryExists(this.SchedulePlanningResultDataFolder))
                {
                    file.CreateDirectory(this.SchedulePlanningResultDataFolder);
                }
                if (!file.FileExists(this.PlanningInfoProviderFilePath))
                {
                    file.CreateFile(this.PlanningInfoProviderFilePath).Close();
                }
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(this.PlanningInfoProviderFilePath, System.IO.FileMode.Truncate, System.IO.FileAccess.ReadWrite))
                {
                    stream.Position = 0;
                    planningData.Save(stream);
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        /// Saves the back plannings.
        /// </summary>
        /// <param name="providerInfo">The provider info.</param>
        public void SaveBackPlannings(XDocument providerInfo)
        {
            this.Save(providerInfo);
        }

        /// <summary>
        /// Saves the data to folder.
        /// </summary>
        /// <param name="datas">The datas.</param>
        /// <param name="namePrefix">The name prefix.</param>
        protected void SaveDataToFolder(string datas, string namePrefix)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists("SchedulePlanning"))
                {
                    file.CreateDirectory("SchedulePlanning");
                }

                if (!file.DirectoryExists(this.SchedulePlanningResultDataFolder))
                {
                    file.CreateDirectory(this.SchedulePlanningResultDataFolder);
                }
                string path = System.IO.Path.Combine(this.SchedulePlanningResultDataFolder, "{0}.SchedulePlanningResultData.xml".FormatWith(new object[] { namePrefix }));
                if (!file.FileExists(path))
                {
                    file.CreateFile(path).Close();
                }
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(path, System.IO.FileMode.Truncate, System.IO.FileAccess.ReadWrite))
                {
                    stream.Position = 0;
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, System.Text.Encoding.Unicode))
                    {
                        writer.Write(datas);
                    }
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// Saves the errors to.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="errorlines">The errorlines.</param>
        protected void SaveErrorsTo(string fileName, params string[] errorlines)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                string path = System.IO.Path.Combine("SchedulePlanning", fileName);
                System.IO.IsolatedStorage.IsolatedStorageFileStream stream = null;
                if (!file.FileExists(path))
                {
                    stream = file.CreateFile(path);
                }
                else
                {
                    stream = file.OpenFile(path, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                }
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
                {
                    foreach (string str2 in errorlines)
                    {
                        writer.WriteLine(string.Format("{0}\t{1}", System.DateTime.Now, str2));
                    }
                }
                stream.Close();
                stream.Dispose();
            }
        }

        /// <summary>
        /// Saves the records to ISO.
        /// </summary>
        /// <param name="nameForDatFile">The name for dat file.</param>
        /// <param name="failedToSave">The failed to save.</param>
        /// <returns></returns>
        public bool SaveRecordsToISO(string nameForDatFile, System.Action<Exception> failedToSave)
        {
            try
            {
                foreach (var itemType in _dataSavedToFileWhenExecuting)
                {
                    if (itemType.Value.Count > 0)
                    {
                        string datas = string.Empty;

                        switch (itemType.Key)
                        {
                            case RecordActionType.CrateExpenseRecord:
                            case RecordActionType.CreateIncomeRecord:
                            case RecordActionType.CreateTranscationRecord:
                                datas = XmlHelper.SerializeToXmlString(itemType.Value.Cast<AccountItem>().ToList());
                                break;
                            case RecordActionType.CreateTransferingRecord:
                                datas = XmlHelper.SerializeToXmlString(itemType.Value.Cast<TransferingItem>().ToList());
                                break;
                            case RecordActionType.CreateBorrowRecord:
                            case RecordActionType.CreateLeanRecord:
                                break;
                            case RecordActionType.BackupDataBase:
                                break;
                            default:
                                break;
                        }

                        var fileName = "{0}.{1}".FormatWith(itemType.Key, nameForDatFile);

                        this.SaveDataToFolder(datas, fileName);
                    }
                }

                return true;
            }
            catch (System.Exception exception)
            {
                failedToSave(exception);
                return false;
            }
        }

        /// <summary>
        /// Setups the task info provider file.
        /// </summary>
        public void SetupTaskInfoProviderFile()
        {
            XDocument providerInfo = this.CalculatePlannings();
            this.SaveBackPlannings(providerInfo);
        }

        /// <summary>
        /// Updates the specified force to update.
        /// </summary>
        /// <param name="forceToUpdate">if set to <c>true</c> [force to update].</param>
        public void Update(bool forceToUpdate = false)
        {
            if (forceToUpdate || this.NeedToUpdatePlan)
            {
                XDocument planningData = this.CalculatePlannings();
                this.Save(planningData);
            }
        }

        /// <summary>
        /// Updates the common data node info.
        /// </summary>
        /// <param name="scheduleTasksXdoc">The schedule tasks xdoc.</param>
        public virtual void UpdateCommonDataNodeInfo(XDocument scheduleTasksXdoc)
        {
            XElement commonDataNode = scheduleTasksXdoc.Root.Element("CommonData");
            if (commonDataNode == null)
            {
                commonDataNode = new XElement("CommonData");
            }
            this.BuildCommonDataNodeInfo(commonDataNode);
        }

        /// <summary>
        /// Updates the task info.
        /// </summary>
        /// <param name="task">The task.</param>
        public virtual void UpdateTaskInfo(TallySchedule task)
        {
            System.Func<XElement, Boolean> predicate = null;
            try
            {
                XDocument providerInfo = this.Load(false);
                if (predicate == null)
                {
                    predicate = p => p.TryGet<string, XAttribute>(x => x.Attribute("autoTokenId"), x => x.Value).ToGuid() == task.Id;
                }
                XElement element = providerInfo.Element("ScheduleTasks").Element("Tasks").Descendants("ScheduleData").Where<XElement>(predicate).FirstOrDefault<XElement>();
                if (element != null)
                {
                    System.Guid taskId = element.TryGet<string, XAttribute>(p => p.Attribute("taskId"), p => p.Value).ToGuid();

                    this.CreateScheduleDataInfoNode(task, taskId, element);
                    this.SaveBackPlannings(providerInfo);
                }
            }
            catch (System.Exception exception)
            {
                this.SaveErrorsTo("HandlingScheduleTaskErrors.log", new string[] { exception.Message });
            }
        }

        /// <summary>
        /// Updates the task info node.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="taskInfoNode">The task info node.</param>
        /// <returns></returns>
        public virtual XElement UpdateTaskInfoNode(TallySchedule task, XElement taskInfoNode = null)
        {
            taskInfoNode = taskInfoNode ?? new XElement("Task");
            System.Guid id = task.Id;
            taskInfoNode.SetAttributeValue("frequency", task.Frequency);
            taskInfoNode.SetAttributeValue("dayOfWeek", task.DayofWeek);
            taskInfoNode.SetAttributeValue("date", task.StartDate);
            return taskInfoNode;
        }

        /// <summary>
        /// Gets or sets the common data in planning.
        /// </summary>
        /// <value>
        /// The common data in planning.
        /// </value>
        public System.Collections.Generic.Dictionary<string, string> CommonDataInPlanning { get; set; }

        /// <summary>
        /// Gets the executed count.
        /// </summary>
        /// <value>
        /// The executed count.
        /// </value>
        public int ExecutedCount
        {
            get
            {
                return this.executedCount;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [need to update plan].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [need to update plan]; otherwise, <c>false</c>.
        /// </value>
        public bool NeedToUpdatePlan { get; set; }

        /// <summary>
        /// Gets a value indicating whether [schedule task avaliable].
        /// </summary>
        /// <value>
        /// <c>true</c> if [schedule task avaliable]; otherwise, <c>false</c>.
        /// </value>
        public bool ScheduleTaskAvaliable
        {
            get
            {
                this.CheckScheduleTaskAvaliable();
                return false;
            }
        }

        /// <summary>
        /// Gets the total tasks.
        /// </summary>
        /// <value>
        /// The total tasks.
        /// </value>
        public int TotalTasks { get; private set; }

        /// <summary>
        /// Saves the transaction records via schedule planning component.
        /// </summary>
        /// <param name="accountItem">The account item.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool SaveTransactionRecords(params AccountItem[] accountItem)
        {
            var handler = new ExpenseOrIncomeScheduleHanlder(this.dataContext);

            resetStatus();

            foreach (var item in accountItem)
            {
                HandlerDataParsed(handler, item);
            }

            var success = true;
            SaveRecordsToISO(DateTime.Now.ToString("yyyyMMddHHmmss"), (ex) =>
            {
                success = false;
            });

            resetStatus();

            return success;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (dataContext != null)
            {
                dataContext.Dispose();
                _dataSavedToFileWhenExecuting = null;
            }
        }
    }
}

