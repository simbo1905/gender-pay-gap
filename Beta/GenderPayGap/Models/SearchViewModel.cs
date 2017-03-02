using GenderPayGap.Models.SqlDatabase;
using System.ComponentModel.DataAnnotations;

namespace GenderPayGap.WebUI.Models
{
    public class SearchViewModel
    {
        [Required(AllowEmptyStrings=false)]
        public string Search { get; set;}
        public Organisation[] Results { get; set; }
    }
}