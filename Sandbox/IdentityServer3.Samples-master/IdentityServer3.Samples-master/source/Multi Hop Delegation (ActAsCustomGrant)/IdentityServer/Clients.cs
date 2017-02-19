﻿using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    static class Clients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Console Client Application",
                    ClientId = "ConsoleApplication",
                    Enabled = true,
                    Flow = Flows.ResourceOwner,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())
                    },
                    AllowedScopes = new List<string>
                    {
                        "WebApi1"
                    }
                },
                new Client
                {
                    ClientName = "WebApi1 Application",
                    ClientId = "WebApi1",
                    Enabled = true,
                    Flow = Flows.Custom,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("4B79A70F-3919-435C-B46C-571068F7AF37".Sha256())
                    },
                    AllowedCustomGrantTypes = new List<string>
                    {
                        "act-as"
                    },
                    AllowedScopes = new List<string>
                    {
                        "WebApi2"
                    }
                },
                new Client 
                {
                    ClientName = "WPF Client",
                    ClientId = "hybridclient",
                    ClientSecrets = new List<Secret>
                    { 
                        new Secret("secret".Sha256())
                    },
                    Flow = Flows.Hybrid,
                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.OfflineAccess,

                        "WebApi1"
                    },
                    
                    ClientUri = "https://identityserver.io",
                    LogoUri = "data:image/png;base64,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",
                    RequireConsent = true,
                    AllowRememberConsent = true,                    
                    RedirectUris = new List<string>
                    {
                        "oob://localhost/wpfclient"
                    }
                }
            };
        }
    }
}