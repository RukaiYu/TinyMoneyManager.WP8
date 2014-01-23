namespace TinyMoneyManager.Data
{
    using RapidRepository;
    using System;
    using System.Collections.Generic;
    using TinyMoneyManager.Data.Model;

    public class AppSettingRepository : RapidRepository.RapidRepository<AppSetting>
    {
        public static AppSettingRepository _instance;

        public AppSetting FirstOfDefault()
        {
            System.Collections.Generic.IList<AppSetting> all = this.GetAll();
            if ((all != null) && (all.Count != 0))
            {
                return all[0];
            }
            return null;
        }

        public static AppSettingRepository Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }
}

