namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AddressStatus
    {
        public AddressStatus()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AddressStatusId { get; set; }

        [Required]
        public long AddressId { get; set; }

        [Required]
        [Column("StatusId")]
        public AddressStatuses Status { get; set; }

        [Required]
        [Index]
        public System.DateTime  StatusDate { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string StatusDetails { get; set; }

        [Required]
        public long ByUserId { get; set; }

        [ForeignKey("AddressId")]
        public virtual OrganisationAddress Address { get; set; }

        [ForeignKey("ByUserId")]
        public virtual User ByUser { get; set; }
    }
}
