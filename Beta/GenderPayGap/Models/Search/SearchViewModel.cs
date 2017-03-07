using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Models.SqlDatabase;

namespace GenderPayGap.WebUI.Models.Search
{
    [Serializable]
    public class SearchViewModel
    {
        [Required(AllowEmptyStrings=false)]
        public string Search { get; set;}
        public Organisation[] Results { get; set; }
    }
}