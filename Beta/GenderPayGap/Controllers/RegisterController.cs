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
using Thinktecture.IdentityModel.Mvc;
using System.Configuration;
using System.Collections.Generic;
using GenderPayGap.WebUI.Properties;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Register")]
    [Route("{action}")]
    public class RegisterController : BaseController
    {
        #region Initialisation
        public RegisterController():base(){}
        public RegisterController(IContainer container): base(container){}

        /// <summary>
        /// This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [Route("Init")]
        public ActionResult Init()
        {
#if DEBUG
            MvcApplication.Log.WriteLine("Register Controller Initialised");
#endif
            return new EmptyResult();
        }
        #endregion

        [Route]
        public ActionResult Redirect()
        {
            return RedirectToAction("Step1");
        }

        [HttpGet]
        [Route("Step1")]
        public ActionResult Step1()
        {
            User currentUser;
            //Ensure user has not completed the registration process
            var result = CheckUserRegisteredOk(out currentUser);
            if (result != null) return result;

            //Clear the stash
            ClearStash();

            //Start new user registration
            return View("Step1", new Models.RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SpamProtection()]
        [Route("Step1")]
        public ActionResult Step1(Models.RegisterViewModel model)
        {
            if (model.Password.ContainsI("password")) ModelState.AddModelError("Password", "Password cannot contain the word 'password'");
            //TODO validate the submitted fields
            if (!ModelState.IsValid) return View("Step1", model);

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();

            //Check this email address isnt already assigned to another user
            var currentUser = DataRepository.FindUserByEmail(model.EmailAddress);
            if (currentUser != null)
            {
                if (currentUser.EmailVerifySendDate != null)
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
                }

                //Delete the previous user org if there is one
                var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);
                if (userOrg!=null)DataRepository.Delete(userOrg);

                //If from a previous user then delete the previous user
                DataRepository.Delete(currentUser);
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
            currentUser.EmailVerifyHash = null;
            //Save the user to ensure UserId>0 for new status
            DataRepository.Insert(currentUser);
            DataRepository.SaveChanges();
            currentUser.SetStatus(UserStatuses.New, currentUser.UserId);

            //Send the verification code and showconfirmation
            StashModel(model);
            return RedirectToAction("Step2");
        }

        //Send the verification code and show confirmation
        bool ResendVerifyCode(User currentUser)
        {
            //Send a verification link to the email address
            try
            {
                var verifyCode = Encryption.EncryptQuerystring(currentUser.UserId + ":" + currentUser.Created.ToSmallDateTime());
                if (!this.SendVerifyEmail(currentUser.EmailAddress, verifyCode))
                    throw new Exception("Could not send verification email. Please try again later.");

                currentUser.EmailVerifyHash = verifyCode.GetSHA512Checksum();
                currentUser.EmailVerifySendDate = DateTime.Now;
                DataRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.Message.EqualsI("This Email Address is not registered with Gov Notify."))
                    ModelState.AddModelError("EmailAddress", ex.Message);
                else
                    ModelState.AddModelError(string.Empty, ex.Message);
            }
            if (!ModelState.IsValid) return false;

            //Prompt user to open email and verification link
            return true;
        }

        [HttpGet]
        [Route("Step2")]
        public ActionResult Step2(string code=null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we are coming from step1 or the user is logged in
            var m = UnstashModel<RegisterViewModel>();
            if (m == null && currentUser == null) return new HttpUnauthorizedResult();

            if (currentUser == null)currentUser = DataRepository.FindUserByEmail(m.EmailAddress);
            var model = new VerifyViewModel() { EmailAddress = currentUser.EmailAddress };
            ClearStash();

            //If email not sent
            if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
            {
                if (!ResendVerifyCode(currentUser))
                    model.Retry = true;
                else
                    model.Sent = true;

                //Tell them to verify email
                return View("Step2", model);
            }

            //If verification code has expired
            if (currentUser.EmailVerifySendDate.Value.AddHours(Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
            {
                model.Expired = true;

                //prompt user to click to request a new one
                return View("Step2", model);
            }

            var remainingLock = currentUser.VerifyAttemptDate == null ? TimeSpan.Zero : currentUser.VerifyAttemptDate.Value.AddMinutes(Properties.Settings.Default.LockoutMinutes) - DateTime.Now;
            var remainingResend = currentUser.EmailVerifySendDate.Value.AddHours(Settings.Default.EmailVerificationMinResendHours) - DateTime.Now;

            if (string.IsNullOrEmpty(code))
            {
                if (remainingResend>TimeSpan.Zero)
                    //Prompt to check email or wait
                    return View("CustomError", new ErrorViewModel(1102, new { remainingTime = remainingLock.ToFriendly(maxParts: 2) }));
                else
                {
                    ModelState.AddModelError("", CustomErrorMessages.GetTitle(1103));

                    //Prompt to click resend
                    model.Resend = true;
                    return View("Step2", model);
                }
            }

            //If too many wrong attempts
            if (currentUser.VerifyAttempts >= Properties.Settings.Default.MaxEmailVerifyAttempts && remainingLock > TimeSpan.Zero)
                return View("CustomError", new ErrorViewModel(1110, new { remainingTime = remainingLock.ToFriendly(maxParts: 2) }));

            ActionResult result;

            if (currentUser.EmailVerifyHash != code.GetSHA512Checksum())
            {
                currentUser.VerifyAttempts++;

                //If code min time has elapsed 
                if (remainingResend <= TimeSpan.Zero)
                {
                    model.Resend = true;
                    model.WrongCode = true;
                    ModelState.AddModelError("", CustomErrorMessages.GetTitle(1111));
                    //Prompt user to request a new verification code
                    result = View("Step2", model);
                }
                else if (currentUser.VerifyAttempts >= Properties.Settings.Default.MaxEmailVerifyAttempts && remainingLock > TimeSpan.Zero)
                    return View("CustomError", new ErrorViewModel(1110, new { remainingTime = remainingLock.ToFriendly(maxParts: 2) }));
                else
                    result = View("CustomError", new ErrorViewModel(1111));
            }
            else 
            {
                //Set the user as verified
                currentUser.EmailVerifiedDate = DateTime.Now;

                //Mark the user as active
                currentUser.SetStatus(UserStatuses.Active, currentUser.UserId, "Email verified");

                currentUser.VerifyAttempts = 0;

                result = View("Step2", new VerifyViewModel() { EmailAddress = currentUser.EmailAddress, Verified = true });
            }
            currentUser.VerifyAttemptDate = DateTime.Now;

            //Save the current user
            DataRepository.SaveChanges();

            //Prompt the user with confirmation
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("Step2")]
        public ActionResult Step2(VerifyViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Reset the verification send date
            currentUser.EmailVerifySendDate = null;
            currentUser.EmailVerifyHash = null;
            DataRepository.SaveChanges();

            //Call GET action which will automatically resend
            return Step2();
        }

        [HttpGet]
        [Auth]
        [Route("Step3")]
        public ActionResult Step3()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = new OrganisationViewModel();
            StashModel(model);
            return View("Step3", model);
        }

        /// <summary>
        /// Get the sector type
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("Step3")]
        public ActionResult Step3(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null)return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            //TODO validate the submitted fields
            ModelState.Clear();

            if (!model.SectorType.EqualsI(SectorTypes.Private, SectorTypes.Public))
            {
                ModelState.AddModelError("SectorType", "You must select your organisation type");
                return View("Step3", model);
            }

            //TODO Remove this when public sector is available
            //if (!model.SectorType.EqualsI(SectorTypes.Private))throw new NotImplementedException();

            StashModel(model);
            return RedirectToAction("Step4");
        }

        /// Search employer
        [HttpGet]
        [Auth]
        [Route("Step4")]
        public ActionResult Step4()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            return View("Step4", model);
        }

        /// <summary>
        /// Search employer submit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("Step4")]
        public ActionResult Step4(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            model.SelectedEmployerIndex = -1;

            model.SearchText = model.SearchText.Trim();
            if (string.IsNullOrWhiteSpace(model.SearchText))
            {
                ModelState.AddModelError("SearchText", "You must enter an employer name or company number");
                return View("Step4", model);
            }
            if (model.SearchText.Length < 3 || model.SearchText.Length > 100)
            {
                ModelState.AddModelError("SearchText", "You must enter between 3 and 100 characters");
                return View("Step4", model);
            }

            switch (model.SectorType)
            {
                case SectorTypes.Private:

                    model.Employers = PrivateSectorRepository.Default.Search(model.SearchText,1, Settings.Default.EmployerPageSize);

                    break;

                case SectorTypes.Public:

                    model.Employers = PublicSectorRepository.Default.Search(model.SearchText, 1, Settings.Default.EmployerPageSize);

                    break;

                default:
                    throw new NotImplementedException();
            }

            ModelState.Clear();
            StashModel(model);

            //Search again if no results
            if (model.Employers.Results.Count<1)return View("Step4", model);

            //Go to step 5 with results
            return RedirectToAction("Step5");           
        }

        /// <summary>
        /// Choose employer view results
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Auth]
        [Route("Step5")]
        public ActionResult Step5()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            return View("Step5", model);
        }


        /// <summary>
        /// Choose employer with paging or search
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("Step5")]
        public ActionResult Step5(OrganisationViewModel model, string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            var nextPage = m.Employers.CurrentPage;

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
                    return View("Step5", model);
                }
                nextPage = 1;
                doSearch = true;
            }
            else if (command == "pageNext")
            {
                if (nextPage >= model.Employers.PageCount)
                {
                    ModelState.AddModelError("", "No more pages");
                    return View("Step5", model);
                }
                nextPage++;
                doSearch = true;
            }
            else if (command == "pagePrev")
            {
                if (nextPage <= 1)
                {
                    ModelState.AddModelError("", "No previous page");
                    return View("Step5", model);
                }
                nextPage--;
                doSearch = true;
            }
            else if (command.StartsWithI("page_"))
            {
                var page = command.AfterFirst("page_").ToInt32();
                if (page < 1 || page > model.Employers.PageCount)
                {
                    ModelState.AddModelError("", "Invalid page number");
                    return View("Step5", model);
                }
                if (page != nextPage)
                {
                    nextPage = page;
                    doSearch = true;
                }
            }

            if (doSearch)
            {
                switch (model.SectorType)
                {
                    case SectorTypes.Private:
                        model.Employers = PrivateSectorRepository.Default.Search(model.SearchText, nextPage, Settings.Default.EmployerPageSize);
                        break;

                    case SectorTypes.Public:
                        model.Employers = PublicSectorRepository.Default.Search(model.SearchText, nextPage, Settings.Default.EmployerPageSize);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                ModelState.Clear();
                StashModel(model);

                //Go back if no results
                if (model.Employers.Results.Count<1)return RedirectToAction("Step4");

                //Otherwise show results
                return View("Step5", model);
            }

            if (command.StartsWithI("employer_"))
            {
                var employerIndex = command.AfterFirst("employer_").ToInt32();
                var employer = model.Employers.Results[employerIndex];

                Organisation org = null;
                if (model.SectorType == SectorTypes.Private)
                    org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.PrivateSectorReference == employer.CompanyNumber);
                else
                    org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.SectorType == SectorTypes.Public && o.OrganisationName.ToLower() == employer.Name.ToLower());

                if (org != null)
                {
                    var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId);
                    if (userOrg != null && userOrg.UserId != currentUser.UserId)
                    {
                        var user = DataRepository.GetAll<User>().FirstOrDefault(u => u.UserId == userOrg.UserId);

                        if (userOrg.PINSentDate != null || org.SectorType == SectorTypes.Public && org.Status == OrganisationStatuses.Active)
                        {
                            ModelState.AddModelError("", $"Another user ({user.Fullname}) has already registered for this organisation.");
                            return View("Step4", model);
                        }
                        else if (org.SectorType == SectorTypes.Private)
                        {
                            var remainingTime = userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostExpiryDays) - DateTime.Now;
                            if (remainingTime > TimeSpan.Zero)
                            {
                                ModelState.AddModelError("", "Another user (" + user.Fullname + ") is trying to register this organisation. Please try again later in " + remainingTime.ToFriendly(maxParts: 2) + ".");
                                return View("Step4", model);
                            }
                        }
                    }
                }

                //Check the user email is authorised for public organisation
                if (model.SectorType == SectorTypes.Public && !employer.IsAuthorised(currentUser.EmailAddress))
                {
                    ModelState.AddModelError("", "Sorry you are not authorised to register for the organisation you selected.");
                    return View("Step5", model);
                }

                model.SelectedEmployerIndex = employerIndex;
            }

            ModelState.Clear();
            StashModel(model);

            //If we havend selected one the reshow same view
            if (model.SelectedEmployerIndex < 0)return View("Step5", model);

            //If we have selected an employer then go to step6
            return RedirectToAction("Step6");
        }

        /// <summary>
        /// Private confirm
        /// Public enter address
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Auth]
        [Route("Step6")]
        public ActionResult Step6()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            if (model.SectorType == SectorTypes.Public) return View("AddAddress",model);
            return View("ConfirmEmployer", model);
        }

        /// <summary>
        /// Private confirm submit
        /// Public address submit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("Step6")]
        public ActionResult Step6(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Load the employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            if (model.SectorType == SectorTypes.Public)
            {
                if (string.IsNullOrWhiteSpace(model.Address1)) ModelState.AddModelError("Address1","You must supply an address");
                if (string.IsNullOrWhiteSpace(model.Address3)) ModelState.AddModelError("Address3", "You must supply a 'Town or City'");
                if (string.IsNullOrWhiteSpace(model.PostCode)) ModelState.AddModelError("PostCode", "You must supply a 'Postcode'");

                //Renter address if invalid
                if (!ModelState.IsValid) return View("AddAddress", model);

                model.SelectedEmployer.Address1 = model.Address1;
                model.SelectedEmployer.Address2 = model.Address2;
                model.SelectedEmployer.Address3 = model.Address3;
                model.SelectedEmployer.Country = model.Country;
                model.SelectedEmployer.PostCode = model.PostCode;
                model.SelectedEmployer.PoBox = model.PoBox;

                //Go to confirm step
                StashModel(model);
                return RedirectToAction("Step7");
            }

            //Save the private sector employer registration
            SaveRegistration(currentUser, model);

            //Redirect to send pin
            StashModel(model);
            return RedirectToAction("SendPIN");
        }


        /// <summary>
        /// Save the current users registration
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="model"></param>
        void SaveRegistration(User currentUser,OrganisationViewModel model)
        {            
            var employer = model.SelectedEmployer;

            //Save the new organisation
            Organisation org = null;
            if (model.SectorType == SectorTypes.Private)
                org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.PrivateSectorReference == employer.CompanyNumber);
            else
                org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.SectorType == SectorTypes.Public && o.OrganisationName.ToLower() == employer.Name.ToLower());

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
                DataRepository.Insert(org);
                DataRepository.SaveChanges();
            }

            //Save the new address
            var address = DataRepository.GetAll<OrganisationAddress>().FirstOrDefault(a => a.OrganisationId == org.OrganisationId);
            if (address == null)
            {
                address = new OrganisationAddress();
                address.OrganisationId = org.OrganisationId;
                DataRepository.Insert(address);
            }
            address.Address1 = employer.Address1;
            address.Address2 = employer.Address2;
            address.Address3 = employer.Address3;
            address.Country = employer.Country;
            address.PostCode = employer.PostCode;
            DataRepository.SaveChanges();

            //TODO Send the PIN and confirm when sent
            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId);

            //If from a previous user then delete the previous user
            if (userOrg != null && userOrg.UserId != currentUser.UserId)
            {
                DataRepository.Delete(userOrg);
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
                DataRepository.Insert(userOrg);
            }
            userOrg.PINHash = null;
            userOrg.PINSentDate = null;
            DataRepository.SaveChanges();

            //Set the status to active
            if (model.SectorType == SectorTypes.Public)
            {
                //Set the user org as confirmed
                userOrg.PINConfirmedDate = DateTime.Now;

                //Mark the organisation as active
                userOrg.Organisation.SetStatus(OrganisationStatuses.Active, currentUser.UserId, "PIN Confirmed");

                //Mark the address as confirmed
                userOrg.Organisation.Address.SetStatus(AddressStatuses.Confirmed, currentUser.UserId, "PIN Confirmed");

                userOrg.ConfirmAttempts = 0;

                DataRepository.SaveChanges();
            }
        }

        [HttpGet]
        [Auth]
        [Route("Step7")]
        public ActionResult Step7()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            return View("ConfirmEmployer", model);
        }

        /// <summary>
        /// Public confirm submit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("Step7")]
        public ActionResult Step7(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Load the employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            //Save the public sector employer registration
            SaveRegistration(currentUser, model);

            //Redirect to complete form
            StashModel(model);
            return RedirectToAction("Complete");
        }

        ActionResult GetSendPIN(string command=null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Get the user organisation
            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            //Get the organisation
            var org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == userOrg.OrganisationId);

            //If a pin has never been sent or resend button submitted then send one immediately
            if (string.IsNullOrWhiteSpace(userOrg.PINHash) || userOrg.PINSentDate.EqualsI(null, DateTime.MinValue) || command.EqualsI("Resend"))
            {
                try
                {
                    //Marke the user org as ready to send a pin
                    userOrg.PINHash = null;
                    userOrg.PINSentDate = null;
                    DataRepository.SaveChanges();

                    //Generate a new pin
                    var pin = ConfigurationManager.AppSettings["TestPIN"];
                    if (string.IsNullOrWhiteSpace(pin))pin = Crypto.GeneratePasscode(Properties.Settings.Default.PINChars.ToCharArray(),Properties.Settings.Default.PINLength);

                    //Try and send the PIN in post
                    var emailPIN = ConfigurationManager.AppSettings["EmailPIN"].ToBoolean(true);
                    if (emailPIN && !this.SendPinInPost(currentUser, org, pin.ToString()))
                        throw new Exception("Could not send PIN in the POST. Please try again later.");

                    //Try and send the confirmation email
                    if (!this.SendConfirmEmail(currentUser.EmailAddress))
                        throw new Exception("Could not send confirmation email. Please try again later.");

                    //Save the PIN and confirm code
                    userOrg.PINHash = pin.GetSHA512Checksum();
                    userOrg.PINSentDate = DateTime.Now;
                    DataRepository.SaveChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View("SendPIN");
                }
            }

            //Prepare view parameters
            ViewBag.Resend = !string.IsNullOrWhiteSpace(userOrg.PINHash) && !userOrg.PINSentDate.EqualsI(null, DateTime.MinValue)
                && userOrg.PINSentDate.Value.AddDays(Properties.Settings.Default.PinInPostMinRepostDays) < DateTime.Now;
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
            ViewBag.Organisation = org.OrganisationName;
            ViewBag.Address = org.Address.GetAddress(",<br/>");
            return View("SendPIN");
        }

        [Auth]
        [HttpGet]
        [Route("SendPIN")]
        public ActionResult SendPIN()
        {
            return GetSendPIN();
        }

        [Auth]
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("SendPIN")]
        public ActionResult SendPIN(string command)
        {
            return GetSendPIN(command);
        }

        [HttpGet]
        [Auth]
        [Route("ConfirmPIN")]
        public ActionResult ConfirmPIN()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Get the user organisation
            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            if (userOrg == null) throw new AuthenticationException();

            ActionResult result1;

            var remaining = userOrg.ConfirmAttemptDate == null ? TimeSpan.Zero : userOrg.ConfirmAttemptDate.Value.AddMinutes(Properties.Settings.Default.LockoutMinutes) - DateTime.Now;
            if (userOrg.ConfirmAttempts >= Properties.Settings.Default.MaxPinAttempts && remaining > TimeSpan.Zero)
                return View("CustomError", new ErrorViewModel(1113, new { remainingTime = remaining.ToFriendly(maxParts: 2) }));

            remaining = userOrg.PINSentDate == null ? TimeSpan.Zero : userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostMinRepostDays) - DateTime.Now;
            var model = new CompleteViewModel();
            model.PIN = null;
            model.AllowResend = remaining <= TimeSpan.Zero;
            model.Remaining = remaining.ToFriendly(maxParts: 2);
            //Show the PIN textbox and button
            return View("ConfirmPIN", model);
        }

        [HttpPost]
        [Auth]
        [ValidateAntiForgeryToken]
        [Route("ConfirmPIN")]
        public ActionResult ConfirmPIN(CompleteViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Ensure they have entered a PIN
            if (!ModelState.IsValid) return View("ConfirmPIN", model);

            //Get the user organisation
            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            ActionResult result1;

            var remaining = userOrg.ConfirmAttemptDate == null ? TimeSpan.Zero : userOrg.ConfirmAttemptDate.Value.AddMinutes(Properties.Settings.Default.LockoutMinutes) - DateTime.Now;
            if (userOrg.ConfirmAttempts >= Properties.Settings.Default.MaxPinAttempts && remaining > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel(1113, new { remainingTime = remaining.ToFriendly(maxParts: 2) }));
            }

            if (userOrg.PINHash == model.PIN.ToUpper().GetSHA512Checksum())
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
            DataRepository.SaveChanges();

            //Prompt the user with confirmation
            return result1;
        }

        [HttpGet]
        [Auth]
        [Route("Complete")]
        public ActionResult Complete()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Ensure the stash is cleared
            ClearStash();

            //Show the confirmation view
            return View("Complete");
        }
    }
}