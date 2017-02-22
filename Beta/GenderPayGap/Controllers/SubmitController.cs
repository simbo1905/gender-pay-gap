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
using GenderPayGap.WebUI.Classes;
using Thinktecture.IdentityModel.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Submit")]
    [Route("{action}")]
    public class SubmitController : BaseController
    {

        public SubmitController():base(){ }
        public SubmitController(IContainer container): base(container){ }

        [Auth]
        [Route("Step1")]
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

        [Auth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step1")]
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

        [Auth]
        [HttpGet]
        [Route("Step2")]
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

        [Auth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step2")]
        public ActionResult Step2 /*Authoriser*/(ReturnViewModel model)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            if (!ModelState.IsValid) return View(model);

            TempData["Model"] = model;

            return RedirectToAction("Step3");
        }

        [Auth]
        [HttpGet]
        [Route("Step3")]
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

        [Auth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step3")]
        public ActionResult Step3 /*GPGInfoLink*/(ReturnViewModel model, string command)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;


            if (!ModelState.IsValid) return View(model);
            TempData["Model"] = model;

            if (command == "Back") return RedirectToAction("Step2");

            return RedirectToAction("Step4");
        }

        [Auth]
        [HttpGet]
        [Route("Step4")]
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


        [Auth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step4")]
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

            return View("Step5",model);
        }

        //Step4: Should have edits to take user to pages for editing
        //Step5 will be cut off from the original steps as this page will not be provided by us

        [Auth]
        [HttpGet]
        [Route("Step5")]
        public ActionResult Step5 /*Step4*/ /*SendConfirmed*/(long id = 0)
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            try
            {
                if (id < 1)
                {
                    return View("Step5");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            //if (!Authorise()) return RedirectToAction("Index", "Register");
            return View("Step5");
        }

        [Auth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step5")]
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
                return View("Step5");
            }
            return RedirectToAction("Complete");
        }

        // GET: Submit/Details/5
        [Auth]
        public ActionResult Complete()
        {
            User currentUser;
            var errorView = CheckUserRegisteredOk(out currentUser);
            if (errorView != null) return errorView;

            var qid = Repository.GetAll<Return>().FirstOrDefault(r => r.ReturnId == 1);
            return View("Complete");
        }
    }
}
