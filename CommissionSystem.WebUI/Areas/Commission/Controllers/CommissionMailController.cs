﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ActionMailer.Net.Mvc;
using CommissionSystem.WebUI.Areas.Commission.Models;
using CommissionSystem.WebUI.Helpers;

namespace CommissionSystem.WebUI.Areas.Commission.Controllers
{
    public class CommissionMailController : MailerBase
    {
        public EmailResult CommissionNotificationEmail(CommissionResult c, EmailInfo mail, ViewDataDictionary viewData)
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

            string view = "CommissionNotification";
            ViewData = viewData;

            return Email(view, c);
        }
    }
}