using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.Data.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class TypedKeyValuePair<TKey, TValue> : NotionObject
    {
        private TKey _key;

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key
        {
            get { return _key; }
            set
            {
                OnNotifyPropertyChanging("Key");
                _key = value;
                OnNotifyPropertyChanged("Key");
            }
        }


        private TValue _value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public TValue Value
        {
            get { return _value; }
            set
            {
                OnNotifyPropertyChanging("Value");
                _value = value;
                OnNotifyPropertyChanged("Value");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedKeyValuePair{TValue}" /> class.
        /// </summary>
        public TypedKeyValuePair()
            : this(default(TKey), default(TValue))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedKeyValuePair{TValue}" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public TypedKeyValuePair(TKey key, TValue value)
        {
            this._key = key;
            this._value = value;
        }

    }
}
