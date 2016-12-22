using System.Collections.Generic;
using System.Configuration;
using Notify.Client;
using Notify.Models;
using Extensions;

namespace GenderPayGap
{
    public static class GovNotifyAPI
    {
        const string ClientReference = "GpgAlphaTest";
        static string ApiKey = ConfigurationManager.AppSettings["GovNotifyApiKey"];
        static string VerifyTemplateId = ConfigurationManager.AppSettings["GovNotifyVerifyTemplateId"];

        private static Notification SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation)
        {
            var client = new NotificationClient(ApiKey);
            var result = client.SendEmail(emailAddress, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;
        }

        private static Notification SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation)
        {
            var client = new NotificationClient(ApiKey);
            var result = client.SendSms(mobileNumber, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;

        }

        public static bool SendVerifyEmail(string emailAddress, string verifyCode)
        {
            string url = GetVerifyUrl(verifyCode);

            var personalisation = new Dictionary<string, dynamic>{{ "url", url }};

            var result = SendEmail(emailAddress, VerifyTemplateId, personalisation);

            return result.status.EqualsI("created","sending","delivered");
        }

        public static string GetVerifyUrl(string verifyCode)
        {
            return string.Format("{0}/Register/Verify?code={1}", ConfigurationManager.AppSettings["GpgWebServer"], verifyCode);
        }
    }
}