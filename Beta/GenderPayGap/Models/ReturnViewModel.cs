using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Models.SqlDatabase;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Models
{
    [Serializable]
    public class ReturnViewModel
    {
        public ReturnViewModel()
        {

        }

        [Required]
        [Range(-200.9, 200.9)]
        [Display(Name = "Enter the difference in mean hourly rate")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? DiffMeanHourlyPayPercent { get; set; }

        [Required]
        [Range(-200.9, 200.9)]
        [Display(Name = "Enter the difference in median hourly rate")]
      //[DisplayFormat(DataFormatString = "{0:F1}", ApplyFormatInEditMode = true)]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? DiffMedianHourlyPercent { get; set; }

        [Required]
        [Display(Name = "Enter the difference in mean bonus pay, calculated from the mean")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        [Range(-200.9, 200.9)]
        public decimal? DiffMeanBonusPercent { get; set; }

        [Required]
        [Display(Name = "Enter the difference in median bonus pay, calculated from the median")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        [Range(-200.9, 200.9)]
        public decimal? DiffMedianBonusPercent { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Males who received bonus pay")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleMedianBonusPayPercent { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Females who received bonus pay")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleMedianBonusPayPercent { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Male")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleLowerPayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Female")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleLowerPayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Male")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleMiddlePayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Female")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleMiddlePayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Male")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleUpperPayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Female")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleUpperPayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Male")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleUpperQuartilePayBand { get; set; }

        [Required]
        [Range(0.0, 200.9) ]
        [Display(Name = "Female")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleUpperQuartilePayBand { get; set; }

        public long ReturnId { get; set; }
        public long OrganisationId { get; set; }

        public DateTime AccountingDate { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime CurrentStatusDate { get; set; }
        public string CurrentStatusDetails { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        //public virtual ICollection<ReturnStatus> ReturnStatuses { get; set; }

        //[ForeignKey("OrganisationId")]
        //public virtual Organisation Organisation { get; set; }


        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Job title")]
        public string JobTitle { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Url]
        //[GenderPayGap.WebUI.Classes.Attribute.Url]
        //Validates url without "http://", "https://" or "www"
        // [RegularExpression(@"((www\.|(http|https|ftp|news|file|)+\:\/\/)?[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])", ErrorMessage = "Please check the url")]
        //[Display(Name = "Link to your gender pay gap information")]
        public string CompanyLinkToGPGInfo { get; set; }
        public bool ReturnToStep4 { get; internal set; }
    }
}



