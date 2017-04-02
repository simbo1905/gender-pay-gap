using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Autofac;
using Extensions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap
{
    public class PublicSectorRepository: IPagedRepository<EmployerRecord>
    {
        #region Properties
        static PublicSectorOrgsSection _PublicSectorOrgs = null;
        private static PublicSectorOrgsSection PublicSectorOrgs
        {
            get
            {
                if (_PublicSectorOrgs == null) _PublicSectorOrgs = (PublicSectorOrgsSection)ConfigurationManager.GetSection("PublicSectorOrgs");
                return _PublicSectorOrgs;
            }
        }

        public void Delete(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        public PagedResult<EmployerRecord> Search(string searchText, int page, int pageSize, bool test=false)
        {
            var result = new PagedResult<EmployerRecord>();
            if (test)
            {
                var employers = new List<EmployerRecord>();
                var repository = MvcApplication.ContainerIOC.Resolve<IRepository>();
                var min = repository.GetAll<Organisation>().Count();

                var id = Extensions.Numeric.Rand(min, int.MaxValue - 1);
                var employer = new EmployerRecord
                {
                    Name = MvcApplication.TestPrefix + "_GovDept_" + id,
                    CompanyNumber = ("_" + id).Left(10),
                    Address1 = "Test Address 1",
                    Address2 = "Test Address 2",
                    Address3 = "Test Address 3",
                    Country = "Test Country",
                    PostCode = "Test Post Code",
                    EmailPatterns = "*@*",
                    PoBox = null
                };
                employers.Add(employer);

                result.RowCount = employers.Count;
                result.CurrentPage = page;
                result.PageSize = pageSize;
                result.PageCount = 1;
                result.Results = employers;
                return result;
            }

            var searchResults = PublicSectorOrgs.Messages.List.Where(o => o.OrgName.ContainsI(searchText));
            result.RowCount = searchResults.Count();
            result.CurrentPage = page;
            result.PageSize = pageSize;
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            result.Results = searchResults.Page(pageSize, page).Select(e=>ToEmployer(e)).ToList();
            return result;
        }

        public string GetSicCodes(string companyNumber)
        {
            return "1";
        }

        EmployerRecord ToEmployer(PublicSectorOrg org)
        {
            var employer = new EmployerRecord();
            employer.Name = org.OrgName;
            employer.EmailPatterns = org.EmailPatterns;
            return employer;
        }
        
    }
}