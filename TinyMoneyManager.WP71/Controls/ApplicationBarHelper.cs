using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Linq;
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

        public List<TuplePredicator<TextBox, IApplicationBar>> _ifConditions;

        public IApplicationBar OriginalBar { get; set; }
        public IApplicationBar ApplicationBarForEditor { get; set; }

        /// <summary>
        /// Gets the source page.
        /// </summary>
        /// <value>
        /// The source page.
        /// </value>
        public PhoneApplicationPage SourcePage { get { return pageAttached; } }

        public bool SelectContentWhenFocus { get; set; }


        public ApplicationBarHelper(PhoneApplicationPage targetPage)
        {
            this.textBoxs = new List<TextBox>();
            this.pageAttached = targetPage;
            this.OriginalBar = pageAttached.ApplicationBar;
            this._ifConditions = new List<TuplePredicator<TextBox, IApplicationBar>>();
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

            var _tb = sender as TextBox;

            var ifCondition = _ifConditions.FirstOrDefault(p => p.IfCondition(_tb) == true);

            if (ifCondition != null)
            {
                ifCondition.WhenFalse(ApplicationBarForEditor);
            }
        }

        void box_GotFocus(object sender, RoutedEventArgs e)
        {
            this.pageAttached.ApplicationBar = ApplicationBarForEditor;

            if (SelectContentWhenFocus)
            {
                var textBox = sender as TextBox;
                textBox.SelectAll();

                var ifCondition = _ifConditions.FirstOrDefault(p => p.IfCondition(textBox) == true);

                if (ifCondition != null)
                {
                    ifCondition.WhenTrue(ApplicationBarForEditor);
                }
            }
        }

        /// <summary>
        /// Ifs the specified if condition.
        /// </summary>
        /// <param name="ifCondition">If condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns></returns>
        public ApplicationBarHelper If(Func<TextBox, bool> ifCondition, Action<IApplicationBar> ifTrue, Action<IApplicationBar> ifFalse)
        {
            this._ifConditions.Add(new TuplePredicator<TextBox, IApplicationBar>()
            {
                IfCondition = ifCondition,
                WhenFalse = ifFalse,
                WhenTrue = ifTrue,
            });

            return this;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class TuplePredicator<TSource, TValue>
    {
        /// <summary>
        /// Gets or sets if condition.
        /// </summary>
        /// <value>
        /// If condition.
        /// </value>
        public Func<TSource, bool> IfCondition { get; set; }

        /// <summary>
        /// Gets or sets if true.
        /// </summary>
        /// <value>
        /// If true.
        /// </value>
        public Action<TValue> WhenTrue { get; set; }

        /// <summary>
        /// Gets or sets if false.
        /// </summary>
        /// <value>
        /// If false.
        /// </value>
        public Action<TValue> WhenFalse { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TuplePredicator" /> class.
        /// </summary>
        public TuplePredicator()
        {

        }
    }
}
