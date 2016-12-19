using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Notify.Client;
using Notify.Models;
using Notify.Models.Responses;
using System.Collections.Generic;

namespace EmbeddedMvc.Controllers
{
    public class CompaniesHouseController : Controller
    {
        public async Task<ActionResult> Lookup()
        {
            var result = await GetCompany("04394391");

            ViewBag.Json = result;
            return View("ShowApiResult");
        }

        private async Task<string> GetCompany(string companyNumber)
        {
            var client = new HttpClient();
            client.SetBasicAuthentication(ConfigurationManager.AppSettings["CompaniesHouseApiKey"], "");
            string url= string.Format("{0}/company/{1}", ConfigurationManager.AppSettings["CompaniesHouseApiServer"],companyNumber);
            var json = await client.GetStringAsync(url);
            return json;
        }
    }
}