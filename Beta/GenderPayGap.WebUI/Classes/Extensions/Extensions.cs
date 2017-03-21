using Extensions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Models.SqlDatabase;
using IdentityServer3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.WebUI.Controllers;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace GenderPayGap.WebUI.Classes
{
    public static class Extensions
    {
        #region IPrinciple
        public static string GetClaim(this IPrincipal principal, string claimType)
        {
            if (principal == null || !principal.Identity.IsAuthenticated) return null;

            var claims = (principal as ClaimsPrincipal).Claims;

            //Use this to lookup the long UserID from the db - ignore the authProvider for now
            var claim = claims.FirstOrDefault(c => c.Type.ToLower() == claimType.ToLower());
            return claim == null ? null : claim.Value;
        }

        public static long GetUserId(this IPrincipal principal)
        {
            return principal.GetClaim(Constants.ClaimTypes.Subject).ToLong();
        }

        #endregion

        #region User Entity

        public static bool IsAdministrator(this User user)
        {
            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");
            if (string.IsNullOrWhiteSpace(MvcApplication.AdminEmails)) throw new ArgumentException("Missing AdminEmails from web.config");
            return user.EmailAddress.LikeAny(MvcApplication.AdminEmails.SplitI(";"));
        }

        public static UserOrganisation GetUserOrg(this IRepository repository, User user)
        {
            return repository.GetAll<UserOrganisation>().FirstOrDefault(uo=>uo.UserId==user.UserId);
        }

        public static User FindUser(this IRepository repository, IPrincipal principal)
        {
            //GEt the logged in users identifier
            var userId = principal.GetUserId();

            //If internal user the load it using the identifier as the UserID
            if (userId > 0) return repository.GetAll<User>().FirstOrDefault(u=>u.UserId==userId);

            return null;
        }

        public static User FindUserByEmail(this IRepository repository, string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))throw new ArgumentNullException("emailAddress");
            
            //If internal user the load it using the identifier as the UserID
            return repository.GetAll<User>().FirstOrDefault(u => u.EmailAddress == emailAddress);
        }

        public static User FindUserByVerifyCode(this IRepository repository, string verifyCode)
        {
            if (string.IsNullOrWhiteSpace(verifyCode)) throw new ArgumentNullException("verifyCode");

            //If internal user the load it using the identifier as the UserID
            var verifyHash = verifyCode.GetSHA512Checksum();
            return repository.GetAll<User>().FirstOrDefault(u => u.EmailVerifyHash == verifyHash);
        }

        public static UserOrganisation FindUserOrganisation(this IRepository repository, IPrincipal principal)
        {
            //GEt the logged in users identifier
            var userId = principal.GetUserId();

            //If internal user the load it using the identifier as the UserID
            if (userId > 0)return repository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == userId);

            return null;
        }

        #endregion

        #region Registraion Helpers

        public static bool SendVerifyEmail(this RegisterController controller, string emailAddress, string verifyCode)
        {
            var verifyUrl=controller.Url.Action("VerifyEmail", "Register", new {code= verifyCode },"https");
            return GovNotifyAPI.SendVerifyEmail(verifyUrl,emailAddress, verifyCode);
        }
 
        public static bool SendPinInPost(this RegisterController controller, UserOrganisation userOrg, string pin, DateTime sendDate)
        {
            var returnUrl = controller.Url.Action("/", "Submit",null,"https");

            var imagePath = new System.UriBuilder(controller.Request.Url.AbsoluteUri){Path = controller.Url.Content(@"~/Content/img/")}.Uri.ToString();

            if (GovNotifyAPI.ManualPip) return GovNotifyAPI.SendPinInPostManual(imagePath,returnUrl, userOrg.User.Fullname, userOrg.User.JobTitle, userOrg.Organisation.OrganisationName, userOrg.Address.GetList(), pin, sendDate, sendDate.AddDays(Properties.Settings.Default.PinInPostExpiryDays));
            return GovNotifyAPI.SendPinInPost(imagePath,returnUrl, userOrg.User.Fullname, userOrg.User.JobTitle, userOrg.Organisation.OrganisationName, userOrg.Address.GetList(), pin, sendDate, sendDate.AddDays(Properties.Settings.Default.PinInPostExpiryDays));
        }
        #endregion

        #region AntiSpam
        public static IHtmlString SpamProtectionTimeStamp(this HtmlHelper helper)
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("id", "SpamProtectionTimeStamp");
            builder.MergeAttribute("name", "SpamProtectionTimeStamp");
            builder.MergeAttribute("type", "hidden");
            builder.MergeAttribute("value", Encryption.EncryptData(DateTime.Now.ToSmallDateTime()));
            return new MvcHtmlString(builder.ToString(TagRenderMode.SelfClosing));
        }
        #endregion

        #region Authentication
        public static void ExpireAllCookies(this HttpContextBase context)
        {
            int cookieCount = context.Request.Cookies.Count;
            for (var i = 0; i < cookieCount; i++)
            {
                var cookie = context.Request.Cookies[i];
                if (cookie != null)
                {
                    var cookieName = cookie.Name;
                    var expiredCookie = new HttpCookie(cookieName) { Expires = DateTime.Now.AddDays(-1) };
                    context.Response.Cookies.Add(expiredCookie); // overwrite it
                }
            }

            // clear cookies server side
            context.Request.Cookies.Clear();
        }

        #endregion

        #region Session Handling

        public static void StashModel<T>(this Controller controller, T model)
        {
            controller.Session[controller + ":Model"] = model;
        }
        public static void ClearStash(this Controller controller)
        {
            controller.Session.Remove(controller + ":Model");
        }

        public static T UnstashModel<T>(this Controller controller, bool delete = false) where T : class
        {
            var result = controller.Session[controller + ":Model"] as T;
            if (delete) controller.Session.Remove(controller + ":Model");
            return result;
        }

        #endregion

        #region Helpers

        public static void CleanModelErrors<TModel>(this Controller controller)
        {
            var containerType = typeof(TModel);
            //Save the old modelstate
            var oldModelState = new ModelStateDictionary();
            foreach (var modelState in controller.ModelState)
            {
                var propertyName = modelState.Key;
                foreach (var error in modelState.Value.Errors)
                {
                    var exists = oldModelState.Any(m => m.Key == propertyName && m.Value.Errors.Any(e => e.ErrorMessage ==error.ErrorMessage));

                    //add the inline message if it doesnt already exist
                    if (!exists)oldModelState.AddModelError(propertyName, error.ErrorMessage);
                }
            }
            //Clear the model state ready for refill
            controller.ModelState.Clear();

            foreach (var modelState in oldModelState)
            {
                //Get the property name
                var propertyName = modelState.Key;

                //Get the validation attributes
                var propertyInfo = string.IsNullOrWhiteSpace(propertyName) ? null : containerType.GetPropertyInfo(propertyName);
                var attributes = propertyInfo == null ? null : propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), false).ToList<ValidationAttribute>();

                //Get the display name
                var displayAttribute = propertyInfo==null ? null : propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                var displayName = displayAttribute == null ? propertyName : displayAttribute.Name;


                foreach (var error in modelState.Value.Errors)
                { 
                    var title = string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;
                    var description = !string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;

                    if (error.ErrorMessage.Like("The value * is not valid for *."))
                    {
                        title = "There is a problem with your values.";
                        description = "The value here is invalid.";
                    }

                    if (attributes == null || !attributes.Any()) goto addModelError;

                    var attribute = attributes.FirstOrDefault(a => a.FormatError(a.GetErrorString(),displayName) == error.ErrorMessage);
                    if (attribute == null) goto addModelError;
                    var validatorKey = $"{containerType.Name}.{propertyName}:{attribute.GetType().Name.TrimSuffix("Attribute")}";
                    var customError = CustomErrorMessages.GetValidationError(validatorKey);
                    if (customError == null) goto addModelError;

                    title = attribute.FormatError(customError.Title, displayName);
                    description = attribute.FormatError(customError.Description, displayName);

                addModelError:

                    //add the summary message if it doesnt already exist
                    if (!string.IsNullOrWhiteSpace(title) && !controller.ModelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                        controller.ModelState.AddModelError("", title);

                    //add the inline message if it doesnt already exist
                    if (!string.IsNullOrWhiteSpace(description) && !string.IsNullOrWhiteSpace(propertyName) && !controller.ModelState.Any(m => m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                        controller.ModelState.AddModelError(propertyName, description);
                }
            }
        }

        public static string GetErrorString(this ValidationAttribute attribute)
        {
            string errorString = Misc.GetPropertyValue(attribute, "ErrorMessageString") as string;
            if (string.IsNullOrWhiteSpace(errorString)) errorString = attribute.ErrorMessage;
            return errorString;
        }

        public static string FormatError(this ValidationAttribute attribute, string error, string displayName)
        {
            if (string.IsNullOrWhiteSpace(error)) return error;

            string par1 = null;
            string par2 = null;

            if (attribute is RangeAttribute)
            {
                par1 = ((RangeAttribute)attribute).Minimum.ToString();
                par2 = ((RangeAttribute)attribute).Maximum.ToString();
            }
            else if (attribute is MinLengthAttribute)
            {
                par1 = ((MinLengthAttribute)attribute).Length.ToString();
            }
            else if (attribute is MaxLengthAttribute)
            {
                par1 = ((MaxLengthAttribute)attribute).Length.ToString();
            }
            else if (attribute is StringLengthAttribute)
            {
                par1 = ((StringLengthAttribute)attribute).MinimumLength.ToString();
                par2 = ((StringLengthAttribute)attribute).MaximumLength.ToString();
            }
            return string.Format(error, displayName, par1, par2);
        }

        public static void AddModelError(this BaseController controller, string errorContext,string propertyName=null, object parameters=null)
        {
            //Try and get the custom error
            var validatorKey = $"{controller.ControllerName.TrimSuffix("Controller")}/{controller.ActionName}:{errorContext}" +(string.IsNullOrWhiteSpace(propertyName) ? null : $":{propertyName}");
            var customError = CustomErrorMessages.GetValidationError(validatorKey);
            if (customError == null) throw new ArgumentException("errorContext", "Cannot find custom error message for this context" + (string.IsNullOrWhiteSpace(propertyName) ? null :" and property"));

            //Add the error to the modelstate
            var title = customError.Title;
            var description = customError.Description;

            //Bind the parameters
            if (parameters != null)
            {
                title = parameters.Format(title);
                description = parameters.Format(description);
            }

            //add the summary message if it doesnt already exist
            if (!string.IsNullOrWhiteSpace(title) && !controller.ModelState.Any(m=>m.Key=="" && m.Value.Errors.Any(e=>e.ErrorMessage==title)))
                controller.ModelState.AddModelError("", title);
            
            //add the inline message if it doesnt already exist
            if (!string.IsNullOrWhiteSpace(description) && !controller.ModelState.Any(m => m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                controller.ModelState.AddModelError(propertyName, description);
        }

        //Removes all but the specified properties from the model state
        public static void Include(this ModelStateDictionary modelState, params string[] properties)
        {
            foreach (var key in modelState.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (properties.ContainsI(key)) continue;
                modelState.Remove(key);
            }
        }

        //Removes all the specified properties from the model state
        public static void Exclude(this ModelStateDictionary modelState, params string[] properties)
        {
            foreach (var key in modelState.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (!properties.ContainsI(key)) continue;
                modelState.Remove(key);
            }
        }

        public static void AddModelError(this ModelStateDictionary modelState, int errorCode, string propertyName=null,object parameters = null)
        {
            //Try and get the custom error
            var customError = CustomErrorMessages.GetError(errorCode);
            if (customError == null) throw new ArgumentException("errorCode", "Cannot find custom error message with this code");

            //Add the error to the modelstate
            var title = customError.Title;
            var description = customError.Description;

            //Resolve the parameters
            if (parameters != null)
            {
                title = parameters.Format(title);
                description = parameters.Format(description);
            }

            //add the summary message if it doesnt already exist
            if (!string.IsNullOrWhiteSpace(title) && !modelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                modelState.AddModelError("", title);

            if (!string.IsNullOrWhiteSpace(description))
            {
                //If no property then add description as second line of summary
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    if (!string.IsNullOrWhiteSpace(title) && !modelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                        modelState.AddModelError("", title);
                }

                //add the inline message if it doesnt already exist
                else if (!modelState.Any(m => m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                    modelState.AddModelError(propertyName, description);
            }
        }

        #endregion
        public static string ResolveUrl(this Controller controller,RedirectToRouteResult redirectToRouteResult)
        {
            return controller.Url.RouteUrl(redirectToRouteResult.RouteName, redirectToRouteResult.RouteValues);
        }
    }
}