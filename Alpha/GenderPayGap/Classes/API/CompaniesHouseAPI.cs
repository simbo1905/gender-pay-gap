using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Extensions;
using GenderPayGap.WebUI.Models;

namespace GenderPayGap
{
    public static class CompaniesHouseAPI
    {
        public static List<EmployerRecord> SearchEmployers(out int totalRecords, string searchText, int page, int pageSize)
        {
            totalRecords = 0;
            var employers = new List<EmployerRecord>();
            Task<string> task;
            if (searchText.IsNumber())
            {

                task = Task.Run<string>(async () => await GetCompany(searchText));

                dynamic company = JsonConvert.DeserializeObject(task.Result);
                if (!string.IsNullOrWhiteSpace(company))
                {
                    var employer = new EmployerRecord();
                    employer.Name = company.company_name;
                    employer.CompanyNumber = company.company_number;
                    employer.Address1 = company.registered_office_address.address_line_1;
                    employer.Address2 = company.registered_office_address.address_line_2;
                    employer.Address3 = company.registered_office_address.locality;
                    employer.Country = company.registered_office_address.country;
                    employer.PostCode = company.registered_office_address.postal_code;
                    employer.PoBox = company.registered_office_address.po_box;
                    employers.Add(employer);
                    totalRecords = 1;
                }
            }
            else
            {
                task = Task.Run<string>(async () => await GetCompanies(searchText,page, pageSize));

                dynamic companies = JsonConvert.DeserializeObject(task.Result);
                if (companies!=null)
                {
                    totalRecords = companies.total_results;
                    if (totalRecords > 0)
                    {
                        foreach (dynamic company in companies.items)
                        {
                            var employer = new EmployerRecord();
                            employer.Name = company.title;
                            employer.CompanyNumber = company.company_number;
                            employer.CompanyStatus = company.company_status;
                            employer.Address1 = company.address.address_line_1;
                            employer.Address2 = company.address.address_line_2;
                            employer.Address3 = company.address.locality;
                            employer.Country = company.address.country;
                            employer.PostCode = company.address.postal_code;
                            employer.PoBox = company.address.po_box;
                            employers.Add(employer);
                        }
                    }
                }

            }

            return employers;
        }

        static async Task<string> GetCompany(string companyNumber)
        {
            var client = new HttpClient();
            client.SetBasicAuthentication(ConfigurationManager.AppSettings["CompaniesHouseApiKey"], "");
            string url = string.Format("{0}/company/{1}", ConfigurationManager.AppSettings["CompaniesHouseApiServer"], companyNumber);
            var json = await client.GetStringAsync(url);
            return json;
        }

        static async Task<string> GetCompanies(string companyName, int page, int pageSize=10)
        {
            var startIndex = (page * pageSize)-10;
            var client = new HttpClient();
            client.SetBasicAuthentication(ConfigurationManager.AppSettings["CompaniesHouseApiKey"], "");
            string url = string.Format("{0}/search/companies/?q={1}&items_per_page={2}&start_index={3}", ConfigurationManager.AppSettings["CompaniesHouseApiServer"], companyName,pageSize,startIndex);
            var json = await client.GetStringAsync(url);

            return json;
        }

    }
}