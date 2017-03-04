using System;
using System.Configuration;
using System.Linq;
using Extensions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Classes;
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

        public PagedResult<EmployerRecord> Search(string searchText, int page, int pageSize)
        {
            var searchResults = PublicSectorOrgs.Messages.List.Where(o => o.OrgName.ContainsI(searchText));
            var result = new PagedResult<EmployerRecord>();
            result.RowCount = searchResults.Count();
            result.CurrentPage = page;
            result.PageSize = pageSize;
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            result.Results = searchResults.Page(pageSize, page).Select(e=>ToEmployer(e)).ToList();
            return result;
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