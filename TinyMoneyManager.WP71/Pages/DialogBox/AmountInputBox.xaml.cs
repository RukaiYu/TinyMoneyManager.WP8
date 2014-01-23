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

using NkjSoft.WPhone.Extensions;
using NkjSoft.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Component.Common;
namespace TinyMoneyManager.Pages.DialogBox
{
    public partial class AmountInputBox : PhoneApplicationPage
    {
        Controls.Calcutor amountInputCalcutor;

        public static double Result { get; set; }

        public static Action<object, double> Confirmed;

        public AmountInputBox()
        {
            InitializeComponent();

            amountInputCalcutor = new Controls.Calcutor();
            amountInputCalcutor.KeyHandler.Confirmed += new EventHandler<EventArgs>(keyHandler_Confirmed);
            ContentPanel.Children.Add(amountInputCalcutor);
        }

        void keyHandler_Confirmed(object sender, EventArgs e)
        {
            Result = amountInputCalcutor.KeyHandler.Result;

            this.SafeGoBack();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                return;

            var defValue = this.GetNavigatingParameter("defValue").ToDecimal(() => 0);

            (this.amountInputCalcutor.KeyHandler as Controls.CalcutorKeyBoard.PriceInputKeyBoardHandler)
                .DisplayResult = defValue.ToMoneyF2(LocalizedStrings.CultureName);

        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (Confirmed != null)
            {
                Confirmed(this, Result);
            }
        }
    }
}