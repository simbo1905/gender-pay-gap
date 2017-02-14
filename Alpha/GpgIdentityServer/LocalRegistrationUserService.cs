using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using GpgDB.Models.GpgDatabase;
using System.Configuration;

namespace GpgIdentityServer
{
    public class LocalRegistrationUserService : UserServiceBase
    {
        public class ExternalUser
        {
            public string Subject { get; set; }
            public string Provider { get; set; }
            public string ProviderID { get; set; }
            public bool IsRegistered { get; set; }
            public List<Claim> Claims { get; set; }
        }

        public class LocalUser
        {
            public string Subject { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public List<Claim> Claims { get; set; }
        }
        
        public static List<LocalUser> InternalUsers = new List<LocalUser>();
        public static List<ExternalUser> ExternalUsers = new List<ExternalUser>();

        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            InternalUsers = LoadInternal();
            //var user = InternalUsers.SingleOrDefault(x => x.Username == context.UserName && x.Password == context.Password);
            var user = InternalUsers.FirstOrDefault(x => x.Username == context.UserName && x.Password == context.Password);
            if (user != null)
            {
                //user.Claims.Add(new Claim(Constants.ClaimTypes.IdentityProvider, "GPG"));
                //user.Claims.Add(new Claim(Constants.ClaimTypes.Subject, user.Subject));
                //context.AuthenticateResult = new AuthenticateResult(user.Subject, user.Username,claims:user.Claims,identityProvider:"GPG");
                context.AuthenticateResult = new AuthenticateResult(user.Subject, user.Username);
            }

            return Task.FromResult(0);
        }

        public override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            var emailClaim = context.ExternalIdentity.Claims.First(x => x.Type == Constants.ClaimTypes.Email);
            if (emailClaim == null) return Task.FromResult(0);

            //InternalUsers = LoadInternal();
            //var internalUser = InternalUsers.SingleOrDefault(x => x.Username == emailClaim.Value.ToLower());
            //if (internalUser != null)
            //{
            //    context.AuthenticateResult = new AuthenticateResult(internalUser.Subject, internalUser.Username);
            //    return Task.FromResult(0);
            //}

            // look for the user in our local identity system from the external identifiers
            var user = ExternalUsers.SingleOrDefault(x => x.Provider == context.ExternalIdentity.Provider && x.ProviderID == context.ExternalIdentity.ProviderId);
            string name = "Unknown";
            if (user == null)
            {
                // new user, so add them here
                
                user = new ExternalUser
                {
                    Subject = Guid.NewGuid().ToString(),
                    Provider = context.ExternalIdentity.Provider,
                    ProviderID = context.ExternalIdentity.ProviderId,
                    Claims = new List<Claim>(context.ExternalIdentity.Claims)
                };
                user.Claims.Add(new Claim(Constants.ClaimTypes.IdentityProvider, context.ExternalIdentity.Provider));
                user.Claims.Add(new Claim(Constants.ClaimTypes.ExternalProviderUserId, context.ExternalIdentity.ProviderId));
                ExternalUsers.Add(user);

                var newUser = new GpgDB.Models.GpgDatabase.User();
                var nameClaim = context.ExternalIdentity.Claims.First(x => x.Type == Constants.ClaimTypes.GivenName);
                if (nameClaim != null) newUser.Firstname = nameClaim.Value;

                nameClaim = context.ExternalIdentity.Claims.First(x => x.Type == Constants.ClaimTypes.FamilyName);
                if (nameClaim != null) newUser.Lastname = nameClaim.Value;

                newUser.EmailAddress = emailClaim.Value;
                newUser.EmailVerifiedDate = DateTime.Now;

                var token = new UserToken()
                {
                    AuthProviderId = context.ExternalIdentity.Provider,
                    TokenIdentifier = context.ExternalIdentity.ProviderId,
                    Created = DateTime.Now
                };

            }

            name = user.Claims.First(x => x.Type == Constants.ClaimTypes.Name).Value;
            context.AuthenticateResult = new AuthenticateResult(user.Subject, name, claims: user.Claims, identityProvider: user.Provider);

            //if (user.IsRegistered)
            //{
            //    // user is registered so continue
            //    context.AuthenticateResult = new AuthenticateResult(user.Subject, name, claims: context.ExternalIdentity.Claims, identityProvider: user.Provider);
            //}
            //else
            //{
            //    // user not registered so we will issue a partial login and redirect them to our registration page
            //    context.AuthenticateResult = new AuthenticateResult(ConfigurationManager.AppSettings["GpgWebServerRegister"], user.Subject, name,claims: context.ExternalIdentity.Claims, identityProvider: user.Provider);
            //}

            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // issue the claims for the user
            var internaluser = InternalUsers.SingleOrDefault(x => x.Subject == context.Subject.GetSubjectId());
            // issue the claims for the user
            var user = InternalUsers.SingleOrDefault(x => x.Subject == context.Subject.GetSubjectId());
            if (user != null)
            {
                context.IssuedClaims = user.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            }

            return Task.FromResult(0);
            if (internaluser != null)
            {
                context.IssuedClaims = internaluser.Claims;
            }
            else
            {
                var externaluser = ExternalUsers.SingleOrDefault(x => x.Subject == context.Subject.GetSubjectId());
                if (externaluser != null)
                {
                    context.IssuedClaims = externaluser.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
                    context.IssuedClaims = externaluser.Claims;
                }
            }

            return Task.FromResult(0);
        }

        private List<LocalUser> LoadInternal()
        {
            //TODO Load users from database
            var users = new List<LocalUser>();
            GpgDatabase.RefreshAll();

            foreach (var user in GpgDatabase.Default.User)
            {
                users.Add(new LocalUser
                {
                    Username = user.EmailAddress,
                    Password = user.Password,
                    Subject = user.UserId.ToString(),
                    Name=user.Fullname,
                    Claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.Subject, user.UserId.ToString()),
                        new Claim(Constants.ClaimTypes.GivenName, user.Firstname),
                        new Claim(Constants.ClaimTypes.FamilyName, user.Lastname),
                        new Claim(Constants.ClaimTypes.Role, "Customer")
                    }
                });
            }
            return users;
        }
        private List<ExternalUser> LoadExternal()
        {
            //TODO Load users from database
            var users = new List<ExternalUser>();
            GpgDatabase.RefreshAll();
            foreach (var userToken in GpgDatabase.Default.UserTokens)
            {
                var user = GpgDatabase.Default.User.Find(userToken.UserId);
                if (user == null) continue;
                users.Add(new ExternalUser
                {
                    IsRegistered = true,
                    Provider=userToken.AuthProviderId.ToString(),
                    ProviderID=userToken.TokenIdentifier,
                    Subject = userToken.UserId.ToString(),

                    Claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.GivenName, user.Firstname),
                        new Claim(Constants.ClaimTypes.FamilyName, user.Lastname),
                        new Claim(Constants.ClaimTypes.Role, "Customer")
                    }
                });
            }
            return users;
        }
    }
}
