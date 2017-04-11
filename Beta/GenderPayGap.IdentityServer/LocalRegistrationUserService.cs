using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using GenderPayGap.Database;
using Extensions;

namespace GenderPayGap.IdentityServer
{
    public class LocalRegistrationUserService : UserServiceBase
    {
        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            //var user = InternalUsers.SingleOrDefault(x => x.Username == context.UserName && x.Password == context.Password);

            var username = context.UserName.ToLower();
            try
            {
                var dbContext = new DbContext();
                User user = null;
                var encryptedUsername = MvcApplication.EncryptEmails && !string.IsNullOrWhiteSpace(username) ? Encryption.EncryptData(username) : null;
                if (MvcApplication.EncryptEmails && !username.StartsWithI(MvcApplication.TestPrefix))
                {
                    user = dbContext.User.FirstOrDefault(x => x.EmailAddressDB == encryptedUsername);
                    if (user == null) user = dbContext.User.FirstOrDefault(x => x.EmailAddressDB == username);
                }
                else
                {
                    user = dbContext.User.FirstOrDefault(x => x.EmailAddressDB == username);
                    if (user == null) user = dbContext.User.FirstOrDefault(x => x.EmailAddressDB == encryptedUsername);
                }

                if (user != null)
                {
                    var remaining = user.LoginDate == null ? TimeSpan.Zero : user.LoginDate.Value.AddMinutes(Properties.Settings.Default.LockoutMinutes) - DateTime.Now;
                    if (user.LoginAttempts >= Properties.Settings.Default.MaxLoginAttempts && remaining > TimeSpan.Zero)
                    {
                        context.AuthenticateResult = new AuthenticateResult("Too many failed sign in attempts. Please try again in " + remaining.ToFriendly(maxParts: 2));
                    }
                    else if (user.PasswordHash != context.Password.GetSHA512Checksum())
                    {
                        context.AuthenticateResult = new AuthenticateResult("Please enter your email address and password again.");
                        user.LoginAttempts++;
                    }
                    else if (IsAdministrator(user) && !IsTrustedIP())
                    {
                        context.AuthenticateResult = new AuthenticateResult("Administrators are not permitted to sign in from your network location.");
                        user.LoginAttempts++;
                    }
                    else 
                    {
                        context.AuthenticateResult = new AuthenticateResult(user.UserId.ToString(), user.Fullname, new[] { new Claim(Constants.ClaimTypes.Subject, user.UserId.ToString()), });
                        user.LoginAttempts = 0;
                    }

                    user.LoginDate = DateTime.Now;
                    dbContext.SaveChanges();
                }
                else
                    context.AuthenticateResult = new AuthenticateResult("Please enter your email address and password again.");

            }
            catch (Exception ex)
            {
                MvcApplication.ErrorLog.WriteLine(ex.Message);
                throw;
            }

            return Task.FromResult(0);
        }

        public static bool IsAdministrator(User user)
        {
            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");
            if (string.IsNullOrWhiteSpace(MvcApplication.AdminEmails)) throw new ArgumentException("Missing AdminEmails from web.config");
            return user.EmailAddress.LikeAny(MvcApplication.AdminEmails.SplitI(";"));
        }

        public static bool IsTrustedIP()
        {
            return string.IsNullOrWhiteSpace(MvcApplication.TrustedIPDomains) || HttpContext.Current.Request.UserHostAddress.IsTrustedAddress(MvcApplication.TrustedIPDomains.SplitI());
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            //Issue the requested claims for the user
            context.IssuedClaims = context.Subject.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));

            return Task.FromResult(0);
        }
    }
}
