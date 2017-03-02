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

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Enter the difference in mean hourly rate")]
        [Range(-200.9, 200.9, ErrorMessage = "Value must be between -200.9 and 200.9")]
        [RegularExpression(@"^[-]?\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? DiffMeanHourlyPayPercent { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Enter the difference in median hourly rate")]
        [Range(-200.9, 200.9, ErrorMessage = "Value must be between -200.9 and 200.9")]
        [RegularExpression(@"^[-]?\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? DiffMedianHourlyPercent { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Enter the difference in mean bonus pay, calculated from the mean")]
        [Range(-200.9, 200.9, ErrorMessage = "Value must be between -200.9 and 200.9")]
        [RegularExpression(@"^[-]?\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? DiffMeanBonusPercent { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Enter the difference in median bonus pay, calculated from the median")]
        [Range(-200.9, 200.9, ErrorMessage = "Value must be between -200.9 and +200.9")]
        [RegularExpression(@"^[-]?\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? DiffMedianBonusPercent { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Males who received bonus pay")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleMedianBonusPayPercent { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Females who received bonus pay")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleMedianBonusPayPercent { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Male")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleLowerPayBand { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Female")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleLowerPayBand { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Male")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleMiddlePayBand { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Female")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleMiddlePayBand { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Male")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleUpperPayBand { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Female")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? FemaleUpperPayBand { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Male")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
        [DisplayFormat(DataFormatString = "{0:0.#}", ApplyFormatInEditMode = true)]
        public decimal? MaleUpperQuartilePayBand { get; set; }

        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Female")]
        [Range(0, 200.9, ErrorMessage = "Value must be between 0 and 200.9")]
        [RegularExpression(@"^\d+(\.{0,1}\d)?$", ErrorMessage = "Value can't have more than 1 decimal place")]
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
        [Required(AllowEmptyStrings=false)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required(AllowEmptyStrings=false)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Url]
        [Display(Name = "Link to your gender pay gap information")]
        public string CompanyLinkToGPGInfo { get; set; }
        public bool ReturnToStep4 { get; internal set; }
    }
}



