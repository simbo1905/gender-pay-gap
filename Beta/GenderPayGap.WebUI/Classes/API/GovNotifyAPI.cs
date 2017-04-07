using System.Collections.Generic;
using System.Configuration;
using Notify.Models;
using Extensions;
using System;
using System.IO;
using GenderPayGap.WebUI.Classes;
using Autofac;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes.API;

namespace GenderPayGap
{
    public static class GovNotifyAPI
    {
        private static readonly string VerifyTemplateId = ConfigurationManager.AppSettings["GovNotifyVerifyTemplateId"];
        private static readonly string ResetTemplateId = ConfigurationManager.AppSettings["GovNotifyResetTemplateId"];
        private static readonly string PinTemplateId = ConfigurationManager.AppSettings["GovNotifyPINTemplateId"];
        private static readonly string RegistrationRequestTemplateId = ConfigurationManager.AppSettings["GovNotifyRegistrationRequestTemplateId"];
        private static readonly string GEODistributionList = ConfigurationManager.AppSettings["GEODistributionList"];
        private static readonly string RegistrationApprovedTemplateId = ConfigurationManager.AppSettings["GovNotifyRegistrationApprovedTemplateId"];
        private static readonly string RegistrationDeclinedTemplateId = ConfigurationManager.AppSettings["GovNotifyRegistrationDeclinedTemplateId"];
        static readonly bool ManualPip = ConfigurationManager.AppSettings["ManualPIP"].ToBoolean();

        private static readonly string SmtpServer = ConfigurationManager.AppSettings["SMTPServer"];
        private static readonly int SmtpPort = ConfigurationManager.AppSettings["SMTPPort"].ToInt32(25);
        private static readonly string SmtpSenderName = ConfigurationManager.AppSettings["SmtpSenderName"];
        private static readonly string SmtpUsername = ConfigurationManager.AppSettings["SMTPUsername"];
        private static readonly string SmtpPassword = ConfigurationManager.AppSettings["SMTPPassword"];

        private static IGovNotify GovNotify;

        public static void Initialise(IContainer container)
        {
            GovNotify=container.Resolve<IGovNotify>();
        }

        #region Emails
        public static bool SendVerifyEmail(string verifyUrl,string emailAddress, string verifyCode, bool test = false)
        {
            var personalisation = new Dictionary<string, dynamic> { { "url", verifyUrl } };

            try
            {
                var result = GovNotify.SendEmail(emailAddress, VerifyTemplateId, personalisation, test);
                if (!result.status.EqualsI("created", "sending", "delivered"))throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.ErrorLog.WriteLine($"Cant send verification email to Gov Notify for {emailAddress} due to following error:{ex.Message}");
                SendGeoMessage("GPG - GOV NOTIFY ERROR", $"Cant send verification email to Gov Notify will try direct send for {emailAddress} due to following error:\n\n{ex.Message}",test);

                try
                {
                    var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/verify.html"));
                    html = html.Replace("((url))", verifyUrl);
                    Email.QuickSend("GPG Registration Verification", SmtpUsername, SmtpSenderName, emailAddress, html, SmtpServer, SmtpUsername, SmtpPassword, SmtpPort,test:test);
                    return true;
                }
                catch (Exception ex1)
                {
                    MvcApplication.ErrorLog.WriteLine($"Cant send verification email directly for {emailAddress} due to following error:{ex1.Message}");
                }
            }
            return false;
        }

        public static bool SendRegistrationRequest(string reviewUrl, string contactName, string contactOrg, string reportingOrg, string reportingAddress, bool test = false)
        {

            var personalisation = new Dictionary<string, dynamic> { { "url", reviewUrl }, {"name",contactName}, { "org1", contactOrg }, { "org2", reportingOrg },{ "address", reportingAddress} };

            var emailAddresses = GEODistributionList.SplitI(";");
            if (emailAddresses.Length==0) throw new ArgumentNullException(nameof(GEODistributionList));
            if (!emailAddresses.ContainsAllEmails()) throw new ArgumentException($"{GEODistributionList} contains an invalid email address",nameof(GEODistributionList));

            var successCount = 0;
            foreach (var emailAddress in emailAddresses)
            {
                try
                {
                    var result = GovNotify.SendEmail(emailAddress, RegistrationRequestTemplateId, personalisation,test);
                    if (!result.status.EqualsI("created", "sending", "delivered")) throw new Exception($"Unexpected status '{result.status}' returned");
                    successCount++;
                }
                catch (Exception ex)
                {
                    MvcApplication.ErrorLog.WriteLine($"Cant send registration request email to Gov Notify for {emailAddress} due to following error:{ex.Message}");
                    SendGeoMessage("GPG - GOV NOTIFY ERROR", $"Cant send registration request email to Gov Notify will try direct send for {emailAddress} due to following error:\n\n{ex.Message}",test);

                    try
                    {
                        var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/RegistrationRequest.html"));
                        html = html.Replace("((url))", reviewUrl);
                        html = html.Replace("((name))", contactName);
                        html = html.Replace("((org1))", contactOrg);
                        html = html.Replace("((org2))", reportingOrg);
                        html = html.Replace("((address))", reportingAddress);
                        Email.QuickSend("Registration Request - Gender pay gap reporting service", SmtpUsername, SmtpSenderName, emailAddress, html, SmtpServer, SmtpUsername, SmtpPassword, SmtpPort,test:test);
                        successCount++;
                    }
                    catch (Exception ex1)
                    {
                        MvcApplication.ErrorLog.WriteLine($"Cant send registration request email directly for {emailAddress} due to following error:{ex1.Message}");
                    }
                }
            }

            return successCount == emailAddresses.Length;
        }

        public static bool SendRegistrationApproved(string returnUrl, string emailAddress, bool test = false)
        {
            //If the email address is a test email then simulate sending
            if (emailAddress.StartsWithI(MvcApplication.TestPrefix)) return true;

            var personalisation = new Dictionary<string, dynamic> { { "url", returnUrl } };

            try
            {
                var result = GovNotify.SendEmail(emailAddress, RegistrationApprovedTemplateId, personalisation,test);
                if (!result.status.EqualsI("created", "sending", "delivered")) throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.ErrorLog.WriteLine($"Cant send registration approved email to Gov Notify for {emailAddress} due to following error:{ex.Message}");
                SendGeoMessage("GPG - GOV NOTIFY ERROR", $"Cant send registration approved email to Gov Notify will try direct send for {emailAddress} due to following error:\n\n{ex.Message}",test);

                try
                {
                    var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/RegistrationApproved.html"));
                    html = html.Replace("((url))", returnUrl);
                    Email.QuickSend("Registration approved - Gender pay gap reporting service", SmtpUsername, SmtpSenderName, emailAddress, html, SmtpServer, SmtpUsername, SmtpPassword, SmtpPort,test:test);
                    return true;
                }
                catch (Exception ex1)
                {
                    MvcApplication.ErrorLog.WriteLine($"Cant send registration approved email directly for {emailAddress} due to following error:{ex1.Message}");
                }
            }
            return false;
        }

        public static bool SendRegistrationDeclined(string returnUrl, string emailAddress, string reason, bool test = false)
        {
            //If the email address is a test email then simulate sending
            if (emailAddress.StartsWithI(MvcApplication.TestPrefix)) return true;

            var personalisation = new Dictionary<string, dynamic> {{ "reason", reason} };

            try
            {
                var result = GovNotify.SendEmail(emailAddress, RegistrationDeclinedTemplateId, personalisation,test);
                if (!result.status.EqualsI("created", "sending", "delivered")) throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.ErrorLog.WriteLine($"Cant send registration declined email to Gov Notify for {emailAddress} due to following error:{ex.Message}");
                SendGeoMessage("GPG - GOV NOTIFY ERROR", $"Cant send registration declined email to Gov Notify will try direct send for {emailAddress} due to following error:\n\n{ex.Message}",test);

                try
                {
                    var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/RegistrationDeclined.html"));
                    html = html.Replace("((url))", returnUrl);
                    html = html.Replace("((reason))", reason);
                    Email.QuickSend("Registration declined - Gender pay gap reporting service", SmtpUsername, SmtpSenderName, emailAddress, html, SmtpServer, SmtpUsername, SmtpPassword, SmtpPort,test:test);
                    return true;
                }
                catch (Exception ex1)
                {
                    MvcApplication.ErrorLog.WriteLine($"Cant send registration declined email directly for {emailAddress} due to following error:{ex1.Message}");
                }
            }
            return false;
        }

        public static bool SendPasswordReset(string resetUrl, string emailAddress, string resetCode, bool test = false)
        {
            //If the email address is a test email then simulate sending
            if (emailAddress.StartsWithI(MvcApplication.TestPrefix)) return true;

            var personalisation = new Dictionary<string, dynamic> { { "url", resetUrl } };

            try
            {
                var result = GovNotify.SendEmail(emailAddress, ResetTemplateId, personalisation,test);
                if (!result.status.EqualsI("created", "sending", "delivered")) throw new Exception($"Unexpected status '{result.status}' returned");
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.ErrorLog.WriteLine($"Cant send password reset email to Gov Notify for {emailAddress} due to following error:{ex.Message}");
                SendGeoMessage("GPG - GOV NOTIFY ERROR", $"Cant send password reset email to Gov Notify will try direct send for {emailAddress} due to following error:\n\n{ex.Message}",test);

                try
                {
                    var html = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/PasswordReset.html"));
                    html = html.Replace("((url))", resetUrl);
                    Email.QuickSend("Password Reset - Gender Pay Gap reporting service", SmtpUsername, SmtpSenderName, emailAddress, html, SmtpServer, SmtpUsername, SmtpPassword, SmtpPort,test:test);
                    return true;
                }
                catch (Exception ex1)
                {
                    MvcApplication.ErrorLog.WriteLine($"Cant send password reset email directly for {emailAddress} due to following error:{ex1.Message}");
                }
            }
            return false;
        }

        #endregion

        #region Postal
        public static bool SendPinInPost(string imagePath, string returnUrl, string contactName, string jobtitle, string organisationName, List<string> address, string pin, DateTime sendDate, DateTime expiresDate, bool test = false)
        {
            if (!ManualPip)
            try
            {
                throw new NotImplementedException("PIN in Post via Gov Notify has not yet been implemented");
            }
            catch (Exception ex)
            {
                MvcApplication.ErrorLog.WriteLine($"Cant send Pin-In-Post to Gov Notify for {address.ToDelimitedString()} due to following error:{ex.Message}");
                SendGeoMessage("GPG - GOV NOTIFY ERROR", $"Cant send Pin-In-Post to Gov Notify will try manual post for {address.ToDelimitedString()} due to following error:\n\n{ex.Message}",test);
            }
            return SendPinInPostManual(imagePath, returnUrl, contactName, jobtitle, organisationName, address, pin, sendDate, expiresDate,test);
        }

        private static bool SendPinInPostManual(string imagePath,string returnUrl, string contactName, string jobtitle, string organisationName, List<string> address, string pin, DateTime sendDate, DateTime expiresDate, bool test = false)
        {
            if (string.IsNullOrWhiteSpace(GEODistributionList))throw new ArgumentNullException(nameof(GEODistributionList));
            if (!GEODistributionList.ContainsAllEmails())throw new ArgumentException($"{GEODistributionList} contains an invalid email address",nameof(GEODistributionList));

            try
            {
                var coverHtml = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/PinCover.html"));
                coverHtml = coverHtml.Replace("((ContactName))", contactName);
                coverHtml = coverHtml.Replace("((OrganisationName))", organisationName);
                coverHtml = coverHtml.Replace("((Address))", address.ToDelimitedString(",<br/>"));

                var pipHtml = System.IO.File.ReadAllText(FileSystem.ExpandLocalPath("~/App_Data/Pin.html"));
                pipHtml = pipHtml.Replace("((ImagePath))", imagePath);
                pipHtml = pipHtml.Replace("((ContactName))", contactName);
                pipHtml = pipHtml.Replace("((OrganisationName))", organisationName);
                pipHtml = pipHtml.Replace("((ContactJobTitle))", jobtitle);
                pipHtml = pipHtml.Replace("((Address))", address.ToDelimitedString(",<br/>"));
                pipHtml = pipHtml.Replace("((Date))", sendDate.ToString("d MMMM yyyy"));
                pipHtml = pipHtml.Replace("((PIN))", pin);
                pipHtml = pipHtml.Replace("((url))", returnUrl);
                pipHtml = pipHtml.Replace("((ExpiresDate))", expiresDate.ToString("d MMMM yyyy"));
                var pdf = PDF.HtmlToPDF(pipHtml);
                if (string.IsNullOrWhiteSpace(SmtpServer) || string.IsNullOrWhiteSpace(SmtpUsername))
                    MvcApplication.MailQueue.Enqueue(pdf,"pdf");
                else
                    Email.QuickSend("GPG PIN-in-Post", SmtpUsername, SmtpSenderName, GEODistributionList, coverHtml, SmtpServer, SmtpUsername, SmtpPassword, SmtpPort,pdf, $"{organisationName.ToProper().Strip(" -_,")}.pdf",test);
                return true;
            }
            catch (Exception ex)
            {
                MvcApplication.ErrorLog.WriteLine($"Cant send manual Pin In POST to {contactName} for {organisationName} at {address.ToDelimitedString()} via {GEODistributionList} directly due to following error:{ex.Message}");
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Send a message to GEO distribution list
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool SendGeoMessage(string subject, string message, bool test=false)
        {

            var emailAddresses = GEODistributionList.SplitI(";");
            if (emailAddresses.Length == 0) throw new ArgumentNullException(nameof(GEODistributionList));
            if (!emailAddresses.ContainsAllEmails()) throw new ArgumentException($"{GEODistributionList} contains an invalid email address", nameof(GEODistributionList));

            var successCount = 0;
            foreach (var emailAddress in emailAddresses)
            {
                try
                {
                    Email.QuickSend(subject, SmtpUsername, SmtpSenderName, emailAddress, message, SmtpServer, SmtpUsername, SmtpPassword, SmtpPort,test:test);
                    successCount++;
                }
                catch (Exception ex1)
                {
                    MvcApplication.ErrorLog.WriteLine($"Cant send message '{subject}' '{message}' directly to {emailAddress} due to following error:{ex1.Message}");
                }
            }

            return successCount == emailAddresses.Length;
        }

    }
}