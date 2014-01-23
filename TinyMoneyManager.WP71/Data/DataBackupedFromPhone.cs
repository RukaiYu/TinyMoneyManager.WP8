namespace TinyMoneyManager.Data
{
    using System;
    using System.Runtime.CompilerServices;

    public class DataBackupedFromPhone
    {
        public string AccountItemXmlSource { get; set; }

        public string AppSettingXmlSource { get; set; }

        public string CategoryListXmlSource { get; set; }

        public byte[] SdfFileContent { get; set; }
    }
}

