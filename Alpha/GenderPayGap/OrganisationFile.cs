//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GenderPayGap
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrganisationFile
    {
        public System.Guid FileId { get; set; }
        public long OrganisationId { get; set; }
        public string FileType { get; set; }
        public Nullable<int> FileSize { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
    
        public virtual Organisation Organisation { get; set; }
    }
}
