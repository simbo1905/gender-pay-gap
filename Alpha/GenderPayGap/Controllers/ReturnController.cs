using Extensions;
using GenderPayGap.WebUI.Models;
using GpgDB.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;

namespace GenderPayGap.WebUI.Controllers
{
    public class ReturnController : BaseController
    {

        public ReturnController():base(){ }
        public ReturnController(IContainer container): base(container){ }

        //Get: Return
        [HttpGet]
        public ActionResult Index()
        {
            //var prevReferrer = Request.UrlReferrer.ToString();
            //var currReferrer = Request.Url.ToString();
            
            var view = View("Index");
            return view;
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            var currentUser = GetCurrentUser();
            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            var model = Repository.GetAll<Return>().FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId);

            if (model == null) model = new Return();
            model.OrganisationId = userOrg.OrganisationId;
            var result = View(model);
            return result;
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

            var original = GpgDatabase.Default.Return.Find(model.ReturnId);
            if (original == null)
            {
                var currentUser = GetCurrentUser();
                var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(uo => uo.UserId == currentUser.UserId);
                model.OrganisationId = userOrg.OrganisationId;
                GpgDatabase.Default.Return.Add(model);
            }
            else
            {
                GpgDatabase.Default.Entry(original).CurrentValues.SetValues(model);
            }
            model.Organisation = GpgDatabase.Default.Organisation.Find(model.OrganisationId);
            model.AccountingDate = DateTime.Now;
            try
            {
                GpgDatabase.Default.SaveChanges();
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
            var qid = GpgDatabase.Default.Return.Find(id);
            return View(qid);
        }

        public ActionResult Details(int id = 1)
        {
            var qid = GpgDatabase.Default.Return.Find(id);
            return View(qid);
        }
    }
}
