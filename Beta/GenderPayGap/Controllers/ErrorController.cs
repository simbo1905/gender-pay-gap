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

        [HttpGet]
        [Route]
        public ActionResult Default(int code=0)
        {
            var model = new ErrorViewModel(code);
            return View("CustomError", model);
        }
    }
}