using GenderPayGap.Models.SqlDatabase;

namespace GenderPayGap.WebUI.Models
{
    public class SearchViewModel
    {
        //[Required]
        public string Search { get; set;}
        public Organisation[] Results { get; set; }
    }
}