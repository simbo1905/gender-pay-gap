using System;
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
    public class PrivateSectorRepository: IPagedRepository<EmployerRecord>
    {
        const SectorTypes Type = SectorTypes.Private;

        public void Delete(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public PagedResult<EmployerRecord> Search(string searchText, int page, int pageSize, bool test=false)
        {
            int totalRecords;
            var searchResults = CompaniesHouseAPI.SearchEmployers(out totalRecords, searchText, page, pageSize, test);
            var result = new PagedResult<EmployerRecord>();
            result.RowCount = totalRecords;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            result.Results = searchResults;
            return result;
        }

        public string GetSicCodes(string companyNumber)
        {
            return CompaniesHouseAPI.GetSicCodes(companyNumber);
        }
    }
}