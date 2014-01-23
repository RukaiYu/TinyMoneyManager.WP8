namespace TinyMoneyManager.Data.ScheduleManager
{
    using NkjSoft.WPhone;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;

    public class TemplateManager
    {
        public const string LogFileName = "templates/tallyLog.xml";
        public const string TallyTemplates = "templates/tallyTemplates.xml";

        static TemplateManager()
        {
            Instance = new TemplateManager();
        }

        public TemplateManager()
        {
            this.TemplateTallyLog = new System.Collections.Generic.Dictionary<Guid, Boolean>();
        }

        public static void GenerateAllForOneDay(System.Collections.Generic.List<TemplateLogEntry> currentItems)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists("templates/tallyTemplates.xml"))
                {
                    System.IO.IsolatedStorage.IsolatedStorageFileStream xmlSourceFileStream = file.OpenFile("templates/tallyTemplates.xml", System.IO.FileMode.Open);
                    System.Collections.Generic.List<TallySchedule> list = null;
                    try
                    {
                        list = XmlHelper.DeserializeFromXmlString<System.Collections.Generic.List<TallySchedule>>(xmlSourceFileStream);
                    }
                    catch (System.Exception exception)
                    {
                        throw exception;
                    }
                    if (list != null)
                    {
                        System.Action<TemplateLogEntry> action = null;
                        System.DateTime date = System.DateTime.Now.Date;
                        System.Collections.Generic.List<TemplateLogEntry> entries = (from p in list
                                                                                     where TaskInfo.EnsureToRun(p, date)
                                                                                     select new TemplateLogEntry { Id = p.Id, IsTallied = false }).ToList<TemplateLogEntry>();
                        if (currentItems != null)
                        {
                            if (action == null)
                            {
                                action = delegate(TemplateLogEntry p)
                                {
                                    TemplateLogEntry entry = entries.FirstOrDefault<TemplateLogEntry>(x => x.Id == p.Id);
                                    if (entry != null)
                                    {
                                        entry.IsTallied = p.IsTallied;
                                    }
                                };
                            }
                            currentItems.ForEach(action);
                        }
                        SaveLogs(entries);
                    }
                }
            }
        }

        public bool IsTalliedToday(System.Guid id)
        {
            if (this.TemplateTallyLog.ContainsKey(id))
            {
                return this.TemplateTallyLog[id];
            }
            return false;
        }

        public void LoadLog()
        {
            System.Action<TemplateLogEntry> action = null;
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.FileExists("templates/tallyLog.xml"))
                {
                    this.SaveLogs();
                }
                if (file.FileExists("templates/tallyLog.xml"))
                {
                    System.Collections.Generic.List<TemplateLogEntry> list = null;
                    System.IO.IsolatedStorage.IsolatedStorageFileStream xmlSourceFileStream = file.OpenFile("templates/tallyLog.xml", System.IO.FileMode.Open);
                    try
                    {
                        list = XmlHelper.DeserializeFromXmlString<System.Collections.Generic.List<TemplateLogEntry>>(xmlSourceFileStream);
                    }
                    catch (System.Exception exception)
                    {
                        throw exception;
                    }
                    finally
                    {
                        if (xmlSourceFileStream != null)
                        {
                            xmlSourceFileStream.Dispose();
                        }
                    }
                    if (list != null)
                    {
                        this.TemplateTallyLog.Clear();
                        if (action == null)
                        {
                            action = delegate(TemplateLogEntry p)
                            {
                                this.TemplateTallyLog.Add(p.Id, p.IsTallied);
                            };
                        }
                        list.ForEach(action);
                    }
                }
            }
        }

        public void SaveFullTemplates(TinyMoneyDataContext db)
        {
            if (!this.HasSaveFull)
            {
                System.Collections.Generic.List<TallySchedule> source = (from p in db.TallyScheduleTable
                                                                         where p.ProfileRecordType == ScheduleRecordType.TempleteRecord
                                                                         select p).ToList<TallySchedule>();
                using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string directoryName = System.IO.Path.GetDirectoryName("templates/tallyTemplates.xml");
                    if (!file.DirectoryExists(directoryName))
                    {
                        file.CreateDirectory(directoryName);
                    }
                    System.IO.IsolatedStorage.IsolatedStorageFileStream fileWriteTo = null;
                    if (!file.FileExists("templates/tallyTemplates.xml"))
                    {
                        fileWriteTo = file.CreateFile("templates/tallyTemplates.xml");
                    }
                    else
                    {
                        fileWriteTo = file.OpenFile("templates/tallyTemplates.xml", System.IO.FileMode.Truncate);
                    }
                    using (fileWriteTo)
                    {
                        fileWriteTo.Position = 0;
                        XmlHelper.SerializeToXmlString<System.Collections.Generic.List<TallySchedule>>(source, fileWriteTo);
                    }
                }
                source.Clear();
                this.HasSaveFull = true;
            }
        }

        public void SaveLogs()
        {
            GenerateAllForOneDay((from p in this.TemplateTallyLog select new TemplateLogEntry { Id = p.Key, IsTallied = p.Value }).ToList<TemplateLogEntry>());
        }

        public static void SaveLogs(System.Collections.Generic.List<TemplateLogEntry> items)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                string directoryName = System.IO.Path.GetDirectoryName("templates/tallyLog.xml");
                if (!file.DirectoryExists(directoryName))
                {
                    file.CreateDirectory(directoryName);
                }
                System.IO.IsolatedStorage.IsolatedStorageFileStream fileWriteTo = null;
                if (file.FileExists("templates/tallyLog.xml"))
                {
                    fileWriteTo = file.OpenFile("templates/tallyLog.xml", System.IO.FileMode.Truncate);
                }
                else
                {
                    fileWriteTo = file.OpenFile("templates/tallyLog.xml", System.IO.FileMode.Create);
                }
                using (fileWriteTo)
                {
                    fileWriteTo.Position = 0;
                    XmlHelper.SerializeToXmlString(items, fileWriteTo);
                }
            }
        }

        public void Setup(TinyMoneyDataContext db)
        {
            this.SaveFullTemplates(db);
            GenerateAllForOneDay(null);
        }

        public void Update(System.Guid id, bool tallied)
        {
            this.TemplateTallyLog[id] = tallied;
        }

        public bool HasSaveFull { get; set; }

        public static TemplateManager Instance
        {
            get;
            set;
        }

        public System.Collections.Generic.Dictionary<Guid, Boolean> TemplateTallyLog { get; set; }
    }
}

