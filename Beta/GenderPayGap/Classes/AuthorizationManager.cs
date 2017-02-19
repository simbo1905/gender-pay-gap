using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Owin.ResourceAuthorization;

namespace GenderPayGap
{
    public class AuthorizationManager : ResourceAuthorizationManager
    {
        public override Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
        {
            switch (context.Resource.First().Value)
            {
                case "Submit":
                    return AuthoriseReturn(context);
                default:
                    return Nok();
            }
        }

        private Task<bool> AuthoriseReturn(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "Read":
                    return Ok();
                case "Submit":
                    //var user = User.FindCurrentUser(context.Principal);
                    //if (user == null || user.EmailVerifiedDate == null || user.EmailVerifiedDate == DateTime.MinValue) return Nok();
                    //var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(u => u.UserId == user.UserId);
                    //if (userOrg == null || userOrg.PINConfirmedDate==null || userOrg.PINConfirmedDate==DateTime.MinValue) return Nok();
                    return Ok();
            }
            return Nok();
        }
    }
}