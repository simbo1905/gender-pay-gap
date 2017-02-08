using GpgDB.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GenderPayGap.WebUI.Models
{
    public class SearchViewModel
    {
        //[Required]
        public string Search { get; set;}
        public Organisation[] Results { get; set; }
    }
}