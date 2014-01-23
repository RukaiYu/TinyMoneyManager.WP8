using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager.Component.ReorderListBox
{
    /// <summary>
    /// 
    /// </summary>
    public class ReorderActionCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the index of the original.
        /// </summary>
        /// <value>
        /// The index of the original.
        /// </value>
        public int OriginalIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the target.
        /// </summary>
        /// <value>
        /// The index of the target.
        /// </value>
        public int TargetIndex { get; set; }

        /// <summary>
        /// Gets or sets the original source.
        /// </summary>
        /// <value>
        /// The original source.
        /// </value>
        public object OriginalSource { get; set; }

        /// <summary>
        /// Gets or sets the target object.
        /// </summary>
        /// <value>
        /// The target object.
        /// </value>
        public object TargetObject { get; set; }

        /// <summary>
        /// Gets or sets the orinentation.
        /// </summary>
        /// <value>
        /// The orinentation.
        /// </value>
        public MovingOrientation Orinentation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReorderActionCompletedEventArgs" /> class.
        /// </summary>
        public ReorderActionCompletedEventArgs()
        {

        }
    }


    public enum MovingOrientation
    {
        None,
        Down,
        Up,
    }

    public static class ReorderActionCompletedEventArgsExtensions
    {
        /// <summary>
        /// Handles the reordering for.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e">The <see cref="ReorderActionCompletedEventArgs" /> instance containing the event data.</param>
        /// <param name="dataSource">The data source.</param>
        /// <param name="action">The action.</param>
        public static void HandleReorderingFor<T>(this ReorderActionCompletedEventArgs e, ItemCollection dataSource, Action actionAfterDone = null)
            where T : class, IOrderable
        {
            if (e != null && e.Orinentation != Component.ReorderListBox.MovingOrientation.None)
            {
                var startIndex = e.OriginalIndex;
                var endIndex = e.TargetIndex;

                if (e.Orinentation == Component.ReorderListBox.MovingOrientation.Up)
                {
                    for (int i = startIndex; i > endIndex; i--)
                    {
                        var item = dataSource[i] as T;
                        item.Order = item.Order.GetValueOrDefault() + 1;
                    }
                }
                else if (e.Orinentation == Component.ReorderListBox.MovingOrientation.Down)
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        var item = dataSource[i] as T;
                        item.Order = item.Order.GetValueOrDefault() - 1;
                    }
                }

                if (e.OriginalSource != null)
                {
                    var senderItem = e.OriginalSource as T;

                    senderItem.Order = e.TargetIndex + 1;
                }

                if (actionAfterDone != null)
                {
                    actionAfterDone();
                }
            }
        }
    }
}
