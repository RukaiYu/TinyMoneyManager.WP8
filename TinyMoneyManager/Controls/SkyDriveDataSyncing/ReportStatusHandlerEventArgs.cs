namespace TinyMoneyManager.Controls.SkyDriveDataSyncing
{
    using System;
    using System.Runtime.CompilerServices;

    public class ReportStatusHandlerEventArgs : System.EventArgs
    {
        public ReportStatusHandlerEventArgs(string key, string message) : this(key, message, null)
        {
        }

        public ReportStatusHandlerEventArgs(string key, string message, System.Exception exp)
        {
            this.ActionName = key;
            this.Message = message;
            this.Excetion = exp;
        }

        public string ActionName { get; private set; }

        public System.Exception Excetion { get; private set; }

        public bool HasError
        {
            get
            {
                return (this.Excetion != null);
            }
        }

        public string Message { get; private set; }
    }
}

