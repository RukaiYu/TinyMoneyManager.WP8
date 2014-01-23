namespace TinyMoneyManager.Data
{
    using System;
    using TinyMoneyManager.Component;

    public class ApplicationStarter
    {
        public static AppSettingRepository Instance
        {
            get
            {
                return ViewModelLocator.instanceLoader.LoadSingelton<AppSettingRepository>("AppSettingRepository");
            }
        }
    }
}

