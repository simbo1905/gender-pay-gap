using GenderPayGap.Models.SqlDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.WebUI.Classes;
using System.Configuration;
using GenderPayGap.WebUI.Models;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Error")]
    [Route("{action}")]
    public class ErrorController : Controller
    {
        static ErrorMessagesSection _ErrorMessages = null;
        private static ErrorMessagesSection ErrorMessages
        {
            get
            {
                if (_ErrorMessages == null) _ErrorMessages = (ErrorMessagesSection)ConfigurationManager.GetSection("ErrorMessages");
                return _ErrorMessages;
            }
        }


        [HttpGet]
        [Route]
        [Route("Default")]
        public ActionResult Default(int code=0)
        {
            var errorMessage = ErrorMessages.Messages[code] ?? ErrorMessages.Messages.Default;

            var model = new ErrorViewModel()
            {
                Title = errorMessage.Title,
                Description = errorMessage.Description,
                CallToAction = errorMessage.CallToAction,
                ActionUrl = errorMessage.ActionUrl,
                ActionText = errorMessage.ActionText
            };

            return View("CustomError", model);
        }
    }
}