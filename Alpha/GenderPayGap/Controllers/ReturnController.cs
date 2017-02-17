using Extensions;
using GenderPayGap.WebUI.Models;
using GenderPayGap.Models.SqlDatabase;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.WebUI.Classes;
using DbContext = GenderPayGap.Models.SqlDatabase.DbContext;

namespace GenderPayGap.WebUI.Controllers
{
    public class ReturnController : BaseController
    {
        //Get: Return
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            var currentUser = Repository.FindUser(User);
            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            var model = Repository.GetAll<Return>().FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId);
            if (model == null) model = new Return();
            model.OrganisationId = userOrg.OrganisationId;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(Return model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            if (!ModelState.IsValid) return View(model);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Authoriser(Return model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            if (Request.UrlReferrer.PathAndQuery.ContainsI("Create") && string.IsNullOrWhiteSpace(model.FirstName) && string.IsNullOrWhiteSpace(model.LastName) && string.IsNullOrWhiteSpace(model.JobTitle))
                ModelState.Clear();

            if (!ModelState.IsValid)
                return View(model);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Confirm(Return model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            if (!ModelState.IsValid) return View(model);
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public ActionResult SendConfirmed(long id = 0)
        {
            try
            {
                if (id < 1)
                {
                    return View(id);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            if (!Authorise()) return RedirectToAction("Index", "Register");
            return View(id);
        }

        [Authorize]
        [HttpPost]
        public ActionResult SendConfirmed(Return model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            var original = Repository.GetAll<Return>().Where(r=>r.ReturnId==model.ReturnId);
            if (original == null)
            {
                var currentUser = Repository.FindUser(User);
                var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
                model.OrganisationId = userOrg.OrganisationId;
                Repository.Insert(model);
            }
            else
            {
                //DbContext.Default.Entry(original).CurrentValues.SetValues(model);
            }
            //model.Organisation = DbContext.Default.Organisation.Find(model.OrganisationId);
            model.AccountingDate = DateTime.Now;
            try
            {
                //DbContext.Default.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            return RedirectToAction("SendConfirmed",new { id=model.ReturnId});
        }

        // GET: Return/Details/5
        public ActionResult Confirm(int id = 1)
        {
            //var qid = DbContext.Default.Return.Find(id);
            return View();
        }

        public ActionResult Details(int id = 1)
        {
            //var qid = DbContext.Default.Return.Find(id);
            return View();
        }
    }
}
