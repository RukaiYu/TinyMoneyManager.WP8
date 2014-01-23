namespace TinyMoneyManager.Controls
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class MoneyInputTextBox : TextBox
    {
        public static readonly DependencyProperty MoneyProperty = DependencyProperty.Register("Money", typeof(decimal), typeof(MoneyInputTextBox), new PropertyMetadata(0.0M));

        static MoneyInputTextBox()
        {
            InputScopeName name = new InputScopeName
            {
                NameValue = InputScopeNameValue.TelephoneNumber
            };
            NumberInputScope = new InputScope();
            NumberInputScope.Names.Add(name);
        }

        public MoneyInputTextBox()
        {
            base.DefaultStyleKey = base.DefaultStyleKey;
            base.InputScope = NumberInputScope;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            base.SelectionStart = 0;
            base.SelectionLength = base.Text.Length;
        }

        public decimal Money
        {
            get
            {
                return (decimal)base.GetValue(MoneyProperty);
            }
            set
            {
                base.SetValue(MoneyProperty, value);
            }
        }

        public static InputScope NumberInputScope
        {
            get;
            set;
        }
    }
}

