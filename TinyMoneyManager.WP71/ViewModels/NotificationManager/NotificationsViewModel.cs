
using mangoProgressIndicator;
using NkjSoft.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using TinyMoneyManager;
using TinyMoneyManager.Component;
using TinyMoneyManager.Controls;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;

namespace TinyMoneyManager.ViewModels.NotificationManager
{
    public class NotificationsViewModel : NkjSoftViewModelBase
    {
        public NotificationsViewModel()
        {
            this.Notifications = new ObservableCollection<TallySchedule>();
        }

        public void DeleteNotification(TallySchedule tag)
        {
        }

        /// <summary>
        /// Adds the notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        public void AddNotification(TallySchedule notification)
        {
            notification.ProfileRecordType = ScheduleRecordType.Notification;

            if (notification.IsActive.GetValueOrDefault())
            {
                AlarmManager.AddNotification(notification);
            }

            this.Notifications.Add(notification);

            this.AccountBookDataContext.TallyScheduleTable.InsertOnSubmit(notification);

            this.AccountBookDataContext.SubmitChanges();
        }

        /// <summary>
        /// Loads the data if not.
        /// </summary>
        public override void LoadDataIfNot()
        {
            if (!base.IsDataLoaded)
            {
                this.Notifications.Clear();
                (from p in this.AccountBookDataContext.TallyScheduleTable
                 where p.ProfileRecordType == ScheduleRecordType.Notification
                 select p).ToList<TallySchedule>().ForEach(new System.Action<TallySchedule>(this.Notifications.Add));
                base.IsDataLoaded = true;
            }
        }

        public ObservableCollection<TallySchedule> Notifications { get; set; }

        /// <summary>
        /// Updates the notification.
        /// </summary>
        /// <param name="notific">The notific.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void UpdateNotification(TallySchedule notification)
        {
            AlarmManager.AddNotification(notification);
            this.AccountBookDataContext.SubmitChanges();
        }

        /// <summary>
        /// Gets the notification by id.
        /// </summary>
        /// <param name="_id">The _id.</param>
        /// <returns></returns>
        internal TallySchedule GetNotificationById(Guid _id)
        {
            var item = AccountBookDataContext.TallyScheduleTable.Where(p => p.Id == _id && p.ProfileRecordType == ScheduleRecordType.Notification)
                .FirstOrDefault();
            return item;
        }

        internal void EnableNotification(TallySchedule item)
        {
            if (item != null)
            {
                item.IsActive = true;
                this.SubmitChanges();

                AlarmManager.AddNotification(item);
            }
        }

        internal void DisableNotification(TallySchedule notification)
        {
            if (notification != null)
            {
                notification.IsActive = false;
                this.SubmitChanges();
                AlarmManager.RemoveAlarmByName(notification.Id.ToString());
            }
        }
    }
}
