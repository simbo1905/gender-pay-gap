using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GenderPayGap.Models
{
    public class SearchViewModel
    {
        //[Required]
        public string Search { get; set;}
        public Organisation[] Results { get; set; }
    }
}