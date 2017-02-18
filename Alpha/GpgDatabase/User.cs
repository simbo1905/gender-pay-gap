namespace GenderPayGap.Models.SqlDatabase
{
    using Extensions;
    using IdentityServer3.Core;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;

    public partial class User
    {
        public User()
        {
            this.UserStatuses = new HashSet<UserStatus>();
            this.Organisations = new HashSet<Organisation>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string JobTitle { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Firstname { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Lastname { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(255)]
        [Index]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(250),MinLength(64)]
        public string PasswordHash { get; set; }

        [MaxLength(255)]
        public string EmailVerifyCode { get; set; }

        public Nullable<System.DateTime> EmailVerifySendDate { get; set; }

        public Nullable<System.DateTime> EmailVerifiedDate { get; set; }

        [Column("StatusId")]
        [Required]
        public UserStatuses Status { get; set; }

        [Required]
        public System.DateTime StatusDate { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string StatusDetails { get; set; }

        public int LoginAttempts { get; set; }
        public DateTime? LoginDate { get; set; }

        public DateTime? VerifyAttemptDate { get; set; }
        public int VerifyAttempts { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        [Required]
        public System.DateTime Modified { get; set; } = DateTime.Now;
 
        public virtual ICollection<Organisation> Organisations { get; set; }

        public virtual ICollection<UserStatus> UserStatuses { get; set; }

        [NotMapped]
        public string Fullname {
            get
            {
                return (Firstname + " " + Lastname).TrimI();
            }
        }

        public void SetStatus(UserStatuses status, long userId, string details=null)
        {
            if (status == Status && details == StatusDetails) return;
            UserStatuses.Add(new UserStatus()
            {
                UserId = userId,
                Status = status,
                StatusDate = DateTime.Now,
                StatusDetails = details,
                ByUserId = userId
            });
            Status = status;
            StatusDate = DateTime.Now;
            StatusDetails = details;
        }

    }
}
