using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.Controls
{
    public class AssociatedItemsSelectorOption : ExportDataOption
    {
        /// <summary>
        /// 
        /// </summary>
        private AssociatedItemType associatedItemType;

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public AssociatedItemType DataType
        {
            get { return associatedItemType; }
            set
            {
                if (associatedItemType != value)
                {
                    OnNotifyPropertyChanging("DataType");
                    associatedItemType = value;
                    OnNotifyPropertyChanged("DataType");
                }
            }
        }

        /// <summary>
        /// Gets or sets the index of the data type.
        /// </summary>
        /// <value>
        /// The index of the data type.
        /// </value>
        public int DataTypeIndex
        {
            get { return (int)DataType; }
            set
            {
                DataType = (AssociatedItemType)value;
                OnNotifyPropertyChanged("DataTypeIndex");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociatedItemsSelectorOption"/> class.
        /// </summary>
        public AssociatedItemsSelectorOption()
        {

        }
    }

}
