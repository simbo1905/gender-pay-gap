namespace GenderPayGap.Models.SqlDatabase
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

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SicCodeId { get; set; }

        [Required]
        public long OrganisationId { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        [ForeignKey("SicCodeId")]
        public virtual SicCode SicCode { get; set; }

        [ForeignKey("OrganisationId")]
        public virtual Organisation Organisation { get; set; }
    }
}
