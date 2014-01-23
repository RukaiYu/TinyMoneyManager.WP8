namespace TinyMoneyManager.ScheduledAgentLib
{
    using Microsoft.Phone.Info;
    using Microsoft.Phone.Scheduler;
    using Microsoft.Phone.Shell;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using TinyMoneyManager.Data.ScheduleManager;
    using TinyMoneyManager.Data.Model;

    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        public static System.TimeSpan endTime = new System.TimeSpan(0, 30, 0);
        public static Uri MainTile = new Uri("/", UriKind.RelativeOrAbsolute);
        public static System.TimeSpan startTime = new System.TimeSpan(0, 0, 0);
        public static string TileInfoUpdatingAgentName = "AccountBook Tile Info Updating Agent";

        public ScheduledAgent()
        {
            System.Action a = null;
            if (!_classInitialized)
            {
                _classInitialized = true;
                if (a == null)
                {
                    a = delegate
                    {
                        Application.Current.UnhandledException += new System.EventHandler<ApplicationUnhandledExceptionEventArgs>(this.ScheduledAgent_UnhandledException);
                    };
                }
                Deployment.Current.Dispatcher.BeginInvoke(a);
            }
        }

        public bool IsNowAtTheTimeShouldRunTask()
        {
            System.TimeSpan timeOfDay = System.DateTime.Now.TimeOfDay;
            return ((timeOfDay >= startTime) && (timeOfDay <= endTime));
        }

        protected override void OnInvoke(ScheduledTask task)
        {
            if (task is PeriodicTask)
            {
                if (this.IsNowAtTheTimeShouldRunTask())
                {
                    ScheduledAgentConfiguration.RestoreTileInfoFromBackupGrid("New Record", 0, "0.00", false);
                }

                int total = 0;
                int totalExecuted = 0;
                string hasRecordsMessageFormatter = string.Empty;
                string noteRecordsMsg = string.Empty;
                string toastTitle = string.Empty;
                try
                {
                    System.Threading.Mutex mutex = new System.Threading.Mutex(true, "accountbook_UpdatingTileInfo");
                    mutex.WaitOne();
                    SecondSchedulePlanningManager manager = new SecondSchedulePlanningManager(null);
                    if (manager.ExecutePlan() != null)
                    {
                        totalExecuted = manager.ExecutedCount;
                        hasRecordsMessageFormatter = manager.CommonDataInPlanning["TaskCompleteWithZeroMessage"];
                        noteRecordsMsg = manager.CommonDataInPlanning["TaskCompletedMessage"];
                        toastTitle = manager.CommonDataInPlanning["AppName"];
                        if (totalExecuted > 0)
                        {
                            this.ShowNotices(total, totalExecuted, toastTitle, hasRecordsMessageFormatter, noteRecordsMsg);
                        }
                    }
                    mutex.ReleaseMutex();
                }
                catch (System.Exception exception)
                {
                    this.ShowNotices(0, 1, "error", exception.Message, "");
                    this.SaveErrorLog(exception.Message);
                }
                base.NotifyComplete();
            }
        }

        private void SaveErrorLog(string content)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile("error.log", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
                stream.Dispose();
            }
        }

        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        public void ShowNotices(int total, int totalExecuted, string toastTitle, string hasRecordsMessageFormatter, string noteRecordsMsg)
        {
            if (total == 0)
            {
                hasRecordsMessageFormatter = noteRecordsMsg;
            }
            string str = string.Format(hasRecordsMessageFormatter, totalExecuted);
            new ShellToast { Title = toastTitle, Content = str }.Show();
            if ((totalExecuted > 0) && ShellTileHelper.IsPinned(MainTile))
            {
                StandardTileData data = new StandardTileData
                {
                    Count = new int?(totalExecuted)
                };
                ShellTileHelper.GetAppTile(MainTile).Update(data);
            }
        }

        private void Test(ScheduledTask task)
        {
            string str = "";
            if (task is PeriodicTask)
            {
                str = "Periodic ";
            }
            else
            {
                str = "Resource-intensive ";
            }
            string str2 = string.Concat(new object[] { "Mem usage: ", DeviceStatus.ApplicationPeakMemoryUsage, "/", DeviceStatus.ApplicationMemoryUsageLimit });
            new ShellToast { Title = str, Content = str2 }.Show();
            base.NotifyComplete();
        }
    }
}

