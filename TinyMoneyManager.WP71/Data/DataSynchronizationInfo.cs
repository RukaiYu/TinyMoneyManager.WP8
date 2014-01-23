namespace TinyMoneyManager.Data
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using TinyMoneyManager;

    public class DataSynchronizationInfo : System.EventArgs
    {
        public DataSynchronizationInfo()
        {
            this.HandlingInfo = new System.Collections.Generic.Dictionary<String, Int32>();
            this.TotalMessage = string.Empty;
        }

        public void Add(string item, int value)
        {
            if (this.HandlingInfo.ContainsKey(item))
            {
                this.HandlingInfo[item] = value;
            }
            else
            {
                this.HandlingInfo.Add(item, value);
            }
        }

        public string GetMessage()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(LocalizedStrings.GetLanguageInfoByKey(this.Action.ToString()));
            builder.AppendLine(":");
            foreach (System.Collections.Generic.KeyValuePair<String, Int32> pair in this.HandlingInfo)
            {
                builder.AppendFormat("\t{0}\t\t: {1}", new object[] { pair.Key, pair.Value });
                builder.AppendLine();
            }
            builder.Append(this.TotalMessage);
            return builder.ToString();
        }

        public HandlerAction Action { get; set; }

        public System.Collections.Generic.Dictionary<String, Int32> HandlingInfo { get; set; }

        public OperationResult Result { get; set; }

        public string TotalMessage { get; set; }
    }
}

