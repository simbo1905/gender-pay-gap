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
        [ValidateAntiForgeryToken]
        public ActionResult Step1(Models.RegisterViewModel model)
        {
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
                if (currentUser.EmailVerifySendDate!=null && currentUser.EmailVerifySendDate.Value.AddHours(Properties.Settings.Default.EmailVerificationExpiryHours) > DateTime.Now)
                {
                    ModelState.AddModelError("EmailAddress", "This email address is currently awaiting verification. Please try again later.");
                    return View("Step1", model);
                }
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
            if (currentUser.UserId==0)GpgDatabase.Default.User.Add(currentUser);
            GpgDatabase.Default.SaveChanges();

            //Send the verification code and showconfirmation
            return ResendVerifyCode(currentUser);
        }

        ////Send the verification code and show confirmation
        ActionResult ResendVerifyCode(User currentUser)
        {
            //Send a verification link to the email address
            try
            {
                var verifyCode = Encryption.EncryptQuerystring(currentUser.EmailAddress + ":" + currentUser.Created.Value.Ticks);
                if (!GovNotifyAPI.SendVerifyEmail(currentUser.EmailAddress, verifyCode))
                    throw new Exception("Could not send verification email. Please try again later.");

                currentUser.EmailVerifyCode = verifyCode;
                currentUser.EmailVerifySendDate = DateTime.Now;
                GpgDatabase.Default.SaveChanges();
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
        public ActionResult Step2()
        {
            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated)throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var model = result.Model as ErrorViewModel;
            if (model == null) throw new AuthenticationException();
            if (model.ActionUrl != Url.Action("Step2", "Register"))return result;

            //Send verification if never sent
            if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
            {
                //Send the verification code and show confirmation
                return ResendVerifyCode(currentUser);
            }

            //Allow resend of verification if sent over 24 hours ago
            return View("Step2", new VerifyViewModel() { EmailAddress = currentUser.EmailAddress,Expired = true});
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
        public ActionResult Step2(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View("Step2");
            }

            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            var errorViewModel = result.Model as ErrorViewModel;
            if (result == null || errorViewModel == null) throw new AuthenticationException();
            if (!currentUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue)) return result;
            if (errorViewModel.ActionUrl == Url.Action("Step2", "Register"))
            {
                return Step2();
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
                        Description = "You have failed too many verification attempts. Please logout and try again later.",
                        CallToAction = "Please log out of the system.",
                        ActionUrl = Url.Action("LogOff", "Account")
                    });
                }

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
            if (errorViewModel.ActionUrl.IsAny(Url.Action("Step3", "Register")))
                return result;

            //TODO validate the submitted fields
            if (!ModelState.IsValid) return View(model);

            if (!model.SectorType.EqualsI(SectorTypes.Private, SectorTypes.Public))
            {
                ModelState.AddModelError("SectorType", "You must select your organisation type");
                return View(model);
            }

            //TODO Remove this when public sector is available
            if (!model.SectorType.EqualsI(SectorTypes.Public))throw new NotImplementedException();

            return View("Step4", model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Step4()
        {
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
            model.SearchText = model.SearchText.Trim();
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


            return View("Step5", model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Step5()
        {
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
            //Go back if requested
            if (command != "back") return View("Step4", model);

            //Ensure the user is logged in
            if (!User.Identity.IsAuthenticated) throw new AuthenticationException();

            //Ensure user has completed the registration process
            User currentUser;
            var result = CheckUserRegisteredOk(out currentUser) as ViewResult;
            if (result == null) throw new AuthenticationException();
            var errorModel = result.Model as ErrorViewModel;
            if (errorModel == null) throw new AuthenticationException();
            if (!errorModel.ActionUrl.IsAny(Url.Action("Step3", "Register"))) return result;

            model.SelectedEmployerIndex = 0;

            //TODO validate the submitted fields
            if (!ModelState.IsValid) return View(model);

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
            else if (command.StartsWith("page_"))
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

            if (command.StartsWith("employer_"))
            {
                var employerIndex = command.AfterFirst("employer_").ToInt32();
                if (employerIndex>0 && model.Employers.Count>0 && employerIndex <= model.Employers.Count)
                    model.SelectedEmployerIndex = employerIndex;
            }

            if (model.SelectedEmployerIndex == 0)
            {
                ModelState.AddModelError("", "Invalid employer index");
                return View("Step5", model);
            }
            return View("Step6", model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Step6()
        {
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
            if (!errorModel.ActionUrl.IsAny(Url.Action("Step3", "Register"), Url.Action("Step6", "Register"))) return result;

            var employer = model.Employers[model.SelectedEmployerIndex];
            //Save the new organisation
            var org = Repository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationRef == employer.CompanyNumber);
            if (org == null)
            {
                var now = DateTime.Now;
                org = new Organisation();
                org.SectorType = model.SectorType.Value;
                org.OrganisationName = employer.Name;
                org.OrganisationRef = employer.CompanyNumber;
                org.CurrentStatus = OrganisationStatuses.New;
                org.CurrentStatusDate = now;
                org.Created = now;
                org.Modified = now;
                Repository.Insert(org);
                Repository.SaveChanges();
            }

            //Save the new address
            var address = Repository.GetAll<OrganisationAddress>().FirstOrDefault(a => a.OrganisationId == org.OrganisationId && a.PostCode.EqualsI(employer.PostCode));
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
            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId && uo.UserId == currentUser.UserId);
            if (userOrg == null)
            {
                userOrg = new GpgDB.Models.GpgDatabase.UserOrganisation()
                {
                    UserId = currentUser.UserId,
                    OrganisationId = org.OrganisationId,
                    Created = DateTime.Now
                };
                Repository.Insert(userOrg);
                Repository.SaveChanges();
            }

            //Send a PIN link to the email address
            model.PINSent = userOrg.PINSentDate != null && userOrg.PINSentDate.Value > DateTime.MinValue;
            model.PINExpired = userOrg.PINSentDate != null && userOrg.PINSentDate.Value.AddDays(Properties.Settings.Default.PinInPostExpiryDays) < DateTime.Now;

            if (!model.PINSent || model.PINExpired)
            try
            {
                var pin = Numeric.Rand(0,999999);
                if (!GovNotifyAPI.SendPinInPost(currentUser.Fullname + " ("+currentUser.JobTitle + ")",currentUser.EmailAddress, pin.ToString()))
                    throw new Exception("Could not send PIN in the POST. Please try again later.");

                //Send a confirmation link to the email address
                var confirmCode = Encryption.EncryptQuerystring(string.Format("{0}:{1}:{2}", userOrg.UserId, userOrg.OrganisationId, DateTime.Now.Ticks));
                if (!GovNotifyAPI.SendConfirmEmail(currentUser.EmailAddress, confirmCode))
                    throw new Exception("Could not send confirmation email. Please try again later.");

                userOrg.PINCode = pin;
                userOrg.PINSentDate = DateTime.Now;
                userOrg.ConfirmCode = confirmCode;
                Repository.SaveChanges();
                model.PINSent = true;
                model.PINExpired = false;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
            ViewBag.Address = employer.FullAddress.Replace(", ",",<br>");
            return View("Step6",model);
        }

        [HttpGet]
        public ActionResult Confirm(string code = null, long pin=0)
        {
            UserOrganisation userOrg=null;
            var currentUser = Repository.FindUser(User);
            if (string.IsNullOrWhiteSpace(code))
            {
                if (currentUser == null)
                {
                    ModelState.AddModelError("", "Invalid user");
                    return View(code);
                }
                userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(uo => uo.UserId == currentUser.UserId && uo.PINSentDate > DateTime.MinValue && uo.PINConfirmedDate == null);
                if (userOrg!=null)code = userOrg.ConfirmCode;
            }
            if (string.IsNullOrWhiteSpace(code)) code = Request.Url.Query.TrimStartI(" ?");

            //Load the user from the verification code
            if (userOrg == null) userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(u => u.ConfirmCode == code);

            var model = new ConfirmViewModel();
            model.Default = pin>0 ? pin.ToString() : null;
            model.ConfirmCode = code;

            //Show an error if the code doesnt exist in db
            if (userOrg == null)
            {
                ModelState.AddModelError("", "Invalid confirmation link");
                return View(model);
            }

            var user = GpgDatabase.Default.User.Find(userOrg.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid confirmation link");
                return View(model);
            }

            if (currentUser!=null && currentUser.UserId != user.UserId)
            {
                ModelState.AddModelError("", "Invalid confirmation link");
                return View(model);
            }

            if (userOrg.PINSentDate < DateTime.Now.AddDays(-6))
            {
                //TODO Resend verification code and prompt user
                ModelState.AddModelError("", "This confirmation link has expired. A new link has been sent to " + currentUser.EmailAddress);
                return View(model);
            }

            try
            {
                code = Encryption.DecryptQuerystring(code);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid confirmation code");
                return View(model);
            }

            //Show an error if the code doesnt match the userId
            if (!string.Format("{0}:{1}", userOrg.UserId, userOrg.OrganisationId).EqualsI(code,Server.UrlDecode(code)))
            {
                ModelState.AddModelError("", "Invalid confirmation code");
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Confirm(ConfirmViewModel model)
        {
            var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(u => u.ConfirmCode == model.ConfirmCode);

            if (model.PIN != userOrg.PINCode)
            {
                ModelState.AddModelError("PIN", "Invalid PIN code");
                return View(model);
            }
            userOrg.PINConfirmedDate = DateTime.Now;
            model.confirmed = true;
            //Save the current user
            GpgDatabase.Default.SaveChanges();
            return View(model);
        }
    }
}