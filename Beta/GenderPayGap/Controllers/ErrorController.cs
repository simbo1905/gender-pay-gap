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
    }
}