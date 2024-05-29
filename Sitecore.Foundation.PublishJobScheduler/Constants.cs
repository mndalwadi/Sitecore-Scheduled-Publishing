using Sitecore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Foundation.PublishJobScheduler
{
    public class Constants
    {
        public struct SMTP
        {
            public static readonly string MailServer = Settings.GetSetting("MailServer");
            public static readonly string MailServerUserName = Settings.GetSetting("MailServerUserName");
            public static readonly string MailServerPassword = Settings.GetSetting("MailServerPassword");

            public static readonly string MailServerSenderMail = Settings.GetSetting("MailServerSenderMail");
            public static readonly string MailServerSenderName = Settings.GetSetting("MailServerSenderName");
            public static readonly string MailServerSenderMailPassword = Settings.GetSetting("MailServerSenderMailPassword");

            public static readonly string MailServerPort = Settings.GetSetting("MailServerPort");
            public static readonly string MailServerUseSsl = Settings.GetSetting("MailServerUseSsl");

            public static readonly string MailServerSUbject = Settings.GetSetting("MailServerSUbject");

            public static readonly string MailServerReceiverMail = Settings.GetSetting("MailServerReceiverMail");
        }
    }
}