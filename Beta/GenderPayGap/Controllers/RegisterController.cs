using System;
using System.Linq;
using System.Security.Authentication;
using System.Web.Mvc;
using Extensions;
using GenderPayGap.Models.SqlDatabase;
using Autofac;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Classes;
using System.Configuration;
using System.Net;
using System.Web;
using GenderPayGap.Core.Classes;
using GenderPayGap.WebUI.Properties;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Models.Register;

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

        #region IoC Properties

        IPagedRepository<EmployerRecord> _PrivateSectorRepository = null;
        public IPagedRepository<EmployerRecord> PrivateSectorRepository
        {
            get
            {

                if (_PrivateSectorRepository == null)
                {
                    _PrivateSectorRepository = containerIOC.ResolveKeyed<IPagedRepository<EmployerRecord>>("Private");
                }
                return _PrivateSectorRepository;
            }
        }

        IPagedRepository<EmployerRecord> _PublicSectorRepository = null;
        public IPagedRepository<EmployerRecord> PublicSectorRepository
        {
            get
            {

                if (_PublicSectorRepository == null)
                {
                    _PublicSectorRepository = containerIOC.ResolveKeyed<IPagedRepository<EmployerRecord>>("Public");
                }
                return _PublicSectorRepository;
            }
        }
        #endregion

        #region about-you
        [Route]
        public ActionResult Redirect()
        {
            return RedirectToAction("AboutYou");
        }

        [HttpGet]
        [Route("about-you")]
        public ActionResult AboutYou()
        {
            User currentUser;
            //Ensure user has not completed the registration process
            var result = CheckUserRegisteredOk(out currentUser);
            if (result != null) return result;

            //Clear the stash
           this.ClearStash();

            //Start new user registration
            return View("AboutYou", new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SpamProtection()]
        [Route("about-you")]
        public ActionResult AboutYou(RegisterViewModel model)
        {
            //Validate the submitted fields
            if (model.Password.ContainsI("password")) AddModelError(3000,"Password");
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<RegisterViewModel>();
                return View("AboutYou", model);
            }

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
                        //A registered user with this email already exists.
                        AddModelError(3001, "EmailAddress");
                        this.CleanModelErrors<RegisterViewModel>();
                        return View("AboutYou", model);
                    }
                    var remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationExpiryHours) - DateTime.Now;
                    if (remainingTime > TimeSpan.Zero)
                    {
                        AddModelError(3002,"EmailAddress",new { remainingTime= remainingTime.ToFriendly(maxParts: 2) });
                        this.CleanModelErrors<RegisterViewModel>();
                        return View("AboutYou", model);
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
            this.StashModel(model);
            return RedirectToAction("VerifyEmail");
        }
        #endregion

        #region PersonResponsible
        //Send the verification code and show confirmation
        public bool ResendVerifyCode(User currentUser)
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
                //Log the exception
                MvcApplication.Log.WriteLine(ex.Message);
                return false;
            }

            //Prompt user to open email and verification link
            return true;
        }

        [HttpGet]
        [Route("verify-email")]
        public ActionResult VerifyEmail(string code=null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we are coming from EnterCalculations or the user is logged in
            var m = this.UnstashModel<RegisterViewModel>();
            if (m == null && currentUser == null) return new HttpUnauthorizedResult();

            if (currentUser == null)currentUser = DataRepository.FindUserByEmail(m.EmailAddress);
            var model = new VerifyViewModel() { EmailAddress = currentUser.EmailAddress };
            this.ClearStash();

            //If email not sent
            if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
            {
                if (!ResendVerifyCode(currentUser))
                    model.Retry = true;
                else
                    model.Sent = true;

                //Tell them to verify email
                return View("VerifyEmail", model);
            }

            //If verification code has expired
            if (currentUser.EmailVerifySendDate.Value.AddHours(Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
            {
                AddModelError(1103);

                model.Resend = true;

                //prompt user to click to request a new one
                return View("VerifyEmail", model);
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
                    //Expired with resend
                    AddModelError(3016);
                    this.CleanModelErrors<VerifyViewModel>();

                    //Prompt to click resend
                    model.Resend = true;
                    return View("VerifyEmail", model);
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
                    AddModelError(3004);

                    //Prompt user to request a new verification code
                    this.CleanModelErrors<VerifyViewModel>();
                    result = View("VerifyEmail", model);
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

                //If not an administrator show confirmation action to choose next step
                result = RedirectToAction("EmailConfirmed");
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
        [Route("verify-email")]
        public ActionResult VerifyEmail(VerifyViewModel model)
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
            return VerifyEmail();
        }
        #endregion

        #region EmailConfirmed

        [HttpGet]
        [Auth]
        [Route("email-confirmed")]
        public ActionResult EmailConfirmed()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            if (currentUser.IsAdministrator())
                //If its an administrator show confirmation and logout message
                return View("CustomError", new ErrorViewModel(1116));

            return View("EmailConfirmed");
        }
        #endregion

        #region OrganisationType
        [HttpGet]
        [Auth]
        [Route("organisation-type")]
        public ActionResult OrganisationType()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = new OrganisationViewModel();
            this.StashModel(model);
            return View("OrganisationType", model);
        }

        /// <summary>
        /// Get the sector type
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("organisation-type")]
        public ActionResult OrganisationType(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null)return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            //TODO validate the submitted fields
            ModelState.Clear();

            if (!model.SectorType.EqualsI(SectorTypes.Private, SectorTypes.Public))
            {
                AddModelError(3005,"SectorType");
                this.CleanModelErrors<OrganisationViewModel>();
                return View("OrganisationType", model);
            }

            //TODO Remove this when public sector is available
            //if (!model.SectorType.EqualsI(SectorTypes.Private))throw new NotImplementedException();

            this.StashModel(model);
            return RedirectToAction("OrganisationSearch");
        }
        #endregion

        #region OrganisationSearch
        /// Search employer
        [HttpGet]
        [Auth]
        [Route("organisation-search")]
        public ActionResult OrganisationSearch()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            model.ManualRegistration = true;
            model.BackAction = "OrganisationSearch";
            model.Name = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.PostCode = null;
            model.PoBox = null;
            
            this.StashModel(model);

            return View("OrganisationSearch", model);
        }

        /// <summary>
        /// Search employer submit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("organisation-search")]
        public ActionResult OrganisationSearch(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            ModelState.Include("SearchText");

            model.ManualRegistration = true;
            model.BackAction = "OrganisationSearch";
            model.SelectedEmployerIndex = -1;
            model.Name = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.PostCode = null;
            model.PoBox = null;

            model.SearchText = model.SearchText.TrimI();

            switch (model.SectorType)
            {
                case SectorTypes.Private:

                    model.Employers = PrivateSectorRepository.Search(model.SearchText,1, Settings.Default.EmployerPageSize);

                    break;

                case SectorTypes.Public:

                    model.Employers = PublicSectorRepository.Search(model.SearchText, 1, Settings.Default.EmployerPageSize);

                    break;

                default:
                    throw new NotImplementedException();
            }

            ModelState.Clear();
            this.StashModel(model);

            //Search again if no results
            if (model.Employers.Results.Count<1)return View("OrganisationSearch", model);

            //Go to step 5 with results
            return RedirectToAction("ChooseOrganisation");           
        }
        #endregion

        #region ChooseOrganisation
        /// <summary>
        /// Choose employer view results
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Auth]
        [Route("choose-organisation")]
        public ActionResult ChooseOrganisation()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            model.ManualRegistration = true;
            model.BackAction = "ChooseOrganisation";
            model.Name = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.PostCode = null;
            model.PoBox = null;

            this.StashModel(model);

            return View("ChooseOrganisation", model);
        }


        /// <summary>
        /// Choose employer with paging or search
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("choose-organisation")]
        public ActionResult ChooseOrganisation(OrganisationViewModel model, string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            var nextPage = m.Employers.CurrentPage;

            ModelState.Include("SearchText");

            model.BackAction = "ChooseOrganisation";
            model.SelectedEmployerIndex = -1;
            model.Name = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.PostCode = null;
            model.PoBox = null;

            bool doSearch = false;
            if (command == "search")
            {
                model.SearchText = model.SearchText.Trim();

                if (string.IsNullOrWhiteSpace(model.SearchText))
                {
                    AddModelError(3006, "SearchText", new { orCompanyNumber = model.SectorType == SectorTypes.Private ? " or company number" : "" });
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View("ChooseOrganisation", model);
                }
                if (model.SearchText.Length < 3 || model.SearchText.Length > 100)
                {
                    AddModelError(3007, "SearchText");
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View("ChooseOrganisation", model);
                }

                nextPage = 1;
                doSearch = true;
            }
            else if (command == "pageNext")
            {
                if (nextPage >= model.Employers.PageCount)
                    throw new Exception("Cannot go past last page");
                nextPage++;
                doSearch = true;
            }
            else if (command == "pagePrev")
            {
                if (nextPage <= 1)
                    throw new Exception("Cannot go before previous page");
                nextPage--;
                doSearch = true;
            }
            else if (command.StartsWithI("page_"))
            {
                var page = command.AfterFirst("page_").ToInt32();
                if (page < 1 || page > model.Employers.PageCount)
                    throw new Exception("Invalid page selected");

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
                        model.Employers = PrivateSectorRepository.Search(model.SearchText, nextPage, Settings.Default.EmployerPageSize);
                        break;

                    case SectorTypes.Public:
                        model.Employers = PublicSectorRepository.Search(model.SearchText, nextPage, Settings.Default.EmployerPageSize);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                ModelState.Clear();
                this.StashModel(model);

                //Go back if no results
                if (model.Employers.Results.Count<1)return RedirectToAction("OrganisationSearch");

                //Otherwise show results
                return View("ChooseOrganisation", model);
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
                        //Another user is already registered for this organisation
                        if ((userOrg.PINSentDate != null || org.SectorType == SectorTypes.Public) && org.Status == OrganisationStatuses.Active)
                        {
                            AddModelError(3008);
                            this.CleanModelErrors<OrganisationViewModel>();
                            return View("OrganisationSearch", model);
                        }

                        //Another user is still trying to register for this this organisation
                        if (org.SectorType == SectorTypes.Private)
                        {
                            var remainingTime = userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostExpiryDays) - DateTime.Now;
                            if (remainingTime > TimeSpan.Zero)
                            {
                                AddModelError(3009);
                                this.CleanModelErrors<OrganisationViewModel>();
                                return View("OrganisationSearch", model);
                            }
                        }
                    }
                }

                model.SelectedEmployerIndex = employerIndex;

                model.Name = model.SelectedEmployer.Name;

                //Check the user email is authorised for public organisation
                if (model.SectorType == SectorTypes.Public)
                {
                    model.ManualRegistration = employer!=null && (string.IsNullOrWhiteSpace(employer.EmailPatterns) || !employer.IsAuthorised(currentUser.EmailAddress));
                    this.StashModel(model);
                    return RedirectToAction("AddOrganisation");
                }
                model.ManualRegistration = false;

                model.CompanyNumber = model.SelectedEmployer.CompanyNumber;
                model.Address1 = model.SelectedEmployer.Address1;
                model.Address2 = model.SelectedEmployer.Address2;
                model.Address3 = model.SelectedEmployer.Address3;
                model.Country = model.SelectedEmployer.Country;
                model.PostCode = model.SelectedEmployer.PostCode;
                model.PoBox = model.SelectedEmployer.PoBox;
            }

            ModelState.Clear();

            //If we havend selected one the reshow same view
            if (model.SelectedEmployerIndex < 0)return View("ChooseOrganisation", model);

            this.StashModel(model);
            //If private sector add organisation address
            return RedirectToAction("ConfirmOrganisation");
        }
        #endregion

        #region AddOrganisation

        [HttpGet]
        [Auth]
        [Route("add-organisation")]
        public ActionResult AddOrganisation()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Get the model from the stash
            var model =this.UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            //Prepopulate name if it empty
            if (string.IsNullOrWhiteSpace(model.Name) && !string.IsNullOrWhiteSpace(model.SearchText))model.Name = model.SearchText;

            return View("AddOrganisation",model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("add-organisation")]
        public ActionResult AddOrganisation(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;


            //Exclude the cotact details
            ModelState.Exclude("ContactFirstName", "ContactLastName", "ContactJobTitle", "ContactOrganisation", "ContactEmailAddress", "ContactPhoneNumber");

            //Check model is valid
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<OrganisationViewModel>();
                return View("AddOrganisation", model);
            }

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            this.StashModel(model);
            if (model.ManualRegistration)
            {
                //Check the organisaton doesnt already exist
                var org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName.ToLower() == model.Name.ToLower());
                if (org != null && org.Status == OrganisationStatuses.Active)
                {
                    AddModelError(3008);
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View("AddOrganisation", model);
                }
                return RedirectToAction("AddContact");
            }
            return RedirectToAction("ConfirmOrganisation");
        }

        #endregion

        #region AddContact

        [HttpGet]
        [Auth]
        [Route("add-contact")]
        public ActionResult AddContact()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Get the model from the stash
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            //Preload contact details
            model.ContactFirstName = string.IsNullOrWhiteSpace(currentUser.ContactFirstName)
                ? currentUser.Firstname
                : currentUser.ContactFirstName;

            model.ContactLastName = string.IsNullOrWhiteSpace(currentUser.ContactFirstName)
                ? currentUser.Lastname
                : currentUser.ContactFirstName;

            model.ContactJobTitle = string.IsNullOrWhiteSpace(currentUser.ContactJobTitle)
                ? currentUser.JobTitle
                : currentUser.ContactJobTitle;

            model.ContactOrganisation = string.IsNullOrWhiteSpace(currentUser.ContactOrganisation)
                ? model.Name
                : currentUser.ContactOrganisation;

            model.ContactEmailAddress = string.IsNullOrWhiteSpace(currentUser.ContactEmailAddress)
                ? currentUser.EmailAddress
                : currentUser.ContactEmailAddress;

            model.ContactPhoneNumber = currentUser.ContactPhoneNumber;

            return View("AddContact",model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("add-contact")]
        public ActionResult AddContact(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Check model is valid
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<OrganisationViewModel>();
                return View("AddContact", model);
            }

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            this.StashModel(model);
            return RedirectToAction("ConfirmOrganisation");
        }
        #endregion

        #region Confirm
        /// <summary>
        /// Show user the confirm organisation view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Auth]
        [Route("confirm-organisation")]
        public ActionResult ConfirmOrganisation()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            return View("ConfirmOrganisation", model);
        }

        /// <summary>
        /// On confirmation save the organisation
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        [Route("confirm-organisation")]
        public ActionResult ConfirmOrganisation(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Load the employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));
            model.Employers = m.Employers;

            //Get the sic codes from companies house
            if (!model.ManualRegistration && model.SectorType == SectorTypes.Private && model.SelectedEmployer!=null)
                model.SelectedEmployer.SicCodes = PrivateSectorRepository.GetSicCodes(model.SelectedEmployer.CompanyNumber);

            //Save the registration
            SaveRegistration(currentUser, model);

            //Redirect to send pin
            this.StashModel(model);

            //If manual registration then show confirm receipt
            if (model.ManualRegistration)return RedirectToAction("RequestReceived");

            //If public sector then we are complete
            if (model.SectorType==SectorTypes.Public) RedirectToAction("Complete");

            //If private sector then send the pin
            return RedirectToAction("PINSent");
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
                org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.SectorType == SectorTypes.Public && o.OrganisationName.ToLower() == model.Name.ToLower());

            if (org == null)
            {
                var now = DateTime.Now;
                org = new Organisation();
                org.SectorType = model.SectorType.Value;
                org.OrganisationName = model.Name;
                org.PrivateSectorReference = employer==null ? null : employer.CompanyNumber;
                org.Created = now;
                org.Modified = now;
                org.Status = OrganisationStatuses.New;
                DataRepository.Insert(org);
                DataRepository.SaveChanges();

                //Use public sector code or get from employer
                var sicCodes = model.SectorType == SectorTypes.Public ? new []{1} : employer.GetSicCodes();
            
                //Save the sic codes for the organisation
                foreach (var code in sicCodes)
                {
                    var sicCode = code==0 ? null : DataRepository.GetAll<SicCode>().FirstOrDefault(sic => sic.SicCodeId == code);
                    if (sicCode != null)org.OrganisationSicCodes.Add(new OrganisationSicCode() {Organisation = org, SicCode = sicCode});
                }

                org.SetStatus(model.ManualRegistration ? OrganisationStatuses.Pending : OrganisationStatuses.Active, currentUser.UserId);
                DataRepository.SaveChanges();
            }
            
            //Save the new address
            var address = DataRepository.GetAll<OrganisationAddress>().FirstOrDefault(a => a.OrganisationId == org.OrganisationId && a.CreatedByUserId==currentUser.UserId);
            if (address == null)
            {
                address = new OrganisationAddress();
                address.OrganisationId = org.OrganisationId;
                address.Status= AddressStatuses.New;
                address.CreatedByUserId = currentUser.UserId;
                DataRepository.Insert(address);
                DataRepository.SaveChanges();
            }
            
            if (model.ManualRegistration || model.SectorType == SectorTypes.Public)
            {
                address.Address1 = model.Address1;
                address.Address2 = model.Address2;
                address.Address3 = model.Address3;
                address.Country = model.Country;
                address.PostCode = model.PostCode;
            }
            else
            {
                address.Address1 = employer.Address1;
                address.Address2 = employer.Address2;
                address.Address3 = employer.Address3;
                address.Country = employer.Country;
                address.PostCode = employer.PostCode;
            }

            if (model.ManualRegistration || model.SectorType == SectorTypes.Private)
                address.SetStatus(AddressStatuses.Pending, currentUser.UserId);
            else
                address.SetStatus(AddressStatuses.Active, currentUser.UserId);

           
            DataRepository.SaveChanges();

            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId && uo.UserId==currentUser.UserId);

            if (userOrg == null)
            {
                userOrg = new UserOrganisation()
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

            if (model.ManualRegistration)
            {
                currentUser.ContactFirstName = model.ContactFirstName;
                currentUser.ContactLastName = model.ContactLastName;
                currentUser.ContactJobTitle = model.ContactJobTitle;
                currentUser.ContactOrganisation = model.ContactOrganisation;
                currentUser.ContactEmailAddress = model.ContactEmailAddress;
                currentUser.ContactPhoneNumber = model.ContactPhoneNumber;
                DataRepository.SaveChanges();

                //Send request to GEO
                var adminEmail = GovNotifyAPI.RegistrationRequestEmailAddress;
#if DEBUG
                if (string.IsNullOrWhiteSpace(adminEmail)) adminEmail = currentUser.EmailAddress;
#endif
                SendRegistrationRequest(userOrg,adminEmail, $"{model.ContactFirstName} {currentUser.ContactLastName} ({currentUser.JobTitle})", currentUser.ContactOrganisation, org.OrganisationName, address.GetAddress());
            }

            //Set the status to active
            else if (model.SectorType == SectorTypes.Public)
            {
                //Set the user org as confirmed
                userOrg.PINConfirmedDate = DateTime.Now;

                //Mark the organisation as active
                userOrg.Organisation.SetStatus(OrganisationStatuses.Active, currentUser.UserId, "PIN Confirmed");

                //Mark the address as confirmed
                address.SetStatus(AddressStatuses.Active, currentUser.UserId, "PIN Confirmed");

                userOrg.ConfirmAttempts = 0;

                DataRepository.SaveChanges();
            }
        }

        //Send the registration request
        protected void SendRegistrationRequest(UserOrganisation userOrg, string adminEmail, string contactName, string contactOrg, string reportingOrg, string reportingAddress)
        {
            //Send a verification link to the email address
            try
            {
                var reviewCode = Encryption.EncryptQuerystring(userOrg.UserId + ":" + userOrg.OrganisationId + ":" + DateTime.Now.ToSmallDateTime());
                var reviewUrl = Url.Action("ReviewRequest", "Register", new { code = reviewCode }, "https");

                if (!GovNotifyAPI.SendRegistrationRequest(adminEmail,reviewUrl,contactName, contactOrg, reportingOrg, reportingAddress))
                    throw new Exception("Could not send registration request email. Please try again later.");
            }
            catch (Exception ex)
            {
                //Log the exception
                MvcApplication.Log.WriteLine(ex.Message);
                throw;
            }
        }


        [HttpGet]
        [Auth]
        [Route("request-received")]
        public ActionResult RequestReceived()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Clear the stash
            this.ClearStash();

            return View("RequestReceived");
        }

        #endregion

        #region ReviewRequest
        [HttpGet]
        [Route("review-request")]
        public ActionResult ReviewRequest(string code)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            var model = new OrganisationViewModel();

            if (string.IsNullOrWhiteSpace(code))
            {
                //Load the employers from session
                model = this.UnstashModel<OrganisationViewModel>();
                if (model == null) return View("CustomError", new ErrorViewModel(1114));

            }
            else
                model.ReviewCode = code;

            //Unwrap code
            UserOrganisation userOrg;
            OrganisationAddress address;
            
            var result=UnwrapRegistrationRequest(model, out userOrg, out address);
            if (result != null) return result;

            //Tell reviewer if this has already been approved
            if (userOrg.Organisation.Status==OrganisationStatuses.Active)
                AddModelError(3017, parameters: new {approvedDate = userOrg.Organisation.StatusDate.ToShortDateString()});

            //Tell reviewer how many other open regitrations for same organisation
            var requestCount = DataRepository.GetAll<UserOrganisation>().Count(uo => uo.UserId != userOrg.UserId && uo.OrganisationId == userOrg.OrganisationId && uo.Organisation.Status==OrganisationStatuses.Pending);
            if (requestCount > 0) AddModelError(3018, parameters: new { requestCount });

            this.StashModel(model);
            return View("ReviewRequest", model);
        }

        private ActionResult UnwrapRegistrationRequest(OrganisationViewModel model, out UserOrganisation userOrg, out OrganisationAddress address)
        {
            userOrg = null;
            address = null;

            long userId=0;
            long orgId=0;
            try
            {
                var code = Encryption.DecryptQuerystring(model.ReviewCode);
                code = HttpUtility.UrlDecode(code);
                var args = code.SplitI(":");
                if (args.Length != 3) throw new ArgumentException("Too few parameters in registration review code");
                userId = args[0].ToLong();
                if (userId == 0) throw new ArgumentException("Invalid user id in registration review code");
                orgId = args[1].ToLong();
                if (orgId == 0) throw new ArgumentException("Invalid organisation id in registration review code");
            }
            catch (Exception ex)
            {
                return View("CustomError", new ErrorViewModel(1114));
            }

            //Get the user oganisation
            userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == userId && uo.OrganisationId == orgId);

            if (userOrg == null) return View("CustomError", new ErrorViewModel(1115));

            switch (userOrg.Organisation.Status)
            {
                case OrganisationStatuses.Active:
                case OrganisationStatuses.Pending:
                    break;
                default:
                    throw new ArgumentException(
                        $"Invalid organisation status {userOrg.Organisation.Status} user {userId} and organisation {orgId} for reviewing registration request");
            }

            address = DataRepository.GetAll<OrganisationAddress>().FirstOrDefault(oa => oa.CreatedByUserId == userId && oa.OrganisationId == orgId);
            if (address == null)throw new Exception($"Cannot find address for user {userId} and organisation {orgId} for reviewing registration request");

            switch (address.Status)
            {
                case AddressStatuses.Active:
                    break;
                case AddressStatuses.Pending:
                    break;
                default:
                    throw new ArgumentException($"Invalid organisation address status {address.Status} for address {address.AddressId}, user {userId} and organisation {orgId} for reviewing registration request");
            }

            //Load view model
            model.ContactFirstName = userOrg.User.ContactFirstName;
            model.ContactLastName = userOrg.User.ContactLastName;
            model.ContactJobTitle = userOrg.User.ContactJobTitle;
            model.ContactOrganisation = userOrg.User.ContactOrganisation;
            model.ContactEmailAddress = userOrg.User.ContactEmailAddress;
            model.ContactPhoneNumber = userOrg.User.ContactPhoneNumber;

            model.Name = userOrg.Organisation.OrganisationName;
            model.SectorType = userOrg.Organisation.SectorType;

            model.Address1 = address.Address1;
            model.Address2 = address.Address2;
            model.Address3 = address.Address3;
            model.Country = address.Country;
            model.PostCode = address.PostCode;
            model.PoBox = address.PoBox;

            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("review-request")]
        public ActionResult ReviewRequest(OrganisationViewModel model,string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Unwrap code
            UserOrganisation userOrg;
            OrganisationAddress address;

            var result = UnwrapRegistrationRequest(model, out userOrg, out address);
            if (result != null) return result;

            if (command.EqualsI("decline"))
            {
                result = RedirectToAction("ConfirmCancellation");
            }
            else if (command.EqualsI("approve"))
            {            
                //Activate the address for this user and organisation
                address.SetStatus(AddressStatuses.Active, SingleAdmin?.UserId ?? currentUser.UserId, "Manually registered");

                //Activate the org user
                userOrg.PINConfirmedDate = DateTime.Now;

                //Activate the organisation 
                userOrg.Organisation.SetStatus(OrganisationStatuses.Active, SingleAdmin?.UserId ?? currentUser.UserId, "Manually registered");

                //Send the approved email to the applicant
                SendRegistrationAccepted(userOrg.User.ContactEmailAddress);

                result = RedirectToAction("RequestAccepted");
            }
            else 
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest,"Invalid command on 'review-request' postback");

            //Save the changes and redirect
            DataRepository.SaveChanges();

            //Save the model for the redirect
            this.StashModel(model);

            return result;
        }

        //Send the registration request
        protected void SendRegistrationAccepted(string emailAddress)
        {
            //Send a verification link to the email address
            try
            {
                string returnUrl = Url.Action("","Submit",null,"https");
                if (!GovNotifyAPI.SendRegistrationApproved(returnUrl, emailAddress))
                    throw new Exception("Could not send registration accepted email.");
            }
            catch (Exception ex)
            {
                //Log the exception
                MvcApplication.Log.WriteLine(ex.Message);
                throw;
            }
        }


        /// <summary>
        /// ask the reviewer for decline reason and confirmation /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("confirm-cancellation")]
        public ActionResult ConfirmCancellation()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            return View("ConfirmCancellation", model);
        }

        /// <summary>
        /// On confirmation save the organisation
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("confirm-cancellation")]
        public ActionResult ConfirmCancellation(OrganisationViewModel model, string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Load the employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", new ErrorViewModel(1112));

            //If cancel button clicked the n return to review page
            if (command.EqualsI("Cancel")) return RedirectToAction("ReviewRequest");

            //Unwrap code
            UserOrganisation userOrg;
            OrganisationAddress address;

            var result = UnwrapRegistrationRequest(model, out userOrg, out address);
            if (result != null) return result;

            //Delete address for this user and organisation
            DataRepository.Delete(address);

            //Delete the org user
            var orgId = userOrg.OrganisationId;
            var emailAddress = userOrg.User.ContactEmailAddress;
            DataRepository.Delete(userOrg);

            //Delete the organisation if there are no other registrations pending
            var requestCount = DataRepository.GetAll<UserOrganisation>().Count(uo => uo.UserId != userOrg.UserId && uo.OrganisationId == orgId);
            if (requestCount == 0)
            {
                var org=DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == orgId);
                if (org!=null)DataRepository.Delete(org);
            }

            //Send the declined email to the applicant
            SendRegistrationDeclined(emailAddress,model.CancellationReason);
            
            //Save the changes and redirect
            DataRepository.SaveChanges();

            //Save the model for the redirect
            this.StashModel(model);

            //If private sector then send the pin
            return RedirectToAction("RequestCancelled");
        }


        //Send the registration request
        protected void SendRegistrationDeclined(string emailAddress,string reason)
        {
            //Send a verification link to the email address
            try
            {
                string returnUrl = Url.Action("OrganisationType", "Register",null,"https");
                if (!GovNotifyAPI.SendRegistrationDeclined(returnUrl,emailAddress, reason))
                    throw new Exception("Could not send registration declined email.");
            }
            catch (Exception ex)
            {
                //Log the exception
                MvcApplication.Log.WriteLine(ex.Message);
                throw;
            }
        }
        /// <summary>
        /// Show review accepted confirmation
        /// <returns></returns>
        [HttpGet]
        [Route("request-accepted")]
        public ActionResult RequestAccepted()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load model from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            //Clear the stash
            this.ClearStash();

            return View("RequestAccepted", model);
        }

        /// <summary>
        /// Show review cancel confirmation
        /// <returns></returns>
        [HttpGet]
        [Route("request-cancelled")]
        public ActionResult RequestCancelled()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Make sure we can load model from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1112));

            //Clear the stash
            this.ClearStash();

            return View("RequestCancelled", model);
        }
        #endregion

        #region PINSent
        ActionResult GetSendPIN()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Get the user organisation
            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            //If a pin has never been sent or resend button submitted then send one immediately
            if (string.IsNullOrWhiteSpace(userOrg.PINHash) || userOrg.PINSentDate.EqualsI(null, DateTime.MinValue))
            {
                try
                {
                    //Generate a new pin
                    var pin = ConfigurationManager.AppSettings["TESTING-Pin"];
                    if (string.IsNullOrWhiteSpace(pin))pin = Crypto.GeneratePasscode(Properties.Settings.Default.PINChars.ToCharArray(),Properties.Settings.Default.PINLength);

                    //Try and send the PIN in post
                    var emailPIN = ConfigurationManager.AppSettings["EmailPIN"].ToBoolean(true);
                    if (emailPIN && !this.SendPinInPost(currentUser, userOrg.Organisation, pin.ToString()))
                        throw new Exception("Could not send PIN in the POST.");

                    //Try and send the confirmation email
                    //if (!this.SendConfirmEmail(currentUser.EmailAddress))
                    //    throw new Exception("Could not send confirmation email. Please try again later.");

                    //Save the PIN and confirm code
                    userOrg.PINHash = pin.GetSHA512Checksum();
                    userOrg.PINSentDate = DateTime.Now;
                    DataRepository.SaveChanges();
                }
                catch (Exception ex)
                {
                    MvcApplication.Log.WriteLine(ex.Message);
                    AddModelError(3014);
                    return View("PINSent");
                }
            }

            //Prepare view parameters
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
            ViewBag.Organisation = userOrg.Organisation.OrganisationName;
            ViewBag.Address = userOrg.Address.GetAddress(",<br/>");
            return View("PINSent");
        }

        [Auth]
        [HttpGet]
        [Route("pin-sent")]
        public ActionResult PINSent()
        {
            return GetSendPIN();
        }

        #endregion

        #region RequestPIN
        [HttpGet]
        [Auth]
        [Route("request-pin")]
        public ActionResult RequestPIN()
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Get the user organisation
            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            //Prepare view parameters
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
            ViewBag.Organisation = userOrg.Organisation.OrganisationName;
            ViewBag.Address = userOrg.Organisation.Address.GetAddress(",<br/>");
            //Show the PIN textbox and button
            return View("RequestPIN");
        }

        [HttpPost]
        [Auth]
        [ValidateAntiForgeryToken]
        [Route("request-pin")]
        public ActionResult RequestPIN(CompleteViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Get the user organisation
            var userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == currentUser.UserId);

            //Marke the user org as ready to send a pin
            userOrg.PINHash = null;
            userOrg.PINSentDate = null;
            DataRepository.SaveChanges();

            return RedirectToAction("PINSent");
        }
        #endregion

        #region ActivateService
        [HttpGet]
        [Auth]
        [Route("activate-service")]
        public ActionResult ActivateService()
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
            return View("ActivateService", model);
        }

        [HttpPost]
        [Auth]
        [ValidateAntiForgeryToken]
        [Route("activate-service")]
        public ActionResult ActivateService(CompleteViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            var checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null) return checkResult;

            //Ensure they have entered a PIN
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<CompleteViewModel>();
                return View("ActivateService", model);
            }

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

                //Set the pending organisation to active
                userOrg.Organisation.SetStatus(OrganisationStatuses.Active, currentUser.UserId, "PIN Confirmed");

                //Set the pending address to active
                userOrg.Address.SetStatus(AddressStatuses.Active, currentUser.UserId);

                userOrg.ConfirmAttempts = 0;

                result1 = RedirectToAction("Complete");
            }
            else
            {
                userOrg.ConfirmAttempts++;
                AddModelError(3015,"PIN");
                result1 = View("ActivateService", model);
            }
            userOrg.ConfirmAttemptDate = DateTime.Now;

            //Save the changes
            DataRepository.SaveChanges();

            //Prompt the user with confirmation
            return result1;
        }
        #endregion

        #region Complete
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
            this.ClearStash();

            //Show the confirmation view
            return View("Complete");
        }
        #endregion
    }
}