using System.Collections.Generic;
using System.Configuration;
using Notify.Client;
using Notify.Models;
using Extensions;
using System;
using System.Threading.Tasks;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap
{
    public class GovNotify: IGovNotify
    {
        const string ClientReference = "GpgAlphaTest";
        static string ApiKey = ConfigurationManager.AppSettings["GovNotifyApiKey"];

        public void SetStatus(string status)
        {
            throw new NotImplementedException();
        }

        public Notification SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation)
        {
            var client = new NotificationClient(ApiKey);
            var result = client.SendEmail(emailAddress, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;
        }

        public Notification SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation)
        {
            var client = new NotificationClient(ApiKey);
            var result = client.SendSms(mobileNumber, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;

        }

        public Notification SendPost(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation)
        {
            var client = new NotificationClient(ApiKey);
            var result = client.SendEmail(emailAddress, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;
        }
    }
}