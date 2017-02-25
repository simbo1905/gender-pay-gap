using System;
using System.Collections.Generic;
using System.Linq;
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
            throw new HttpException(429,"Too Many Requests");
        }
    }
}