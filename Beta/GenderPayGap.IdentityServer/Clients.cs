using IdentityServer3.Core.Models;
using System.Collections.Generic;
using System.Configuration;
using Extensions;

namespace GenderPayGap.IdentityServer
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client 
                {
                    ClientName = "Gender pay gap reporting service",
                    ClientId = "gpgWeb",
                    Flow = Flows.Implicit,
                    RequireConsent = false,
                    RedirectUris = new List<string>
                    {
                        ConfigurationManager.AppSettings["GpgWebServer"].TrimI("/")+"/"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        ConfigurationManager.AppSettings["GpgWebServer"].TrimI("/")+"/Submit/enter-calculations",
                        ConfigurationManager.AppSettings["DoneUrl"],
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        "roles",
                        ConfigurationManager.AppSettings["GpgApiScope"]
                    }
                }
           };
        }
    }
}