namespace GenderPayGap.Database
{
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class OrganisationAddress
    {
        public OrganisationAddress()
        {
            this.AddressStatuses = new HashSet<AddressStatus>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AddressId { get; set; }
        public long CreatedByUserId { get; set; }

        [MaxLength(100)]
        public string Address1 { get; set; }

        [MaxLength(100)]
        public string Address2 { get; set; }

        [MaxLength(100)]
        public string Address3 { get; set; }

        [MaxLength(100)]
        public string TownCity { get; set; }

        [MaxLength(100)]
        public string County { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(30)]
        public string PoBox { get; set; }

        [MaxLength(20)]
        public string PostCode { get; set; }

        [Required]
        [Column("StatusId")]
        [Index]
        public AddressStatuses Status { get; set; }

        [Required]
        [Index]
        public System.DateTime StatusDate { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string StatusDetails { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        [Required]
        public System.DateTime Modified { get; set; } = DateTime.Now;

        [Required]
        public long OrganisationId { get; set; }

        [ForeignKey("OrganisationId")]
        public virtual Organisation Organisation { get; set; }

        public virtual ICollection<AddressStatus> AddressStatuses { get; set; }

        public List<string> GetList()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Address1))list.Add(Address1);
            if (!string.IsNullOrWhiteSpace(Address2)) list.Add(Address2);
            if (!string.IsNullOrWhiteSpace(Address3)) list.Add(Address3);
            if (!string.IsNullOrWhiteSpace(TownCity)) list.Add(TownCity);
            if (!string.IsNullOrWhiteSpace(County)) list.Add(County);
            if (!string.IsNullOrWhiteSpace(Country)) list.Add(Country);
            if (!string.IsNullOrWhiteSpace(PostCode)) list.Add(PostCode);
            if (!string.IsNullOrWhiteSpace(PoBox)) list.Add(PoBox);
            return list;
        }

        public string GetAddress(string delimiter=", ")
        {
            return GetList().ToDelimitedString(delimiter);
        }

        public void SetStatus(AddressStatuses status, long byUserId, string details = null)
        {
            if (status == Status && details == StatusDetails) return;
            AddressStatuses.Add(new AddressStatus()
            {
                AddressId = this.AddressId,
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
