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

    public partial class UserOrganisation
    {
        public UserOrganisation()
        {
            Created = DateTime.Now;
            Modified = DateTime.Now;
        }

        [Key, Column(Order = 0)]
        public long UserId { get; set; }
        [Key, Column(Order = 1)]
        public long OrganisationId { get; set; }
        public string ConfirmCode { get; set; }
        public long PINCode { get; set; }
        public Nullable<System.DateTime> PINSentDate { get; set; }
        public Nullable<System.DateTime> PINConfirmedDate { get; set; }

        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }

        [ForeignKey("OrganisationId")]
        public virtual Organisation Organisation { get; set; }


        [ForeignKey("UserId")]
        public virtual User User { get; set; }


    }
}
