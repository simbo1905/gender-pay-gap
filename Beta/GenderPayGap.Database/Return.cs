namespace GenderPayGap.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Return
    {
        public Return()
        {
            this.ReturnStatuses = new HashSet<ReturnStatus>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReturnId { get; set; }

        [Required]
        [Index]
        public long OrganisationId { get; set; }

        [Required]
        [Index]
        public System.DateTime AccountingDate { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal DiffMeanHourlyPayPercent { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal DiffMedianHourlyPercent { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal DiffMeanBonusPercent { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal DiffMedianBonusPercent { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal MaleMedianBonusPayPercent { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal FemaleMedianBonusPayPercent { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal MaleLowerPayBand { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal FemaleLowerPayBand { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal MaleMiddlePayBand { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal FemaleMiddlePayBand { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal MaleUpperPayBand { get; set; }

        [Required]
        public decimal FemaleUpperPayBand { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal MaleUpperQuartilePayBand { get; set; }

        [Required]
        [DataType("decimal(4,1")]
        public decimal FemaleUpperQuartilePayBand { get; set; }

        [MaxLength(255)]
        public string CompanyLinkToGPGInfo { get; set; }

        [Required]
        [Column("StatusId")]
        [Index]
        public ReturnStatuses Status { get; set; }

        [Required]
        public System.DateTime StatusDate { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string StatusDetails { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        [Required]
        public System.DateTime Modified { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string JobTitle { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        public virtual ICollection<ReturnStatus> ReturnStatuses { get; set; }

        [ForeignKey("OrganisationId")]
        public virtual Organisation Organisation { get; set; }

        public void SetStatus(ReturnStatuses status, long byUserId, string details = null)
        {
            if (status == Status && details == StatusDetails) return;
            ReturnStatuses.Add(new ReturnStatus()
            {
                ReturnId = this.ReturnId,
                Status = status,
                StatusDate = DateTime.Now,
                StatusDetails = details,
                ByUserId = byUserId
            });
            Status = status;
            StatusDate = DateTime.Now;
            StatusDetails = details;
        }

        public bool Equals(Return model)
        {
            if (AccountingDate != model.AccountingDate)return false;
            if (CompanyLinkToGPGInfo != model.CompanyLinkToGPGInfo)return false;
            if (DiffMeanBonusPercent != model.DiffMeanBonusPercent)return false;
            if (DiffMeanHourlyPayPercent != model.DiffMeanHourlyPayPercent)return false;
            if (DiffMedianBonusPercent != model.DiffMedianBonusPercent)return false;
            if (DiffMedianHourlyPercent != model.DiffMedianBonusPercent)return false;
            if (FemaleLowerPayBand != model.FemaleLowerPayBand)return false;
            if (FemaleMedianBonusPayPercent != model.FemaleMedianBonusPayPercent)return false;
            if (FemaleMiddlePayBand != model.FemaleMiddlePayBand)return false;
            if (FemaleUpperPayBand != model.FemaleUpperPayBand)return false;
            if (FemaleUpperQuartilePayBand != model.FemaleUpperQuartilePayBand)return false;
            if (FirstName != model.FirstName)return false;
            if (LastName != model.LastName)return false;
            if (JobTitle != model.JobTitle)return false;
            if (MaleLowerPayBand != model.MaleLowerPayBand)return false;
            if (MaleMedianBonusPayPercent != model.MaleMedianBonusPayPercent)return false;
            if (MaleUpperQuartilePayBand != model.MaleUpperQuartilePayBand)return false;
            if (MaleMiddlePayBand != model.MaleMiddlePayBand)return false;
            if (MaleUpperPayBand != model.MaleUpperPayBand)return false;
            if (OrganisationId != model.OrganisationId) return false;
            return true;
        }

        public DownloadRecord ToDownloadRecord()
        {
            var employer = Organisation.ToEmployerRecord();
            return new DownloadRecord()
            {
                EmployerName = employer.Name,
                CompanyNumber = employer.CompanyNumber,
                SicCodes = employer.SicCodes,
                Address1 = employer.Address1,
                Address2 = employer.Address2,
                Address3 = employer.Address3,
                Country = employer.Country,
                PostCode = employer.PostCode,
                PoBox = employer.PoBox,
                AccountingDate = this.AccountingDate,
                CompanyLinkToGPGInfo = this.CompanyLinkToGPGInfo,
                DiffMeanBonusPercent = this.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = this.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = this.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = this.DiffMedianHourlyPercent,
                FemaleLowerPayBand = this.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = this.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = this.FemaleMiddlePayBand,
                FemaleUpperPayBand = this.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = this.FemaleUpperQuartilePayBand,
                MaleLowerPayBand = this.MaleLowerPayBand,
                MaleMedianBonusPayPercent = MaleMedianBonusPayPercent,
                MaleMiddlePayBand = MaleMiddlePayBand,
                MaleUpperPayBand = MaleUpperPayBand,
                MaleUpperQuartilePayBand = MaleUpperQuartilePayBand,
                ResponsiblePerson = $"{FirstName} {LastName} ({JobTitle})",
                SubmittedDate = this.StatusDate
            };
        }

    }
}
