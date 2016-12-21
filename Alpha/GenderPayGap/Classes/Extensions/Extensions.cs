using GenderPayGap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Extensions
{
    public static class Other
    {
        public static string GetUserIdentifier(this IPrincipal principal)
        {
            if (principal == null || !principal.Identity.IsAuthenticated) return null;

            var claims = (principal as ClaimsPrincipal).Claims;

            //Use this to lookup the long UserID from the db - ignore the authProvider for now
            return claims.FirstOrDefault().Value;
        }

        public static GenderPayGap.Models.GpgDatabase.User FindCurrentUser(this IPrincipal principal)
        {
            //GEt the logged in users identifier
            var userIdentifier = principal.GetUserIdentifier();
            if (string.IsNullOrWhiteSpace(userIdentifier)) return null;

            //If internal user the load it using the identifier as the UserID
            long userId = userIdentifier.ToLong();
            if (userId > 0) return MvcApplication.Database.User.Find(userId);

            //TODO If external user the load it using the identifier
            return null;
        }

        public static bool IsUrl(this string url)
        {
            try
            {
                if (!url.StartsWithI("http:") && !url.StartsWithI("https:") && !url.StartsWithI("file:")) return false;
                var uri = new Uri(url, UriKind.Absolute);
                return uri.IsAbsoluteUri;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsLong(this string text)
        {
            long value;
            return long.TryParse(text, out value);
        }

        public static long ToLong(this string text)
        {
            long value;
            long.TryParse(text, out value);
            return value;
        }

    }
}