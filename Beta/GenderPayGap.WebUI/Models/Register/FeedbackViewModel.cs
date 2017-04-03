using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Models.Register
{

    [Serializable]
    public class FeedbackViewModel
    {
        public FeedbackViewModel()
        {

        }

        [Required(AllowEmptyStrings = false)]
        public DifficultyTypes? SectorType { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        [Display(Name = "feedback comments")]
        public string Comments { get; set; }

        [EmailAddress]
        [Display(Name = "Your email address (optional)")]
        public string EmailAddress { get; set; }

        [Phone]
        [Display(Name = "Your phone number (optional)")]
        public string PhoneNumber { get; set; }

    }
}