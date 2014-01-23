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
using TinyMoneyManager.Component;
using TinyMoneyManager.Language;

namespace TinyMoneyManager.Controls.CalcutorKeyBoard
{
    public class PriceInputKeyBoardHandler : NotionObject, Controls.IKeyHandler
    {
        private string displayResult;

        public string DisplayResult
        {
            get
            {
                return displayResult;
            }
            set
            {
                if (displayResult != value)
                {
                    OnNotifyPropertyChanging("DisplayResult");
                    displayResult = value;
                    OnNotifyPropertyChanged("DisplayResult");
                }
            }
        }

        private double result;

        public double Result
        {
            get
            {
                return result;
            }
            set
            {
                if (result != value)
                {
                    OnNotifyPropertyChanging("Result");
                    result = value;
                    OnNotifyPropertyChanged("Result");
                }
            }
        }

        public event EventHandler<EventArgs> Confirmed;

        private KeyOperator lastOperator;

        public KeyOperator LastOperator
        {
            get { return lastOperator; }
            set
            {
                if (lastOperator != value)
                {
                    OnNotifyPropertyChanging("LastOperator");
                    lastOperator = value;
                    HasOperatorChanged = true;
                    OnNotifyPropertyChanged("LastOperator");
                }
            }
        }
        public bool ClearTextBeforeNumbersInput { get; set; }


        private bool _hasOperatorChanged;
        public static string HasOperatorChangedProperty = "HasOperatorChanged";
        /// <summary>
        /// Gets or sets a value indicating whether this instance has operator changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has operator changed; otherwise, <c>false</c>.
        /// </value>
        public bool HasOperatorChanged
        {
            get { return _hasOperatorChanged; }
            set
            {
                if (value != _hasOperatorChanged)
                {
                    OnNotifyPropertyChanging(HasOperatorChangedProperty);
                    _hasOperatorChanged = value;
                    OnNotifyPropertyChanged(HasOperatorChangedProperty);
                }
            }
        }

        public bool HasError { get; set; }

        public PriceInputKeyBoardHandler()
        {
            ClearTextBeforeNumbersInput = true;
            HasOperatorChanged = false;
        }

        public void HandleChar(CustomizedKey key)
        {
            var charWord = key.KeyChar;

            if (HasError)
            {
                if (charWord == KeyBoardWords.Clear)
                {
                    ClearDisplayText();
                }

                _lastKey = key;
                return;
            }

            if (key.Type == KeyType.Number)
            {
                SendingOperating = false;
                if (ClearTextBeforeNumbersInput)
                {
                    DisplayResult = string.Empty;
                    ClearTextBeforeNumbersInput = false;
                }

                if (charWord == KeyBoardWords.Clear)
                {
                    ClearDisplayText();
                    _lastKey = key;
                    return;
                }
                else if (charWord == KeyBoardWords.Dot)
                {
                    if (!DisplayResult.Contains(KeyBoardWords.Dot))
                    {
                        if (!SendingOperating)
                        {
                            if (displayResult.Length == 0)
                            {
                                displayResult += KeyBoardWords.Zero;
                            }

                            DisplayResult = displayResult + KeyBoardWords.Dot;
                        }
                    }
                }
                else
                {
                    if (charWord == KeyBoardWords.DoubleZero || charWord == KeyBoardWords.Zero)
                    {
                        if (displayResult.Length > 0)
                        {
                            if (displayResult[0].ToString() == KeyBoardWords.Zero)
                            {
                                if (!displayResult.Contains(KeyBoardWords.Dot))
                                {
                                    _lastKey = key;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            _lastKey = key;
                            DisplayResult = KeyBoardWords.Zero;
                            return;
                        }
                    }
                    else
                    {
                        if (displayResult.Length == 1 && displayResult[0].ToString() == KeyBoardWords.Zero)
                        {
                            displayResult = string.Empty;
                        }
                    }

                    if (!SendingOperating)
                    {
                        DisplayResult = displayResult + charWord;
                        lastStepValue = double.Parse(displayResult);
                    }
                }
            }
            else if (key.Type == KeyType.Operator)
            {
                if (key.Opeartor == KeyOperator.Close)
                {
                    _lastKey = key;
                    OnConfirmed(EventArgs.Empty);
                    return;
                }

                if (key.Opeartor == KeyOperator.Del)
                {
                    if (displayResult.Length > 1)
                    {
                        DisplayResult = displayResult.Remove(displayResult.Length - 1, 1);
                    }

                    if (displayResult.Length == 1)
                    {
                        DisplayResult = KeyBoardWords.Zero;
                    }

                }
                else if (key.Opeartor == KeyOperator.Clear)
                {
                    _lastKey = key;
                    ClearDisplayText();
                    return;
                }
                else
                {
                    ShowResult(key.Opeartor);
                }
            }

            _lastKey = key;
        }

        private bool sendingOpearting = false;

        public bool SendingOperating
        {
            get { return sendingOpearting; }
            set
            {
                if (sendingOpearting != value)
                {
                    OnNotifyPropertyChanging("SendingOperating");
                    sendingOpearting = value;
                    OnNotifyPropertyChanged("SendingOperating");
                }
            }
        }

        private bool isFirstOperator = true;
        public double lastStepValue { get; set; }

        private void ShowResult(KeyOperator keyOperator)
        {
            var currentValue = Double.Parse(displayResult);

            if (keyOperator == KeyOperator.GetResult)
            {
                currentValue = lastStepValue;

                if (keyOperator != KeyOperator.None)
                {
                    keyOperator = lastOperator;
                }

                HasOperatorChanged = false;

                SendingOperating = false;
            }
            else
            {
                HasOperatorChanged = true;
                SendingOperating = true;
            }

            ClearTextBeforeNumbersInput = true;
            LastOperator = keyOperator;

            if (lastOperator == KeyOperator.None)
            {
                result = lastStepValue;
            }

            if (isFirstOperator)
            {
                result = currentValue;
                isFirstOperator = false;
                return;
            }

            if (keyOperator != KeyOperator.GetResult && HasOperatorChanged)
            {
                if (_lastKey.Type == KeyType.Number)
                {
                    if (keyOperator == KeyOperator.Plus)
                        result += lastStepValue;
                    else if (keyOperator == KeyOperator.Minus)
                        result = result - lastStepValue;
                    else if (keyOperator == KeyOperator.Multiply)
                        result = result * lastStepValue;
                    else if (keyOperator == KeyOperator.Divide)
                    {
                        if (currentValue == 0)
                        {
                            DisplayResult = AppResources.DividerCantBeZero;
                            HasError = true;
                            return;
                        }
                        else
                        {
                            result = result / lastStepValue;
                        }
                    }
                }
                else
                {
                    lastStepValue = currentValue;
                }

                DisplayCalculatingResult(result);
                return;
            }

            if (keyOperator == KeyOperator.Plus)
                result += currentValue;
            else if (keyOperator == KeyOperator.Minus)
                result = result - currentValue;
            else if (keyOperator == KeyOperator.Multiply)
                result = result * currentValue;
            else if (keyOperator == KeyOperator.Divide)
            {
                if (currentValue == 0)
                {
                    DisplayResult = AppResources.DividerCantBeZero;
                    HasError = true;
                    return;
                }
                else
                {
                    result = result / currentValue;
                }
            }

            DisplayCalculatingResult(result);
        }

        private void DisplayCalculatingResult(double result)
        {
            this.DisplayResult = result.ToString(LocalizedStrings.CultureName.NumberFormat);
            ClearTextBeforeNumbersInput = true;
        }

        private void ClearDisplayText()
        {
            Result = 0;
            HasError = false;
            ClearTextBeforeNumbersInput = true;
            this.DisplayResult = KeyBoardWords.Zero;
            lastOperator = KeyOperator.GetResult;
            isFirstOperator = true;
        }

        protected virtual void OnConfirmed(EventArgs e)
        {
            var handle = this.Confirmed;
            if (handle != null)
                handle(this, e);
        }

        private bool sendingNumbers;
        private CustomizedKey _lastKey;

        public bool SendingNumbers
        {
            get { return sendingNumbers; }
            set
            {
                if (sendingNumbers != value)
                {
                    OnNotifyPropertyChanging("SendingNumbers");
                    sendingNumbers = value;
                    OnNotifyPropertyChanged("SendingNumbers");
                }
            }
        }

    }
}
