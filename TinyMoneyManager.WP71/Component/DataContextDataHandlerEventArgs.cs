namespace TinyMoneyManager.Component
{
    using System;
    using System.Runtime.CompilerServices;

    public class DataContextDataHandlerEventArgs : System.EventArgs
    {
        public DataContextDataHandlerEventArgs(string data)
        {
            this.Data = data;
        }

        public string Data { get; private set; }
    }
}

