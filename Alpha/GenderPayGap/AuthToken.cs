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
    
    public partial class AuthToken
    {
        public int AuthTokenId { get; set; }
        public string AuthUserTokenId { get; set; }
        public int AuthProviderId { get; set; }
        public long UserUserId { get; set; }
    
        public virtual User User { get; set; }
    }
}
