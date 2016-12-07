//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GenderPayGap.Models.GpgEntityModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Return
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Return()
        {
            this.OrganisationGPGReturns = new HashSet<OrganisationGPGReturns>();
            this.ReturnHit = new HashSet<ReturnHit>();
            this.ReturnStatuses = new HashSet<ReturnStatuses>();
        }
    
        public long ReturnId { get; set; }
        public decimal DiffMeanHourlyPayPercent { get; set; }
        public decimal DiffMedianHourlyPercent { get; set; }
        public decimal DiffMeanBonusPercent { get; set; }
        public decimal DiffMedianBonusPercent { get; set; }
        public decimal MaleMedianBonusPayPercent { get; set; }
        public decimal FemaleMedianBonusPayPercent { get; set; }
        public decimal MaleLowerPayBand { get; set; }
        public decimal FemaleLowerPayBand { get; set; }
        public decimal MaleMiddlePayBand { get; set; }
        public decimal FemaleMiddlePayBand { get; set; }
        public decimal MaleUpperPayBand { get; set; }
        public decimal FemaleUpperPayBand { get; set; }
        public decimal MaleUpperQuartilePayBand { get; set; }
        public decimal FemaleUpperQuartilePayBand { get; set; }
        public string CompanyLinkToGPGInfo { get; set; }
        public string CurrentStatus { get; set; }
        public Nullable<System.DateTime> CurrentStatusDate { get; set; }
        public string CurrentStatusDetails { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrganisationGPGReturns> OrganisationGPGReturns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReturnHit> ReturnHit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReturnStatuses> ReturnStatuses { get; set; }
    }
}
