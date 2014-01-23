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
using TinyMoneyManager.Data.Model;
using System.ComponentModel;
using TinyMoneyManager.Component;

namespace TinyMoneyManager.ViewModels
{
    public abstract class PasswordSettingViewModel : NotionObject
    {
        public string PageTitle { get; set; }
        private string currentPasswordError;
        public string CurrentPasswordError
        {
            get
            {
                return currentPasswordError;
            }
        }
        private string newPasswordConfrimError;
        public string NewPasswordConfirmError
        {
            get
            {
                return newPasswordConfrimError;
            }
        }

        protected AppSetting appSetting;

        public PasswordSettingViewModel(AppSetting app)
        {
            this.appSetting = app;

            this.PageTitle = LocalizedStrings.GetLanguageInfoByKey("EnableLockPassword");
            this.currentPasswordError = LocalizedStrings.GetLanguageInfoByKey("CurrentPasswordError");
            this.newPasswordConfrimError = LocalizedStrings.GetLanguageInfoByKey("NewPasswordConfrimError");
        }

        public virtual void Submit(string newPassword)
        {
            AppSetting.Instance.EnablePoketLock = true;
            AppSetting.Instance.Password = newPassword;

            if (AppSetting.Instance.ShowRepaymentInfoOnTile)
            {
                AppSetting.Instance.ShowRepaymentInfoOnTile = false;
            }

            new TinyMoneyManager.Data.AppSettingRepository().Update(appSetting);
        }

        public virtual string ValidatePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword.Trim().Length == 0 || newPassword.Trim() != confirmPassword)
                return NewPasswordConfirmError;
            return string.Empty;
        }

        private Visibility isChangeingPassword;
        public Visibility CurrentPasswordVisibility
        {
            get
            {

                return isChangeingPassword;
            }
            set
            {
                this.isChangeingPassword = value;
                OnNotifyPropertyChanged("CurrentPasswordVisibility");
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class ChangePasswordViewModel : PasswordSettingViewModel
    {
        public ChangePasswordViewModel(AppSetting app)
            : base(app)
        {
            this.PageTitle = LocalizedStrings.GetLanguageInfoByKey("ChangePasswordButtonText");
        }

        public override string ValidatePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (currentPassword.Trim() != appSetting.Password)
            {
                return this.CurrentPasswordError;
            }
            return base.ValidatePassword(currentPassword, newPassword, confirmPassword);
        }
    }

    public class NewPasswordViewModel : PasswordSettingViewModel
    {
        public NewPasswordViewModel(AppSetting app)
            : base(app)
        {
            this.CurrentPasswordVisibility = Visibility.Collapsed;
        }
    }
}
