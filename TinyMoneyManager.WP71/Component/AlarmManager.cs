namespace TinyMoneyManager.Component
{
    using Microsoft.Phone.Scheduler;
    using NkjSoft.Extensions;
    using System;
    using System.Linq;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class AlarmManager
    {
        public static void AddAlarm(Alarm alarm)
        {
            try
            {
                ScheduledActionService.Add(alarm);
            }
            catch (System.Exception)
            {
            }
        }

        public static void AddReminder(Reminder reminder)
        {
            try
            {
                ScheduledActionService.Add(reminder);
            }
            catch (System.Exception)
            {
            }
        }

        public static void AddRepaymentAlarmNotification(Repayment repayment, bool useAlarm)
        {
            string name = repayment.Id.ToString();
            if (ScheduledActionService.GetActions<ScheduledNotification>().FirstOrDefault<ScheduledNotification>(p => (p.Name == name)) != null)
            {
                try
                {
                    ScheduledActionService.Remove(name);
                }
                catch (System.Exception)
                {
                }
            }
            ScheduledNotification action = null;
            if (useAlarm)
            {
                Alarm alarm = new Alarm(name)
                {
                    Sound = LocalizedStrings.GetLocailizedResourceUriFrom("/Resources/sounds/RepaymentNotification/RepaymentArrive.{0}.mp3", new object[0])
                };
                action = alarm;
            }
            else
            {
                Reminder reminder = new Reminder(name)
                {
                    NavigationUri = new Uri("/Pages/RepaymentManagerViews/RepaymentItemEditor.xaml?action={0}&itemId={1}".FormatWith(new object[] { PageActionType.Edit, repayment.Id }), UriKind.RelativeOrAbsolute),
                    Title = LocalizedStrings.GetLanguageInfoByKey("RepaymentNotificationList")
                };
                action = reminder;
            }
            action.RecurrenceType = WarpFrequency(repayment.Frequency);
            action.BeginTime = repayment.RepayAt;
            action.ExpirationTime = repayment.DueDate;
            action.Content = repayment.GetContentUsedForNotification();
            try
            {
                ScheduledActionService.Add(action);
            }
            catch (System.Exception)
            {
            }
        }

        /// <summary>
        /// Adds the repayment alarm notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        public static void AddNotification(TallySchedule notification)
        {
            string name = notification.Id.ToString();
            if (ScheduledActionService.GetActions<ScheduledNotification>().FirstOrDefault<ScheduledNotification>(p => (p.Name == name)) != null)
            {
                try
                {
                    ScheduledActionService.Remove(name);
                }
                catch (System.Exception)
                {
                }
            }

            ScheduledNotification action = null;

            Reminder reminder = new Reminder(name)
            {
                NavigationUri = new Uri("/Pages/NotificationCenter/NotificationDetailsViewer.xaml?id={0}".FormatWith(new object[] { notification.Id }), UriKind.RelativeOrAbsolute),
                Title = AppResources.Notification.ToLower(),
            };

            action = reminder;

            action.RecurrenceType = notification.RecurrenceInterval.GetValueOrDefault().ToEnum<RecurrenceInterval>();
            action.BeginTime = notification.StartTime;
            action.ExpirationTime = notification.EndTime;
            action.Content = notification.Name;
            try
            {
                ScheduledActionService.Add(action);
            }
            catch (System.Exception)
            {
            }
        }

        private static RecurrenceInterval WarpFrequency(ScheduleType? scheduleType)
        {
            var freq = scheduleType.GetValueOrDefault();
            var result = RecurrenceInterval.None;
            switch (freq)
            {
                case ScheduleType.None:
                    break;
                case ScheduleType.EveryDay:
                    result = RecurrenceInterval.Daily;
                    break;
                case ScheduleType.EveryWeek:
                    result = RecurrenceInterval.Weekly;
                    break;
                case ScheduleType.EveryMonth:
                    result = RecurrenceInterval.Monthly;
                    break;
                case ScheduleType.EveryYear:
                    result = RecurrenceInterval.Yearly;
                    break;
                case ScheduleType.EveryOtherDay:
                    break;
                case ScheduleType.EveryOtherWeek:
                    break;
                case ScheduleType.EveryOtherMonth:
                    break;
                case ScheduleType.Workday:
                    result = RecurrenceInterval.EndOfMonth;
                    break;
                case ScheduleType.Weekend:
                    result = RecurrenceInterval.EndOfMonth;
                    break;
                case ScheduleType.Customize:
                    result = RecurrenceInterval.EndOfMonth;
                    break;
                case ScheduleType.SpecificDate:
                    result = RecurrenceInterval.EndOfMonth;
                    break;
                default:
                    break;
            }

            return result;
        }

        public static void RemoveAlarmByName(string name)
        {
            if (ScheduledActionService.Find(name) != null)
            {
                try
                {
                    ScheduledActionService.Remove(name);
                }
                catch (System.Exception)
                {
                }
            }
        }

    }
}

