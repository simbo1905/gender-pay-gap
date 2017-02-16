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
using GenderPayGap.Models.SqlDatabase;
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

        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            InternalUsers = LoadInternal();
            //var user = InternalUsers.SingleOrDefault(x => x.Username == context.UserName && x.Password == context.Password);
            var user = InternalUsers.FirstOrDefault(x => x.Username == context.UserName && x.Password == context.Password);
            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Subject, user.Username);
            }

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
        }

        private List<LocalUser> LoadInternal()
        {
            //TODO Load users from database
            var users = new List<LocalUser>();
            DbContext.RefreshAll();

            foreach (var user in DbContext.Default.User)
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
    }
}
