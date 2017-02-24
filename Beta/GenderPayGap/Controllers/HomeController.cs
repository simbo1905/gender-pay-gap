using GenderPayGap.Models.SqlDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Home")]
    [Route("{action}")]
    public class HomeController : Controller
    {
        [Route("~/")]
        public ActionResult Redirect()
        {
            return RedirectToAction("Step1","Submit");
        }

        [HttpGet]
        [Route]
        [Route("Index")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Delete")]
        public ActionResult Delete()
        {
            DbContext.Truncate();
            return RedirectToAction("Index");
        }

        [Route("LogOut")]
        public ActionResult Logout()
        {
            Session.Abandon();
            Request.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Step1","Submit");
        }

        [Route("TimeOut")]
        public ActionResult TimeOut()
        {
            Session.Abandon();
            Request.GetOwinContext().Authentication.SignOut(new AuthenticationProperties { RedirectUri = Url.Action("Step1","Submit") });
            return null;
        }

    }
}