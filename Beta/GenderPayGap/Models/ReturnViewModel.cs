using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Models.SqlDatabase;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenderPayGap.WebUI.Models
{
    public class ReturnViewModel
    {
        public ReturnViewModel()
        {

        }

        [Required]
        [Range(-200.9, 200.9)]
        [Display(Name = "Enter the difference in mean hourly rate")]
        public decimal DiffMeanHourlyPayPercent { get; set; }

        [Required]
        [Range(-200.9, 200.9)]
        [Display(Name = "Enter the difference in median hourly rate")]
        public decimal DiffMedianHourlyPercent { get; set; }

        [Required]
        [Display(Name = "Enter the difference in mean bonus pay")]
        [Range(-200.9, 200.9)]
        public decimal DiffMeanBonusPercent { get; set; }

        [Required]
        [Display(Name = "Enter the difference in median bonus pay")]
        [Range(-200.9, 200.9)]
        public decimal DiffMedianBonusPercent { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Males who received bonus pay %")]
        public decimal MaleMedianBonusPayPercent { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Females who received bonus pay %")]
        public decimal FemaleMedianBonusPayPercent { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Male")]
        public decimal MaleLowerPayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Female")]
        public decimal FemaleLowerPayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Male")]
        public decimal MaleMiddlePayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Female")]
        public decimal FemaleMiddlePayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Male")]
        public decimal MaleUpperPayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Female")]
        public decimal FemaleUpperPayBand { get; set; }

        [Required]
        [Range(0.0, 200.9)]
        [Display(Name = "Male")]
        public decimal MaleUpperQuartilePayBand { get; set; }

        [Required]
        [Range(0.0, 200.9) ]
        [Display(Name = "Female")]
        public decimal FemaleUpperQuartilePayBand { get; set; }

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
        [Display(Name = "Title")]
        public string JobTitle { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Url]
        [Display(Name = "Link to your gender pay gap information")]
        public string CompanyLinkToGPGInfo { get; set; }

    }
}



