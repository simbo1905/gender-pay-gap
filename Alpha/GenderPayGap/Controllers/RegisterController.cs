using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GenderPayGap;
using Extensions;
using GenderPayGap.Models;

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

            var currentUser = GetCurrentUser();
            model.EmailAddress = model.EmailAddress.ToLower();

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
            else if (currentUser.EmailVerifySendDate > DateTime.MinValue)
            {
                //Prompt user to resend PIN
                return View(model);
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

            //Prompt user to click verification link
            return View(model);
        }


        public ActionResult Verify(string code)
        {
            try
            {
                //TODO Decrypt the id value to get the id of the user
                code = global::Extensions.Encryption.DecryptQuerystring(code);
            }
            catch (Exception ex)//Exception thrown when decrypt fails
            {
                //Show error message
                return View();
            }

            //Load the user from the verification code
            var currentUser = MvcApplication.Database.User.FirstOrDefault(u=>u.EmailVerifyCode==code);

            //Show an error if the code doesnt exist in db
            if (currentUser == null)
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(code);
            }

            if (currentUser.EmailVerifySendDate<DateTime.Now.AddDays(-6))
            {
                //TODO Resend verification code and prompt user
                ModelState.AddModelError("", "This verification link has expired. A new link has been sent to " + currentUser.EmailAddress);
                return View(code);
            }

            //Show an error if the code doesnt match the userId
            if (currentUser.UserId != code.ToLong())
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(code);
            }

            //Set the user as verified
            currentUser.EmailVerifiedDate = DateTime.Now;

            //Save the current user
            MvcApplication.Database.SaveChanges();
            ViewData["currentUser"] = currentUser;

            var model = new VerifyViewModel();
            //Take the user through the process of lookup address and send pin code
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