using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Extensions;

namespace GenderPayGap.WebUI.Classes
{
    public class SpamProtectionAttribute : FilterAttribute, IAuthorizationFilter
    {
        public SpamProtectionAttribute(int minimumSeconds = 10)
        {
            _minimumSeconds = minimumSeconds;
        }

        private readonly int _minimumSeconds;

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var remoteTime = DateTime.MinValue;

            try
            {
                remoteTime = Encryption.DecryptData(filterContext.RequestContext.HttpContext.Request.Params["SpamProtectionTimeStamp"]).FromSmallDateTime(true);
                if (remoteTime.AddSeconds(_minimumSeconds) < DateTime.Now) return;
            }
            catch (Exception ex)
            {
            }

#if DEBUG || TEST
            if (ConfigurationManager.AppSettings["TESTING-SkipSpamProtection"].ToBoolean()) return;
#endif
            throw new HttpException(429,"Too Many Requests");
        }
    }
}