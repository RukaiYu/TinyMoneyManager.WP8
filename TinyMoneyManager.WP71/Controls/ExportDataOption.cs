using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.Controls
{
    public class ExportDataOption : DetailsCondition
    {
        private string _subject;

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject
        {
            get { return _subject; }
            set
            {
                if (_subject != value)
                {
                    OnNotifyPropertyChanging("Subject");
                    _subject = value;
                    OnNotifyPropertyChanged("Subject");
                }
            }
        }

        public Action<ExportDataOption> CustomizedDateSetter;

        private SummarySendingMode _summarySendingMode;
        public SummarySendingMode ExportDataMode
        {
            get { return _summarySendingMode; }
            set
            {
                if (_summarySendingMode != value)
                {
                    OnNotifyPropertyChanging("ExportDataMode");
                    _summarySendingMode = value;
                    OnNotifyPropertyChanged("ExportDataMode");
                    OnNotifyPropertyChanged("ExportDataModeIndex");
                }
            }
        }

        private int _ExportDataModeIndex;

        public int ExportDataModeIndex
        {
            get { return (int)ExportDataMode; }
            set
            {
                if (value != _ExportDataModeIndex)
                {
                    OnNotifyPropertyChanging("ExportDataModeIndex");
                    _ExportDataModeIndex = value;
                    ExportDataMode = (SummarySendingMode)value;
                    OnNotifyPropertyChanged("ExportDataModeIndex");
                }
            }
        }


        private SummaryDataType _exportDataType;

        public SummaryDataType ExportDataType
        {
            get { return _exportDataType; }
            set
            {
                if (_exportDataType != value)
                {
                    OnNotifyPropertyChanging("ExportDataType");
                    _exportDataType = value;
                    OnNotifyPropertyChanged("ExportDataType");
                }
            }
        }

        private bool enableChangeSearchingScope;

        /// <summary>
        /// Gets or sets a value indicating whether [enable change searching scope].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [enable change searching scope]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableChangeSearchingScope
        {
            get { return enableChangeSearchingScope; }
            set
            {
                if (enableChangeSearchingScope != value)
                {
                    OnNotifyPropertyChanging("EnableChangeSearchingScope");
                    enableChangeSearchingScope = value;
                    OnNotifyPropertyChanged("EnableChangeSearchingScope");
                }
            }
        }

        private bool enableChangeExportDataMode;

        /// <summary>
        /// Gets or sets a value indicating whether [enable change export data mode].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [enable change export data mode]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableChangeExportDataMode
        {
            get { return enableChangeExportDataMode; }
            set
            {
                if (enableChangeExportDataMode != value)
                {
                    OnNotifyPropertyChanging("EnableChangeExportDataMode");
                    enableChangeExportDataMode = value;
                    OnNotifyPropertyChanged("EnableChangeExportDataMode");
                }
            }
        }

        private bool enableChangeExportDataType;

        public bool EnableChangeExportDataType
        {
            get { return enableChangeExportDataType; }
            set
            {
                if (enableChangeExportDataType != value)
                {
                    OnNotifyPropertyChanging("EnableChangeExportDataType");
                    enableChangeExportDataType = value;
                    OnNotifyPropertyChanged("EnableChangeExportDataType");
                }
            }
        }

        private int searchingScopeIndex;
        /// <summary>
        /// Gets or sets the index of the searching scope.
        /// </summary>
        /// <value>
        /// The index of the searching scope.
        /// </value>
        public int SearchingScopeIndex
        {
            get { return searchingScopeIndex; }
            set
            {
                if (searchingScopeIndex != value)
                {
                    searchingScopeIndex = value;

                    switch (value)
                    {
                        default:
                        case 0:
                            SearchingScope = Component.SearchingScope.Today;
                            break;
                        case 1:
                            SearchingScope = Component.SearchingScope.CurrentWeek;
                            break;
                        case 2:
                            SearchingScope = Component.SearchingScope.CurrentMonth;
                            break;
                        case 3:
                            SearchingScope = Component.SearchingScope.CurrentYear;
                            break;
                        case 4:
                            SearchingScope = Component.SearchingScope.Customize;
                            if (CustomizedDateSetter != null)
                                CustomizedDateSetter(this);
                            break;
                    }

                }

            }
        }

    }
}
