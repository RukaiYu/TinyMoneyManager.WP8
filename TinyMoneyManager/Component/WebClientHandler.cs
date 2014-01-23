namespace TinyMoneyManager.Component
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class WebClientHandler
    {
        private System.Action<String> downLoadCompletedCallBack;
        private System.Action<String> openReadCompletedCallBack;

        public event System.EventHandler<EventArgs> WebClientIsBusyEvent;

        public WebClientHandler()
        {
            this.WebClient = new System.Net.WebClient();
            this.WebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(this.WebClient_DownloadStringCompleted);
            this.WebClient.OpenReadCompleted += new OpenReadCompletedEventHandler(this.WebClient_OpenReadCompleted);
        }

        public System.Net.WebClient DownloadStringAsync(Uri uri, System.Action<String> callBack)
        {
            this.downLoadCompletedCallBack = callBack;
            if (!this.WebClient.IsBusy)
            {
                this.WebClient.DownloadStringAsync(uri);
            }
            else
            {
                this.OnWebClientIsBusyEvent(System.EventArgs.Empty);
            }
            return this.WebClient;
        }

        protected virtual void OnWebClientIsBusyEvent(System.EventArgs e)
        {
            System.EventHandler<EventArgs> webClientIsBusyEvent = this.WebClientIsBusyEvent;
            if (webClientIsBusyEvent != null)
            {
                webClientIsBusyEvent(this, e);
            }
        }

        public System.Net.WebClient OpenReadAsync(Uri uri, System.Action<String> callBack)
        {
            this.openReadCompletedCallBack = callBack;
            if (!this.WebClient.IsBusy)
            {
                this.WebClient.OpenReadAsync(uri, null);
            }
            else
            {
                this.OnWebClientIsBusyEvent(System.EventArgs.Empty);
            }
            return this.WebClient;
        }

        private void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if ((e.Error == null) && !e.Cancelled)
            {
                object userState = e.UserState;
                string result = e.Result;
                if (this.downLoadCompletedCallBack != null)
                {
                    this.downLoadCompletedCallBack(result);
                }
            }
        }

        private void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (((e.Error == null) && !e.Cancelled) && (this.openReadCompletedCallBack != null))
            {
                string str = string.Empty;
                using (System.IO.StreamReader reader = new System.IO.StreamReader(e.Result))
                {
                    str = reader.ReadToEnd();
                }
                this.openReadCompletedCallBack(str);
            }
        }

        public System.Net.WebClient WebClient { get; private set; }
    }
}

