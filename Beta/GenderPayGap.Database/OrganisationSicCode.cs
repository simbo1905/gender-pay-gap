namespace GenderPayGap.Database
{
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class OrganisationSicCode
    {
        public OrganisationSicCode()
        {
        }

        [Key, Column(Order = 0)]
        public int SicCodeId { get; set; }

        [Key, Column(Order = 1)]
        public long OrganisationId { get; set; }

        public System.DateTime Created { get; set; } = DateTime.Now;

        [ForeignKey("SicCodeId")]
        public virtual SicCode SicCode { get; set; }

        [ForeignKey("OrganisationId")]
        public virtual Organisation Organisation { get; set; }
    }
}
