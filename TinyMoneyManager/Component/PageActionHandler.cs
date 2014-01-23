using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager.Component
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageActionHandler<T> where T : class
    {
        public Action<T> AfterAdd;

        public Action<T> AfterSelected;

        public Action<T> AfterDelete;

        public Func<IEnumerable<T>> GetItems;
        public System.Func<T> GetSelected;

        public Action<T> Update;

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureActionHandler"/> class.
        /// </summary>
        public PageActionHandler()
        {
            this.AfterAdd = new Action<T>((i) => { });
            this.AfterDelete = new Action<T>((i) => { });
        }

        /// <summary>
        /// Called when [after delete].
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        public void OnAfterDelete(T itemInfo)
        {
            if (AfterDelete != null)
            {
                AfterDelete(itemInfo);
            }
        }

        /// <summary>
        /// Called when [after add].
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        public void OnAfterAdd(T itemInfo)
        {
            if (AfterAdd != null)
            {
                AfterAdd(itemInfo);
            }
        }

        /// <summary>
        /// Called when [get items].
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> OnGetItems()
        {
            if (GetItems != null)
                return GetItems();

            return new List<T>();
        }

        /// <summary>
        /// Called when [update].
        /// </summary>
        /// <param name="item">The item.</param>
        public void OnUpdate(T item)
        {
            if (Update != null)
            {
                Update(item);
            }
        }

        /// <summary>
        /// Called when [selected].
        /// </summary>
        /// <param name="item">The item.</param>
        public void OnSelected(T item)
        {
            if (this.AfterSelected != null)
                this.AfterSelected(item);
        }

        public T OnGetSelected()
        {
            if (this.GetSelected != null)
            {
                return this.GetSelected();
            }
            return default(T);
        }

    }

}
