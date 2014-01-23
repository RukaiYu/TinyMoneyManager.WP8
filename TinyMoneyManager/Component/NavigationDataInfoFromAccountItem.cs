namespace TinyMoneyManager
{
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Runtime.CompilerServices;

    public class NavigationDataInfoFromAccountItem : INavigationServiceInfoProvider<System.Guid>
    {
        public System.Guid Key { get; set; }

        public string PageName { get; set; }
    }
}

