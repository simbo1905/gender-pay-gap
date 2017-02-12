using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace GenderPayGap
{
    public static class CompaniesHouseAPI
    {
        public static dynamic Lookup(string companyNumber)
        {
            Task<string> task = Task.Run<string>(async () => await GetCompany(companyNumber));

            dynamic company = JsonConvert.DeserializeObject(task.Result);

            return company;
        }

        static async Task<string> GetCompany(string companyNumber)
        {
            var client = new HttpClient();
            client.SetBasicAuthentication(ConfigurationManager.AppSettings["CompaniesHouseApiKey"], "");
            string url = string.Format("{0}/company/{1}", ConfigurationManager.AppSettings["CompaniesHouseApiServer"], companyNumber);
            var json = await client.GetStringAsync(url);
            return json;
        }

        static async Task<List<string>> GetCompanies(string companyName, int page, int pageSize=10)
        {
            var startIndex = (page * pageSize)-10;
            var client = new HttpClient();
            client.SetBasicAuthentication(ConfigurationManager.AppSettings["CompaniesHouseApiKey"], "");
            string url = string.Format("{0}/search/companies/q={1}&items_per_page={2}&start_index={3}", ConfigurationManager.AppSettings["CompaniesHouseApiServer"], companyName,pageSize,startIndex);
            var json = await client.GetStringAsync(url);
            return json;
        }

    }
}