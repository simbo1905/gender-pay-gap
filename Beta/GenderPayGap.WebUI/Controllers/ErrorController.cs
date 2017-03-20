using System.Web.Mvc;
using GenderPayGap.WebUI.Models;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Error")]
    [Route("{action}")]
    public class ErrorController : Controller
    {

        [HttpGet]
        [Route]
        [OutputCache(Duration = 86400, VaryByParam = "code")]
        public ActionResult Default(int code=0)
        {
            var model = new ErrorViewModel(code);
            return View("CustomError", model);
        }

        [HttpGet]
        [Route("service-unavailable")]
        [OutputCache(Duration = 86400, VaryByParam = "none")]
        public ActionResult ServiceUnavailable()
        {
            var model = new ErrorViewModel(1119);
            return View("CustomError", model);
        }
    }
}