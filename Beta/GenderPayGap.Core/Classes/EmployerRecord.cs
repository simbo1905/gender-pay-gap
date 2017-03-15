using Extensions;
using System;
using System.Collections.Generic;

namespace GenderPayGap.Core.Classes
{

    [Serializable]
    public class EmployerRecord
    {
        public long Id { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyStatus { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string PoBox { get; set; }
        public string SicCodes { get; set; }

        public IEnumerable<int> GetSicCodes()
        {
            var codes=new HashSet<int>();
            foreach (var sicCode in SicCodes.SplitI())
                codes.Add(sicCode.ToInt32());
            return codes;
        } 

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

        public string SicSectors { get; set; }

        public string GetEncryptedId()
        {
            return Encryption.EncryptQuerystring(Id.ToString());
        }

        public bool IsAuthorised(string emailAddress)
        {
            if (!emailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");
            if (string.IsNullOrWhiteSpace(EmailPatterns)) throw new ArgumentException("Missing email pattern");
            return emailAddress.LikeAny(EmailPatterns.SplitI(";"));
        }
    }
    
}