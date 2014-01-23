namespace TinyMoneyManager.Component.Common
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;

    public class TwoLineListerner<T> : NotionObject, ITitleInfoListener where T : INotifyPropertyChanged
    {
        public System.Func<T, String, String> format;
        private bool hasRegisted;
        private string navigateUri;
        private string secondTitle;
        private string secondTitleFormatter;
        private string title;
        private string titleKey;

        public TwoLineListerner()
        {
        }

        public TwoLineListerner(string titleKey)
        {
            this.TitleKey = titleKey;
            this.Title = LocalizedStrings.GetLanguageInfoByKey(titleKey);
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.propertiesListened.Contains<string>(e.PropertyName))
            {
                this.Title = LocalizedStrings.GetLanguageInfoByKey(this.TitleKey);
                this.SecondTitle = this.format(this.ObjectListenTo, this.SecondTitleFormatter);
            }
        }

        public void NotifyFormat()
        {
            this.Title = LocalizedStrings.GetLanguageInfoByKey(this.TitleKey);
            this.SecondTitle = this.format(this.ObjectListenTo, this.secondTitleFormatter);
        }

        public TwoLineListerner<T> RegisterObjectPropertyChanged(System.Func<T, String, String> formatWay, params string[] properties)
        {
            if (!this.hasRegisted)
            {
                this.ObjectListenTo.PropertyChanged += new PropertyChangedEventHandler(this.Instance_PropertyChanged);
                this.propertiesListened = properties;
                this.hasRegisted = true;
                this.format = formatWay;
            }
            return this;
        }

        public string NavigateUri
        {
            get
            {
                return this.navigateUri;
            }
            set
            {
                if (this.navigateUri != value)
                {
                    this.OnNotifyPropertyChanging("NavigateUri");
                    this.navigateUri = value;
                    this.OnNotifyPropertyChanged("NavigateUri");
                }
            }
        }

        public T ObjectListenTo
        {
            get;
            set;
        }

        public string[] propertiesListened
        {
            get;
            set;
        }

        public string SecondTitle
        {
            get
            {
                return this.secondTitle;
            }
            set
            {
                if (this.secondTitle != value)
                {
                    this.OnNotifyPropertyChanging("SecondTitle");
                    this.secondTitle = value;
                    this.OnNotifyPropertyChanged("SecondTitle");
                }
            }
        }

        public string SecondTitleFormatter
        {
            get
            {
                return this.secondTitleFormatter;
            }
            set
            {
                if (this.secondTitleFormatter != value)
                {
                    this.OnNotifyPropertyChanging("TitleFormatter");
                    this.secondTitleFormatter = value;
                    this.OnNotifyPropertyChanged("TitleFormatter");
                }
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (this.title != value)
                {
                    this.OnNotifyPropertyChanging("Title");
                    this.title = value;
                    this.OnNotifyPropertyChanged("Title");
                }
            }
        }

        public string TitleKey
        {
            get
            {
                return this.titleKey;
            }
            set
            {
                if (this.titleKey != value)
                {
                    this.OnNotifyPropertyChanging("TitleKey");
                    this.titleKey = value;
                    this.OnNotifyPropertyChanged("TitleKey");
                }
            }
        }
    }
}

