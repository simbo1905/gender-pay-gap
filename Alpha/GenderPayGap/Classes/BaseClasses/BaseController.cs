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
            Log.WriteLine(filterContext.Exception.ToString());

            // Output a nice error page
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                filterContext.ExceptionHandled = true;
                if (filterContext.Exception is HttpException)
                    filterContext.Result = RedirectToAction("HttpError","Error", new {code = ((HttpException) filterContext.Exception).GetHttpCode()});
                else if (filterContext.Exception is IdentityNotMappedException)
                    filterContext.Result=View("CustomError", new ErrorViewModel()
                    {
                        Title = "Unauthorised Request",
                        Description = "Unrecognised user.",
                        CallToAction = "Please log out of the system.",
                        ActionUrl = Url.Action("LogOut", "Home")
                    });
                else if (filterContext.Exception is UnauthorizedAccessException)
                    filterContext.Result =  View("CustomError", new ErrorViewModel()
                    {
                        Title = "Unauthorised Request",
                        Description = "You do not have the required permission to access this option.",
                        CallToAction = "Please log in as a user with the correct permissions.",
                        ActionUrl = Url.Action("LogOut", "Home")
                    });
                else if (filterContext.Exception is AuthenticationException)
                    filterContext.Result = View("CustomError", new ErrorViewModel()
                    {
                        Title = "Login Required",
                        Description = "You must first login to access this option.",
                        CallToAction = "Next Step: Login to service",
                        ActionUrl = Url.Action("LogOut", "Home")
                    });
                else
                    filterContext.Result = View("CustomError", new ErrorViewModel()
                    {
                        Title = "Unexpected Error",
                        Description = "An unexpected error has occurred and has been reported to the administrator.",
                        CallToAction = "Please try again later"
                    });
            }
        }

        private Logger Log => new Logger(FileSystem.ExpandLocalPath(Path.Combine(Settings.Default.LogPath, "Errors")));

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
                if (this is ReturnController) throw new UnauthorizedAccessException();
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
                    return View("CustomError", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You have not verified your email address.",
                        CallToAction = "Click the button below to continue registration and verify your email address",
                        ActionUrl = Url.Action("Step2", "Register")
                    });

                //Allow resend of verification if sent over 24 hours ago
                if (currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
                    return View("CustomError", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You did not verified your email address within the allowed time.",
                        CallToAction = "Please click the button below to request a new verification email",
                        ActionUrl = Url.Action("Step2", "Register")
                    });

                //Otherwise prompt user to check account only
                var remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationMinResendHours) - DateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                    return View("CustomError", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You have not yet verified your email address.",
                        CallToAction = "Please check your email account and follow the instructions to verify your email address.<br/>Alternatively, try again in " + remainingTime.ToFriendly(maxParts: 2) + " to request another verification email."
                    });
                return View("CustomError", new ErrorViewModel()
                {
                    Title = "Incomplete Registration",
                    Description = "You have not yet verified your email address.",
                    CallToAction = "Please check your email account and follow the instructions to verify your email address.",
                    ActionUrl = Url.Action("Step2", "Register")
                });
            }

            //Get the current users organisation registration
            var userOrg = Repository.GetUserOrg(currentUser);

            //If they didnt have started organisation registration step then prompt to continue registration
            if (userOrg == null)
                return View("CustomError", new ErrorViewModel()
                {
                    Title = "Incomplete Registration",
                    Description = "You have not completed the registration process.",
                    CallToAction = "Next Step: Select your organisation",
                    ActionUrl = Url.Action("Step3", "Register")
                });

            if (userOrg.PINConfirmedDate.EqualsI(null, DateTime.MinValue))
            {
                //Allow resend of PIN if sent over 2 weeks ago
                if (userOrg.PINSentDate.EqualsI(null, DateTime.MinValue))
                    return View("CustomError", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You have not been sent a PIN in the post.",
                        CallToAction = "Please click the button below to send a PIN to your organisations address.",
                        ActionUrl = Url.Action("SendPIN", "Register")
                    });
                if (userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostExpiryDays) < DateTime.Now)
                    return View("CustomError", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You did not confirm the PIN sent to you in the post in the allowed time.",
                        CallToAction = "Please click the button below to request a new PIN to be sent to your organisations address.",
                        ActionUrl = Url.Action("SendPIN", "Register")
                    });
                var remainingTime = userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostMinRepostDays) - DateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                    return View("CustomError", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You have not yet confirmed the PIN sent to you in the post.",
                        CallToAction = "Click the button below to enter the PIN you have received in the post or try again in "+ remainingTime.ToFriendly(maxParts:2) +" to request another PIN.",
                        ActionUrl = Url.Action("ConfirmPIN", "Register")
                    });
                return View("CustomError", new ErrorViewModel()
                {
                    Title = "Incomplete Registration",
                    Description = "You have not confirmed the PIN sent to you in the post.",
                    CallToAction = "Click the button below to enter the PIN you have received in the post or to request another PIN.",
                    ActionUrl = Url.Action("ConfirmPIN", "Register")
                });
            }

            if (this is RegisterController)
                //Ensure user has completed the registration process
                //If user is fully registered then start submit process
                return View("CustomError", new ErrorViewModel()
                {
                    Title = "Registration Complete",
                    Description = "You have already completed registration.",
                    CallToAction = "Next Step: Submit your Gender Pay Gap data",
                    ActionUrl = Url.Action("Create", "Return")
                });

            return null;
        }

        #endregion

        #region Session Handling

        protected void StashModel<T>(T model)
        {
            Session[this+":Model"] = model;
        }
        protected T UnstashModel<T>()
        {
            return (T)Session[this + ":Model"];
        }

        #endregion

        //Current account Year method
        public DateTime GetCurrentAccountYearStartDate(Organisation org)
        {
            return DateTime.MinValue;
        }
    }
}