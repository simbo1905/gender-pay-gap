using Autofac;
using Extensions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.Models.SqlDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Properties;
using System.Linq.Expressions;

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
        protected IRepository DataRepository
        {
            get
            {
                if (_DataRepository==null)_DataRepository = containerIOC.Resolve<IRepository>();
                return _DataRepository;
            }
        }

        #endregion

        #region Public fields

        public string ActionName => ControllerContext.RouteData.Values["action"].ToString();

        public string ControllerName => ControllerContext.RouteData.Values["controller"].ToString();

        #endregion

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

        public void AddModelError(int errorCode, object parameters = null)
        {
            GenderPayGap.WebUI.Classes.Extensions.AddModelError(this,errorCode,parameters);
        }
        #endregion

        #region Authorisation Methods
        protected ActionResult CheckUserRegisteredOk(out User currentUser)
        {
            currentUser = null;
            //Ensure user is logged in submit or rest of of registration
            if (!User.Identity.IsAuthenticated)
                return IsAnyAction("Register/Step1", "Register/Step2") ? null : new HttpUnauthorizedResult();

            //Ensure we get a valid user from the database
            currentUser = DataRepository.FindUser(User);
            if (currentUser == null) throw new IdentityNotMappedException();

            //When email not verified
            if (currentUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                //If email not sent
                if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
                {
                    if (IsAnyAction("Register/Step2")) return null;
                    //Tell them to verify email
                    return View("CustomError", new ErrorViewModel(1100));
                }

                //If verification code has expired
                if (currentUser.EmailVerifySendDate.Value.AddHours(Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
                {
                    if (IsAnyAction("Register/Step2")) return null;

                    //prompt user to click to request a new one
                    return View("CustomError", new ErrorViewModel(1101));
                }

                //If code min time hasnt elapsed 
                var remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(Settings.Default.EmailVerificationMinResendHours) - DateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    //Process the code if there is one
                    if (IsAnyAction("Register/Step2") && !string.IsNullOrWhiteSpace(Request.QueryString["code"])) return null;

                    //tell them to wait
                    return View("CustomError", new ErrorViewModel(1102, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
                }

                //if the code is still valid but min sent time has elapsed
                if (IsAnyAction("Register/Step2")) return null;

                //Prompt user to request a new verification code
                return View("CustomError", new ErrorViewModel(1103));
            }

            //Get the current users organisation registration
            var userOrg = DataRepository.GetUserOrg(currentUser);
            var org = userOrg==null ? null : DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == userOrg.OrganisationId);

            //If they didnt have started organisation registration step then prompt to continue registration
            if (userOrg == null || org==null)
            {
                if (IsAnyAction("Register/Step3", "Register/Step4", "Register/Step5", "Register/Step6")) return null;

                if ((org==null || org.SectorType== SectorTypes.Public) && IsAnyAction("Register/Step7")) return null;
                return View("CustomError", new ErrorViewModel(1104));
            }

            if (org.SectorType == SectorTypes.Private)
            {
                if (userOrg.PINConfirmedDate.EqualsI(null, DateTime.MinValue))
                {
                    //If pin never sent restart step3
                    if (userOrg.PINSentDate.EqualsI(null, DateTime.MinValue))
                    {
                        if (IsAnyAction("Register/SendPIN")) return null;
                        return RedirectToAction("SendPIN", "Register");
                    }

                    //If PIN sent and expired then prompt to request a new pin
                    if (userOrg.PINSentDate.Value.AddDays(Settings.Default.PinInPostExpiryDays) < DateTime.Now)
                    {
                        if (IsAnyAction("Register/SendPIN")) return null;
                        return View("CustomError", new ErrorViewModel(1106));
                    }

                    //If PIN resends are allowed and currently on PIN send page then allow it to continue
                    var remainingTime = userOrg.PINSentDate.Value.AddHours(Settings.Default.PinInPostMinRepostDays) - DateTime.Now;
                    if (remainingTime <= TimeSpan.Zero && IsAnyAction("Register/SendPIN")) return null;

                    //If PIN Not expired redirect to confirmPIN where they can either enter the same pin or request a new one 
                    if (IsAnyAction("Register/ConfirmPIN")) return null;
                    return RedirectToAction("ConfirmPIN", "Register");
                }
            }

            //Ensure user has completed the registration process
            //If user is fully registered then start submit process
            if (this is RegisterController)
            {
                if (IsAnyAction("Register/Complete")) return null;
                return RedirectToAction("Complete", "Register");
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
            return Request.UrlReferrer==null ? false : Request.UrlReferrer.PathAndQuery.EqualsI(Url.Action("Step4", controllerName, routeValues));
        }

        public bool IsAction(string actionName, string controllerName=null)
        {
            return actionName.EqualsI(ActionName) && (controllerName==ControllerName || string.IsNullOrWhiteSpace(controllerName));
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
        public DateTime GetCurrentAccountYearStartDate(Organisation org)
        {
            var tempDay = 0;
            var tempMonth = 0;

            var Now = DateTime.Now;
            DateTime currAccountYearStartDate = DateTime.MinValue;

            if ((org.SectorType == SectorTypes.Private))
            {
                tempDay = Settings.Default.PrivateAccountingDate.Day;
                tempMonth = Settings.Default.PrivateAccountingDate.Month;

                DateTime TempDate = new DateTime(Now.Year, tempMonth, tempDay);

                currAccountYearStartDate = Now > TempDate ? TempDate : TempDate.AddYears(-1);
            }

            if ((org.SectorType == SectorTypes.Public))
            { 
                tempDay = Settings.Default.PublicAccountingDate.Day;
                tempMonth = Settings.Default.PublicAccountingDate.Month;

                DateTime TempDate = new DateTime(Now.Year, tempMonth, tempDay);

                if (Now > TempDate)
                    currAccountYearStartDate = TempDate;
                else
                    currAccountYearStartDate = TempDate.AddYears(-1);
            }
            
            return currAccountYearStartDate;
        }
    }
}