using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GenderPayGap.WebUI.Models
{
    public class ErrorViewModel
    {
        public ErrorViewModel(int code, object parameters=null)
        {
            
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CallToAction { get; set; }
        public string ActionText { get; set; } = "Continue";
        public string ActionUrl { get; set; }
    }
}