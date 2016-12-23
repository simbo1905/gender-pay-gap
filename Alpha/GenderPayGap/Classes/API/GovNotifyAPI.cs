using System.Collections.Generic;
using System.Configuration;
using Notify.Client;
using Notify.Models;
using Extensions;
using System;

namespace GenderPayGap
{
    public static class GovNotifyAPI
    {
        const string ClientReference = "GpgAlphaTest";
        static string ApiKey = ConfigurationManager.AppSettings["GovNotifyApiKey"];
        static string VerifyTemplateId = ConfigurationManager.AppSettings["GovNotifyVerifyTemplateId"];
        static string PINTemplateId = ConfigurationManager.AppSettings["GovNotifyPINTemplateId"];
        static string ConfirmTemplateId = ConfigurationManager.AppSettings["GovNotifyConfirmTemplateId"];

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

            var personalisation = new Dictionary<string, dynamic> { { "url", url } };

            Notification result = null;
            try
            {
                result = SendEmail(emailAddress, VerifyTemplateId, personalisation);
            }
            catch (Exception ex)
            {
                if (ex.Message.EqualsI("This Email Address is not registered with Gov Notify."))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/verify.txt"));
                        Email.QuickSend("GPG Registration Verification", emailAddress, html);
                        result = new Notification() { status = "delivered" };
                    }
                    catch (Exception ex1)
                    {
                        
                    }
                }
            }
            return result.status.EqualsI("created", "sending", "delivered");
        }

        public static string GetVerifyUrl(string verifyCode)
        {
            return string.Format("{0}/Register/Verify?code={1}", ConfigurationManager.AppSettings["GpgWebServer"], verifyCode);
        }

        public static bool SendConfirmEmail(string emailAddress, string confirmCode)
        {
            string url = GetConfirmUrl(confirmCode);

            var personalisation = new Dictionary<string, dynamic> { { "url", url } };

            var result = SendEmail(emailAddress, ConfirmTemplateId, personalisation);

            return result.status.EqualsI("created", "sending", "delivered");
        }

        public static string GetConfirmUrl(string confirmCode)
        {
            return string.Format("{0}/Register/Confirm?code={1}", ConfigurationManager.AppSettings["GpgWebServer"], confirmCode);
        }

        public static bool SendPinInPost(string name, string address, string pin)
        {
            var personalisation = new Dictionary<string, dynamic> { { "PIN", pin } };

            var result = SendEmail(address, PINTemplateId, personalisation);

            return result.status.EqualsI("created", "sending", "delivered");
        }
    }
}