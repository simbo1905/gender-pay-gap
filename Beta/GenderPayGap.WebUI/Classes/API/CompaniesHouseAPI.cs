﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Models.SqlDatabase;
using GenderPayGap.WebUI.Classes;
using Microsoft.Owin.Security.Provider;

namespace GenderPayGap
{
    public class CompaniesHouseAPI
    {

        public static List<EmployerRecord> SearchEmployers(out int totalRecords, string searchText, int page, int pageSize)
        {
            totalRecords = 0;
            var employers = new List<EmployerRecord>();
#if DEBUG || TEST
            if (!string.IsNullOrWhiteSpace(searchText) && searchText.EqualsI(ConfigurationManager.AppSettings["TESTING-SearchKeyWord"]))
            {
                totalRecords = 1;
                var repository = MvcApplication.ContainerIOC.Resolve<IRepository>();
                var min = repository.GetAll<Organisation>().Count();

                var id = Extensions.Numeric.Rand(min, int.MaxValue-1);
                var employer = new EmployerRecord();
                employer.Name = "Company_" + id;
                employer.CompanyNumber = ("_" + id).Left(10);
                employer.CompanyStatus = "active";
                employer.Address1 = $"address{id} line1";
                employer.Address2 = $"address{id} line2";
                employer.Address3 = $"locality{id}";
                employer.Country = $"country{id}";
                employer.PostCode = $"PostCode";
                employer.PoBox = null;
                employers.Add(employer);
                return employers;
            }
#endif
            Task<string> task;
            try
            {
                task = Task.Run<string>(async () => await GetCompanies(searchText, page, pageSize));

                dynamic companies = JsonConvert.DeserializeObject(task.Result);
                if (companies != null)
                {
                    totalRecords = companies.total_results;
                    if (totalRecords > 0)
                    {
                        foreach (var company in companies.items)
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
            catch (AggregateException aex)
            {
                var httpEx = aex.InnerException as HttpRequestException;
                if (httpEx != null && httpEx.Message != "Response status code does not indicate success: 404 (Not Found).") throw;
            }
            return employers;
        }

        //public static EmployerRecord GetEmployer(string companyNumber)
        //{
        //    var task = Task.Run<string>(async () => await GetCompany(companyNumber));

        //    dynamic company = JsonConvert.DeserializeObject(task.Result);
        //    if (string.IsNullOrWhiteSpace(company)) return null;

        //    var employer = new EmployerRecord
        //    {
        //        Name = company.company_name,
        //        CompanyNumber = company.company_number,
        //        Address1 = company.registered_office_address.address_line_1,
        //        Address2 = company.registered_office_address.address_line_2,
        //        Address3 = company.registered_office_address.locality,
        //        Country = company.registered_office_address.country,
        //        PostCode = company.registered_office_address.postal_code,
        //        PoBox = company.registered_office_address.po_box,
        //        SicCodes = company.sic_codes
        //    };
        //    return employer;
        //}

        public static string GetSicCodes(string companyNumber)
        {
            var codes = new HashSet<string>();

#if DEBUG || TEST
            if (companyNumber.StartsWithI("_") && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TESTING-SearchKeyWord"]))
            {
                var repository = MvcApplication.ContainerIOC.Resolve<IRepository>();
                var maxCodes = Extensions.Numeric.Rand(1, 5);

                var max = repository.GetAll<SicCode>().Count();
                while (codes.Count <= maxCodes)
                {
                    var code=repository.GetAll<SicCode>().OrderBy(s=>s.SicCodeId).Skip(Extensions.Numeric.Rand(1, max - 1)).FirstOrDefault();
                    if (code!=null)codes.Add(code.SicCodeId.ToString());
                }
                return codes.ToDelimitedString();
            }
#endif

            var task = Task.Run<string>(async () => await GetCompany(companyNumber));

            dynamic company = JsonConvert.DeserializeObject(task.Result);
            if (company==null) return null;
            if (company.sic_codes!=null)
            foreach (var code in company.sic_codes)
                codes.Add(code.Value);

            return codes.ToDelimitedString();
        }

        static async Task<string> GetCompany(string companyNumber)
        {
            var client = new HttpClient();
            client.SetBasicAuthentication(ConfigurationManager.AppSettings["CompaniesHouseApiKey"], "");
            var url = string.Format("{0}/company/{1}", ConfigurationManager.AppSettings["CompaniesHouseApiServer"], companyNumber);
            var json = await client.GetStringAsync(url);
            return json;
        }

        static async Task<string> GetCompanies(string companyName, int page, int pageSize=10)
        {
            var startIndex = (page * pageSize)-10;
            var client = new HttpClient();
            client.SetBasicAuthentication(ConfigurationManager.AppSettings["CompaniesHouseApiKey"], "");
            var url = string.Format("{0}/search/companies/?q={1}&items_per_page={2}&start_index={3}", ConfigurationManager.AppSettings["CompaniesHouseApiServer"], companyName,pageSize,startIndex);
            var json = await client.GetStringAsync(url);

            return json;
        }

    }
}