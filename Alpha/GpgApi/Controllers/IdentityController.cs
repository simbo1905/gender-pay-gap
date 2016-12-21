using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api
{
    [Route("identity")]
    [Authorize]
    public class IdentityController : ApiController
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult Get()
        {
            var user = User as ClaimsPrincipal;
            var claims = from c in user.Claims
                         select new
                         {
                             type = c.Type,
                             value = c.Value
                         };

            return Json(claims);
        }
    }
}