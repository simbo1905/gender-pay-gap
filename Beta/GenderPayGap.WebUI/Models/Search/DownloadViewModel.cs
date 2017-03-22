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
            public string Title { get; set; }
            public string Count { get; set; }
            public string Size { get; set; }
            public string Url { get; set; }
            public string Extension { get; set; }
        }

        public List<Download> Downloads { get; set; }
    }
}