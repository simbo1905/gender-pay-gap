using Extensions;
using GenderPayGap.Models.SqlDatabase;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using Autofac;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Submit;

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
            return RedirectToAction("EnterCalculations");
        }
        #endregion

        [Route("enter-calculations")]
        [HttpGet]
        public ActionResult EnterCalculations(string returnUrl = null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            var org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == userOrg.OrganisationId);

            var expectStartDate = GetAccountYearStartDate(org.SectorType);

            var @return = DataRepository.GetAll<Return>().OrderByDescending
                (r => r.AccountingDate).FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId && r.AccountingDate == expectStartDate && r.Status==ReturnStatuses.Submitted);

            var model = this.UnstashModel<ReturnViewModel>();

            if (model == null)
            {
                model = new ReturnViewModel();
                model.SectorType = org.SectorType;

                if (@return == null)
                {
                    model.AccountingDate = expectStartDate;
                    model.OrganisationId = org.OrganisationId;
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
            model.ReturnToStep4 = returnUrl.EqualsI("CheckData");

            this.StashModel(model);

            var result = View("EnterCalculations", model);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("enter-calculations")]
        public ActionResult EnterCalculations/*Create*/(ReturnViewModel model,string returnUrl=null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("JobTitle");
            if ((model.MaleUpperQuartilePayBand + model.FemaleUpperQuartilePayBand) != 100)
            {
                AddModelError(2052,nameof(model.FemaleUpperQuartilePayBand));
            }
            if ((model.MaleUpperPayBand + model.FemaleUpperPayBand) != 100)
            {
                AddModelError(2052, nameof(model.FemaleUpperPayBand));
            }
            if ((model.MaleMiddlePayBand + model.FemaleMiddlePayBand) != 100)
            {
                AddModelError(2052, nameof(model.FemaleMiddlePayBand));
            }
            if ((model.MaleLowerPayBand + model.FemaleLowerPayBand) != 100)
            {
                AddModelError(2052, nameof(model.FemaleLowerPayBand));
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View(model);
            }

            this.StashModel(model);

            return RedirectToAction(returnUrl.EqualsI("CheckData") ? "CheckData" : model.SectorType== SectorTypes.Public ? "EmployerWebsite" : "PersonResponsible");
        }

        [HttpGet]
        [Route("person-responsible")]
        public ActionResult PersonResponsible(string returnUrl = null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = this.UnstashModel<ReturnViewModel>();

            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("EnterCalculations");
            }

            //If redirected from step 4 then save to session and return to view
            model.ReturnToStep4 = returnUrl.EqualsI("CheckData");

            var result = View("PersonResponsible", model);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("person-responsible")]
        public ActionResult PersonResponsible(ReturnViewModel model, string returnUrl = null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View(model);
            }


            this.StashModel(model);
            
            return RedirectToAction(returnUrl.EqualsI("CheckData") ? "CheckData" : "EmployerWebsite");
        }

        [HttpGet]
        [Route("employer-website")]
        public ActionResult EmployerWebsite(string returnUrl=null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = this.UnstashModel<ReturnViewModel>();

            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("EnterCalculations");
            }

            if (model.SectorType == SectorTypes.Public)
            {
                ModelState.Remove(nameof(model.FirstName));
                ModelState.Remove(nameof(model.LastName));
                ModelState.Remove(nameof(model.JobTitle));
            }


            //If redirected from step 4 then save to session and return to view
            model.ReturnToStep4 = returnUrl.EqualsI("CheckData");

            var result = View("EmployerWebsite", model);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("employer-website")]
        public ActionResult EmployerWebsite /*GPGInfoLink*/(ReturnViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            if (model.SectorType == SectorTypes.Public)
            {
                ModelState.Remove(nameof(model.FirstName));
                ModelState.Remove(nameof(model.LastName));
                ModelState.Remove(nameof(model.JobTitle));
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View(model);
            }

            this.StashModel(model);

            return RedirectToAction("CheckData");
        }

        [HttpGet]
        [Route("check-data")]
        public ActionResult CheckData  /*Confirm*/()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = this.UnstashModel<ReturnViewModel>();
            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("EnterCalculations");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("check-data")]
        public ActionResult CheckData  /*Confirm*/(ReturnViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            if (model.SectorType == SectorTypes.Public)
            {
                ModelState.Remove("FirstName");
                ModelState.Remove("LastName");
                ModelState.Remove("JobTitle");
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View(model);
            }

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
            if (oldReturn != null)
            {
                if (oldReturn.Equals(newReturn))
                    newReturn = oldReturn;
                else
                    oldReturn.SetStatus(ReturnStatuses.Retired, currentUser.UserId);
            }

            //add the new one
            if (oldReturn == null || oldReturn.Status==ReturnStatuses.Retired)
                DataRepository.Insert(newReturn);

            using (var scope = new TransactionScope())
            {
                DataRepository.SaveChanges();

                newReturn.SetStatus(ReturnStatuses.Submitted, currentUser.UserId);
                DataRepository.SaveChanges();

                scope.Complete();
            }

            return RedirectToAction("SubmissionComplete");
        }

        //CheckData: Should have edits to take user to pages for editing
        //SubmissionComplete will be cut off from the original steps as this page will not be provided by us

        [HttpGet]
        [Route("submission-complete")]
        public ActionResult SubmissionComplete(/*long id = 1*/ )
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = this.UnstashModel<ReturnViewModel>();
            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("EnterCalculations");
            }
            //Make sure the stash is cleared
            this.ClearStash();

            return View(model);
        }
        
    }
}
