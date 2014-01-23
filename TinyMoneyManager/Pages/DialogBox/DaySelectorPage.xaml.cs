using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using TinyMoneyManager.Component;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Language;
using NkjSoft.Extensions;
using Microsoft.Phone.Controls.Primitives;
using TinyMoneyManager.Data.Common;

namespace TinyMoneyManager.Pages.DialogBox
{
    public partial class DaySelectorPage : PhoneApplicationPage
    {
        public static Action<int> AfterConfirmed;

        private IntLoopingDataSource dataSource;

        public DaySelectorPage()
        {
            InitializeComponent();

            this.ApplicationBar.GetIconButtonFrom(1)
                .Text = AppResources.Cancel;

            this.ApplicationBar.GetIconButtonFrom(0)
                .Text = AppResources.OK;
            dataSource = new IntLoopingDataSource() { MinValue = 1, MaxValue = 31 };
            this.selector.DataSource = dataSource;
        }


        private void Calcel_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (AfterConfirmed != null)
                AfterConfirmed(Convert.ToInt32(dataSource.SelectedItem));

            this.NavigationService.GoBack();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                this.PageTitle.Text = this.GetNavigatingParameter("title").ToUpperInvariant();

                this.dataSource.SelectedItem = this.GetNavigatingParameter("defValue", DateTime.Now.Day).ToInt32();
            }
        }
    }



    // abstract the reusable code in a base class
    // this will allow us to concentrate on the specifics when implementing deriving looping data source classes
    public abstract class LoopingDataSourceBase : ILoopingSelectorDataSource
    {
        private object selectedItem;

        #region ILoopingSelectorDataSource Members

        public abstract object GetNext(object relativeTo);

        public abstract object GetPrevious(object relativeTo);

        public object SelectedItem
        {
            get
            {
                return this.selectedItem;
            }
            set
            {
                // this will use the Equals method if it is overridden for the data source item class
                if (!object.Equals(this.selectedItem, value))
                {
                    // save the previously selected item so that we can use it 
                    // to construct the event arguments for the SelectionChanged event
                    object previousSelectedItem = this.selectedItem;
                    this.selectedItem = value;
                    // fire the SelectionChanged event
                    this.OnSelectionChanged(previousSelectedItem, this.selectedItem);
                }
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        protected virtual void OnSelectionChanged(object oldSelectedItem, object newSelectedItem)
        {
            EventHandler<SelectionChangedEventArgs> handler = this.SelectionChanged;
            if (handler != null)
            {
                handler(this, new SelectionChangedEventArgs(new object[] { oldSelectedItem }, new object[] { newSelectedItem }));
            }
        }

        #endregion
    }

    public class IntLoopingDataSource : LoopingDataSourceBase
    {
        private int minValue;
        private int maxValue;
        private int increment;

        public IntLoopingDataSource()
        {
            this.MaxValue = 10;
            this.MinValue = 0;
            this.Increment = 1;
            this.SelectedItem = 0;
        }

        public int MinValue
        {
            get
            {
                return this.minValue;
            }
            set
            {
                if (value >= this.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("MinValue", "MinValue cannot be equal or greater than MaxValue");
                }
                this.minValue = value;
            }
        }

        public int MaxValue
        {
            get
            {
                return this.maxValue;
            }
            set
            {
                if (value <= this.MinValue)
                {
                    throw new ArgumentOutOfRangeException("MaxValue", "MaxValue cannot be equal or lower than MinValue");
                }
                this.maxValue = value;
            }
        }

        public int Increment
        {
            get
            {
                return this.increment;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("Increment", "Increment cannot be less than or equal to zero");
                }
                this.increment = value;
            }
        }

        public override object GetNext(object relativeTo)
        {
            int nextValue = (int)relativeTo + this.Increment;
            if (nextValue > this.MaxValue)
            {
                nextValue = this.MinValue;
            }
            return nextValue;
        }

        public override object GetPrevious(object relativeTo)
        {
            int prevValue = (int)relativeTo - this.Increment;
            if (prevValue < this.MinValue)
            {
                prevValue = this.MaxValue;
            }
            return prevValue;
        }
    }
}