﻿using System.Collections.Generic;
using System.Configuration;
using Notify.Models;
using Extensions;
using System;
using GenderPayGap.WebUI.Classes;
using Autofac;
using GenderPayGap.Models.SqlDatabase;

namespace GenderPayGap
{
    public static class GovNotifyAPI
    {
        static string VerifyTemplateId = ConfigurationManager.AppSettings["GovNotifyVerifyTemplateId"];
        static string PINTemplateId = ConfigurationManager.AppSettings["GovNotifyPINTemplateId"];
        static string ConfirmTemplateId = ConfigurationManager.AppSettings["GovNotifyConfirmTemplateId"];
        static string RegistrationRequestTemplateId = ConfigurationManager.AppSettings["GovNotifyRegistrationRequestTemplateId"];
        public static string RegistrationRequestEmailAddress = ConfigurationManager.AppSettings["RegistrationRequestEmailAddress"];
        static string RegistrationApprovedTemplateId = ConfigurationManager.AppSettings["GovNotifyRegistrationApprovedTemplateId"];
        static string RegistrationDeclinedTemplateId = ConfigurationManager.AppSettings["GovNotifyRegistrationDeclinedTemplateId"];

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
                if (!result.status.EqualsI("created", "sending", "delivered"))throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.Log.WriteLine($"Cant send verification email to Gov Notify for {emailAddress} due to following error:{ex.Message}");

                if (ex.Message.ContainsI("This Email Address is not registered with Gov Notify.", "Can’t send to this recipient", "invalid token"))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/verify.html"));
                        html = html.Replace("((VerifyUrl))", verifyUrl);
                        Email.QuickSend("GPG Registration Verification", emailAddress, html);
                        result = new Notification() { status = "delivered" };
                    }
                    catch (Exception ex1)
                    {
                        
                    }
                }
            }
            return false;
        }

 
        public static bool SendPinInPost(string returnUrl,string name, string address, string pin)
        {
            var personalisation = new Dictionary<string, dynamic> { { "PIN", pin } };

            Notification result = null;
            try
            {
                result= GovNotify.SendPost(address, PINTemplateId, personalisation);
                if (!result.status.EqualsI("created", "sending", "delivered")) throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.Log.WriteLine($"Cant send Pin In POST to Gov Notify for {address} due to following error:{ex.Message}");

                if (ex.Message.ContainsI("This Email Address is not registered with Gov Notify.", "Can’t send to this recipient", "invalid token"))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/Pin.html"));
                        html = html.Replace("((PIN))", pin);
                        Email.QuickSend("GPG Registration Confirmation", address, html);
                        result = new Notification() { status = "delivered" };
                    }
                    catch (Exception ex1)
                    {

                    }
                }
            }

            return false;
        }

        public static bool SendRegistrationRequest(string emailAddress,string reviewUrl, string contactName, string contactOrg, string reportingOrg, string reportingAddress)
        {
            var personalisation = new Dictionary<string, dynamic> { { "url", reviewUrl }, {"name",contactName}, { "org1", contactOrg }, { "org2", reportingOrg },{ "address", reportingAddress} };

            Notification result = null;
            try
            {
                result = GovNotify.SendEmail(emailAddress, RegistrationRequestTemplateId, personalisation);
                if (!result.status.EqualsI("created", "sending", "delivered")) throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.Log.WriteLine($"Cant send registration request email to Gov Notify for {emailAddress} due to following error:{ex.Message}");

                if (ex.Message.ContainsI("This Email Address is not registered with Gov Notify.", "Can’t send to this recipient", "invalid token"))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/RegistrationRequest.html"));
                        html = html.Replace("((url))", reviewUrl);
                        html = html.Replace("((name))", contactName);
                        html = html.Replace("((org1))", contactOrg);
                        html = html.Replace("((org2))", reportingOrg);
                        html = html.Replace("((address))", reportingAddress);
                        Email.QuickSend("Registration Request - Gender pay gap reporting service", emailAddress, html);
                        result = new Notification() { status = "delivered" };
                    }
                    catch (Exception ex1)
                    {

                    }
                }
            }
            return false;
        }

        public static bool SendRegistrationApproved(string returnUrl, string emailAddress)
        {
            var personalisation = new Dictionary<string, dynamic> { { "url", returnUrl } };

            Notification result = null;
            try
            {
                result = GovNotify.SendEmail(emailAddress, RegistrationApprovedTemplateId, personalisation);
                if (!result.status.EqualsI("created", "sending", "delivered")) throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.Log.WriteLine($"Cant send registration approved email to Gov Notify for {emailAddress} due to following error:{ex.Message}");

                if (ex.Message.ContainsI("This Email Address is not registered with Gov Notify.", "Can’t send to this recipient", "invalid token"))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/RegistrationApproved.html"));
                        html = html.Replace("((url))", returnUrl);
                        Email.QuickSend("Registration approved - Gender pay gap reporting service", emailAddress, html);
                        result = new Notification() { status = "delivered" };
                    }
                    catch (Exception ex1)
                    {

                    }
                }
            }
            return false;
        }

        public static bool SendRegistrationDeclined(string returnUrl, string emailAddress, string reason)
        {
            var personalisation = new Dictionary<string, dynamic> {{ "reason", reason} };

            Notification result = null;
            try
            {
                result = GovNotify.SendEmail(emailAddress, RegistrationDeclinedTemplateId, personalisation);
                if (!result.status.EqualsI("created", "sending", "delivered")) throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.Log.WriteLine($"Cant send registration declined email to Gov Notify for {emailAddress} due to following error:{ex.Message}");

                if (ex.Message.ContainsI("This Email Address is not registered with Gov Notify.", "Can’t send to this recipient", "invalid token"))
                {
                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/RegistrationDeclined.html"));
                        html = html.Replace("((url))", returnUrl);
                        html = html.Replace("((reason))", reason);
                        Email.QuickSend("Registration declined - Gender pay gap reporting service", emailAddress, html);
                        result = new Notification() { status = "delivered" };
                    }
                    catch (Exception ex1)
                    {

                    }
                }
            }
            return false;
        }
    }
}