using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GpgDB;
using GpgDB.Models.GpgDatabase;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using static GpgDB.Models.GpgDatabase.Organisation;
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
        [Display(Name = "Enter the difference in mean hourly rate")]
        public decimal DiffMeanHourlyPayPercent { get; set; }

        [Required]
        [Display(Name = "Enter the difference in median hourly rate")]
        public decimal DiffMedianHourlyPercent { get; set; }

        [Required]
        [Display(Name = "Enter the difference in mean bonus pay")]
        public decimal DiffMeanBonusPercent { get; set; }

        [Required]
        [Display(Name = "Enter the difference in median bonus pay")]
        public decimal DiffMedianBonusPercent { get; set; }

        [Required]
        [Display(Name = "Males who received bonus pay %")]
        public decimal MaleMedianBonusPayPercent { get; set; }
        [Required]
        [Display(Name = "Females who received bonus pay %")]
        public decimal FemaleMedianBonusPayPercent { get; set; }
        [Required]
        [Display(Name = "Male")]
        public decimal MaleLowerPayBand { get; set; }
        [Required]
        [Display(Name = "Female")]
        public decimal FemaleLowerPayBand { get; set; }
        [Required]
        [Display(Name = "Male")]
        public decimal MaleMiddlePayBand { get; set; }
        [Required]
        [Display(Name = "Female")]
        public decimal FemaleMiddlePayBand { get; set; }
        [Required]
        [Display(Name = "Male")]
        public decimal MaleUpperPayBand { get; set; }
        [Required]
        [Display(Name = "Female")]
        public decimal FemaleUpperPayBand { get; set; }
        [Required]
        [Display(Name = "Male")]
        public decimal MaleUpperQuartilePayBand { get; set; }
        [Required]
        [Display(Name = "Female")]
        public decimal FemaleUpperQuartilePayBand { get; set; }

        public long ReturnId { get; set; }
        public long OrganisationId { get; set; }

        //public DateTime? AccountingDate { get; set; }
        //public string CurrentStatus { get; set; }
        //public DateTime? CurrentStatusDate { get; set; }
        //public string CurrentStatusDetails { get; set; }
        //public DateTime? Created { get; set; }
        //public DateTime? Modified { get; set; }

        //public virtual ICollection<ReturnStatus> ReturnStatuses { get; set; }

        //[ForeignKey("OrganisationId")]
        //public virtual Organisation Organisation { get; set; }

    }

    public class PersonResponsibleViewModel
    {
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
    }

    public class CompanyLinkToGPGInfoViewModel
    {
        [Url]
        [Display(Name = "Link to your gender pay gap information")]
        public string CompanyLinkToGPGInfo { get; set; }
    }

}



