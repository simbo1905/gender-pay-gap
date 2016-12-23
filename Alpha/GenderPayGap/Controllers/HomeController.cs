using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GenderPayGap.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete()
        {
            GpgDatabase.Default.Return.RemoveRange(GpgDatabase.Default.Return);
            GpgDatabase.Default.UserOrganisations.RemoveRange(GpgDatabase.Default.UserOrganisations);
            GpgDatabase.Default.Organisation.RemoveRange(GpgDatabase.Default.Organisation);
            GpgDatabase.Default.UserTokens.RemoveRange(GpgDatabase.Default.UserTokens);
            GpgDatabase.Default.User.RemoveRange(GpgDatabase.Default.User);
            GpgDatabase.Default.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect("/");
        }
    }
}