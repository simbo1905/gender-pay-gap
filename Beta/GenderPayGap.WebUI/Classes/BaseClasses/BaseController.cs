using Autofac;
using Extensions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.Models.SqlDatabase;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.Core.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Properties;

namespace GenderPayGap
{
    public class BaseController:Controller
    {
        #region Constructors
        public BaseController():this(MvcApplication.ContainerIOC)
        {
        }

        public BaseController(IContainer containerIOC)
        {
            this.containerIOC = containerIOC;
        }
        #endregion

        #region IoC fields
        protected IContainer containerIOC;

        IRepository _DataRepository;
        public IRepository DataRepository
        {
            get
            {
                if (_DataRepository==null)_DataRepository = containerIOC.Resolve<IRepository>();
                return _DataRepository;
            }
            set { _DataRepository = null; }
        }

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

        /// <summary>
        /// Return admin if only one concrete admin email who exists in database
        /// </summary>
        private User _SingleAdmin = null;
        public User SingleAdmin
        {
            get
            {
                if (_SingleAdmin == null)
                {
                    var args = MvcApplication.AdminEmails.SplitI(";");
                    if (args.Length == 1 && !string.IsNullOrWhiteSpace(args[0]) && !args[0].ContainsAny('*', '?') &&
                        args[0].IsEmailAddress())
                        _SingleAdmin=DataRepository.FindUserByEmail(args[0].ToLower());
                }
                return _SingleAdmin;
            }
        }

        #endregion

        #region Public fields

        public string ActionName => ControllerContext.RouteData.Values["action"].ToString();

        public string ControllerName => ControllerContext.RouteData.Values["controller"].ToString();

        public string LastAction
        {
            get { return Session["LastAction"] as string; }
            set { Session["LastAction"] = value; }
        }

        public string LastController
        {
            get { return Session["LastController"] as string; }
            set { Session["LastController"] = value; }
        }

        #endregion

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            LastAction = ActionName;
            LastController = ControllerName;
            base.OnActionExecuted(filterContext);
        }

        #region Exception handling methods


        protected override void OnException(ExceptionContext filterContext)
        {
            //Add to the log
            MvcApplication.Log.WriteLine(filterContext.Exception.ToString());

            // Output a nice error page
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                filterContext.ExceptionHandled = true;
                if (filterContext.Exception is HttpException)
                    filterContext.Result = View("CustomError",new ErrorViewModel(((HttpException) filterContext.Exception).GetHttpCode()));
                else if (filterContext.Exception is IdentityNotMappedException)
                    filterContext.Result=View("CustomError", new ErrorViewModel(1000));
                else if (filterContext.Exception is UnauthorizedAccessException)
                    filterContext.Result = View("CustomError", new ErrorViewModel(1001));
                else if (filterContext.Exception is AuthenticationException)
                    filterContext.Result = View("CustomError", new ErrorViewModel(1002));
                else
                    filterContext.Result = View("CustomError", new ErrorViewModel(1003));
            }
        }

        protected void AddModelError(string propertyName, string errorContext, object parameters = null)
        {
            GenderPayGap.WebUI.Classes.Extensions.AddModelError(this, propertyName, errorContext,parameters);
        }

        public void AddModelError(int errorCode, string propertyName=null, object parameters = null)
        {
            GenderPayGap.WebUI.Classes.Extensions.AddModelError(this.ModelState,errorCode, propertyName,parameters);
        }

        #endregion

        #region Authorisation Methods
        protected ActionResult CheckUserRegisteredOk(out User currentUser)
        {

            currentUser = null;
            if (MvcApplication.MaintenanceMode)
                return RedirectToAction("ServiceUnavailable", "Error");

            //Ensure user is logged in submit or rest of of registration
            if (!User.Identity.IsAuthenticated)
            {
                //Allow anonymous users when in single admin mode
                if (SingleAdmin!=null && IsAnyAction("Register/ReviewRequest", "Register/ConfirmCancellation", "Register/RequestAccepted", "Register/RequestCancelled")) return null;

                //Allow anonymous users when starting registration
                if (IsAnyAction("Register/AboutYou", "Register/VerifyEmail")) return null;

                //Otherwise ask the user to login
                return new HttpUnauthorizedResult();
            }

            //Always allow the viewing controller
            if (this is ViewingController)return null;

            //Ensure we get a valid user from the database
            currentUser = DataRepository.FindUser(User);
            if (currentUser == null) throw new IdentityNotMappedException();

            //When email not verified
            if (currentUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                //If email not sent
                if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
                {
                    if (IsAnyAction("Register/VerifyEmail")) return null;
                    //Tell them to verify email
                    return View("CustomError", new ErrorViewModel(1100));
                }

                //If verification code has expired
                if (currentUser.EmailVerifySendDate.Value.AddHours(Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
                {
                    if (IsAnyAction("Register/VerifyEmail")) return null;

                    //prompt user to click to request a new one
                    return View("CustomError", new ErrorViewModel(1101));
                }

                //If code min time hasnt elapsed 
                var remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(Settings.Default.EmailVerificationMinResendHours) - DateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    //Process the code if there is one
                    if (IsAnyAction("Register/VerifyEmail") && !string.IsNullOrWhiteSpace(Request.QueryString["code"])) return null;

                    //tell them to wait
                    return View("CustomError", new ErrorViewModel(1102, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
                }

                //if the code is still valid but min sent time has elapsed
                if (IsAnyAction("Register/VerifyEmail", "Register/EmailConfirmed")) return null;

                //Prompt user to request a new verification code
                return View("CustomError", new ErrorViewModel(1103));
            }

            //Ensure manual registration pages only allowed by GEO email addresses
            if (!currentUser.IsAdministrator() && IsAnyAction("Register/ReviewRequest", "Register/ConfirmCancellation", "Register/RequestAccepted", "Register/RequestCancelled")) 
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            //Ensure manual registration pages only allowed by GEO email addresses
            if (currentUser.IsAdministrator())
            {
                if (IsAnyAction("Register/VerifyEmail", "Register/EmailConfirmed", "Register/ReviewRequest",
                    "Register/ConfirmCancellation", "Register/RequestAccepted", "Register/RequestCancelled"))
                    return null;
                return View("CustomError", new ErrorViewModel(1117));
            }
            //Get the current users organisation registration
            var userOrg = DataRepository.GetUserOrg(currentUser);

            //If they didnt have started organisation registration step then prompt to continue registration
            if (userOrg == null)
            {
                if (IsAnyAction("Register/EmailConfirmed","Register/OrganisationType", "Register/OrganisationSearch", "Register/ChooseOrganisation", "Register/AddOrganisation", "Register/AddContact", "Register/ConfirmOrganisation")) return null;
                return View("CustomError", new ErrorViewModel(1104));
            }

            if (userOrg.Organisation.Status == OrganisationStatuses.Pending)
            {
                if (IsAnyAction("Register/RequestReceived")) return null;
                return RedirectToAction("RequestReceived", "Register");
            }
            else if (userOrg.Organisation.SectorType == SectorTypes.Private)
            {
                if (userOrg.PINConfirmedDate.EqualsI(null, DateTime.MinValue))
                {
                    //If pin never sent restart EmployerWebsite
                    if (userOrg.PINSentDate.EqualsI(null, DateTime.MinValue))
                    {
                        if (IsAnyAction("Register/PINSent", "Register/RequestPIN")) return null;
                        return RedirectToAction("PINSent", "Register");
                    }

                    //If PIN sent and expired then prompt to request a new pin
                    if (userOrg.PINSentDate.Value.AddDays(Settings.Default.PinInPostExpiryDays) < DateTime.Now)
                    {
                        if (IsAnyAction("Register/PINSent", "Register/RequestPIN")) return null;
                        return View("CustomError", new ErrorViewModel(1106));
                    }

                    //If PIN resends are allowed and currently on PIN send page then allow it to continue
                    var remainingTime = userOrg.PINSentDate.Value.AddHours(Settings.Default.PinInPostMinRepostDays) -
                                        DateTime.Now;
                    if (remainingTime <= TimeSpan.Zero && IsAnyAction("Register/PINSent", "Register/RequestPIN"))
                        return null;

                    //If PIN Not expired redirect to ActivateService where they can either enter the same pin or request a new one 
                    if (IsAnyAction("Register/ActivateService")) return null;
                    return RedirectToAction("ActivateService", "Register");
                }
            }
            

            //Ensure user has completed the registration process
            //If user is fully registered then start submit process
            if (this is RegisterController)
            {
                if (IsAnyAction("Register/RequestReceived")) return null;
                if (IsAnyAction("Register/Complete") && WasAnyAction("Register/ActivateService","Register/ConfirmOrganisation")) return null;
                return View("CustomError", new ErrorViewModel(1109));
            }

            return null;
        }

        #endregion

        /// <summary>
        /// returns true if previous action 
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public bool WasAction(string actionName, string controllerName = null, object routeValues=null)
        {
            if (string.IsNullOrWhiteSpace(controllerName)) controllerName = ControllerName;
            return Request.UrlReferrer==null ? false : Request.UrlReferrer.PathAndQuery.EqualsI(Url.Action(actionName, controllerName, routeValues));
        }

        public bool WasAnyAction(params string[] actionUrls)
        {
            for (var i = 0; i < actionUrls.Length; i++)
            {
                var actionUrl = actionUrls[i].TrimI(@" /\");
                var actionName = actionUrl.AfterFirst("/");
                var controllerName = actionUrl.BeforeFirst("/", includeWhenNoSeperator: false);
                if (WasAction(actionName, controllerName)) return true;
            }
            return false;
        }

        public bool WasController(string controllerName)
        {
            var referrer = Request.UrlReferrer==null ? Url.Action(LastAction, LastController) : Request.UrlReferrer.PathAndQuery;
            return !string.IsNullOrWhiteSpace(referrer) && referrer.StartsWithI(Url.Action("/", controllerName));
        }

        public bool IsAction(string actionName, string controllerName=null)
        {
            return actionName.EqualsI(ActionName) && (controllerName.EqualsI(ControllerName) || string.IsNullOrWhiteSpace(controllerName));
        }

        public bool IsAnyAction(params string[] actionUrls)
        {
            for (var i=0;i<actionUrls.Length;i++)
            {
                var actionUrl = actionUrls[i].TrimI(@" /\");
                var actionName = actionUrl.AfterFirst("/");
                var controllerName = actionUrl.BeforeFirst("/",includeWhenNoSeperator:false);
                if (IsAction(actionName, controllerName)) return true;
            }
            return false;
        }

        //Current account Year method
        public DateTime GetAccountYearStartDate(SectorTypes sectorType, int year=0)
        {
            var tempDay = 0;
            var tempMonth = 0;

            var now = DateTime.Now;

            switch (sectorType)
            {
                case SectorTypes.Private:
                    tempDay = Settings.Default.PrivateAccountingDate.Day;
                    tempMonth = Settings.Default.PrivateAccountingDate.Month;
                    break;
                case SectorTypes.Public:
                    tempDay = Settings.Default.PublicAccountingDate.Day;
                    tempMonth = Settings.Default.PublicAccountingDate.Month;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sectorType),sectorType,"Cannot calculate accounting date for this sector type");
            }

            var tempDate = new DateTime(year==0 ? DateTime.Now.Year : year, tempMonth, tempDay);
            return now > tempDate ? tempDate : tempDate.AddYears(-1);
        }
    }
}