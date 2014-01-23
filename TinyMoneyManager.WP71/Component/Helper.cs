namespace TinyMoneyManager.Component
{
    using Microsoft.Phone.Tasks;
    using System;
    using TinyMoneyManager.Data.Model;

    public class Helper
    {
        public static void SendEmail(string subject, string body)
        {
            string email = AppSetting.Instance.Email;
            new EmailComposeTask { To = email, Subject = subject, Body = body }.Show();
        }

        public static void SendEmailToTeam(string subject, string body)
        {
            string str = "rukai.yu@outlook.com";
            try
            {
                new EmailComposeTask { To = str, Subject = subject, Body = body }.Show();
            }
            catch
            {

            }
        }
    }
}

