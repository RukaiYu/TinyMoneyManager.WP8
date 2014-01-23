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
using Microsoft.Phone.Controls;
using System.Windows.Controls.Primitives;

namespace TinyMoneyManager.Controls
{
    public class DialogBoxBase : UserControl
    {
        public event EventHandler Closed;

        public void Close(object sender, EventArgs e)
        {
            OnDialogClosed(e);
            this.ChildWindowPopup.IsOpen = false;
            if (_reshowAppBar)
                ((PhoneApplicationPage)RootVisual.Content).ApplicationBar.IsVisible = true;
        }

        public MessageBoxResult DialogResult { get; set; }

        static bool _reshowAppBar;

        #region properties

        internal Popup ChildWindowPopup
        {
            get;
            private set;
        }

        private static PhoneApplicationFrame RootVisual
        {
            get
            {
                return Application.Current == null ? null : Application.Current.RootVisual as PhoneApplicationFrame;
            }
        }

        private string title;
        public virtual string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        #endregion

        #region Methods
        public void Show()
        {
            if (this.ChildWindowPopup == null)
            {
                this.ChildWindowPopup = new Popup();

                try
                {
                    this.ChildWindowPopup.Child = this;
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("The control is already shown.");
                }
            }

            if (this.ChildWindowPopup != null && Application.Current.RootVisual != null)
            {
                //SystemTray.IsVisible = false;
                // Show popup
                this.ChildWindowPopup.IsOpen = true;
            }

            if (RootVisual != null)
            {
                var page = ((PhoneApplicationPage)RootVisual.Content);

                if (page.ApplicationBar != null && page.ApplicationBar.IsVisible)
                {
                    _reshowAppBar = true;
                    page.ApplicationBar.IsVisible = false;
                }

                // Hook up into the back key press event of the current page
                page.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(page_BackKeyPress);
            }
        }
        void page_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Unhook the BackKeyPress event handler
            ((PhoneApplicationPage)RootVisual.Content).BackKeyPress -= new EventHandler<System.ComponentModel.CancelEventArgs>(page_BackKeyPress);
            Close(sender, e);
            e.Cancel = true;
        }

        protected void OnDialogClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        #endregion

        public DialogBoxBase()
        {
            //this.DefaultStyleKey = typeof(CategoryPickerDialogBox);
            TiltEffect.SetIsTiltEnabled(this, true);
        }

        private void Close()
        {
            // Raise closed eventI  
            Close(this, null);
        }

    }
}
