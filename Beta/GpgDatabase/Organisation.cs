using System.Linq;

namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Organisation
    {
        public Organisation()
        {
            this.OrganisationAddresses = new HashSet<OrganisationAddress>();
            this.OrganisationStatuses = new HashSet<OrganisationStatus>();
            this.Returns = new HashSet<Return>();
        }


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrganisationId { get; set; }

        [MaxLength(10)]
        public string PrivateSectorReference { get; set; }

        [MaxLength(10)]
        public string PublicSectorReference { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100), MinLength(5)]
        [Index]
        public string OrganisationName { get; set; }

        [Required]
        [Column("SectorTypeId")]
        [Index]
        public SectorTypes SectorType { get; set; }

        [Required]
        [Column("StatusId")]
        public OrganisationStatuses Status { get; set; }

        [Required]
        public System.DateTime StatusDate { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string StatusDetails { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        [Required]
        public System.DateTime Modified { get; set; } = DateTime.Now;

        public virtual ICollection<OrganisationAddress> OrganisationAddresses { get; set; }

        public virtual ICollection<OrganisationStatus> OrganisationStatuses { get; set; }

        public virtual ICollection<Return> Returns { get; set; }

        public void SetStatus(OrganisationStatuses status, long byUserId, string details = null)
        {
            if (status == Status && details == StatusDetails) return;
            OrganisationStatuses.Add(new OrganisationStatus()
            {
                OrganisationId = this.OrganisationId,
                Status = status,
                StatusDate = DateTime.Now,
                StatusDetails = details,
                ByUserId = byUserId
            });
            Status = status;
            StatusDate = DateTime.Now;
            StatusDetails = details;
        }

        /// <summary>
        /// Latest ACTIVE address
        /// </summary>
        public OrganisationAddress Address
        {
            get
            {
                //Get the latest address for the organisation
                return OrganisationAddresses.OrderByDescending(oa => oa.Modified).FirstOrDefault(oa => oa.OrganisationId == OrganisationId && oa.Status==AddressStatuses.Active);
            }
        }
    }
}
