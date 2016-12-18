using IdentityServer3.Core.Models;
using System.Collections.Generic;
using System.Configuration;

namespace GpgIdentityServer
{
    public static class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            var scopes = new List<Scope>
            {
                new Scope
                {
                    Enabled = true,
                    Name = "roles",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim("role")
                    }
                },
                new Scope
                {
                    Enabled = true,
                    DisplayName = "GPG API",
                    Name = ConfigurationManager.AppSettings["GpgApiScope"],
                    Description = "Access to a GPG API",
                    Type = ScopeType.Resource,

                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim("role")
                    }
                }
            };
            
            scopes.AddRange(StandardScopes.All);

            return scopes;
        }
    }
}