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
            return repository.GetAll<User>().FirstOrDefault(u => u.EmailVerifyCode == verifyCode);
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
            var verifyUrl=controller.Url.Action("Step2", "Register", new {code= verifyCode },"https");
            return GovNotifyAPI.SendVerifyEmail(verifyUrl,emailAddress, verifyCode);
        }
        public static bool SendConfirmEmail(this RegisterController controller, string emailAddress)
        {
            var confirmUrl = controller.Url.Action("ConfirmPIN", "Register", null,"https");
            return GovNotifyAPI.SendConfirmEmail(confirmUrl, emailAddress);
        }

        public static bool SendPinInPost(this RegisterController controller, User user, Organisation organisation, string pin)
        {
            var name = user.Fullname + " (" + user.JobTitle + ")";
            var address = organisation.Address.GetAddress();
            var returnUrl = controller.Url.Action("Step6",null,null,"https");
            return GovNotifyAPI.SendPinInPost(returnUrl, name, user.EmailAddress, pin);
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
    }
}