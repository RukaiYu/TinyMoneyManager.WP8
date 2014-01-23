namespace TinyMoneyManager.ViewModels.CustomizedTallyManager
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Data.ScheduleManager;

    public class CustomizedTallyViewModel : NkjSoftViewModelBase
    {
        private ObservableCollection<TallySchedule> _accountItemTemplates;
        private TinyMoneyManager.Data.Model.ScheduleType scheduleType;

        public CustomizedTallyViewModel()
        {
            this.AccountItemTemplates = new ObservableCollection<TallySchedule>();
            this.FavoritesList = new ObservableCollection<TallySchedule>();
            this.ScheduleType = TinyMoneyManager.Data.Model.ScheduleType.None;
            this.TemplateManager = TinyMoneyManager.Data.ScheduleManager.TemplateManager.Instance;
            TallySchedule.IsCompletedForTodayHandler = new System.Func<Guid, Boolean>(this.TemplateManager.IsTalliedToday);
            this.TemplateManager.LoadLog();
        }

        public void AddAccountItemTemplete(TallySchedule infoProvider)
        {
            infoProvider.ProfileRecordType = ScheduleRecordType.TempleteRecord;
            this.AccountBookDataContext.TallyScheduleTable.InsertOnSubmit(infoProvider);
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.TemplateManager.SaveFullTemplates(this.AccountBookDataContext);
                TinyMoneyManager.Data.ScheduleManager.TemplateManager.GenerateAllForOneDay(null);
            });


            this.SubmitChanges();
        }

        public override void Delete<T>(T obj)
        {
            base.Delete<T>(obj);
            this.AccountItemTemplates.Remove(obj as TallySchedule);
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.TemplateManager.SaveFullTemplates(this.AccountBookDataContext);
                TinyMoneyManager.Data.ScheduleManager.TemplateManager.GenerateAllForOneDay(null);
            });
        }

        public override void LoadData()
        {
            IOrderedQueryable<TallySchedule> source = from p in this.AccountBookDataContext.TallyScheduleTable
                                                      where p.ProfileRecordType == ScheduleRecordType.TempleteRecord
                                                      orderby p.RecordType
                                                      select p;
            System.Collections.Generic.List<TallySchedule> list = null;
            if (this.ScheduleType != TinyMoneyManager.Data.Model.ScheduleType.None)
            {
                list = (from p in source
                        where ((int)p.Frequency) == ((int)this.ScheduleType)
                        select p).ToList<TallySchedule>();
            }
            else
            {
                list = source.ToList<TallySchedule>();
            }
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.AccountItemTemplates = new ObservableCollection<TallySchedule>(list);
                this.IsDataLoaded = true;
            });
        }

        public void LoadItemsInStartupPage()
        {
            this.TemplateManager.LoadLog();
            System.Collections.Generic.List<Guid> todays = (from p in this.AccountBookDataContext.AccountItems
                                                            where p.CreateTime.Date == System.DateTime.Now.Date
                                                            select p.CategoryId).Distinct<System.Guid>().ToList<System.Guid>();
            System.Collections.Generic.List<Guid> ids = (from p in this.TemplateManager.TemplateTallyLog select p.Key).ToList<System.Guid>();
            System.Collections.Generic.List<TallySchedule> items = (from p in this.AccountBookDataContext.TallyScheduleTable
                                                                    where ((p.ProfileRecordType == ScheduleRecordType.TempleteRecord) && p.IsFavorite) && ids.Contains(p.Id)
                                                                    select p).ToList<TallySchedule>();
            items.ForEach(delegate(TallySchedule p)
            {
                bool flag = todays.Contains(p.CategoryId);
                this.TemplateManager.TemplateTallyLog[p.Id] = flag;
            });
            this.TemplateManager.SaveLogs();
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.FavoritesList.Clear();
                foreach (TallySchedule schedule in items)
                {
                    this.FavoritesList.Add(schedule);
                }
                this.HasLoadItemsInStarupPage = true;
            });
        }

        public void RemoveFromHomeScreen(TallySchedule item)
        {
            this.FavoritesList.Remove(item);
            item.IsFavorite = false;
            this.TemplateManager.TemplateTallyLog.Remove(item.Id);
            this.SubmitChanges();
        }

        public override void Update<T>(T instance)
        {
            base.Update<T>(instance);
            this.TemplateManager.SaveFullTemplates(this.AccountBookDataContext);
            TallySchedule tallySchedule = instance as TallySchedule;
            if (!TaskInfo.EnsureToRun(tallySchedule, System.DateTime.Now))
            {
                this.TemplateManager.TemplateTallyLog.Remove(tallySchedule.Id);
            }
            this.TemplateManager.SaveLogs();
        }

        public void UpdateToComplete(TallySchedule item)
        {
            this.TemplateManager.Update(item.Id, true);
            item.RaiseTallyCompleted();
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                this.TemplateManager.SaveLogs();
            });
        }

        public ObservableCollection<TallySchedule> AccountItemTemplates
        {
            get
            {
                return this._accountItemTemplates;
            }
            set
            {
                if (value != this._accountItemTemplates)
                {
                    this.OnNotifyPropertyChanging("AccountItemTemplates");
                    this._accountItemTemplates = value;
                    this.OnNotifyPropertyChanged("AccountItemTemplates");
                }
            }
        }

        public ObservableCollection<TallySchedule> FavoritesList { get; set; }

        public bool HasLoadItemsInStarupPage { get; set; }

        public TinyMoneyManager.Data.Model.ScheduleType ScheduleType
        {
            get
            {
                return this.scheduleType;
            }
            set
            {
                if (value != this.scheduleType)
                {
                    this.OnNotifyPropertyChanging("ScheduleType");
                    this.scheduleType = value;
                    this.OnNotifyPropertyChanged("ScheduleType");
                }
            }
        }

        public TinyMoneyManager.Data.ScheduleManager.TemplateManager TemplateManager { get; set; }
    }
}

