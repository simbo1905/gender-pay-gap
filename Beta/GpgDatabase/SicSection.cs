namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class SicSection
    {
        public SicSection()
        {
            this.SicCodes = new HashSet<SicCode>();
        }

        [Key]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(1)]
        public string SicSectionId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(250)]
        public string Description { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        public virtual ICollection<SicCode> SicCodes { get; set; }

    }
}
