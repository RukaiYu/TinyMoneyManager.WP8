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
using TinyMoneyManager.Controls.CalcutorKeyBoard;

namespace TinyMoneyManager.Controls
{
    public partial class KeyBoardKey : UserControl
    {
        public IKeyHandler KeyHandler { get; set; }

        private CustomizedKey key;
        public CustomizedKey Key
        {
            get { return key; }
            set
            {
                key = value;
            }
        }

        public int GridRowIndex { get; set; }

        public int GridColumnIndex { get; set; }
        private bool Reverted;

        public string Text
        {
            get { return this.KeyButton.Text.ToString(); }
            set { this.KeyButton.Text = value; }
        }

        public KeyBoardKey()
        {
            InitializeComponent();
        }

        public KeyBoardKey(CustomizedKey key, IKeyHandler keyHandler)
            : this()
        {
            this.Key = key;

            this.KeyHandler = keyHandler;

            this.KeyButton.Text = key.KeyChar;
        }

        /// <summary>
        /// Handles the Click event of the KeyButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            KeyHandler.HandleChar(Key);
        }

        private void GestureListener_Tap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            KeyHandler.HandleChar(Key);
        }

        private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.Reverted)
            {
                this.Revert();
                this.Reverted = false;
            }
        }

        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.Reverted)
            {
                this.Revert();
                this.Reverted = true;
            }
        }

        private void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.Reverted)
            {
                this.Revert();
                this.Reverted = false;
            }
        }

        private void Revert()
        {
            Brush background = this.LayoutRoot.Background;
            Brush foreground = this.KeyButton.Foreground;
            this.LayoutRoot.Background = foreground;
            this.KeyButton.Foreground = background;
        }

        public Brush Background
        {
            get
            {
                return this.LayoutRoot.Background;
            }
            set
            {
                this.LayoutRoot.Background = value;
            }
        }
    }
}
