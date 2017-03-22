namespace GenderPayGap.Models.SqlDatabase
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Serializable]
    public partial class SicCode
    {
        public SicCode()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SicCodeId { get; set; }

        [Index]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(1)]
        public string SicSectionId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(250)]
        public string Description { get; set; }

        [Required]
        public System.DateTime Created { get; set; } = DateTime.Now;

        public virtual SicSection SicSection { get; set; }
    }
}
