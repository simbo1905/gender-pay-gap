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
    
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.Group = new HashSet<Group>();
            this.Group1 = new HashSet<Group>();
            this.OrganisationStatuses = new HashSet<OrganisationStatuses>();
            this.ReturnStatuses = new HashSet<ReturnStatuses>();
            this.UserGroups = new HashSet<UserGroups>();
            this.UserStatuses = new HashSet<UserStatuses>();
            this.UserStatuses1 = new HashSet<UserStatuses>();
        }
    
        public long UserId { get; set; }
        public string UserRef { get; set; }
        public Nullable<int> AuthProviderId { get; set; }
        public string AuthUserTokenId { get; set; }
        public string UserType { get; set; }
        public string Title { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string TownCity { get; set; }
        public string County { get; set; }
        public string CountryCode { get; set; }
        public string PostCode { get; set; }
        public string WorkPhone1 { get; set; }
        public string WorkPhone2 { get; set; }
        public string HomePhone { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string Web { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string CurrentStatus { get; set; }
        public System.DateTime CurrentStatusDate { get; set; }
        public string CurrentStatusDetails { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Group> Group { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Group> Group1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrganisationStatuses> OrganisationStatuses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReturnStatuses> ReturnStatuses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserGroups> UserGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserStatuses> UserStatuses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserStatuses> UserStatuses1 { get; set; }
    }
}
