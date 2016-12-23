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
using GenderPayGap.Models.GpgDatabase;

namespace GpgIdentityServer
{
    public class LocalRegistrationUserService : UserServiceBase
    {
        public static GpgDatabase Database = new GpgDatabase();

        public class CustomUser
        {
            public string Subject { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public List<Claim> Claims { get; set; }
        }
        
        public static List<CustomUser> Users = new List<CustomUser>();

        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            Users = Load();
            var user = Users.SingleOrDefault(x => x.Username == context.UserName && x.Password == context.Password);
            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Subject, user.Username);
            }

            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // issue the claims for the user
            var user = Users.SingleOrDefault(x => x.Subject == context.Subject.GetSubjectId());
            if (user != null)
            {
                context.IssuedClaims = user.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            }

            return Task.FromResult(0);
        }

        private List<CustomUser> Load()
        {
            //TODO Load users from database
            var users = new List<CustomUser>();
            foreach (var user in Database.User)
            {
                users.Add(new CustomUser
                {
                    Username = user.EmailAddress,
                    Password = user.Password,
                    Subject = user.UserId.ToString(),

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
