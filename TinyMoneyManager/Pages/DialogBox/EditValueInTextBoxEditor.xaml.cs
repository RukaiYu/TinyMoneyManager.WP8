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
using TinyMoneyManager.Language;

using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;

using TinyMoneyManager.Component;
using TinyMoneyManager.Component.Common;
namespace TinyMoneyManager.Pages.DialogBox
{
    public static class DialogBoxesExtension
    {
        public static void NavigateToEditValueInTextBoxEditorPage(this PhoneApplicationPage page, string keyName, object val = null)
        {
            NkjSoft.WPhone.Extensions.WindowsPhoneExtensions.NavigateTo(page, "/Pages/DialogBox/EditValueInTextBoxEditor.xaml?keyName={0}&value={1}", keyName, val);

        }

        public static void NavigateToEditValueInTextBoxEditorPage(this PhoneApplicationPage page, string keyName, object defValue = null,
            Action<TextBox> attrSetter = null, Func<string, bool> resultValidator = null, Action<string> resultSetter = null)
        {

            EditValueInTextBoxEditor.TextBoxAttributeSetter = attrSetter;

            EditValueInTextBoxEditor.ResultCallBack = resultSetter;

            EditValueInTextBoxEditor.ValidatingCallBack = resultValidator;

            NavigateToEditValueInTextBoxEditorPage(page, keyName, defValue);
        }
    }

    public partial class EditValueInTextBoxEditor : PhoneApplicationPage
    {
        private string keyNameToEdit;
        public string KeyNameToEdit
        {
            get { return keyNameToEdit; }
            set
            {
                if (KeyNameBlock.Text != value)
                {
                    PageTitle.Text = (AppResources.Edit + " " + value).ToUpper();

                    keyNameToEdit = value;

                    KeyNameBlock.Text = value;
                }
            }
        }

        public static Action<string> ResultCallBack;
        public static Func<string, bool> ValidatingCallBack;

        public static Action<TextBox> TextBoxAttributeSetter;

        public EditValueInTextBoxEditor()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(EditValueInTextBoxEditor_Loaded);

            this.KeyNameResultBox.KeyDown += new KeyEventHandler(KeyNameResultBox_KeyDown);

            this.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.Save;
        }

        void KeyNameResultBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
                SaveButton_Click(sender, e);
            }
        }

        void EditValueInTextBoxEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyNameResultBox.SelectionStart = this.KeyNameResultBox.Text.Length;

            if (TextBoxAttributeSetter != null)
                TextBoxAttributeSetter(this.KeyNameResultBox);

            this.KeyNameResultBox.Focus();
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {  
            if (ValidatingCallBack != null)
            {
                if (!ValidatingCallBack(this.KeyNameResultBox.Text))
                    return;
            }
             
            if (ResultCallBack != null)
            {
                ResultCallBack(this.KeyNameResultBox.Text);

                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                return;

            KeyNameToEdit = this.GetNavigatingParameter("keyName");

            this.KeyNameResultBox.Text = this.GetNavigatingParameter("value");
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            TextBoxAttributeSetter = null;
            ValidatingCallBack = null;
            ResultCallBack = null;
        }
    }
}