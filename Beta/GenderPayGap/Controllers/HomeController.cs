using GenderPayGap.Models.SqlDatabase;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Autofac;
using GenderPayGap.WebUI.Classes;

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

        [HttpGet]
        [Route("Execute")]
        public ActionResult Execute()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Execute")]
        public ActionResult Execute(string command)
        {
            var userId = User.GetUserId();
            switch (command)
            {
                case "SignIn":
                    return new HttpUnauthorizedResult();
                case "DeleteOrganisations":
                    DbContext.DeleteOrganisations(userId);
                    break;
                case "DeleteReturns":
                    DbContext.DeleteReturns(userId);
                    break;
                case "DeleteAccount":
                    DbContext.DeleteAccount(userId);
                    Session.Abandon();
                    Request.GetOwinContext().Authentication.SignOut();
                    break;
                case "ClearDatabase":
                    DbContext.Truncate();
                    break;
            }
            return RedirectToAction("Index");
        }

        [Route("SignOut")]
        public ActionResult SignOut()
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