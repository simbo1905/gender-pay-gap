using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Home
{
    public class FeedbackViewModel
    {
        public string Date { get; set; } = DateTime.Now.ToShortDateString();

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Difficulty")]
        public DifficultyTypes? DifficultyType { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [StringLength(2000, ErrorMessage = "Please enter up to 2000 characters")]
        [Display(Name = "Feedback details")]
        public string Details { get; set; }

    }
}