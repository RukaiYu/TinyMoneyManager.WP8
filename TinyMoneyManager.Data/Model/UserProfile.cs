namespace TinyMoneyManager.Data.Model
{
    using System;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component;

    public class UserProfile : NotionObject
    {
        public string ClosestPeople { get; set; }

        public bool EnableIndividuationTipsShowing { get; set; }

        public string NickName { get; set; }

        public string NickNameForApp { get; set; }
    }
}

