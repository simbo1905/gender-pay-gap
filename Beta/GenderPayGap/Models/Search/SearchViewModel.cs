using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Models.SqlDatabase;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Models.Search
{
    [Serializable]
    public class SearchViewModel
    {
        public SearchViewModel()
        {

        }

        [Serializable]
        public partial class SicSection
        {
            public SicSection()
            {
            }

            public string SicSectionId { get; set; }

            public string Description { get; set; }
        }

        public string SearchText { get; set; }

        public int Year { get; set; }
        
        // This property contains the available options
        public IEnumerable<SicSection> AllSectors { get; set; }

        // This property contains the selected options
        public IEnumerable<string> NewSectors { get; set; }
        
        public List<Core.Classes.SelectedItem> SectorSources { get; set; }

        public PagedResult<EmployerRecord> Employers { get; set; }

        public int EmployerStartIndex
        {
            get
            {
                if (Employers == null || Employers.Results == null || Employers.Results.Count < 1) return 1;
                return ((Employers.CurrentPage * Employers.PageSize) - Employers.PageSize) + 1;
            }
        }
        public int EmployerEndIndex
        {
            get
            {
                if (Employers == null || Employers.Results == null || Employers.Results.Count < 1) return 1;
                return EmployerStartIndex + Employers.Results.Count - 1;
            }
        }
        public int PagerStartIndex
        {
            get
            {
                if (Employers == null || Employers.PageCount <= 5) return 1;
                if (Employers.CurrentPage < 4) return 1;
                if (Employers.CurrentPage + 2 > Employers.PageCount) return Employers.PageCount - 4;

                return Employers.CurrentPage - 2;
            }
        }
        public int PagerEndIndex
        {
            get
            {
                if (Employers == null) return 1;
                if (Employers.PageCount <= 5) return Employers.PageCount;
                if (PagerStartIndex + 4 > Employers.PageCount) return Employers.PageCount;
                return PagerStartIndex + 4;
            }
        }

        public string LastSearch { get; set; }
        public string LastSectors { get; set; }
        public int LastPage { get; set; }
        public int LastPageSize { get; set; }
        public int LastYear { get; set; }
    }
}