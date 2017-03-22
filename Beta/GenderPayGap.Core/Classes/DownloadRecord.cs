namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DownloadRecord
    {
        public DownloadRecord()
        {
        }

        public System.DateTime AccountingDate { get; set; }

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

        public string ResponsiblePerson { get; set; }

        public DateTime SubmittedDate { get; set; }

        public string EmployerName { get; set; }
        public string CompanyNumber { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string PoBox { get; set; }
        public string SicCodes { get; set; }

    }
}
