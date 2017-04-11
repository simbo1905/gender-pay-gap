using Notify.Models;
using System.Collections.Generic;

namespace GenderPayGap.WebUI.Classes
{
    public interface IGovNotify
    {
        /// <summary>
        /// Used for unit testing only
        /// </summary>
        /// <param name="status"></param>
        void SetStatus(string status);

        Notification SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation, bool test = false);

        Notification SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation, bool test = false);

        Notification SendPost(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation, bool test=false);

    }

}
