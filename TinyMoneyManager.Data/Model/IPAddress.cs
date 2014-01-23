namespace TinyMoneyManager.Data.Model
{
    using System;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component;

    public class IPAddress : NotionObject
    {
        private string ipAddressA;
        private string ipAddressB;
        private string ipAddressC;
        private string ipAddressD;
        private int serverSyncPort;

        public IPAddress()
        {
        }

        public IPAddress(string address, int port)
        {
            this.serverSyncPort = port;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", new object[] { this.ipAddressA, this.ipAddressB, this.ipAddressC, this.ipAddressD });
        }

        public string Address
        {
            get
            {
                return this.ToString();
            }
            set
            {
                try
                {
                    string[] strArray = value.Split(new char[] { '.' });
                    this.IPAddressA = strArray[0];
                    this.IPAddressB = strArray[1];
                    this.IPAddressC = strArray[2];
                    this.IPAddressD = strArray[3];
                }
                catch (System.Exception)
                {
                    this.Address = "192.168.1.101";
                }
            }
        }

        public string HttpAddress
        {
            get
            {
                if (!(this.Address == string.Empty) && !(this.Address == "..."))
                {
                    return string.Format("{0}:{1}", this.Address, this.serverSyncPort);
                }
                return string.Empty;
            }
        }

        public string IPAddressA
        {
            get
            {
                return this.ipAddressA;
            }
            set
            {
                this.ipAddressA = value;
                this.OnNotifyPropertyChanged("IPAddressA");
                this.OnNotifyPropertyChanged("Address");
            }
        }

        public string IPAddressB
        {
            get
            {
                return this.ipAddressB;
            }
            set
            {
                this.ipAddressB = value;
                this.OnNotifyPropertyChanged("IPAddressB");
                this.OnNotifyPropertyChanged("Address");
            }
        }

        public string IPAddressC
        {
            get
            {
                return this.ipAddressC;
            }
            set
            {
                this.ipAddressC = value;
                this.OnNotifyPropertyChanged("IPAddressC");
                this.OnNotifyPropertyChanged("Address");
            }
        }

        public string IPAddressD
        {
            get
            {
                return this.ipAddressD;
            }
            set
            {
                this.ipAddressD = value;
                this.OnNotifyPropertyChanged("IPAddressD");
                this.OnNotifyPropertyChanged("Address");
            }
        }

        public int ServerSyncPort
        {
            get
            {
                return this.serverSyncPort;
            }
            set
            {
                this.serverSyncPort = value;
                this.OnNotifyPropertyChanged("ServerSyncPort");
                this.OnNotifyPropertyChanged("Address");
            }
        }

        public bool Uselocalhost { get; set; }
    }
}

