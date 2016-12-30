//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GenderPayGap.Models.GpgDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ReturnModel
    {
        public ReturnModel()
        {
        }
        public long ReturnId { get; set; }

        public string OrganisationName { get; set; }

        public Nullable<System.DateTime> AccountingDate { get; set; }

        public decimal DiffMeanHourlyPayPercent { get; set; }
        public decimal DiffMedianHourlyPercent { get; set; }
        public decimal DiffMeanBonusPercent { get; set; }
        public decimal DiffMedianBonusPercent { get; set; }
        public decimal MaleMedianBonusPayPercent { get; set; }
        public decimal FemaleMedianBonusPayPercent { get; set; }
        public decimal MaleLowerPayBand { get; set; }
        public decimal FemaleLowerPayBand { get; set; }
        public decimal MaleMiddlePayBand { get; set; }
        public decimal FemaleMiddlePayBand { get; set; }
        public decimal MaleUpperPayBand { get; set; }
        public decimal FemaleUpperPayBand { get; set; }
        public decimal MaleUpperQuartilePayBand { get; set; }
        public decimal FemaleUpperQuartilePayBand { get; set; }
        public string CompanyLinkToGPGInfo { get; set; }
        public string CurrentStatus { get; set; }
        public Nullable<System.DateTime> CurrentStatusDate { get; set; }
        public string CurrentStatusDetails { get; set; }
        public string JobTitle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
