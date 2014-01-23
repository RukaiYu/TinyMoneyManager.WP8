namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Runtime.CompilerServices;

    public class LiveConnectorSyncHandlerEventArgs : System.EventArgs
    {
        public LiveConnectorSyncHandlerEventArgs(LiveConnectorSyncObject syncObject, System.Exception exception, string msg)
        {
            this.CurrentSyncObject = syncObject;
            this.Exception = exception;
            this.Message = msg;
        }

        public LiveConnectorSyncObject CurrentSyncObject { get; private set; }

        public System.Exception Exception { get; private set; }

        public string Message { get; private set; }
    }
}

