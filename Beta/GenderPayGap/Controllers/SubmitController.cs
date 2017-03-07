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

namespace GenderPayGap.WebUI.Controllers
{
    [Auth]
    [RoutePrefix("Submit")]
    [Route("{action}")]
    public class SubmitController : BaseController
    {
        #region Initialisation
        public SubmitController() : base() { }
        public SubmitController(IContainer container) : base(container) { }

        /// <summary>
        /// This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [Route("Init")]
        public ActionResult Init()
        {
#if DEBUG
            MvcApplication.Log.WriteLine("Submit Controller Initialised");
#endif
            return new EmptyResult();
        }

        /// <summary>
        /// This action is used to redirect the user to the starting action when only the controller is specified in the url and no action
        /// </summary>
        /// <returns></returns>
        [Route]
        public ActionResult Redirect()
        {
            return RedirectToAction("Step1");
        }
        #endregion

        [Route("Step1")]
        [HttpGet]
        public ActionResult Step1(string returnUrl = null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            var Org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == userOrg.OrganisationId);

            var expectStartDate = GetCurrentAccountYearStartDate(Org);

            var @return = DataRepository.GetAll<Return>().OrderByDescending
                (r => r.AccountingDate).FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId && r.AccountingDate == expectStartDate && r.Status==ReturnStatuses.Submitted);

            var model = this.UnstashModel<ReturnViewModel>();

            if (model == null)
            {
                model = new ReturnViewModel();

                if (@return == null)
                {
                    model.AccountingDate = expectStartDate;
                    model.OrganisationId = Org.OrganisationId;
                }
                else
                {
                    //create new return viewmode
                    //populate with return from db
                    model.ReturnId = @return.ReturnId;
                    model.OrganisationId = @return.OrganisationId;
                    model.DiffMeanBonusPercent = @return.DiffMeanBonusPercent;
                    model.DiffMeanHourlyPayPercent = @return.DiffMeanHourlyPayPercent;
                    model.DiffMedianBonusPercent = @return.DiffMedianBonusPercent;
                    model.DiffMedianHourlyPercent = @return.DiffMedianHourlyPercent;
                    model.FemaleLowerPayBand = @return.FemaleLowerPayBand;
                    model.FemaleMedianBonusPayPercent = @return.FemaleMedianBonusPayPercent;
                    model.FemaleMiddlePayBand = @return.FemaleMiddlePayBand;
                    model.FemaleUpperPayBand = @return.FemaleUpperPayBand;
                    model.FemaleUpperQuartilePayBand = @return.FemaleUpperQuartilePayBand;
                    model.MaleLowerPayBand = @return.MaleLowerPayBand;
                    model.MaleMedianBonusPayPercent = @return.MaleMedianBonusPayPercent;
                    model.MaleMiddlePayBand = @return.MaleMiddlePayBand;
                    model.MaleUpperPayBand = @return.MaleUpperPayBand;
                    model.MaleUpperQuartilePayBand = @return.MaleUpperQuartilePayBand;
                    model.JobTitle = @return.JobTitle;
                    model.FirstName = @return.FirstName;
                    model.LastName = @return.LastName;
                    model.CompanyLinkToGPGInfo = @return.CompanyLinkToGPGInfo;
                    model.AccountingDate = @return.AccountingDate;
                }
            }

            if (TempData.ContainsKey("ErrorMessage")) ModelState.AddModelError("", TempData["ErrorMessage"].ToString());

            //If redirected from step 4 then save to session and return to view
            model.ReturnToStep4 = returnUrl.EqualsI("Step4");

            this.StashModel(model);

            var result = View("Step1", model);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step1")]
        public ActionResult Step1 (ReturnViewModel model,string returnUrl=null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("JobTitle");

            if (!ModelState.IsValid) return View("Step1", model);

            this.StashModel(model);

            return RedirectToAction(returnUrl.EqualsI("Step4") ? "Step4" : "Step2");
        }

        [HttpGet]
        [Route("Step2")]
        public ActionResult Step2(string returnUrl = null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = this.UnstashModel<ReturnViewModel>();

            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("Step1");
            }

            //If redirected from step 4 then save to session and return to view
            model.ReturnToStep4 = returnUrl.EqualsI("Step4");

            var result = View("Step2", model);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step2")]
        public ActionResult Step2(ReturnViewModel model, string returnUrl = null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid) return View("Step2", model);

            this.StashModel(model);
            
            return RedirectToAction(returnUrl.EqualsI("Step4") ? "Step4" : "Step3");
        }

        [HttpGet]
        [Route("Step3")]
        public ActionResult Step3(string returnUrl=null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = this.UnstashModel<ReturnViewModel>();

            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("Step1");
            }

            //If redirected from step 4 then save to session and return to view
            model.ReturnToStep4 = returnUrl.EqualsI("Step4");

              return View("Step3", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step3")]
        public ActionResult Step3 /*GPGInfoLink*/(ReturnViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            this.StashModel(model);

            return RedirectToAction("Step4");
        }

        [HttpGet]
        [Route("Step4")]
        public ActionResult Step4  /*Confirm*/()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = this.UnstashModel<ReturnViewModel>();
            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("Step1");
            }

            return View("Step4", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step4")]
        public ActionResult Step4  /*Confirm*/(ReturnViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid) return View(model);

            var oldReturn = DataRepository.GetAll<Return>().FirstOrDefault(r => r.ReturnId == model.ReturnId);

            var newReturn = new Return()
            {
                AccountingDate = model.AccountingDate,
                CompanyLinkToGPGInfo = model.CompanyLinkToGPGInfo,
                DiffMeanBonusPercent = model.DiffMeanBonusPercent.Value,
                DiffMeanHourlyPayPercent = model.DiffMeanHourlyPayPercent.Value,
                DiffMedianBonusPercent = model.DiffMedianBonusPercent.Value,
                DiffMedianHourlyPercent = model.DiffMedianBonusPercent.Value,
                FemaleLowerPayBand = model.FemaleLowerPayBand.Value,
                FemaleMedianBonusPayPercent = model.FemaleMedianBonusPayPercent.Value,
                FemaleMiddlePayBand = model.FemaleMiddlePayBand.Value,
                FemaleUpperPayBand = model.FemaleUpperPayBand.Value,
                FemaleUpperQuartilePayBand = model.FemaleUpperQuartilePayBand.Value,
                FirstName = model.FirstName,
                LastName = model.LastName,
                JobTitle = model.JobTitle,
                MaleLowerPayBand = model.MaleLowerPayBand.Value,
                MaleMedianBonusPayPercent = model.MaleMedianBonusPayPercent.Value,
                MaleUpperQuartilePayBand = model.MaleUpperQuartilePayBand.Value,
                MaleMiddlePayBand = model.MaleMiddlePayBand.Value,
                MaleUpperPayBand = model.MaleUpperPayBand.Value,
                Status = ReturnStatuses.Draft,
                OrganisationId = model.OrganisationId
            };

            //Retire the old one 
            if (oldReturn != null && !oldReturn.Equals(newReturn))
                oldReturn.SetStatus(ReturnStatuses.Retired, currentUser.UserId);

            //add the new one
            if (oldReturn == null || oldReturn.Status==ReturnStatuses.Retired)
                DataRepository.Insert(newReturn);

            DataRepository.SaveChanges();

            newReturn.SetStatus(ReturnStatuses.Submitted, currentUser.UserId);
            DataRepository.SaveChanges();

            return View("Step5", model);
        }

        //Step4: Should have edits to take user to pages for editing
        //Step5 will be cut off from the original steps as this page will not be provided by us

        [HttpGet]
        [Route("Step5")]
        public ActionResult Step5(/*long id = 1*/ )
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = this.UnstashModel<ReturnViewModel>();
            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("Step1");
            }

            return View(model);
        }
        
    }
}
