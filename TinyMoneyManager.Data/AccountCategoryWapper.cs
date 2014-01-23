namespace TinyMoneyManager.Data
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class AccountCategoryWapper
    {
        public AccountCategoryWapper() : this(AccountCategory.BankCard, string.Empty)
        {
        }

        public AccountCategoryWapper(AccountCategory category, string color)
        {
            this.Category = category;
            this.Color = color;
        }

        [XmlAttribute]
        public AccountCategory Category { get; set; }

        [XmlAttribute]
        public string Color { get; set; }

        public string LocalizedName
        {
            get
            {
                return LocalizedObjectHelper.GetLocalizedStringFrom(this.Category.ToString());
            }
        }
    }
}

