//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GpgDB.Models.GpgDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserStatus
    {
        public UserStatus()
        {
            StatusDate = DateTime.Now;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserStatusId { get; set; }
        public long UserId { get; set; }

        [Column("StatusId")]
        public UserStatuses Status { get; set; }
        public Nullable<System.DateTime>  StatusDate { get; set; }
        public string StatusMessage { get; set; }
        public Nullable<long> ByUserId { get; set; }    
    }
}
