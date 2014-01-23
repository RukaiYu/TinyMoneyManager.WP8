using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager
{
    public class MainPageItemViewModel : INotifyPropertyChanged
    {
        private string _TileNumber;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string TileNumber
        {
            get
            {
                return _TileNumber;
            }
            set
            {
                if (value != _TileNumber)
                {
                    _TileNumber = value;
                    NotifyPropertyChanged("TileNumber");
                }
            }
        }

        private string _tileImagePath;

        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string TileImagePath
        {
            get
            {
                return _tileImagePath;
            }
            set
            {
                if (value != _tileImagePath)
                {
                    _tileImagePath = value;
                    NotifyPropertyChanged("TileImagePath");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _key;

        /// <summary>
        /// Gets or sets the key.
        /// This key can be used to get localization language Title.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                Title = OnGettingLocalizedTitle(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Func<string, string> LocalizationTitleGetter;

        /// <summary>
        /// Called when [getting localized title].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string OnGettingLocalizedTitle(string key)
        {
            if (!string.IsNullOrEmpty(key) && LocalizationTitleGetter != null)
            {
                return LocalizationTitleGetter(key).ToLower();
            }
            return key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageItemViewModel"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public MainPageItemViewModel(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageItemViewModel"/> class.
        /// </summary>
        public MainPageItemViewModel()
            : this(string.Empty)
        {

        }
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        public string PageUri { get; set; }

    }
}