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
using TinyMoneyManager.Language;

namespace TinyMoneyManager.Controls.CalcutorKeyBoard
{
    public class KeyBoardWords
    {
        public const string Clear = "C";
        public const string One = "1";
        public const string Two = "2";
        public const string Three = "3";
        public const string Four = "4";
        public const string Five = "5";
        public const string Six = "6";
        public const string Seven = "7";
        public const string Eight = "8";
        public const string Nine = "9";
        public const string Zero = "0";
        public const string DoubleZero = "00";
        public const string Dot = ".";
        public const string Plue = "＋";
        public const string Minus = "－";
        public const string Multiply = "×";
        public const string Divide = "÷";

        public const string Enter = "＝";
        public const string BackSpace = "DEL";
        public static string OK
        {
            get { return AppResources.OK; }
        }
    }

    public class KeyBoardKyes
    {
        public static CustomizedKey Clear = new CustomizedKey(KeyBoardWords.Clear, KeyType.Operator, KeyOperator.Clear);
        public static CustomizedKey One = new CustomizedKey(KeyBoardWords.One, KeyType.Number);
        public static CustomizedKey Two = new CustomizedKey(KeyBoardWords.Two, KeyType.Number);
        public static CustomizedKey Three = new CustomizedKey(KeyBoardWords.Three);
        public static CustomizedKey Four = new CustomizedKey(KeyBoardWords.Four);
        public static CustomizedKey Five = new CustomizedKey(KeyBoardWords.Five);
        public static CustomizedKey Six = new CustomizedKey(KeyBoardWords.Six);
        public static CustomizedKey Seven = new CustomizedKey(KeyBoardWords.Seven);
        public static CustomizedKey Eight = new CustomizedKey(KeyBoardWords.Eight);
        public static CustomizedKey Nine = new CustomizedKey(KeyBoardWords.Nine);
        public static CustomizedKey Zero = new CustomizedKey(KeyBoardWords.Zero);
        public static CustomizedKey DoubleZero = new CustomizedKey(KeyBoardWords.DoubleZero);
        public static CustomizedKey Backspace = new CustomizedKey(KeyBoardWords.BackSpace, KeyType.Operator, KeyOperator.Del);
        public static CustomizedKey Dot = new CustomizedKey(KeyBoardWords.Dot);

        public static CustomizedKey Plus = new CustomizedKey(KeyBoardWords.Plue, KeyType.Operator, KeyOperator.Plus);
        public static CustomizedKey Minus = new CustomizedKey(KeyBoardWords.Minus, KeyType.Operator, KeyOperator.Minus);
        public static CustomizedKey Multiply = new CustomizedKey(KeyBoardWords.Multiply, KeyType.Operator, KeyOperator.Multiply);
        public static CustomizedKey Divide = new CustomizedKey(KeyBoardWords.Divide, KeyType.Operator, KeyOperator.Divide);
        public static CustomizedKey Enter = new CustomizedKey(KeyBoardWords.Enter, KeyType.Operator, KeyOperator.GetResult);

        public static CustomizedKey Ok = new CustomizedKey(KeyBoardWords.OK, KeyType.Operator, KeyOperator.Close);
    }

    public class CustomizedKey
    {
        public string KeyChar { set; get; }
        public KeyType Type { get; set; }

        public KeyOperator Opeartor { get; set; }

        public CustomizedKey()
            : this(string.Empty, KeyType.Number, KeyOperator.None)
        {

        }

        public CustomizedKey(string keyChar, KeyType keyType = KeyType.Number, KeyOperator opt = KeyOperator.None)
        {
            this.KeyChar = keyChar;
            this.Type = keyType;
            this.Opeartor = opt;
        }


    }

    public enum KeyType
    {
        Number,
        Operator,
    }

    public enum KeyOperator
    {
        None,
        Plus,
        Minus,
        Multiply,
        Divide,
        Clear,
        GetResult,
        Del,
        Close,
    }
}
