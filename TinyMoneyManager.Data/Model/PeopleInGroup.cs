namespace TinyMoneyManager.Data.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class PeopleInGroup : System.Collections.Generic.List<PeopleProfile>
    {
        public PeopleInGroup(string category)
        {
            this.Key = category;
        }

        public bool HasItems
        {
            get
            {
                return (base.Count > 0);
            }
        }

        public string Key { get; set; }
    }
}

