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
    public class DownloadViewModel
    {
        public DownloadViewModel()
        {

        }

        public class Download
        {
            
        }

        [Required]
        [StringLength(100, ErrorMessage = "You must enter an employers name between 3 and 100 characters in length", MinimumLength = 3)]
        [DisplayName("Search")]
        public string SearchText { get; set; }

        public int Year { get; set; }
        
        // This property contains the available options
        public IEnumerable<SicSection> AllSectors { get; set; }

        // This property contains the selected options
        public IEnumerable<string> NewSectors { get; set; }
        public List<string> OldSectors { get; set; }
        public SelectList SectorSources { get; set; }

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
    }
}