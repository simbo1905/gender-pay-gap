using IdentityServer3.Core.Models;
using System.Collections.Generic;
using System.Configuration;
using Extensions;

namespace GpgIdentityServer
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client 
                {
                    ClientName = "Gender Pay Gap (Beta)",
                    ClientId = "gpgWeb",
                    Flow = Flows.Implicit,
                    RequireConsent = false,
                    RedirectUris = new List<string>
                    {
                        ConfigurationManager.AppSettings["GpgWebServer"].TrimI("/")+"/"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        ConfigurationManager.AppSettings["GpgWebServer"].TrimI("/")+"/"
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        "roles",
                        ConfigurationManager.AppSettings["GpgApiScope"]
                    }
                },
                new Client
                {
                    ClientName = "Gender Pay Gap (Beta)",   
                    ClientId = "gpg_portal",
                    Flow = Flows.ClientCredentials,
                    RequireConsent = false,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = new List<string>
                    {
                        ConfigurationManager.AppSettings["GpgApiScope"]
                    }
                }
            };
        }
    }
}