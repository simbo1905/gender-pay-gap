using System;
using System.Linq;
using System.Security.Authentication;
using System.Web;
using System.Web.Mvc;
using Extensions;
using GenderPayGap.Models.SqlDatabase;
using Autofac;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Classes;
using System.Net;
using System.Security.Principal;
using System.Web.WebPages;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Register")]
    [Route("{action}")]
    public class RegisterController : BaseController
    {
        public RegisterController():base(){}
        public RegisterController(IContainer container): base(container){}

        [HttpGet]
        [Route("Step1")]
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
                return View("CustomError", new ErrorViewModel()
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
        [ValidateAntiForgeryToken]
        [SpamProtection()]
        [Route("Step1")] 
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
                    ModelState.AddModelError("", "A registered user with this email already exists.");
                    ModelState.AddModelError("", "Please enter a different email address.");
                    return View("Step1", model);
                }
                var remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationExpiryHours) - DateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    ModelState.AddModelError("", "Another user is trying to register using this email address.");
                    ModelState.AddModelError("", "Please enter a different email address or try again in " + remainingTime.ToFriendly(maxParts: 2) + ".");
                    return View("Step1", model);
                }

                
                //If from a previous user then delete the previous user
                Repository.Delete(currentUser);

            }

            //Save the submitted fields
            currentUser = new User();
            currentUser.Created = DateTime.Now;
            currentUser.Modified = currentUser.Created;
            currentUser.Firstname = model.FirstName;
            currentUser.Lastname = model.LastName;
            currentUser.JobTitle = model.JobTitle;
            currentUser.EmailAddress = model.EmailAddress;
            currentUser.PasswordHash = model.Password.GetSHA512Checksum();
            currentUser.EmailVerifySendDate = null;
            currentUser.EmailVerifiedDate = null;
            currentUser.EmailVerifyCode = null;
            //Save the user to ensure UserId>0 for new status
            Repository.Insert(currentUser);
            Repository.SaveChanges();
            currentUser.SetStatus(UserStatuses.New,currentUser.UserId);

            //Send the verification code and showconfirmation
            return ResendVerifyCode(currentUser);
        }

        ////Send the verification code and show confirmation
        ActionResult ResendVerifyCode(User currentUser)
        {
            //Send a verification link to the email address
            try
            {
                var verifyCode = Encryption.EncryptQuerystring(currentUser.UserId + ":" + currentUser.Created.ToSmallDateTime());
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
            if (!ModelState.IsValid) return View("Step1");

            //Prompt user to open email and verification link
            return View("Step2", new VerifyViewModel() { EmailAddress = currentUser.EmailAddress });
        }

        [HttpGet]
        [Authorize]
        [Route("Step2")]
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
            if (errorViewModel.ActionUrl != Url.Action("Step2", "Register") && errorViewModel.Description!= "You have not yet verified your email address.")
                return result;

            //Allow resend of verification if sent over 24 hours ago
            if (currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
                return View("Step2", new VerifyViewModel() { EmailAddress = currentUser.EmailAddress, Expired = true });

            ActionResult result1;

            var remaining = currentUser.VerifyAttemptDate == null ? TimeSpan.Zero : currentUser.VerifyAttemptDate.Value.AddMinutes(Properties.Settings.Default.LockoutMinutes) - DateTime.Now;
            if (currentUser.VerifyAttempts >= Properties.Settings.Default.MaxEmailVerifyAttempts && remaining > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel()
                {
                    Title = "Invalid Verification Code",
                    Description = "You have failed too many verification attempts.",
                    CallToAction = "Please log out of the system and try again in "+remaining.ToFriendly(maxParts:2)+".",
                    ActionUrl = Url.Action("LogOut", "Home")
                });
            }

            if (currentUser.EmailVerifyCode != code)
            {
                currentUser.VerifyAttempts++;
                result1 = View("CustomError", new ErrorViewModel()
                {
                    Title = "Invalid Verification Code",
                    Description = "The verification code you have entered is invalid."
                });
            }
            else 
            {
                //Set the user as verified
                currentUser.EmailVerifiedDate = DateTime.Now;

                //Mark the user as active
                currentUser.SetStatus(UserStatuses.Active, currentUser.UserId, "Email verified");

                currentUser.VerifyAttempts = 0;

                result1 = View("Step2", new VerifyViewModel() { EmailAddress = currentUser.EmailAddress, Verified = true });
            }
            currentUser.VerifyAttemptDate = DateTime.Now;

            //Save the current user
            Repository.SaveChanges();

            //Prompt the user with confirmation
            return result1;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Route("Step2")]
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
        [Route("Step3")]
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

            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) model = new OrganisationViewModel();
            return View("Step3", model);
        }

        /// <summary>
        /// Get the sector type
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Route("Step3")]
        public ActionResult Step3(OrganisationViewModel model)
        {
            var m = UnstashModel<OrganisationViewModel>();
            if (m != null && m.Employers != null && m.Employers.Count > 0) model.Employers = m.Employers;

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
        [Route("Step4")]
        public ActionResult Step4()
        {
            var model=UnstashModel<OrganisationViewModel>();

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
        [Route("Step4")]
        public ActionResult Step4(OrganisationViewModel model,string command)
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
            model.EmployerCurrentPage = 1;
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
        [Route("Step5")]
        public ActionResult Step5()
        {
            var model = UnstashModel<OrganisationViewModel>();

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
        [Route("Step5")]
        public ActionResult Step5(OrganisationViewModel model, string command)
        {
            var m = UnstashModel<OrganisationViewModel>();
            if (m != null && m.Employers!=null && m.Employers.Count>0) model.Employers = m.Employers;

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
                ModelState.Clear();
                StashModel(model);
                return View("Step5", model);
            }

            if (command.StartsWithI("employer_"))
            {
                var employerIndex = command.AfterFirst("employer_").ToInt32();
                var reference = model.Employers[employerIndex].CompanyNumber;
                var org = Repository.GetAll<Organisation>().FirstOrDefault(o => o.PrivateSectorReference == reference);
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
                model.SelectedEmployerIndex = employerIndex;
            }

            ModelState.Clear();
            StashModel(model);
            if (model.SelectedEmployerIndex<0)
                return View("Step5", model);

            return RedirectToAction("Step6");
        }

        [HttpGet]
        [Authorize]
        [Route("Step6")]
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
        [Route("Step6")]
        public ActionResult Step6(OrganisationViewModel model,string command)
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
                org.Created = now;
                org.Modified = now;
                org.SetStatus(OrganisationStatuses.New, currentUser.UserId);
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
            if (userOrg!=null && userOrg.UserId != currentUser.UserId)
            {
                Repository.Delete(userOrg);
                userOrg = null;
            }

            if (userOrg == null)
            {
                userOrg = new GenderPayGap.Models.SqlDatabase.UserOrganisation()
                {
                    UserId = currentUser.UserId,
                    OrganisationId = org.OrganisationId,
                    Created = DateTime.Now
                };
                Repository.Insert(userOrg);
            }
            userOrg.PINCode = null;
            userOrg.PINSentDate = null;
            Repository.SaveChanges();

            return RedirectToAction("SendPIN");
        }

        ActionResult GetSendPIN()
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

            //Get the organisation
            var org = Repository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == userOrg.OrganisationId);

            //If a pin has never been sent or resend button submitted then send one immediately
            if (string.IsNullOrWhiteSpace(userOrg.PINCode) || userOrg.PINSentDate.EqualsI(null, DateTime.MinValue) || Request.HttpMethod.EqualsI("POST"))
            {
                try
                {
                    //Marke the user org as ready to send a pin
                    userOrg.PINCode = null;
                    userOrg.PINSentDate = null;
                    Repository.SaveChanges();

                    //Generate a new pin
                    var pin = Numeric.Rand(0, 999999);

                    //Try and send the PIN in post
                    if (!this.SendPinInPost(currentUser, org, pin.ToString()))
                        throw new Exception("Could not send PIN in the POST. Please try again later.");

                    //Try and send the confirmation email
                    if (!this.SendConfirmEmail(currentUser.EmailAddress))
                        throw new Exception("Could not send confirmation email. Please try again later.");

                    //Save the PIN and confirm code
                    userOrg.PINCode = pin.ToString("000000");
                    userOrg.PINSentDate = DateTime.Now;
                    Repository.SaveChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View("SendPIN");
                }
            }

            //Prepare view parameters
            ViewBag.Resend = !string.IsNullOrWhiteSpace(userOrg.PINCode) && !userOrg.PINSentDate.EqualsI(null, DateTime.MinValue)
                && userOrg.PINSentDate.Value.AddDays(Properties.Settings.Default.PinInPostMinRepostDays) < DateTime.Now;
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
            ViewBag.Organisation = org.OrganisationName;
            ViewBag.Address = org.Address.GetAddress(",<br/>");
            return View("SendPIN");
        }

        [Authorize]
        [HttpGet]
        [Route("SendPIN")]
        public ActionResult SendPIN()
        {
            return GetSendPIN();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("SendPIN")]
        public ActionResult SendPIN(string command)
        {
            return GetSendPIN();
        }

        [HttpGet]
        [Authorize]
        [Route("ConfirmPIN")]
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

            if (userOrg == null) throw new AuthenticationException();

            ActionResult result1;

            var remaining = userOrg.ConfirmAttemptDate == null ? TimeSpan.Zero : userOrg.ConfirmAttemptDate.Value.AddMinutes(Properties.Settings.Default.LockoutMinutes) - DateTime.Now;
            if (userOrg.ConfirmAttempts >= Properties.Settings.Default.MaxPinAttempts && remaining > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel()
                {
                    Title = "Invalid PIN Code",
                    Description = "You have tried too many incorrect PIN codes.",
                    CallToAction = "Please log out of the system and try again in " + remaining.ToFriendly(maxParts: 2) + ".",
                    ActionUrl = Url.Action("LogOut", "Home")
                });
            }

            remaining = userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostMinRepostDays) - DateTime.Now;
            var model=new CompleteViewModel();
            model.PIN = null;
            model.AllowResend = remaining <= TimeSpan.Zero;
            model.Remaining = remaining.ToFriendly(maxParts:2);
            //Show the PIN textbox and button
            return View("ConfirmPIN",model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("ConfirmPIN")]
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


            if (userOrg == null) throw new AuthenticationException();

            ActionResult result1;

            var remaining = userOrg.ConfirmAttemptDate == null ? TimeSpan.Zero : userOrg.ConfirmAttemptDate.Value.AddMinutes(Properties.Settings.Default.LockoutMinutes) - DateTime.Now;
            if (userOrg.ConfirmAttempts >= Properties.Settings.Default.MaxPinAttempts && remaining > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel()
                {
                    Title = "Invalid PIN Code",
                    Description = "You have tried too many incorrect PIN codes.",
                    CallToAction = "Please log out of the system and try again in " + remaining.ToFriendly(maxParts: 2) + ".",
                    ActionUrl = Url.Action("LogOut", "Home")
                });
            }

            if (userOrg.PINCode == model.PIN)
            {
                //Set the user org as confirmed
                userOrg.PINConfirmedDate = DateTime.Now;

                //Mark the organisation as active
                userOrg.Organisation.SetStatus(OrganisationStatuses.Active, currentUser.UserId, "PIN Confirmed");

                //Mark the address as confirmed
                userOrg.Organisation.Address.SetStatus(AddressStatuses.Confirmed, currentUser.UserId, "PIN Confirmed");

                userOrg.ConfirmAttempts = 0;

                result1 = RedirectToAction("Complete");
            }
            else
            {
                userOrg.ConfirmAttempts++;
                ModelState.AddModelError("PIN", "This PIN code is incorrect.");
                result1 = View("ConfirmPIN", model);
            }
            userOrg.ConfirmAttemptDate = DateTime.Now;

            //Save the current user
            Repository.SaveChanges();

            //Prompt the user with confirmation
            return result1;
        }

        [HttpGet]
        [Authorize]
        [Route("Complete")]
        public ActionResult Complete()
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorViewModel = result.Model as ErrorViewModel;
            if (errorViewModel == null) throw new AuthenticationException();
            if (errorViewModel.ActionUrl != Url.Action("Create", "Return"))
                return result;

            //Show the confirmation view
            return View("Complete");
        }
    }
}