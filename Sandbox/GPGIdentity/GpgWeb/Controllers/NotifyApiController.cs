using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Notify.Client;
using Notify.Models;
using Notify.Models.Responses;
using System.Collections.Generic;

namespace EmbeddedMvc.Controllers
{
    public class GovNotifyController : Controller
    {
        const string ClientReference = "GpgAlphaTest";
        string ApiKey = ConfigurationManager.AppSettings["GovNotifyKey"];

        public async Task<ActionResult> SendEmail()
        {
            var emailAddress ="***REMOVED***";
            var templateId = "ed3672eb-4a88-4db4-ae80-2884e5e7c68e";
            var personalisation = new Dictionary<String, dynamic>
                {
                    { "url", "http://genderpaygap.azurewebsites.com" }
                };

            var result = SendEmail(emailAddress, templateId, personalisation);

            ViewBag.Json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

            return View("ShowApiResult");
        }

        public async Task<ActionResult> SendSMS()
        {
            var mobileNumber = "07904436733";
            var templateId = "d4bf43bb-d0d7-433b-9891-8f17e00b6ef5";
            var personalisation = new Dictionary<String, dynamic>
                {
                    { "PIN", "123456" }
                };

            var result = SendSms(mobileNumber, templateId, personalisation);

            ViewBag.Json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return View("ShowApiResult");
        }

        private Notification SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation)
        {
            var client = new NotificationClient(ApiKey);
            var result=client.SendEmail(emailAddress, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;
        }

        private Notification SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation)
        {
            var client = new NotificationClient(ApiKey);
            var result = client.SendSms(mobileNumber, templateId, personalisation, ClientReference);
            var notification = client.GetNotificationById(result.id);
            return notification;

        }
    }
}