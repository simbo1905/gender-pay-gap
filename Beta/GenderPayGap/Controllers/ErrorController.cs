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
        static HttpErrorMessagesSection _HttpErrorMessages = null;
        internal static HttpErrorMessagesSection HttpErrorMessages
        {
            get
            {
                if (_HttpErrorMessages == null) _HttpErrorMessages = (HttpErrorMessagesSection)ConfigurationManager.GetSection("HttpErrorMessages");
                return _HttpErrorMessages;
            }
        }


        [HttpGet]
        [Route("HttpError")]
        public ActionResult HttpError(int code)
        {
            var errorMessage = HttpErrorMessages.Messages[code]?? HttpErrorMessages.Messages.Default;

            var model = new ErrorViewModel()
            {
                Title = errorMessage.Title,
                Description = errorMessage.Description,
                ActionUrl = errorMessage.ActionUrl,
                ActionText = errorMessage.ActionText
            };


            return View("CustomError", model);
        }
    }
}