//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GenderPayGap.Models.GpgDatabase
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
            Created = DateTime.Now;
            Modified = DateTime.Now;
        }

        public enum OrgTypes:int
        {
            Unknown=0,
            Company=1,
            Charity=2,
            Government=3
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrganisationId { get; set; }
        public string OrganisationRef { get; set; }
        public string OrganisationName { get; set; }
        public OrgTypes OrganisationType { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Web { get; set; }
        public string CurrentStatus { get; set; }
        public Nullable<System.DateTime> CurrentStatusDate { get; set; }
        public string CurrentStatusDetails { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }

        public virtual ICollection<OrganisationAddress> OrganisationAddresses { get; set; }
        public virtual ICollection<OrganisationStatus> OrganisationStatuses { get; set; }
        public virtual ICollection<Return> Returns { get; set; }
    }
}
