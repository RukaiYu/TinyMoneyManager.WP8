namespace TinyMoneyManager.Component
{
    using Microsoft.Phone.Scheduler;
    using NkjSoft.Extensions;
    using System;
    using System.Linq;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;

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
                Alarm alarm = new Alarm(name) {
                    Sound = LocalizedStrings.GetLocailizedResourceUriFrom("/Resources/sounds/RepaymentNotification/RepaymentArrive.{0}.mp3", new object[0])
                };
                action = alarm;
            }
            else
            {
                Reminder reminder = new Reminder(name) {
                    NavigationUri = new Uri("/Pages/RepaymentManagerViews/RepaymentItemEditor.xaml?action={0}&itemId={1}".FormatWith(new object[] { PageActionType.Edit, repayment.Id }), UriKind.RelativeOrAbsolute),
                    Title = LocalizedStrings.GetLanguageInfoByKey("RepaymentNotificationList")
                };
                action = reminder;
            }
            action.RecurrenceType = RecurrenceInterval.Daily;
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

