using System.Linq;

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

        [MaxLength(250), MinLength(64)]
        public string PINHash { get; set; }

        public Nullable<System.DateTime> PINSentDate { get; set; }

        public Nullable<System.DateTime> PINConfirmedDate { get; set; }

        public Nullable<System.DateTime> ConfirmAttemptDate { get; set; }

        public int ConfirmAttempts { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        [Required]
        public System.DateTime Modified { get; set; } = DateTime.Now;

        [ForeignKey("OrganisationId")]
        public virtual Organisation Organisation { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Latest ACTIVE address
        /// </summary>
        public OrganisationAddress Address
        {
            get
            {
                //Get the latest address for the organisation
                return Organisation.OrganisationAddresses.FirstOrDefault(a => a.OrganisationId == OrganisationId && a.CreatedByUserId == UserId); ;
            }
        }
    }
}
