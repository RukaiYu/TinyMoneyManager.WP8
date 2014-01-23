using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.Controls
{
    public class ApplicationBarHelper
    {
        private PhoneApplicationPage pageAttached;
        private List<TextBox> textBoxs;

        public TextBox[] TextBoxs
        {
            get { return textBoxs.ToArray(); }
        }

        public Func<TextBox, bool> IfCondition;

        public Action<IApplicationBar> ifTrueAction;

        public Action<IApplicationBar> ifFalseAction;


        public IApplicationBar OriginalBar { get; set; }
        public IApplicationBar ApplicationBarForEditor { get; set; }

        public bool SelectContentWhenFocus { get; set; }

        public ApplicationBarHelper(PhoneApplicationPage targetPage)
        {
            this.textBoxs = new List<TextBox>();
            this.pageAttached = targetPage;
            this.OriginalBar = pageAttached.ApplicationBar;
            InitializeEditingBar();
        }

        private void InitializeEditingBar()
        {
            ApplicationBarIconButton okButton = new ApplicationBarIconButton();
            okButton.IconUri = new Uri("/icons/appbar.check.rest.png", UriKind.RelativeOrAbsolute);
            okButton.Text = pageAttached.GetLanguageInfoByKey("OK");
            okButton.Click += new EventHandler((o, e) => pageAttached.Focus());

            ApplicationBarForEditor = new ApplicationBar() { Opacity = 0.78 };
            ApplicationBarForEditor.Buttons.Add(okButton);
            //textBoxProperty = new Dictionary<TextBox, bool>();
        }

        public ApplicationBarHelper AddTextBox(params TextBox[] textBox)
        {
            foreach (var box in textBox)
            {
                box.GotFocus += new RoutedEventHandler(box_GotFocus);
                box.LostFocus += new RoutedEventHandler(box_LostFocus);
                box.KeyDown += new KeyEventHandler(box_KeyUp);
            }
            return this;
        }

        public ApplicationBarHelper AddTextBox(bool registerKeyDown, params TextBox[] textBox)
        {
            foreach (var box in textBox)
            {
                box.GotFocus += new RoutedEventHandler(box_GotFocus);
                box.LostFocus += new RoutedEventHandler(box_LostFocus);
                if (registerKeyDown)
                    box.KeyDown += new KeyEventHandler(box_KeyUp);
                //textBoxs.Add(box);
            }
            return this;
        }

        void box_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pageAttached.Focus();
                pageAttached.ApplicationBar = OriginalBar;
            }
        }

        void box_LostFocus(object sender, RoutedEventArgs e)
        {
            this.pageAttached.Focus();
            this.pageAttached.ApplicationBar = OriginalBar;

            if (IfCondition != null)
            {
                if (IfCondition(sender as TextBox))
                {
                    ifFalseAction(ApplicationBarForEditor);
                }
            }
        }

        void box_GotFocus(object sender, RoutedEventArgs e)
        {
            this.pageAttached.ApplicationBar = ApplicationBarForEditor;


            if (SelectContentWhenFocus)
            {
                var textBox = sender as TextBox;
                textBox.SelectAll();

                if (IfCondition != null)
                {
                    if (IfCondition(textBox))
                    {
                        ifTrueAction(ApplicationBarForEditor);
                    }
                    else
                    {
                        ifFalseAction(ApplicationBarForEditor);
                    }
                }
            }
        }

        public void If(Func<TextBox, bool> ifCondition, Action<IApplicationBar> ifTrue, Action<IApplicationBar> ifFalse)
        {
            this.IfCondition = ifCondition;
            this.ifTrueAction = ifTrue;
            this.ifFalseAction = ifFalse;
        }
    }
}
