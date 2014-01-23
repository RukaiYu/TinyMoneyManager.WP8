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

using NkjSoft.WPhone.Extensions;
namespace TinyMoneyManager.Controls
{
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    public partial class Calcutor : UserControl
    {
        public PriceInputKeyBoardHandler KeyHandler { get; set; }

        public Calcutor()
        {
            InitializeComponent();

            KeyHandler = new PriceInputKeyBoardHandler();
            this.DataContext = this;
            IntializedKeys();
        }

        private void IntializedKeys()
        {
            var keyDiv = KeyHandler.CreateKey(KeyBoardKyes.Divide, 0, 0);
            var keyMulti = KeyHandler.CreateKey(KeyBoardKyes.Multiply, 0, 1);
            var keyMinus = KeyHandler.CreateKey(KeyBoardKyes.Minus, 0, 2);
            var keyPlus = KeyHandler.CreateKey(KeyBoardKyes.Plus, 0, 3);

            var key7 = KeyHandler.CreateKey(KeyBoardKyes.Seven, 1, 0);
            var key8 = KeyHandler.CreateKey(KeyBoardKyes.Eight, 1, 1);
            var key9 = KeyHandler.CreateKey(KeyBoardKyes.Nine, 1, 2);
            var keyDel = KeyHandler.CreateKey(KeyBoardKyes.Backspace, 1, 3);

            var key4 = KeyHandler.CreateKey(KeyBoardKyes.Four, 2, 0);
            var key5 = KeyHandler.CreateKey(KeyBoardKyes.Five, 2, 1);
            var key6 = KeyHandler.CreateKey(KeyBoardKyes.Six, 2, 2);
            var keyClear = KeyHandler.CreateKey(KeyBoardKyes.Clear, 2, 3);

            var key1 = KeyHandler.CreateKey(KeyBoardKyes.One, 3, 0);
            var key2 = KeyHandler.CreateKey(KeyBoardKyes.Two, 3, 1);
            var key3 = KeyHandler.CreateKey(KeyBoardKyes.Three, 3, 2);
            var keyEnter = KeyHandler.CreateKey(KeyBoardKyes.Enter, 3, 3);

            var key0 = KeyHandler.CreateKey(KeyBoardKyes.Zero, 4, 0);
            var key00 = KeyHandler.CreateKey(KeyBoardKyes.DoubleZero, 4, 1);
            var keyDot = KeyHandler.CreateKey(KeyBoardKyes.Dot, 4, 2);
            var keyOk = KeyHandler.CreateKey(KeyBoardKyes.Ok, 4, 3);
            keyOk.Background = App.SystemAccentBrush;


            var keys = new KeyBoardKey[] {
            keyDiv   ,
            keyMulti ,
            keyMinus ,
            keyPlus  ,
 
            key7     ,
            key8     ,
            key9     ,
            keyDel   ,
       
            key4     ,
            key5     ,
            key6     ,
            keyClear ,
       
            key1     ,
            key2     ,
            key3     ,
            keyEnter ,
       
            key0     ,
            key00    ,
            keyDot   ,
            keyOk    ,
            };

            foreach (var key in keys)
            {
                key.SetRow(key.GridRowIndex, key.GridColumnIndex)
                    .AddToGrid(KeysContainer);
            }

            KeyHandler.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(KeyHandler_PropertyChanged);
        }

        void KeyHandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayResult")
            {
                var length = KeyHandler.DisplayResult.Length;
                if (length <= 11)
                    ResultText.FontSize = 72;
                else if (length > 12 && length < 25)
                {
                    ResultText.FontSize = 48;
                }
                else if (length > 25)
                {
                    ResultText.FontSize = 32;
                }
            }
            else if (e.PropertyName == "LastOperator" || e.PropertyName == PriceInputKeyBoardHandler.HasOperatorChangedProperty)
            {
                if (KeyHandler.SendingOperating)
                {
                    Operator.Text = "{0} {1}".FormatWith(KeyHandler.lastStepValue.ToString("F2", LocalizedStrings.CultureName), KeyHandler.LastOperator.ToString());
                }
                else
                {
                    Operator.Text = "{0} {1}".FormatWith(KeyHandler.DisplayResult, KeyHandler.LastOperator.ToString());
                }
            }
        }

        void ResultText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public static string PageRef = "/Pages/DialogBox/AmountInputBox.xaml?defValue={0}";
    }
}
