﻿using Autofac;
using Extensions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Classes;
using GpgDB.Models.GpgDatabase;
using System;
using System.Collections.Generic;
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
            WriteLog(Settings.Default.LogErrorFile, filterContext.Exception.ToString());

            // Output a nice error page
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                filterContext.ExceptionHandled = true;
                View("Error").ExecuteResult(ControllerContext);
            }
        }

        /// <summary>
        /// Logs a message to the given log file
        /// </summary>
        /// <param name="logFile">The filename to log to</param>
        /// <param name="text">The message to log</param>
        static void WriteLog(string logFile, string text)
        {
            //TODO: Format nicer
            StringBuilder message = new StringBuilder();
            message.AppendLine(DateTime.Now.ToString());
            message.AppendLine(text);
            message.AppendLine("=========================================");

            System.IO.File.AppendAllText(logFile, message.ToString());
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
                    return View("Error", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You have not verified your email address.",
                        CallToAction = "Click the button below to continue registration and verify your email address",
                        ActionUrl = Url.Action("Step2", "Register")
                    });

                //Allow resend of verification if sent over 24 hours ago
                if (currentUser.EmailVerifySendDate.Value.AddHours(WebUI.Properties.Settings.Default.EmailVerificationExpiryHours) < DateTime.Now)
                    return View("Error", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You did not verified your email address within the allowed time.",
                        CallToAction = "Please click the button below to request a new verification email",
                        ActionUrl = Url.Action("Step2", "Register")
                    });

                //Otherwise prompt user to check account only
                return View("Error", new ErrorViewModel()
                {
                    Title = "Incomplete Registration",
                    Description = "You have not verified your email address.",
                    CallToAction = "Please check your email account and follow the instructions to verify your email address.",
                });
            }

            //Get the current users organisation registration
            var userOrg = Repository.GetUserOrg(currentUser);

            //If they didnt have started organisation registration step then prompt to continue registration
            if (userOrg == null)
                return View("Error", new ErrorViewModel()
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
                    return View("Error", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You have not been sent a PIN in the post.",
                        CallToAction = "Please click the button below to request a PIN be sent to your organisations address.",
                        ActionUrl = Url.Action("Step3", "Register")
                    });
                if (userOrg.PINSentDate.Value.AddDays(WebUI.Properties.Settings.Default.PinInPostExpiryDays) < DateTime.Now)
                    return View("Error", new ErrorViewModel()
                    {
                        Title = "Incomplete Registration",
                        Description = "You did not confirm the PIN sent to you in the post in the allowed time.",
                        CallToAction = "Please click the button below to request a new PIN to be sent to your organisations address.",
                        ActionUrl = Url.Action("Step6", "Register")
                    });
                return View("Error", new ErrorViewModel()
                {
                    Title = "Incomplete Registration",
                    Description = "You have not confirmed the PIN sent to you in the post.",
                    CallToAction = "Please check your mail for this letter and follow the instructions to complete registration.",
                });
            }

            if (this is RegisterController)
                //Ensure user has completed the registration process
                //If user is fully registered then start submit process
                return View("Error", new ErrorViewModel()
                {
                    Title = "Registration Complete",
                    Description = "You have already completed registration.",
                    CallToAction = "Next Step: Submit your Gender Pay Gap data",
                    ActionUrl = Url.Action("Create", "Return")
                });

            return null;
        }

        public bool Authorise()
        {
            var user = Repository.FindUser(User); //TODO:There is BUG Here
            if (user == null || user.EmailVerifiedDate == null || user.EmailVerifiedDate == DateTime.MinValue)
                return false;

            var userOrg = Repository.GetAll<UserOrganisation>().FirstOrDefault(u => u.UserId == user.UserId);
            if (userOrg == null || userOrg.PINConfirmedDate == null || userOrg.PINConfirmedDate == DateTime.MinValue)
                return false;

            return true;
        }
        #endregion

        #region Error handling actions
        [HandleError(ExceptionType = typeof(IdentityNotMappedException))]
        public ActionResult IdentityNotMapped()
        {
            return View("Error", new ErrorViewModel()
            {
                Title = "Unauthorised Request",
                Description = "Unrecognised user.",
                CallToAction = "Please log out of the system.",
                ActionUrl = Url.Action("LogOff", "Account")
            });
        }

        [HandleError(ExceptionType = typeof(UnauthorizedAccessException))]
        public ActionResult UnauthorizedAccess()
        {
            return View("Error", new ErrorViewModel()
            {
                Title = "Unauthorised Request",
                Description = "You do not have the required permission to access this option.",
                CallToAction = "Please log in as a user with the correct permissions.",
                ActionUrl = Url.Action("LogOff", "Account")
            });
        }

        [HandleError(ExceptionType = typeof(AuthenticationException))]
        public ActionResult NotAuthenticated()
        {
            return View("Error", new ErrorViewModel()
            {
                Title = "Login Required",
                Description = "You must first login to access this option.",
                CallToAction = "Next Step: Login to service",
                ActionUrl = Url.Action("Logon", "Identity")
            });
        }
        #endregion

    }
}