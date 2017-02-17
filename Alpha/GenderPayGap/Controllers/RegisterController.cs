using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Web;
using System.Web.Mvc;
using GenderPayGap;
using Extensions;
using Newtonsoft.Json;
using IdentityServer3.Core;
using GenderPayGap.Core.Interfaces;
using GpgDB.Models.GpgDatabase;
using Autofac;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Classes;
using System.Security.Principal;
using GpgDB;
using System.Net;

namespace GenderPayGap.WebUI.Controllers
{
    public class RegisterController : BaseController
    {
        public RegisterController():base(){}
        public RegisterController(IContainer container): base(container){}

        [HttpGet]
        public ActionResult Step1()
        {
            User currentUser;
            //The user can then go through the process of changing their details and email then sending another verification email
            if (User.Identity.IsAuthenticated)
            {
                //Ensure user has completed the registration process
                var result = CheckUserRegisteredOk(out currentUser);
                if (result != null) return result;

                //If user is fully registered then start submit process
                return View("Error", new ErrorViewModel()
                {
                    Title = "Registration Complete",
                    Description = "You have already completed registration.",
                    CallToAction = "Next Step: Submit your Gender Pay Gap data",
                    ActionUrl = Url.Action("Create", "Return")
                });
            }

            //Start new user registration
            return View("Step1",new Models.RegisterViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [SpamProtection()]
        public ActionResult Step1(Models.RegisterViewModel model)
        {
            if (model.Password.ContainsI("password"))ModelState.AddModelError("Password","Password cannot contain the word 'password'");
            //TODO validate the submitted fields
            if (!ModelState.IsValid)return View("Step1",model);

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();

            //Check this email address isnt already assigned to another user
            var currentUser = Repository.FindUserByEmail(model.EmailAddress);
            if (currentUser!=null)
            {
                if (currentUser.EmailVerifiedDate != null)
                {
                    ModelState.AddModelError("EmailAddress", "A registered user with this email already exists. Please enter a different email address.");
                    return View("Step1", model);
                }
                var remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationExpiryHours) - DateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    ModelState.AddModelError("EmailAddress", "Another user is trying to register using this email and has "+ remainingTime.ToFriendly(maxParts: 2) + " to verify this address. Please enter a different email address or try again later.");
                    return View("Step1", model);
                }

                
                //If from a previous user then delete the previous user
                Repository.Delete(currentUser);

            }
            else
            {
                currentUser=new User();
            }

            //Save the submitted fields
            currentUser.Created = DateTime.Now;
            currentUser.Modified = currentUser.Created;
            currentUser.Firstname = model.FirstName;
            currentUser.Lastname = model.LastName;
            currentUser.JobTitle = model.JobTitle;
            currentUser.EmailAddress = model.EmailAddress;
            currentUser.Password = model.Password;
            currentUser.EmailVerifySendDate = null;
            currentUser.EmailVerifiedDate = null;
            currentUser.EmailVerifyCode = null;
            currentUser.CurrentStatus=UserStatuses.New;
            currentUser.CurrentStatusDate = DateTime.Now;

            //Save the user to DB
            if (currentUser.UserId==0)Repository.Insert(currentUser);
            Repository.SaveChanges();

            //Send the verification code and showconfirmation
            return ResendVerifyCode(currentUser);
        }

        ////Send the verification code and show confirmation
        ActionResult ResendVerifyCode(User currentUser)
        {
            //Send a verification link to the email address
            try
            {
                var verifyCode = Encryption.EncryptQuerystring(currentUser.UserId + ":" + currentUser.Created.ToShortDateTime());
                if (!this.SendVerifyEmail(currentUser.EmailAddress, verifyCode))
                    throw new Exception("Could not send verification email. Please try again later.");

                currentUser.EmailVerifyCode = verifyCode;
                currentUser.EmailVerifySendDate = DateTime.Now;
                Repository.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.Message.EqualsI("This Email Address is not registered with Gov Notify."))
                    ModelState.AddModelError("EmailAddress", ex.Message);
                else
                    ModelState.AddModelError(string.Empty, ex.Message);
            }

            //Prompt user to open email and verification link
            return View("Step2", new VerifyViewModel() { EmailAddress = currentUser.EmailAddress });
        }

        [HttpGet]
        [Authorize]
        public ActionResult Step2(string code=null)
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorViewModel = result.Model as ErrorViewModel;
            if (errorViewModel == null) throw new AuthenticationException();
            if (errorViewModel.ActionUrl != Url.Action("Step2", "Register") && errorViewModel.Description!= "You have not verified your email address.")
                return result;

            //Allow resend of verification if sent over 24 hours ago
            if (currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
                return View("Step2", new VerifyViewModel() { EmailAddress = currentUser.EmailAddress, Expired = true });

            if (string.IsNullOrWhiteSpace(code) )
            {
                ModelState.AddModelError("Diffmean", "Invalid email verification code");
                return View("Step2");
            }

            //Show an error if the code doesnt exist in db
            if (currentUser.EmailVerifyCode!=code)
            {
                var attempts = Session["EmailVerificationAttempts:" + currentUser.EmailAddress].ToInt32();
                if (attempts > 3)
                {
                    return View("Error", new ErrorViewModel()
                    {
                        Title = "Invalid Verification Code",
                        Description = "You have failed too many verification attempts.",
                        CallToAction = "Please log out of the system and try again later.",
                        ActionUrl = Url.Action("LogOut", "Home")
                    });
                }
                Session["EmailVerificationAttempts:" + currentUser.EmailAddress] = attempts + 1;
                return View("Error", new ErrorViewModel()
                {
                    Title = "Invalid Verification Code",
                    Description = "The verification code you have entered is invalid."
                });
            }

            //Set the user as verified
            currentUser.EmailVerifiedDate = DateTime.Now;

            //Save the current user
            Repository.SaveChanges();

            //Prompt the user with confirmation
            return View("Step2", new VerifyViewModel() { EmailAddress = currentUser.EmailAddress, Verified = true});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Step2(VerifyViewModel model)
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            var errorViewModel = result.Model as ErrorViewModel;
            if (result == null || errorViewModel == null) throw new AuthenticationException();
            if (errorViewModel.ActionUrl != Url.Action("Step2", "Register")) return result;

            //Reset the verification send date
            currentUser.EmailVerifySendDate = null;
            currentUser.EmailVerifyCode = null;
            Repository.SaveChanges();

            //Call GET action which will automatically resend
            return Step2();
        }

        [HttpGet]
        [Authorize]
        public ActionResult Step3()
        {

            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user needs to select an organisation
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorViewModel = result.Model as ErrorViewModel;
            if (errorViewModel == null) throw new AuthenticationException();
            if (!errorViewModel.ActionUrl.IsAny(Url.Action("Step3", "Register")))
                return result;
            return View("Step3", new OrganisationViewModel());
        }

        /// <summary>
        /// Get the sector type
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Step3(OrganisationViewModel model)
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user needs to select an organisation
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            var errorViewModel = result.Model as ErrorViewModel;
            if (result == null || errorViewModel == null) throw new AuthenticationException();
            if (!errorViewModel.ActionUrl.IsAny(Url.Action("Step3", "Register")))
                return result;

            //TODO validate the submitted fields
            ModelState.Clear();

            if (!model.SectorType.EqualsI(SectorTypes.Private, SectorTypes.Public))
            {
                ModelState.AddModelError("SectorType", "You must select your organisation type");
                return View("Step3", model);
            }

            //TODO Remove this when public sector is available
            if (!model.SectorType.EqualsI(SectorTypes.Private))throw new NotImplementedException();

            StashModel(model);
            return RedirectToAction("Step4");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Step4()
        {
            var model=TempData["Model"] as OrganisationViewModel;

            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //This should always throw an error then redirect to step3
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorViewModel = result.Model as ErrorViewModel;
            if (errorViewModel == null) throw new AuthenticationException();
            if (model==null || !errorViewModel.ActionUrl.IsAny(Url.Action("Step3", "Register"))) return result;

            return View("Step4", model);
        }


        /// <summary>
        /// Get the search text
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Step4(OrganisationViewModel model)
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorModel = result.Model as ErrorViewModel;
            if (errorModel == null) throw new AuthenticationException();
            if (!errorModel.ActionUrl.IsAny(Url.Action("Step3", "Register"))) return result;

            //TODO validate the submitted fields
            model.SearchText = model.SearchText.TrimI();
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(model.SearchText))
            {
                ModelState.AddModelError("SearchText", "You must enter an employer name or company number");
                return View("Step4", model);
            }
            if (model.SearchText.Length<3 || model.SearchText.Length > 100)
            {
                ModelState.AddModelError("SearchText", "You must enter between 3 and 100 characters");
                return View("Step4", model);
            }

            model.SelectedEmployerIndex = 0;

            switch (model.SectorType)
            {
                case SectorTypes.Private:
                    var employerRecords = model.EmployerRecords;
                    model.Employers = CompaniesHouseAPI.SearchEmployers(out employerRecords, model.SearchText, model.EmployerCurrentPage, model.EmployerPageSize);
                    model.EmployerRecords = employerRecords;
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (model.EmployerRecords<=0)return View("Step4", model);

            StashModel(model);
            return RedirectToAction("Step5");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Step5()
        {
            var model = TempData["Model"] as OrganisationViewModel;
            if (model != null) return Step5(model,null);

            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //This should always throw an error then redirect to step3
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorViewModel = result.Model as ErrorViewModel;
            if (errorViewModel == null) throw new AuthenticationException();
            return result;
        }

        /// <summary>
        /// Get the search text
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Step5(OrganisationViewModel model, string command)
        {
            var m = TempData["Model"] as OrganisationViewModel;
            if (m != null && m.Employers!=null && m.Employers.Count>0) model.Employers = m.Employers;

            //Go back if requested
            if (command == "back") return View("Step4", model);

            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorModel = result.Model as ErrorViewModel;
            if (errorModel == null) throw new AuthenticationException();
            if (!errorModel.ActionUrl.IsAny(Url.Action("Step3", "Register"))) return result;

            model.SelectedEmployerIndex = -1;

            
            bool doSearch = false;
            if (command == "search")
            {
                model.SearchText = model.SearchText.Trim();
                if (string.IsNullOrWhiteSpace(model.SearchText))
                {
                    ModelState.AddModelError("SearchText", "You must enter an employer name or company number");
                    return View("Step5", model);
                }
                if (model.SearchText.Length < 3 || model.SearchText.Length > 100)
                {
                    ModelState.AddModelError("SearchText", "You must enter between 3 and 100 characters");
                    return View("Step4", model);
                }
                doSearch = true;
            }
            else if (command == "pageNext")
            {
                if (model.EmployerCurrentPage >= model.EmployerPages)
                {
                    ModelState.AddModelError("", "No more pages");
                    return View("Step5", model);
                }
                model.EmployerCurrentPage++;
                doSearch = true;
            }
            else if (command == "pagePrev")
            {
                if (model.EmployerCurrentPage<=1)
                {
                    ModelState.AddModelError("", "No previous page");
                    return View("Step5", model);
                }
                model.EmployerCurrentPage--;
                doSearch = true;
            }
            else if (command.StartsWithI("page_"))
            {
                var page = command.AfterFirst("page_").ToInt32();
                if (page<1 || page>model.EmployerPages)
                {
                    ModelState.AddModelError("", "Invalid page number");
                    return View("Step5", model);
                }
                if (page != model.EmployerCurrentPage)
                {
                    model.EmployerCurrentPage = page;
                    doSearch = true;
                }
            }

            if (doSearch)
            {
                switch (model.SectorType)
                {
                    case SectorTypes.Private:
                        var employerRecords = model.EmployerRecords;
                        model.Employers = CompaniesHouseAPI.SearchEmployers(out employerRecords, model.SearchText, model.EmployerCurrentPage, model.EmployerPageSize);
                        model.EmployerRecords = employerRecords;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return View("Step5", model);
            }

            if (command.StartsWithI("employer_"))
            {
                var employerIndex = command.AfterFirst("employer_").ToInt32();

                var org = Repository.GetAll<Organisation>().FirstOrDefault(o => o.PrivateSectorReference == model.Employers[employerIndex].CompanyNumber);
                if (org != null)
                {
                    var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId);
                    if (userOrg.UserId != currentUser.UserId)
                    {
                        var user = Repository.GetAll<User>().FirstOrDefault(u => u.UserId == userOrg.UserId);

                        if (userOrg.PINSentDate != null)
                        {
                            ModelState.AddModelError("", "Another user ("+user.Fullname+") has already registered for this organisation.");
                            return View("Step1", model);
                        }

                        var remainingTime = userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostExpiryDays) - DateTime.Now;
                        if (remainingTime > TimeSpan.Zero)
                        {
                            ModelState.AddModelError("EmailAddress", "Another user (" + user.Fullname + ") is trying to register this organisation and has " + remainingTime.ToFriendly(maxParts: 2) + " to confirm their registered address. Please try again later.");
                            return View("Step1", model);
                        }
                    }
                }
            }

            StashModel(model);
            if (model.SelectedEmployerIndex<0)
                return View("Step5", model);

            return RedirectToAction("Step6");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Step6()
        {
            var model = UnstashModel<OrganisationViewModel>();
            if (model != null)
            {
                StashModel(model);
                return View("Step6",model);
            }

            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //This should always throw an error then redirect to step3
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorViewModel = result.Model as ErrorViewModel;
            if (errorViewModel == null) throw new AuthenticationException();
            return result;

        }

        /// <summary>
        /// Create the organisation and send a PIN in the POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Step6(OrganisationViewModel model)
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorModel = result.Model as ErrorViewModel;
            if (errorModel == null) throw new AuthenticationException();
            if (errorModel.ActionUrl != Url.Action("Step3", "Register")) return result;

            //Load the employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) throw new HttpException((int)HttpStatusCode.BadRequest,"Missing session data");
            if (m.Employers != null && m.Employers.Count > 0) model.Employers = m.Employers;

            var employer = model.Employers[model.SelectedEmployerIndex];
            //Save the new organisation
            var org = Repository.GetAll<Organisation>().FirstOrDefault(o => o.PrivateSectorReference == employer.CompanyNumber);
            if (org == null)
            {
                var now = DateTime.Now;
                org = new Organisation();
                org.SectorType = model.SectorType.Value;
                org.OrganisationName = employer.Name;
                org.PrivateSectorReference = employer.CompanyNumber;
                org.CurrentStatus = OrganisationStatuses.New;
                org.CurrentStatusDate = now;
                org.Created = now;
                org.Modified = now;
                Repository.Insert(org);
                Repository.SaveChanges();
            }

            //Save the new address
            var address = Repository.GetAll<OrganisationAddress>().FirstOrDefault(a => a.OrganisationId == org.OrganisationId && a.PostCode==employer.PostCode);
            if (address == null)
            {
                address = new OrganisationAddress();
                address.OrganisationId = org.OrganisationId;
                address.Address1 = employer.Address1;
                address.Address2 = employer.Address2;
                address.Address3 = employer.Address3;
                address.Country = employer.Country;
                address.PostCode = employer.PostCode;
                Repository.Insert(address);
                Repository.SaveChanges();
            }
     
            //TODO Send the PIN and confirm when sent
            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId);

            //If from a previous user then delete the previous user
            if (userOrg.UserId != currentUser.UserId)
            {
                Repository.Delete(userOrg);
                userOrg = null;
            }

            if (userOrg == null)
            {
                userOrg = new GpgDB.Models.GpgDatabase.UserOrganisation()
                {
                    UserId = currentUser.UserId,
                    OrganisationId = org.OrganisationId,
                    Created = DateTime.Now
                };
                Repository.Insert(userOrg);
            }
            userOrg.PINCode = null;
            userOrg.ConfirmCode = null;
            userOrg.PINSentDate = null;
            Repository.SaveChanges();

            return RedirectToAction("SendPIN");
        }

        bool SendPIN(User user, UserOrganisation userOrg)
        {
            try
            {
                //Marke the user org as ready to send a pin
                userOrg.PINCode = null;
                userOrg.PINSentDate = null;
                userOrg.ConfirmCode = null;
                Repository.SaveChanges();

                //Generate a new pin
                var pin = Numeric.Rand(0, 999999);

                //Try and send the PIN in post
                if (!this.SendPinInPost(user.Fullname + " (" + user.JobTitle + ")", user.EmailAddress, pin.ToString()))
                    throw new Exception("Could not send PIN in the POST. Please try again later.");

                //Generate a confimation link
                var confirmCode = Encryption.EncryptQuerystring(string.Format("{0}:{1}:{2}", userOrg.UserId, userOrg.OrganisationId, DateTime.Now.ToShortDateTime()));

                //Try and send the confirmation email
                if (!this.SendConfirmEmail(user.EmailAddress, confirmCode))
                    throw new Exception("Could not send confirmation email. Please try again later.");

                //Save the PIN and confirm code
                userOrg.PINCode = pin.ToString("000000");
                userOrg.PINSentDate = DateTime.Now;
                userOrg.ConfirmCode = confirmCode;
                Repository.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return false;
            }
            //Prompt user to open email and verification link
            return true;
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult SendPIN()
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process up to PIN confirmation
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorModel = result.Model as ErrorViewModel;
            if (errorModel == null) throw new AuthenticationException();
            if (!errorModel.ActionUrl.IsAny(Url.Action("SendPIN", "Register"))) return result;

            //Get the user organisation
            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            //Get the latest address for the organisation
            var address = Repository.GetAll<OrganisationAddress>().OrderByDescending(oa=>oa.Modified).FirstOrDefault(oa => oa.OrganisationId==userOrg.OrganisationId);

            //If a pin has never been sent or resend button submitted then send one immediately
            if (string.IsNullOrWhiteSpace(userOrg.PINCode) || userOrg.PINSentDate.EqualsI(null,DateTime.MinValue) || Request.HttpMethod.EqualsI("POST")) SendPIN(currentUser, userOrg);

            //Prepare view parameters
            ViewBag.Resend = !string.IsNullOrWhiteSpace(userOrg.PINCode) && !userOrg.PINSentDate.EqualsI(null, DateTime.MinValue)
                && userOrg.PINSentDate.Value.AddDays(Properties.Settings.Default.PinInPostMinRepostDays) < DateTime.Now;
            ViewBag.UserFullName=currentUser.Fullname;
            ViewBag.Address = address.GetAddress(",<br/>");
            return View("SendPIN");
        }

        [HttpGet]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPIN()
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorViewModel = result.Model as ErrorViewModel;
            if (errorViewModel == null) throw new AuthenticationException();
            if (errorViewModel.ActionUrl != Url.Action("ConfirmPIN", "Register"))
                return result;

            //Get the user organisation
            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            var remainingTime = userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostMinRepostDays) - DateTime.Now;
            var model=new CompleteViewModel();
            model.PIN = null;
            model.AllowResend = remainingTime <= TimeSpan.Zero;
            model.Remaining = remainingTime.ToFriendly(maxParts:2);
            //Show the PIN textbox and button
            return View("ConfirmPIN");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPIN(CompleteViewModel model)
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorViewModel = result.Model as ErrorViewModel;
            if (errorViewModel == null) throw new AuthenticationException();
            if (errorViewModel.ActionUrl != Url.Action("ConfirmPIN", "Register"))
                return result;

            //Ensure they have entered a PIN
            if (!ModelState.IsValid) return View("ConfirmPIN", model);

            //Get the user organisation
            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            //Show an error if the code doesnt exist in db
            if (userOrg.PINCode != model.PIN)
            {
                var attempts = Session["PINAttempts:" + currentUser.EmailAddress].ToInt32();
                if (attempts > 3)
                {
                    return View("Error", new ErrorViewModel()
                    {
                        Title = "Invalid PIN Code",
                        Description = "You have tried too many incorrect PIN codes.",
                        CallToAction = "Please log out of the system and try again later.",
                        ActionUrl = Url.Action("LogOut", "Home")
                    });
                }
                Session["PIN:" + currentUser.EmailAddress] = attempts + 1;
                ModelState.AddModelError("PIN","This PIN code is incorrect.");
                return View("ConfirmPIN",model);
            }

            //Set the user as verified
            currentUser.EmailVerifiedDate = DateTime.Now;

            //Save the current user
            Repository.SaveChanges();

            //Prompt the user with confirmation
            return RedirectToAction("Complete");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Complete()
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result != null) return result;

            //Show the confirmation view
            return View("Complete");
        }

        [HttpGet]
        public ActionResult Error()
        {
            //Show the confirmation view
            return View();
        }
    }
}