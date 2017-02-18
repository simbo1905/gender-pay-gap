using Extensions;
using GenderPayGap.WebUI.Models;
using GenderPayGap.Models.SqlDatabase;
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
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            // userOrg.Organisation.SectorType == SectorTypes.Private
            //Settings for accounting date 
            //Accounting year

            var expectStartDate = DateTime.MinValue; //from Base Controloer extecte AccountYear
            var expectEndDate = expectStartDate.AddYears(1).Date.AddDays(1);

            var @return = Repository.GetAll<Return>().OrderByDescending
                (r => r.AccountingDate).FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId && r.AccountingDate >= expectStartDate && r.AccountingDate < expectEndDate);
           
            //var @return = Repository.GetAll<Return>().FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId);

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
        [ValidateAntiForgeryToken]
        public ActionResult Step1/*Create*/(ReturnViewModel model)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("JobTitle");

            if (!ModelState.IsValid) return View(model);

            TempData["Model"] = model;
            return RedirectToAction("Step2");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Step2 /*Create*/()
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            ReturnViewModel model = null; //= (!TempData["Model"].IsNull()) ? (ReturnViewModel) TempData["Model"] : null;

            if (!TempData["Model"].IsNull())
                model = (ReturnViewModel)TempData["Model"];

            if (model == null) model = new ReturnViewModel();

            var result = View("Step2", model);
            return result;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Step2 /*Authoriser*/(ReturnViewModel model)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            if (!ModelState.IsValid) return View(model);

            TempData["Model"] = model;

            return RedirectToAction("Step3");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Step3 /*GPGInfoLink*/()
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null)  return errorView; 

            ReturnViewModel model = null; 

            if (!TempData["Model"].IsNull())
                model = (ReturnViewModel)TempData["Model"];

            if (model == null) model = new ReturnViewModel();

            var result = View("Step3", model);
            return result;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Step3 /*GPGInfoLink*/(ReturnViewModel model, string command)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");


            if (!ModelState.IsValid) return View(model);
            TempData["Model"] = model;

            if (command == "Back") return RedirectToAction("Step2");

            return RedirectToAction("Step4");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Step4  /*Confirm*/()
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            ReturnViewModel model = null;

            if (!TempData["Model"].IsNull())
                model = (ReturnViewModel)TempData["Model"];

            return View(model);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Step4  /*Confirm*/(ReturnViewModel model)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            if (!ModelState.IsValid) return View(model);

            var @return = Repository.GetAll<Return>().FirstOrDefault(r => r.ReturnId == model.ReturnId);

            if(!@return.IsNull())
            {
                //TODO:mark this return as retired 
                
            }

            @return = new Return()
            {
                AccountingDate = DateTime.Now,
                CompanyLinkToGPGInfo = model.CompanyLinkToGPGInfo,
                Created = DateTime.Now,

                DiffMeanBonusPercent = model.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = model.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = model.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = model.DiffMedianBonusPercent,
                FemaleLowerPayBand = model.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = model.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = model.FemaleMiddlePayBand,
                FemaleUpperPayBand = model.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = model.FemaleUpperQuartilePayBand,
                FirstName = model.FirstName,
                LastName = model.LastName,
                JobTitle = model.JobTitle,
                MaleLowerPayBand = model.MaleLowerPayBand,
                MaleMedianBonusPayPercent = model.MaleMedianBonusPayPercent,
                MaleUpperQuartilePayBand = model.MaleUpperQuartilePayBand,
                MaleMiddlePayBand = model.MaleMiddlePayBand,
                MaleUpperPayBand = model.MaleUpperPayBand,
                //TODO SetStatus
                //Modified                  = model.Modified,
                OrganisationId = model.OrganisationId,
                ReturnId = model.ReturnId
                                   
            };

            Repository.Insert<Return>(@return);
            Repository.SaveChanges();

            return View(model);
        }

        //Step4: Should have edits to take user to pages for editing
        //Step5 will be cut off from the original steps as this page will not be provided by us

        [Authorize]
        [HttpGet]
        public ActionResult Step5 /*Step4*/ /*SendConfirmed*/(long id = 0)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

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
        [ValidateAntiForgeryToken]
        public ActionResult Step5 /*Step4*/  /*SendConfirmed*/(Return model)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            //var original = GpgDatabase.Default.Return.Find(model.ReturnId);
            var original = Repository.GetAll<Return>().FirstOrDefault(m => m.ReturnId == model.ReturnId);

            if (original == null)
            {
                var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
                model.OrganisationId = userOrg.OrganisationId;
                Repository.Insert<Return>(model);
               
            }
            else
            {
             //   GpgDatabase.Default.Entry(original).CurrentValues.SetValues(model);
            }

            model.Organisation = Repository.GetAll<Organisation>().FirstOrDefault(m => m.OrganisationId == model.OrganisationId);
           
                model.AccountingDate = DateTime.Now;
            try
            {
                Repository.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            return RedirectToAction("Step4", new { id = model.ReturnId });
        }

        // GET: Return/Details/5
        [Authorize]
        public ActionResult Details(int id = 1)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            var qid = Repository.GetAll<Return>().FirstOrDefault(r => r.ReturnId == id);
            return View(qid);
        }

        [HttpGet]
        public ActionResult Error()
        {
            //Show the confirmation view
            return View();
        }
    }
}
