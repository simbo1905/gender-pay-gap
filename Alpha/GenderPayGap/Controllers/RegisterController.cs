using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GenderPayGap;
using Extensions;
using GenderPayGap.Models;
using Newtonsoft.Json;

namespace GenderPayGap.Controllers
{
    public class RegisterController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {

            //The user can then go through the process of changing their details and email then sending another verification email
            var currentUser = GetCurrentUser();
            var model = new Models.RegisterViewModel(currentUser);
            if (currentUser!=null) ViewData["currentUser"] = currentUser;
            
            //The user can then go through the process of changing their details and email then sending another verification email
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(Models.RegisterViewModel model)
        {
            //TODO validate the submitted fields
            if (!ModelState.IsValid)return View(model);
            model.EmailAddress = model.EmailAddress.ToLower();

            var currentUser = GetCurrentUser();

            if (currentUser == null) currentUser = MvcApplication.Database.User.FirstOrDefault(u => u.EmailAddress == model.EmailAddress);

            if (currentUser == null) currentUser = new Models.GpgDatabase.User();

            if (currentUser.UserId==0)
            {
                //Check the email doesnt already exist
                if (MvcApplication.Database.User.Any(u => u.EmailAddress==model.EmailAddress))
                {
                    ModelState.AddModelError("EmailAddress", "A user with this email already exists");
                    return View(model);
                }
            }

            else if (currentUser.EmailVerifiedDate>DateTime.MinValue)
            {
                //Go to the verification process
                return RedirectToAction("Verify", new { code = currentUser.EmailVerifyCode });
            }

            //Save the submitted fields
            currentUser.Firstname = model.FirstName;
            currentUser.Lastname = model.LastName;
            currentUser.JobTitle = model.JobTitle;
            currentUser.EmailAddress = model.EmailAddress;
            currentUser.Password = model.Password;
            currentUser.EmailVerifiedDate = null;

            //Save the user to DB
            if (currentUser.UserId==0)MvcApplication.Database.User.Add(currentUser);
            MvcApplication.Database.SaveChanges();

            //Send a verification link to the email address
            try
            {
                var verifyCode = Encryption.EncryptQuerystring(currentUser.UserId.ToString());
                if (!GovNotifyAPI.SendVerifyEmail(currentUser.EmailAddress, verifyCode))
                    throw new Exception("Could not send verification email. Please try again later.");

                currentUser.EmailVerifyCode = verifyCode;
                currentUser.EmailVerifySendDate = DateTime.Now;
                MvcApplication.Database.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.Message.EqualsI("This Email Address is not registered with Gov Notify."))
                    ModelState.AddModelError("EmailAddress", ex.Message);
                else
                    ModelState.AddModelError(string.Empty, ex.Message);
            }

            //Set the verification link and save it to the db
            ViewData["currentUser"] = currentUser;

            //Pass the verify url back to the view for confirmation
            model.VerifyUrl = string.IsNullOrWhiteSpace(currentUser.EmailVerifyCode) ? null : GovNotifyAPI.GetVerifyUrl(currentUser.EmailVerifyCode);

            //Prompt user to click verification link
            return View(model);
        }


        [HttpGet]
        public ActionResult Verify(string code=null)
        {
            if (string.IsNullOrWhiteSpace(code))code = Request.Url.Query.TrimStartI(" ?");

            //Load the user from the verification code
            var currentUser = MvcApplication.Database.User.FirstOrDefault(u=>u.EmailVerifyCode==code);

            var model = new VerifyViewModel();

            //Show an error if the code doesnt exist in db
            if (currentUser == null)
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }

            if (currentUser.EmailVerifySendDate<DateTime.Now.AddDays(-6))
            {
                //TODO Resend verification code and prompt user
                ModelState.AddModelError("", "This verification link has expired. A new link has been sent to " + currentUser.EmailAddress);
                return View(model);
            }

            try
            {
                code = Encryption.DecryptQuerystring(code);
            }
            catch 
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }

            //Show an error if the code doesnt match the userId
            if (currentUser.UserId != code.ToLong())
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }

            //Set the user as verified
            currentUser.EmailVerifiedDate = DateTime.Now;

            //Save the current user
            MvcApplication.Database.SaveChanges();
            ViewData["currentUser"] = currentUser;

            //Take the user through the process of lookup address and send pin code

            model.UserId = currentUser.UserId;
            return View(model);
        }

        [HttpPost]
        public ActionResult Verify(VerifyViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public ActionResult Organisation(string code=null)
        {
            if (string.IsNullOrWhiteSpace(code)) code = Request.Url.Query.TrimStartI(" ?");

            //Load the user from the verification code
            var currentUser = MvcApplication.Database.User.FirstOrDefault(u => u.EmailVerifyCode == code);

            var model = new OrganisationViewModel();
            
            //Show an error if the code doesnt exist in db
            if (currentUser == null)
            {
                ModelState.AddModelError("", "Invalid request");
                return View(model);
            }

            try
            {
                code = Encryption.DecryptQuerystring(code);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }

            //Show an error if the code doesnt match the userId
            if (currentUser.UserId != code.ToLong())
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }
            model.UserId = currentUser.UserId;

            if (currentUser != null) ViewData["currentUser"] = currentUser;

            return View(model);
        }

        [HttpPost]
        public ActionResult Organisation(OrganisationViewModel model)
        {
            if (model == null || model.UserId==0)
            {
                ModelState.AddModelError("", "Invalid request");
                return View(model);
            }

            //TODO validate the submitted fields
            if (!ModelState.IsValid) return View(model);

            var currentUser = GetCurrentUser();

            if (model.OrganisationType != Models.GpgDatabase.Organisation.OrgTypes.Unknown)
            {
                if (string.IsNullOrWhiteSpace(model.OrganisationRef))
                {
                    switch (model.OrganisationType)
                    {
                        case Models.GpgDatabase.Organisation.OrgTypes.Company:
                            ModelState.AddModelError("OrganisationRef", "You must enter your company number");
                            break;
                        case Models.GpgDatabase.Organisation.OrgTypes.Charity:
                            ModelState.AddModelError("OrganisationRef", "You must enter your charity number");
                            break;
                        case Models.GpgDatabase.Organisation.OrgTypes.Government:
                            ModelState.AddModelError("OrganisationRef", "You must enter your department reference");
                            break;
                    }
                    return View(model);
                }
                else if (string.IsNullOrWhiteSpace(model.OrganisationName))
                {
                    //TODO Lookup the company details
                    var company = CompaniesHouseAPI.Lookup(model.OrganisationRef);

                    model.OrganisationName = company.company_name;
                    model.OrganisationAddress = company.registered_office_address.address_line_1;
                    model.OrganisationAddress = company.registered_office_address.address_line_2;
                    model.OrganisationAddress = company.registered_office_address.country;
                    model.OrganisationAddress = company.registered_office_address.post_code;
                }
                else
                {
                    //TODO Send the PIN and confirm when sent

                }
            }

            return View(model);
        }


        public ActionResult Find(VerifyViewModel model)
        {
            //Validate the submitted fields
            if (!ModelState.IsValid) return View(model);

            var currentUser = GetCurrentUser();


            //Prompt user to click verification link
            return View(currentUser);
        }

        public ActionResult Confirm(VerifyViewModel model)
        {
            //Validate the submitted fields
            if (!ModelState.IsValid) return View(model);

            var currentUser = GetCurrentUser();


            //Prompt user to click verification link
            return View(currentUser);
        }

        //Reset the users password
        public ActionResult Reset()
        {
            return View();
        }
    }
}