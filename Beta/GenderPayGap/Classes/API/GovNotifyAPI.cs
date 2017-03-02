using System.Collections.Generic;
using System.Configuration;
using Notify.Client;
using Notify.Models;
using Extensions;
using System;
using System.Threading.Tasks;
using GenderPayGap.WebUI.Classes;
using Autofac.Core;
using Autofac;

namespace GenderPayGap
{
    public static class GovNotifyAPI
    {
        static string VerifyTemplateId = ConfigurationManager.AppSettings["GovNotifyVerifyTemplateId"];
        static string PINTemplateId = ConfigurationManager.AppSettings["GovNotifyPINTemplateId"];
        static string ConfirmTemplateId = ConfigurationManager.AppSettings["GovNotifyConfirmTemplateId"];

        static IGovNotify GovNotify;

        public static void Initialise(IContainer container)
        {
            GovNotify=container.Resolve<IGovNotify>();
        }

        public static bool SendVerifyEmail(string verifyUrl,string emailAddress, string verifyCode)
        {
            var personalisation = new Dictionary<string, dynamic> { { "url", verifyUrl } };

            Notification result = null;
            try
            {
                result = GovNotify.SendEmail(emailAddress, VerifyTemplateId, personalisation);
            }
            catch (Exception ex)
            {
                if (ex.Message.ContainsI("This Email Address is not registered with Gov Notify.", "Can’t send to this recipient", "invalid token"))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/verify.txt"));
                        html = html.Replace("((VerifyUrl))", verifyUrl);
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

        public static bool SendConfirmEmail(string confirmUrl,string emailAddress)
        {
            var personalisation = new Dictionary<string, dynamic> { { "url", confirmUrl } };

            Notification result = null;
            try
            {
                result = GovNotify.SendEmail(emailAddress, ConfirmTemplateId, personalisation);
            }
            catch (Exception ex)
            {
                if (ex.Message.ContainsI("This Email Address is not registered with Gov Notify.", "Can’t send to this recipient", "invalid token"))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/Confirm.txt"));
                        html = html.Replace("((ConfirmUrl))", confirmUrl);
                        Email.QuickSend("GPG Registration Confirmation", emailAddress, html);
                        result = new Notification() { status = "delivered" };
                    }
                    catch (Exception ex1)
                    {

                    }
                }
            }

            return result.status.EqualsI("created", "sending", "delivered");
        }

        public static bool SendPinInPost(string returnUrl,string name, string address, string pin)
        {
            var personalisation = new Dictionary<string, dynamic> { { "PIN", pin } };

            Notification result = null;
            try
            {
                result= GovNotify.SendPost(address, PINTemplateId, personalisation);
            }
            catch (Exception ex)
            {
                if (ex.Message.ContainsI("This Email Address is not registered with Gov Notify.", "Can’t send to this recipient", "invalid token"))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/Pin.txt"));
                        html = html.Replace("((PIN))", pin);
                        Email.QuickSend("GPG Registration Confirmation", address, html);
                        result = new Notification() { status = "delivered" };
                    }
                    catch (Exception ex1)
                    {

                    }
                }
            }

            return result.status.EqualsI("created", "sending", "delivered");
        }
    }
}