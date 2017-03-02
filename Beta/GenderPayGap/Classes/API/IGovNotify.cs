using Notify.Client;
using Notify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenderPayGap.WebUI.Classes
{
    public interface IGovNotify
    {
        /// <summary>
        /// Used for unit testing only
        /// </summary>
        /// <param name="status"></param>
        void SetStatus(string status);

        Notification SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation);

        Notification SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation);

        Notification SendPost(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation);
    }

}
