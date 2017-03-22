using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Mail;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace Extensions
{
    public static class Email
    {
        const string MatchEmailPattern = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

        public static bool IsHostName(this string hostName)
        {
            return ("admin@" + hostName).IsEmailAddress();
        }

        [DebuggerStepThrough]
        public static bool IsEmailAddress(this string inputEmail)
        {
            if (string.IsNullOrWhiteSpace(inputEmail)) return false;
            try
            {
                var email=new MailAddress(inputEmail);
                if (email == null || string.IsNullOrWhiteSpace(email.Address)) return false;
            }
            catch (FormatException)
            {
                return false;
            }
            return true;
        }

        public static bool IsEmailMatch(this string text)
        {
            return text.Match(MatchEmailPattern, true);
        }

        public static HashSet<string> FindEmailAddresses(this string text)
        {
            var results = new HashSet<string>();

            var emails = text.FindAll(MatchEmailPattern,true);

            Parallel.ForEach<string>(emails, (email) =>
            {
                if (email.IsEmailAddress()) results.Add(email);
            });
            return results;
        }

        public static string GetEmailName(this string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";

            try
            {
                return new MailAddress(email).DisplayName;
            }
            catch (FormatException fex)
            {
                return "";
            }
        }
        public static string GetEmailDomain(this string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";

            var i = email.IndexOf('@');
            if (i < 0) return email;
            try
            {
                return new MailAddress(email).Host;
            }
            catch (FormatException)
            {
                return "";
            }
        }

        public static string GetEmailUsername(this string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            try
            {
                return new MailAddress(email).User;
            }
            catch (FormatException)
            {
                return "";
            }
        }

        public static string GetEmailAddress(this string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            try
            {
                return new MailAddress(email).Address.ToLower();
            }
            catch (FormatException fex)
            {
                return "";
            }
        }
        public static string GetEmailFull(this string email, string displayName = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            if (string.IsNullOrWhiteSpace(displayName)) displayName = email.GetEmailName();
            var address = email.GetEmailAddress();
            if (!string.IsNullOrWhiteSpace(displayName)) return displayName.ReplaceAll(@"\", "") + " <" + address.ToLower() + ">";
            return address.ToLower();
        }
        public static string GetEmailFriendlyFull(this string email, string displayName=null)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            if (string.IsNullOrWhiteSpace(displayName)) displayName = email.GetEmailName();
            var address = email.GetEmailAddress();
            if (!string.IsNullOrWhiteSpace(displayName)) return displayName.ReplaceAll(@"\", "") + " (" + address + ")";
            return address;
        }

        public static string GetEmailFriendlyShort(this string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var name = email.GetEmailName();
            if (!string.IsNullOrWhiteSpace(name)) return name.ReplaceAll(@"\", "");
            return email.GetEmailAddress();
        }


        public static string GetTopLevelDomain(this string emailHostDomain)
        {
            if (emailHostDomain.IsEmailAddress())
                emailHostDomain = emailHostDomain.GetEmailDomain();
            else if (emailHostDomain.IsUrl())
                emailHostDomain = new Uri(emailHostDomain).Host;

            var parts = emailHostDomain.Split('.');
            
            if (parts.Length >= 2)
            {
                emailHostDomain = parts[parts.Length - 2] + "." + parts[parts.Length - 1];
            }
            return emailHostDomain;
        }

        public static bool ContainsAllEmails(this string inputEmail)
        {
            if (string.IsNullOrWhiteSpace(inputEmail)) return false;
            var emails = inputEmail.SplitI();
            if (emails.Length < 1) return false;
            var found = false;
            foreach (var email in emails)
            {
                if (string.IsNullOrWhiteSpace(email)) continue;
                var address=email.GetEmailAddress();
                if (string.IsNullOrWhiteSpace(address)) return false;
                if (!address.IsEmailAddress()) return false;
                found = true;
            }
            return found;
        }

        public static void QuickSend(string subject, string senderEmailAddress, string senderName, string recipients, string html, string smtpServer, string smtpUsername, string smtpPassword, int smtpPort = 25, byte[] attachment=null, string attachmentFilename="attachment.dat")
        {
            var mySmtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                EnableSsl = true,
                UseDefaultCredentials = false
            };

            // set smtp-client with basicAuthentication
            var basicAuthenticationInfo = new NetworkCredential(smtpUsername, smtpPassword);
            mySmtpClient.Credentials = basicAuthenticationInfo;

            var myMail = new MailMessage
            {
                From = new MailAddress(senderEmailAddress,senderName),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = html,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            // set body-message and encoding
            // text or html

            // add mailaddresses
            foreach (var recipient in recipients.SplitI(";"))
                myMail.To.Add(new MailAddress(recipient));
            
            //Add the attachment
            if (attachment == null)
                mySmtpClient.Send(myMail);
            else using (var stream = new MemoryStream(attachment))
            {
                myMail.Attachments.Add(new Attachment(stream,attachmentFilename));
                mySmtpClient.Send(myMail);
            }
        }

    }

}
