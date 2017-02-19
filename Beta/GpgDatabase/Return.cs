namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
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

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string JobTitle { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
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
    }
}
