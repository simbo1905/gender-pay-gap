namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ReturnStatus
    {
        public ReturnStatus()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReturnStatusId { get; set; }

        [Required]
        public long ReturnId { get; set; }

        [Required]
        [Column("StatusId")]
        public ReturnStatuses Status { get; set; }

        [Required]
        [Index]
        public System.DateTime  StatusDate { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string StatusDetails { get; set; }

        [Required]
        public long ByUserId { get; set; }

        [ForeignKey("ReturnId")]
        public virtual Return Return { get; set; }

        [ForeignKey("ByUserId")]
        public virtual User ByUser { get; set; }
    }
}
