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
            var view = View("Index");
            return view;
        }

        [Authorize]
        [HttpGet]
        public ActionResult Step1 /*Create*/() 
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            var currentUser = GetCurrentUser();
            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            //var @return = Repository.GetAll<Return>().OrderByDescending(r => r.AccountingDate).FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId && r.AccountingDate.Value.AddYears(1) < DateTime.Now);
            var @return = Repository.GetAll<Return>().FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId);

            var model = new ReturnViewModel();

            if (@return != null)
            {
                model.ReturnId                     =  @return.ReturnId;
                model.OrganisationId               =  @return.OrganisationId;
                model.DiffMeanBonusPercent         =  @return.DiffMeanBonusPercent;
                model.DiffMeanHourlyPayPercent     =  @return.DiffMeanHourlyPayPercent;
                model.DiffMedianBonusPercent       =  @return.DiffMedianBonusPercent;
                model.DiffMedianHourlyPercent      =  @return.DiffMeanHourlyPayPercent;
                model.FemaleLowerPayBand           =  @return.DiffMeanHourlyPayPercent;
                model.FemaleMedianBonusPayPercent  =  @return.DiffMeanHourlyPayPercent;
                model.FemaleMiddlePayBand          =  @return.DiffMeanHourlyPayPercent;
                model.FemaleUpperPayBand           =  @return.DiffMeanHourlyPayPercent;
                model.FemaleUpperQuartilePayBand   =  @return.DiffMeanHourlyPayPercent;
                model.MaleLowerPayBand             =  @return.MaleLowerPayBand;
                model.MaleMedianBonusPayPercent    =  @return.MaleMedianBonusPayPercent;
                model.MaleMiddlePayBand            =  @return.MaleMiddlePayBand;
                model.MaleUpperPayBand             =  @return.MaleUpperPayBand;
                model.MaleUpperQuartilePayBand     =  @return.MaleUpperQuartilePayBand;
                model.JobTitle                     =  @return.JobTitle;
                model.FirstName                    =  @return.FirstName;
                model.LastName                     =  @return.LastName;
            }

            model.OrganisationId = userOrg.OrganisationId;
            var result = View("Step1", model);
            return result;
        }

        [Authorize]
        [HttpPost]
        public ActionResult Step1/*Create*/(ReturnViewModel model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("JobTitle");

            if (!ModelState.IsValid) return View(model);

            TempData["Model"] = model;
            return RedirectToAction("Step2");
        }

        //[Authorize]
        //[HttpGet]
        //public ActionResult Step2 /*Create*/()
        //{
        //    if (!Authorise()) return RedirectToAction("Index", "Register");

        //    var currentUser = GetCurrentUser();
        //    var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
        //    var model = Repository.GetAll<Return>().FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId);

        //    var returnModel = (ReturnViewModel)ViewBag.Model;

        //    if (model == null) model = new Return();
        //    model.OrganisationId = userOrg.OrganisationId;
        //    var result = View("Step2", model);
        //    return result;
        //}

        [Authorize]
        [HttpGet]
        public ActionResult Step2 /*Create*/()
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            var currentUser = GetCurrentUser();

            ReturnViewModel model = null; //= (!TempData["Model"].IsNull()) ? (ReturnViewModel) TempData["Model"] : null;

            if (!TempData["Model"].IsNull())
                model = (ReturnViewModel)TempData["Model"];

           // TempData.Keep();


            if (model == null) model = new ReturnViewModel();
            //model.OrganisationId = userOrg.OrganisationId;

            var result = View("Step2", model);
            return result;
        }

        [Authorize]
        [HttpPost]
        public ActionResult Step2 /*Authoriser*/(ReturnViewModel model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            if (!ModelState.IsValid) return View(model);

            TempData["Model"] = model;
   //         TempData.Keep();

            return RedirectToAction("Step3");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Step3 /*GPGInfoLink*/()
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            var currentUser = GetCurrentUser();
           

            ReturnViewModel model = null; 

            if (!TempData["Model"].IsNull())
                model = (ReturnViewModel)TempData["Model"];

            if (model == null) model = new ReturnViewModel();
           // model.OrganisationId = userOrg.OrganisationId;

            var result = View("Step3", model);
            return result;
        }

        [Authorize]
        [HttpPost]
        public ActionResult Step3 /*GPGInfoLink*/(ReturnViewModel model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            if (!ModelState.IsValid) return View(model);

            TempData["Model"] = model;
  //          TempData.Keep();

            return RedirectToAction("Step4");
        }

        //[Authorize]
        //[HttpGet]
        //public ActionResult Step4  /*Confirm*/(int id = 1)
        //{
        //    if (!Authorise()) return RedirectToAction("Index", "Register");

        //    var qid = GpgDatabase.Default.Return.Find(id);
        //    return View(qid);
        //}

        [Authorize]
        [HttpGet]
        public ActionResult Step4  /*Confirm*/()
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            ReturnViewModel model = null;

            if (!TempData["Model"].IsNull())
                model = (ReturnViewModel)TempData["Model"];

            return View(model);

           // var qid = Repository.GetAll<Return>().FirstOrDefault(r => r.ReturnId == id);
          //  return View(qid);
        }


        //[Authorize]
        //[HttpPost]
        //public ActionResult Step4  /*Confirm*/(ReturnViewModel model)
        //{
        //    if (!Authorise()) return RedirectToAction("Index", "Register");

        //    if (!ModelState.IsValid) return View(model);
        //    return View(model);
        //}

        [Authorize]
        [HttpPost]
        public ActionResult Step4  /*Confirm*/(ReturnViewModel model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            if (!ModelState.IsValid) return View(model);

            Repository.SaveChanges();
            return View(model);
        }


        //Step4: Should have edits to take user to pages for editing
        //Step5 will be cut off from the original steps as this page will not be provided by us

        [Authorize]
        [HttpGet]
        public ActionResult Step5 /*Step4*/ /*SendConfirmed*/(long id = 0)
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
        public ActionResult Step5 /*Step4*/  /*SendConfirmed*/(Return model)
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
            return RedirectToAction("Step4", new { id = model.ReturnId });
        }

        // GET: Return/Details/5
        public ActionResult Details(int id = 1)
        {
            var qid = GpgDatabase.Default.Return.Find(id);
            return View(qid);
        }
    }
}
