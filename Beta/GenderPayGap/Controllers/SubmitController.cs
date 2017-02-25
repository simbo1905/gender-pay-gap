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
        public ActionResult Step1 /*Create*/()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            ShowAndActivateCancelButtonLink();

            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            var Org = Repository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == userOrg.OrganisationId);

            var expectStartDate = GetCurrentAccountYearStartDate(Org);
            var expectEndDate = expectStartDate.AddYears(1).Date.AddDays(1);

            var @return = Repository.GetAll<Return>().OrderByDescending
                (r => r.AccountingDate).FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId && r.AccountingDate >= expectStartDate && r.AccountingDate < expectEndDate);

            var model = UnstashModel<ReturnViewModel>();

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


            // var model = new ReturnViewModel();

            //copy existing return into  ViewModel
            //if (@return != null)
            //{
            //model.ReturnId = @return.ReturnId;
            //model.OrganisationId = @return.OrganisationId;
            //model.DiffMeanBonusPercent = @return.DiffMeanBonusPercent;
            //model.DiffMeanHourlyPayPercent = @return.DiffMeanHourlyPayPercent;
            //model.DiffMedianBonusPercent = @return.DiffMedianBonusPercent;
            //model.DiffMedianHourlyPercent = @return.DiffMedianHourlyPercent;
            //model.FemaleLowerPayBand = @return.FemaleLowerPayBand;
            //model.FemaleMedianBonusPayPercent = @return.FemaleMedianBonusPayPercent;
            //model.FemaleMiddlePayBand = @return.FemaleMiddlePayBand;
            //model.FemaleUpperPayBand = @return.FemaleUpperPayBand;
            //model.FemaleUpperQuartilePayBand = @return.FemaleUpperQuartilePayBand;
            //model.MaleLowerPayBand = @return.MaleLowerPayBand;
            //model.MaleMedianBonusPayPercent = @return.MaleMedianBonusPayPercent;
            //model.MaleMiddlePayBand = @return.MaleMiddlePayBand;
            //model.MaleUpperPayBand = @return.MaleUpperPayBand;
            //model.MaleUpperQuartilePayBand = @return.MaleUpperQuartilePayBand;
            //model.JobTitle = @return.JobTitle;
            //model.FirstName = @return.FirstName;
            //model.LastName = @return.LastName;
            //model.CompanyLinkToGPGInfo = @return.CompanyLinkToGPGInfo;
            //model.AccountingDate = @return.AccountingDate;
            //}

            // model.OrganisationId = userOrg.OrganisationId;

            if (TempData.ContainsKey("ErrorMessage")) ModelState.AddModelError("", TempData["ErrorMessage"].ToString());

            StashModel(model);

            if(Request.UrlReferrer != null && Request.UrlReferrer.Segments[2].ToString() == "Step4")
                TempData["Review"] = Request.UrlReferrer.Segments[2].ToString();

            var result = View("Step1", model);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step1")]
        public ActionResult Step1/*Create*/(ReturnViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("JobTitle");

            if (!ModelState.IsValid) return View(model);

            StashModel(model);


            var review = TempData["Review"];

            if (/*command == "Cancel" && */  review != null && review.ToString() == "Step4")
            {
                ShowAndActivateCancelButtonLink();
                return RedirectToAction("Step4");
            }

            return RedirectToAction("Step2");
        }

        [HttpGet]
        [Route("Step2")]
        public ActionResult Step2 /*Create*/()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            ChangeAndActivateBackButtonLinkName();

            //if (TempData.ContainsKey("ErrorMessage")) ModelState.AddModelError("", TempData["ErrorMessage"].ToString());

            ReturnViewModel model = null;
            model = UnstashModel<ReturnViewModel>();

            if (model == null)
            {
                TempData["ErrorMessage"] = "You session has timed out and you need to restart";
                return RedirectToAction("Step1");
            }

            if (Request.UrlReferrer != null && Request.UrlReferrer.Segments[2].ToString() == "Step4")
                TempData["Review"] = Request.UrlReferrer.Segments[2].ToString();

            var result = View("Step2", model);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step2")]
        public ActionResult Step2(ReturnViewModel model, string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid) return View(model);

            StashModel(model);

            var review = TempData["Review"];
            if (command == "Cancel" || command == "Continue" && review != null && review.ToString() == "Step4")
                return RedirectToAction("Step4");

            if (command == "Back") return RedirectToAction("Step1");

            return RedirectToAction("Step3");
        }

        [HttpGet]
        [Route("Step3")]
        public ActionResult Step3 /*GPGInfoLink*/()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            ChangeAndActivateBackButtonLinkName();

            ReturnViewModel model = null;

            model = UnstashModel<ReturnViewModel>();

            if (model == null)
            {
                model = new ReturnViewModel();
                model.AccountingDate = DateTime.Now;
            }

            //if (model == null)
            //{
            //    TempData["ErrorMessage"] = "You session has timed out and you need to restart";
            //    return RedirectToAction("Step2");
            //}

            if (Request.UrlReferrer != null && Request.UrlReferrer.Segments[2].ToString() == "Step4")
                TempData["Review"] = Request.UrlReferrer.Segments[2].ToString();

            var result = View("Step3", model);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step3")]
        public ActionResult Step3 /*GPGInfoLink*/(ReturnViewModel model, string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            StashModel(model);

            var review = TempData["Review"];
            if (command == "Cancel" || command == "Continue" && review != null && review.ToString() == "Step4")
                return RedirectToAction("Step4");

            if (command == "Back") return RedirectToAction("Step2");

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

            ReturnViewModel model = null;

            model = UnstashModel<ReturnViewModel>();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step4")]
        public ActionResult Step4  /*Confirm*/(ReturnViewModel model, string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            // model.AccountingDate = gExpectStartDate;

            if (!ModelState.IsValid) return View(model);

            //if (command == "Cancel" && Request.UrlReferrer.ToString() == "Step4")
            //    RedirectToAction("Step4");

            //if (command == "Back") return RedirectToAction("Step3");

            var @return = Repository.GetAll<Return>().FirstOrDefault(r => r.ReturnId == model.ReturnId);

            if (!@return.IsNull())
            {
                @return.Status = ReturnStatuses.Retired;
            }

            @return = new Return()
            {
                AccountingDate = gExpectStartDate,
                CompanyLinkToGPGInfo = model.CompanyLinkToGPGInfo,
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
                Status = ReturnStatuses.Draft,
                OrganisationId = model.OrganisationId
            };

            Repository.Insert<Return>(@return);
            Repository.SaveChanges();

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

            var model = UnstashModel<ReturnViewModel>();

            //try
            //{
            //    if (id < 1)
            //    {
            //        return View(id);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ModelState.AddModelError(string.Empty, ex.Message);
            //}

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Step5")]
        public ActionResult Step5(Return model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

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

        // GET: Submit/Details/5

        public ActionResult Details(int id = 1)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //  var qid = Repository.GetAll<Return>().FirstOrDefault(r => r.ReturnId == id);
            //  return View(qid);

            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            var @return = Repository.GetAll<Return>().OrderByDescending(r => r.AccountingDate).FirstOrDefault();
            // var @return = Repository.GetAll<Return>().OrderByDescending
            //     (r => r.AccountingDate).FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId && r.AccountingDate >= gExpectStartDate && r.AccountingDate < gExpectEndDate);
            return View(@return);
        }

        public void ShowAndActivateCancelButtonLink()
        {
           // var prevReferrer = controller.Request.UrlReferrer.ToString();

            //Set link initial value as there is noreferrer yet
            TempData["type"] = "hidden";

            if (Request.UrlReferrer != null && Request.UrlReferrer.Segments[2].ToString() == "Step4")
            {
                TempData["value"] = "Cancel";
                TempData["type"] = "submit";
            }
        }

        //public void UnhideAndChangeLinkName()
        //{
        //    //Set link initial value as there is noreferrer yet
        //    TempData["type"] = "hidden";

        //    if (Request.UrlReferrer == null)
        //    {
        //        TempData["value"] = "Back";
        //    }
        //    else
        //    {
        //        if (Request.UrlReferrer.ToString() == "Step4")
        //        {
        //            TempData["value"] = "Cancel";
        //        }
        //    }
        //}

        //helper methods

        public void ChangeAndActivateBackButtonLinkName()
        {
            //Set link initial value as there is noreferrer yet
            TempData["type"] = "submit";
            TempData["value"] = "Back";

            if ((Request.UrlReferrer == null) || (Request.UrlReferrer.Segments[2].ToString() == "Step2") || (Request.UrlReferrer.Segments[2].ToString() == "Step3"))
            {
                //Set link initial value as there is noreferrer yet
                TempData["value"] = "Back";
            }
            else
            {
                if (Request.UrlReferrer.Segments[2].ToString() == "Step4")
                {
                    TempData["value"] = "Cancel";
                }
            }
        }

        public void GenericLinkBackButtonHandler(string _viewName)
        {
            //Set link initial value as there is no referrer yet
            TempData["type"] = "submit";
            TempData["value"] = "Back";

            if (Request.UrlReferrer == null && _viewName == "Step1")
            {
                TempData["type"] = "hidden";
            }
            else if ((Request.UrlReferrer == null) && (_viewName == "Step2") || (_viewName == "Step3"))
            {
                TempData["value"] = "Back";
            }
            else
            {
                if (Request.UrlReferrer.Segments[2].ToString() == "Step4")
                {
                    TempData["value"] = "Cancel";
                }
            }
        }

        [HttpGet]
        public ActionResult Error()
        {
            //Show the confirmation view
            return View();
        }

        
    }
}
