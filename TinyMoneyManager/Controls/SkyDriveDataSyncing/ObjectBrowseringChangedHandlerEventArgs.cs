namespace TinyMoneyManager.Controls.SkyDriveDataSyncing
{
    using System;
    using System.Runtime.CompilerServices;

    public class ObjectBrowseringChangedHandlerEventArgs : System.EventArgs
    {
        public ObjectBrowseringChangedHandlerEventArgs(string objType, string sharedWith, string name)
        {
            this.ObjectType = objType;
            this.SelectAsResult = true;
            this.Name = name;
            this.SharedWith = sharedWith;
        }

        public string Name { get; set; }

        public string ObjectType { get; private set; }

        public bool SelectAsResult { get; set; }

        public string SharedWith { get; private set; }
    }
}

