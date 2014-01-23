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
using TinyMoneyManager.Data;
using System.ComponentModel;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using TinyMoneyManager.Component;
using Microsoft.Phone.Controls;

namespace TinyMoneyManager.Controls.PeopleManager
{
    using NkjSoft.Extensions;
    using TinyMoneyManager.Data;
    public partial class PeopleInfoEditor : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnNotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                //Deployment.Current.Dispatcher.BeginInvoke(delegate
                //{
                handler(this, new PropertyChangedEventArgs(propertyName));
                //});
            }
        }

        public PageActionType PageAction { get; set; }
        private string controlActionTitle;

        public string ControlActionTitle
        {
            get { return controlActionTitle; }
            set
            {
                if (controlActionTitle != value)
                {
                    controlActionTitle = value;
                    OnNotifyPropertyChanged("ControlActionTitle");
                }
            }
        }

        public ViewModels.PeopleViewModel peopleProfileManagerViewModel;

        public ApplicationBarHelper aph;

        private PeopleProfile currentObject;

        public PeopleProfile CurrentObject
        {
            get { return currentObject; }
            set
            {
                if (currentObject != value)
                {
                    currentObject = value;
                    OnNotifyPropertyChanged("CurrentObject");
                }
            }
        }

        public PeopleInfoEditor(PhoneApplicationPage page)
        {
            InitializeComponent();

            aph = new ApplicationBarHelper(page);

            aph.AddTextBox(PeopleName, DescriptionTextBox, EmailBox);

            ControlActionTitle = AppResources.AddPerson;
            peopleProfileManagerViewModel = ViewModelLocator.PeopleViewModel;

            this.CurrencyType.ItemsSource = CurrencyHelper.CurrencyTable;

            this.CurrencyType.SelectedItem = CurrencyHelper.CurrencyTable.FirstOrDefault(p => p.Currency == AppSetting.Instance.DefaultCurrency);

            this.DataContext = this;
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            MoreInfoPanel.Visibility = Visibility.Visible;
            MoreInfoButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Initializes the edit object.
        /// </summary>
        /// <param name="peopleProfile">The people profile.</param>
        public void InitializeEditObject(PeopleProfile peopleProfile, bool isAdding = true)
        {
            CurrentObject = peopleProfile;

            isAdding = currentObject.Id == Guid.Empty;
            PageAction = PageActionType.Add;
            ControlActionTitle = AppResources.AddPerson.ToUpperInvariant();

            if (!isAdding)
            {
                PageAction = PageActionType.Edit;
                ControlActionTitle = AppResources.EditPerson.ToUpperInvariant();
            }
        }

        /// <summary>
        /// Gets the edited current object.
        /// </summary>
        /// <returns></returns>
        public PeopleProfile GetEditedCurrentObject()
        {
            CurrentObject.Name = PeopleName.Text;
            CurrentObject.Notes = DescriptionTextBox.Text;
            CurrentObject.PersonalEmail = EmailBox.Text;
            CurrentObject.CurrencyInfo = CurrencyType.SelectedItem as CurrencyWapper;
            return CurrentObject;
        }

        public bool Save()
        {
            if (!isEmailValidated)
            {
                this.AlertNotification(AppResources.NotAvaliableEmailAddressMessage);
                return false;
            }

            if (string.IsNullOrEmpty(PeopleName.Text))
            {
                this.AlertNotification("{0} {1}".FormatWith(AppResources.Name, AppResources.EmptyTextMessage));
                return false;
            }

            var obj = GetEditedCurrentObject();

            bool isToAdd = false;

            if (currentObject.Id == Guid.Empty)
            {
                isToAdd = true;
                currentObject.Id = Guid.NewGuid();
                currentObject.AssociatedGroup = peopleProfileManagerViewModel.AccountBookDataContext.PeopleGroups.FirstOrDefault();
            }
            else
            {
                if (currentObject.GroupId == Guid.Empty)
                {
                    currentObject.AssociatedGroup = peopleProfileManagerViewModel.AccountBookDataContext.PeopleGroups.FirstOrDefault();
                }
            }

            peopleProfileManagerViewModel.Save(obj, isToAdd);
            return true;
        }

        bool isEmailValidated = true;
        private void EmailBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EmailBox != null && EmailBox.Text.Length > 0)
            {
                if (!AppSetting.IsEmail(EmailBox.Text))
                {
                    EmailValidationMsg.Text = AppResources.NotAvaliableEmailAddressMessage;
                    isEmailValidated = false;
                }
                else
                {
                    isEmailValidated = true;
                    EmailValidationMsg.Text = string.Empty;
                }
            }
            else
            {
                isEmailValidated = true;
            }
        }

    }
}
