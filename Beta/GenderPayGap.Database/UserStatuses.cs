namespace GenderPayGap.Database
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserStatus
    {
        public UserStatus()
        {
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserStatusId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Column("StatusId")]

        public UserStatuses Status { get; set; }

        [Index]
        public System.DateTime StatusDate { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string StatusDetails { get; set; }

        [Required]
        public long ByUserId { get; set; }

        [ForeignKey("ByUserId")]
        public virtual User ByUser { get; set; }
    }
}
