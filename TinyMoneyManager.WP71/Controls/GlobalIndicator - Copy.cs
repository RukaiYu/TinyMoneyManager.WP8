namespace mangoProgressIndicator
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Navigation;
    using System.Windows.Threading;

    public class GlobalIndicator : INotifyPropertyChanged
    {
        private static GlobalIndicator _in;
        private bool _isBusy;
        private Microsoft.Phone.Shell.ProgressIndicator _mangoIndicator;
        private PhoneApplicationPage CurrentPage;
        private int step;
        private int TimeOut;
        private DispatcherTimer timer;
        private int timesGoing;

        public event PropertyChangedEventHandler PropertyChanged;

        public void BusyForWork(string workText, params object[] args)
        {
            this.Text = string.Format(workText, args);
            this.IsBusy = true;
        }

        public void DelayWorkFor(int timeOut, string text)
        {
            this.Text = text;
            this.timer = new DispatcherTimer();
            this.timer.Interval = new System.TimeSpan((long)timeOut);
            this.TimeOut = timeOut;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            this.timesGoing = 0;
            this.step = this.TimeOut / 0x3e8;
            this.timer.Start();
        }

        private void doingSomeThing(bool value)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                if (this.CurrentPage != null)
                {
                    this.CurrentPage.SetValue(SystemTray.IsVisibleProperty, value);
                }

                this._mangoIndicator.IsVisible = value;
                this._mangoIndicator.IsIndeterminate = value;
            });
        }

        private void frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (this.IsBusy)
            {
                this.IsBusy = false;
            }
        }

        public void Initialize(PhoneApplicationFrame frame)
        {
            this._mangoIndicator = new Microsoft.Phone.Shell.ProgressIndicator();
            frame.Navigating += new NavigatingCancelEventHandler(this.frame_Navigating);
            frame.Navigated += new NavigatedEventHandler(this.OnRootFrameNavigated);
        }

        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            PhoneApplicationPage content = e.Content as PhoneApplicationPage;
            this.CurrentPage = content;
            if (content != null)
            {
                this._mangoIndicator.IsVisible = true;
                content.SetValue(SystemTray.ProgressIndicatorProperty, this._mangoIndicator);
                content.SetValue(SystemTray.OpacityProperty, 0.0);
            }
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void timer_Tick(object sender, System.EventArgs e)
        {
            if (this.timesGoing == this.TimeOut)
            {
                this.Text = string.Empty;
                this.IsBusy = false;
                if (this.timer != null)
                {
                    this.timer.Stop();
                    this.timer = null;
                }
            }
            else
            {
                this.timesGoing = this.TimeOut;
            }
        }

        public void WorkDone()
        {
            this.Text = string.Empty;
            this.IsBusy = false;
        }

        public void WorkDoneStillShowTray()
        {
            this.Text = string.Empty;
            this._mangoIndicator.IsVisible = false;
            this._mangoIndicator.IsIndeterminate = false;
        }

        public static GlobalIndicator Instance
        {
            get
            {
                if (_in == null)
                {
                    _in = new GlobalIndicator();
                }
                return _in;
            }
        }

        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }
            set
            {
                if (this._isBusy != value)
                {
                    this._isBusy = value;
                    this.doingSomeThing(this._isBusy);
                }
            }
        }

        public Microsoft.Phone.Shell.ProgressIndicator ProgressIndicator
        {
            get
            {
                return this._mangoIndicator;
            }
            set
            {
                this._mangoIndicator = value;
            }
        }

        public string Text
        {
            set
            {
                if (this._mangoIndicator == null)
                {
                    this._mangoIndicator = new Microsoft.Phone.Shell.ProgressIndicator();
                }
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    this._mangoIndicator.Text = value;
                });
            }
        }
    }
}

