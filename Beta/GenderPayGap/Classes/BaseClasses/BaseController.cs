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

        IRepository _Repository;
        protected IRepository Repository
        {
            get
            {
                if (_Repository==null)_Repository = containerIOC.Resolve<IRepository>();
                return _Repository;
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

        #endregion

        #region Authorisation Methods
        protected ActionResult CheckUserRegisteredOk(out User user)
        {
            user = null;
            //Ensure only checking authenticated users
            if (!User.Identity.IsAuthenticated)
            {
                //When enrolling return not errors
                if (this is RegisterController) return null;

                //When submitting ensure user is authenticated
                if (this is SubmitController) return new HttpUnauthorizedResult();
            }

            //The user can then go through the process of changing their details and email then sending another verification email

            //Get the mapped user from the principle
            var currentUser = Repository.FindUser(User);
            if (currentUser == null) throw new IdentityNotMappedException();
            user = currentUser;

            //Ensure the email address is verified
            if (currentUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                //Allow resend of verification if sent over 24 hours ago
                if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
                    return View("CustomError", new ErrorViewModel(1100));

                //Allow resend of verification if sent over 24 hours ago
                if (currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
                    return View("CustomError", new ErrorViewModel(1101));

                //Otherwise prompt user to check account only
                var remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationMinResendHours) - DateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                    return View("CustomError", new ErrorViewModel(1102, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));

                return View("CustomError", new ErrorViewModel(1103));
            }

            //Get the current users organisation registration
            var userOrg = Repository.GetUserOrg(currentUser);

            //If they didnt have started organisation registration step then prompt to continue registration
            if (userOrg == null)
                return View("CustomError", new ErrorViewModel(1104));

            if (userOrg.PINConfirmedDate.EqualsI(null, DateTime.MinValue))
            {
                //Allow resend of PIN if sent over 2 weeks ago
                if (userOrg.PINSentDate.EqualsI(null, DateTime.MinValue))
                    return View("CustomError", new ErrorViewModel(1105));
                if (userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostExpiryDays) < DateTime.Now)
                    return View("CustomError", new ErrorViewModel(1106));
                var remainingTime = userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostMinRepostDays) - DateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                    return View("CustomError", new ErrorViewModel(1107, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
                return View("CustomError", new ErrorViewModel(1108));
            }

            if (this is RegisterController)
                //Ensure user has completed the registration process
                //If user is fully registered then start submit process
                return View("CustomError", new ErrorViewModel(1109));

            return null;
        }

        #endregion

        #region Session Handling

        protected void StashModel<T>(T model)
        {
            Session[this+":Model"] = model;
        }
        protected T UnstashModel<T>(bool delete=false)
        {
            var result=(T)Session[this + ":Model"];
            if (delete) Session.Remove(this + ":Model");
            return result;
        }

        #endregion

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