using GenderPayGap.Models.SqlDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Autofac;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Home")]
    [Route("{action}")]
    public class HomeController : BaseController
    {
        #region Initialisation
        public HomeController() : base() { }
        public HomeController(IContainer container) : base(container) { }


        /// <summary>
        /// This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [Route("Init")]
        public ActionResult Init()
        {
#if DEBUG
            MvcApplication.Log.WriteLine("Home Controller Initialised");
#endif
            return new EmptyResult();
        }
        #endregion
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