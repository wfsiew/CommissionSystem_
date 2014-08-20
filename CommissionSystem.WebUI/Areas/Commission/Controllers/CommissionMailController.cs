using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ActionMailer.Net.Mvc;
using CommissionSystem.WebUI.Areas.Commission.Models;
using CommissionSystem.WebUI.Helpers;
using CommissionSystem.Domain.ProtoBufModels;

namespace CommissionSystem.WebUI.Areas.Commission.Controllers
{
    public class CommissionMailController : MailerBase
    {
        public const string COMMISSIONNOTIFICATION_FIBREPLUS = "CommissionNotification_FibrePlus";
        public const string COMMISSIONNOTIFICATION_DATA = "CommissionNotification_Data";
        public const string COMMISSIONNOTIFICATION_VOICE = "CommissionNotification_Voice";

        public EmailResult CommissionNotificationEmail(CommissionResult c, EmailInfo mail, ViewDataDictionary viewData, string view)
        {
            foreach (string email in mail.ToList)
            {
                To.Add(email);
            }

            foreach (Attachment att in mail.AttList)
            {
                Attachments.Add(att.Filename, att.Data);
            }

            From = string.Format("{0} {1}", mail.DisplayName, Constants.MAIL_SENDER);
            Subject = mail.Subject;

            ViewData = viewData;

            return Email(view, c);
        }

        public EmailResult CommissionNotificationEmail(VoiceCommissionResult c, EmailInfo mail, ViewDataDictionary viewData, string view)
        {
            foreach (string email in mail.ToList)
            {
                To.Add(email);
            }

            foreach (Attachment att in mail.AttList)
            {
                Attachments.Add(att.Filename, att.Data);
            }

            From = string.Format("{0} {1}", mail.DisplayName, Constants.MAIL_SENDER);
            Subject = mail.Subject;

            ViewData = viewData;

            return Email(view, c);
        }
    }
}
