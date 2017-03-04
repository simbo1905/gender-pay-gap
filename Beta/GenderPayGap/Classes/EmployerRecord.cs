using System;
using System.Collections.Generic;
using Extensions;

namespace GenderPayGap.WebUI.Classes
{

    [Serializable]
    public class EmployerRecord
    {
        public string CompanyNumber { get; set; }
        public string CompanyStatus { get; set; }
        public string Name { get; internal set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string PoBox { get; set; }

        //Public Sector
        public string EmailPatterns { get; set; }

        public string FullAddress
        {
            get
            {
                var list = new List<string>();
                list.Add(Address1);
                list.Add(Address2);
                list.Add(Address3);
                list.Add(Country);
                list.Add(PostCode);
                list.Add(PoBox);
                return list.ToDelimitedString(", ");
            }
        }

        public bool IsAuthorised(string emailAddress)
        {
            if (!emailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");
            if (string.IsNullOrWhiteSpace(EmailPatterns)) throw new ArgumentException("Missing email pattern");
            return emailAddress.LikeAny(EmailPatterns.SplitI(";"));
        }
    }
    
}