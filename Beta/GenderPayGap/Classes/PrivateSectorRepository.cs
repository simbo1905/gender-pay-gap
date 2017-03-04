using System;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Classes;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap
{
    public class PrivateSectorRepository: IPagedRepository<EmployerRecord>
    {
        const Models.SqlDatabase.SectorTypes Type = Models.SqlDatabase.SectorTypes.Private;

        public void Delete(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public PagedResult<EmployerRecord> Search(string searchText, int page, int pageSize)
        {
            int totalRecords;
            var searchResults = CompaniesHouseAPI.SearchEmployers(out totalRecords, searchText, page, pageSize);
            var result = new PagedResult<EmployerRecord>();
            result.RowCount = totalRecords;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            result.Results = searchResults;
            return result;
        }
        
    }
}