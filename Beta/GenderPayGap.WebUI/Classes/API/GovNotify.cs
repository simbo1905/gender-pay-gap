using System.Collections.Generic;
using System.Configuration;
using Notify.Client;
using Notify.Models;
using System;
using Extensions;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap
{
    public class GovNotify: IGovNotify
    {
        const string ClientReference = "GpgAlphaTest";
        static string _apiKey = ConfigurationManager.AppSettings["GovNotifyApiKey"];
        static string _apiTestKey = ConfigurationManager.AppSettings["GovNotifyApiTestKey"];

        public void SetStatus(string status)
        {
            throw new NotImplementedException();
        }

        public Notification SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation, bool test = false)
        {
            var client = new NotificationClient(test && !string.IsNullOrWhiteSpace(_apiTestKey) ? _apiTestKey : _apiKey);
            var result = client.SendEmail(emailAddress, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;
        }

        public Notification SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation, bool test = false)
        {
            var client = new NotificationClient(test && !string.IsNullOrWhiteSpace(_apiTestKey) ? _apiTestKey : _apiKey);
            var result = client.SendSms(mobileNumber, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;

        }

        public Notification SendPost(string address, string templateId, Dictionary<string, dynamic> personalisation, bool test = false)
        {
            var client = new NotificationClient(test && !string.IsNullOrWhiteSpace(_apiTestKey) ? _apiTestKey : _apiKey);
            var result = client.SendEmail(address, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;
        }
    }
}