namespace TinyMoneyManager.Controls
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;

    public class PasswordTextBox : TextBox
    {
        private System.Text.StringBuilder _inputText = new System.Text.StringBuilder();
        public static readonly DependencyProperty PasswordCharProperty = DependencyProperty.Register("PasswordChar", typeof(char), typeof(PasswordTextBox), new PropertyMetadata('●'));
        private string svalue = string.Empty;

        public PasswordTextBox()
        {
            base.DefaultStyleKey = base.DefaultStyleKey;
            this.Value = string.Empty;
        }

        private void DelayInputSelect(object value)
        {
            System.Threading.Thread.Sleep(250);
            base.Dispatcher.BeginInvoke(delegate
            {
                base.Focus();
                base.SelectAll();
            });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            base.TextChanged += new TextChangedEventHandler(this.PasswordTextBox_TextChanged);
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int length = base.Text.Length - this._inputText.Length;
            if (length < 0)
            {
                length *= -1;
                int startIndex = (base.SelectionStart + 1) - length;
                if (startIndex < 0)
                {
                    startIndex = 0;
                }
                this._inputText.Remove(startIndex, length);
                this.Value = this._inputText.ToString();
            }
            else if (length > 0)
            {
                int selectionStart = base.SelectionStart;
                if (selectionStart != 0)
                {
                    string str = base.Text.Substring(base.SelectionStart - 1, length);
                    this._inputText.Append(str);
                    this.Value = this._inputText.ToString();
                    if (base.Text.Length >= 1)
                    {
                        System.Text.StringBuilder builder = new System.Text.StringBuilder();
                        builder.Insert(0, this.PasswordChar.ToString(), base.Text.Length);
                        base.Text = builder.ToString();
                    }
                    base.SelectionStart = selectionStart;
                }
            }
        }

        public char PasswordChar
        {
            get
            {
                return (char)base.GetValue(PasswordCharProperty);
            }
            set
            {
                base.SetValue(PasswordCharProperty, value);
            }
        }

        public string Value
        {
            get
            {
                return this._inputText.ToString();
            }
            set
            {
                this.svalue = value;
            }
        }
    }
}

