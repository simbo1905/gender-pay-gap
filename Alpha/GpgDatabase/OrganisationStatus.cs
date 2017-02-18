namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class OrganisationStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrganisationStatusId { get; set; }

        [Required]
        public long OrganisationId { get; set; }

        [Required]
        [Column("StatusId")]
        public OrganisationStatuses Status { get; set; }

        [Required]
        [Index]
        public System.DateTime  StatusDate { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string StatusDetails { get; set; }

        [Required]
        public long ByUserId { get; set; }

        [ForeignKey("OrganisationId")]
        public virtual Organisation Organisation { get; set; }

        [ForeignKey("ByUserId")]
        public virtual User ByUser { get; set; }
    }
}
