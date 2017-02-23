using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GenderPayGap.WebUI.Classes
{
    public class AuthAttribute : AuthorizeAttribute
    {
        static List<DateTime> _IdentityRedirects = new List<DateTime>();
        public static List<DateTime> IdentityRedirects
        {
            get
            {
                var _IdentityRedirects = HttpContext.Current?.Session?["IdentityRedirects"] as List<DateTime>;
                if (_IdentityRedirects == null) _IdentityRedirects = new List<DateTime>();
                return _IdentityRedirects;
            }
            set
            {
                HttpContext.Current.Session["IdentityRedirects"] = value;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // 403 we know who you are, but you haven't been granted access
                filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }
            else if (IdentityRedirects.Count >= 10)
            {
                IdentityRedirects = null;
                filterContext.Result = new RedirectResult("/Error/?code=310");
            }
            else
            {
                var now = DateTime.Now;
                var redirects = IdentityRedirects;
                while (IdentityRedirects.Count >= 10 || (redirects.Count > 0 && redirects[0].AddSeconds(10) < now))
                    IdentityRedirects.RemoveAt(0);

                redirects.Add(now);
                IdentityRedirects = redirects;

                // 401 who are you? go login and then try again
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}