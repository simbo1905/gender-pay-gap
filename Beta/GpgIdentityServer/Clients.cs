using IdentityServer3.Core.Models;
using System.Collections.Generic;
using System.Configuration;

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
                    ClientName = "Gender Pay Gap (Alpha)",
                    ClientId = "gpgWeb",
                    Flow = Flows.Implicit,

                    RedirectUris = new List<string>
                    {
                        ConfigurationManager.AppSettings["GpgWebServer"]
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        ConfigurationManager.AppSettings["GpgWebServer"]
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
                    ClientName = "Gender Pay Gap (Alpha)",   
                    ClientId = "gpg_portal",
                    Flow = Flows.ClientCredentials,

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