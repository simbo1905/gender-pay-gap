namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserOrganisation
    {
        public UserOrganisation()
        {
        }

        [Key, Column(Order = 0)]
        public long UserId { get; set; }

        [Key, Column(Order = 1)]
        public long OrganisationId { get; set; }

        [MaxLength(255)]
        public string ConfirmCode { get; set; }

        [MaxLength(20)]
        public string PINCode { get; set; }

        public Nullable<System.DateTime> PINSentDate { get; set; }

        public Nullable<System.DateTime> PINConfirmedDate { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        [Required]
        public System.DateTime Modified { get; set; } = DateTime.Now;

        [ForeignKey("OrganisationId")]
        public virtual Organisation Organisation { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }


    }
}
