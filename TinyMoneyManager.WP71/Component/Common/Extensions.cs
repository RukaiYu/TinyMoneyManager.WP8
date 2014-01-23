using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Controls;

namespace NkjSoft.WPhone.Extensions
{
    public static class ExtensionsWP
    {
        /// <summary>
        /// Safes the go back.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageNavigateTo">The page navigate to.</param>
        /// <param name="removeCurrentStep">if set to <c>true</c> [remove current step].</param>
        public static void SafeGoBack(this PhoneApplicationPage page, string pageNavigateTo, bool removeCurrentStep = false)
        {
            page.NavigateTo(pageNavigateTo);


        }
    }
}
