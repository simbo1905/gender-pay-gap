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
using Extensions;

namespace GpgIdentityServer
{
    public class LocalRegistrationUserService : UserServiceBase
    {
        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            //var user = InternalUsers.SingleOrDefault(x => x.Username == context.UserName && x.Password == context.Password);

            var username = context.UserName.ToLower();

            var dbContext = new DbContext();
            var user = dbContext.User.FirstOrDefault(x => x.EmailAddress == username);

            if (user != null)
            {
                var remaining = user.LoginDate==null ? TimeSpan.Zero : user.LoginDate.Value.AddMinutes(Properties.Settings.Default.LockoutMinutes) - DateTime.Now;
                if (user.LoginAttempts >=Properties.Settings.Default.MaxLoginAttempts && remaining > TimeSpan.Zero)
                {
                    context.AuthenticateResult = new AuthenticateResult("You have failed too many login attempts. Please try again in " + remaining.ToFriendly(maxParts:2));
                }
                else if (user.PasswordHash == context.Password.GetSHA512Checksum())
                {
                    context.AuthenticateResult = new AuthenticateResult(user.UserId.ToString(), user.Fullname,new [] {new Claim(Constants.ClaimTypes.Subject, user.UserId.ToString()), });
                    user.LoginAttempts=0;
                }
                else
                {
                    context.AuthenticateResult = new AuthenticateResult("Invalid username or password.");
                    user.LoginAttempts++;
                }
                user.LoginDate = DateTime.Now;
                dbContext.SaveChanges();
            }
            else
                context.AuthenticateResult = new AuthenticateResult("Invalid username or password.");

            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            //Issue the requested claims for the user
            context.IssuedClaims = context.Subject.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));

            return Task.FromResult(0);
        }
    }
}
