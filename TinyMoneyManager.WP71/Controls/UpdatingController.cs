using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Phone.Controls;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;

namespace TinyMoneyManager.Controls
{
    public class UpdatingController
    {
        public static bool HasSomeThingToDo
        {
            get
            {
                var result = true;

                IsolatedStorageSettings.ApplicationSettings.TryGetValue("DoSomethingOnce", out result);

                return result;
            }
        }

        /// <summary>
        /// Determines whether [has some thing to do before go to main page].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has some thing to do before go to main page]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasSomeThingToDoBeforeGoToMainPage(PhoneApplicationPage fromPage)
        {
            try
            {
                if (HasSomeThingToDo && App.SeniorVersion == "1530.1222")
                {
                    fromPage.BusyForWork(AppResources.UpgratingUnderProcess);

                    IsolatedAppSetingsHelper.ShowTipsByVerion("DoSomethingOnce", () =>
                    {
                        ViewModelLocator.ScheduleManagerViewModel.SetupPlanningFirstTime();
                        Repayment.UpdateTableStructureAtV1_9_8(ViewModelLocator.AccountBookDataContext, ViewModels.BorrowLeanManager.BorrowLeanViewModel.EnsureStatus);
                    });

                    fromPage.WorkDone();
                }

            }
            catch (Exception)
            {

            }

            return HasSomeThingToDo;
        }
    }
}
