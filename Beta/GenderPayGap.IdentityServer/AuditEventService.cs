using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Extensions;
using IdentityServer3.Core.Events;
using IdentityServer3.Core.Services;

namespace GpgIdentityServer
{
    public class AuditEventService : IEventService
    {
        public Task RaiseAsync<T>(Event<T> evt)
        {
            //Authentication
            var loginDetails = evt.Details as LoginDetails;
            var logoutDetails = evt.Details as LogoutDetails;
            var tokenRevokedDetails = evt.Details as TokenRevokedDetails;
            var localLoginDetails = evt.Details as LocalLoginDetails;
            var externalLoginDetails = evt.Details as ExternalLoginDetails;
            var authenticationDetails = evt.Details as AuthenticationDetails;
            var clientAuthenticationDetails = evt.Details as ClientAuthenticationDetails;

            //Informational
            var signingCertificateDetail = evt.Details as SigningCertificateDetail;
            var cspReportDetails = evt.Details as CspReportDetails;

            //Token Service
            var accessTokenIssuedDetails = evt.Details as AccessTokenIssuedDetails;
            var authorizationCodeDetails = evt.Details as AuthorizationCodeDetails;
            var refreshTokenDetails = evt.Details as RefreshTokenDetails;
            var refreshTokenRefreshDetails = evt.Details as RefreshTokenRefreshDetails;
            var tokenIssuedDetailsBase = evt.Details as TokenIssuedDetailsBase;

            if (localLoginDetails != null)
            {
                Global.Log.WriteLine($"LOGIN ATTEMPT:{evt.EventType},REMOTEIP:{HttpContext.Current.Request.UserHostAddress}, Username:{localLoginDetails.LoginUserName},Details:{evt.Message},Name:{evt.Name}");
            }
            else if (externalLoginDetails!=null)
            {
                
            }
            else if (loginDetails != null)
            {

            }
            else if (logoutDetails != null)
            {

            }
            else if (clientAuthenticationDetails != null)
            {

            }
            else if (authenticationDetails != null)
            {

            }
            else if (tokenRevokedDetails != null)
            {

            }
            else if (signingCertificateDetail != null)
            {

            }
            else if (cspReportDetails != null)
            {

            }
            else if (accessTokenIssuedDetails != null)
            {

            }
            else if (authorizationCodeDetails != null)
            {

            }
            else if (refreshTokenDetails != null)
            {

            }
            else if (refreshTokenRefreshDetails != null)
            {

            }
            else if (tokenIssuedDetailsBase != null)
            {

            }
            else
            {
                
            }
            return Task.FromResult(0);
        }
    }
}