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
using TinyMoneyManager.Component.Common;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager.ViewModels.AppSettingManager
{
    public class AppSettingListener : TwoLineListerner<AppSetting>
    {
        public AppSettingListener(string key)
            : base(key)
        {

        }
    }
}
