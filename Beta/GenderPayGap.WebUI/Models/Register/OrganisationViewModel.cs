using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using System.Collections.Generic;
using Extensions;

namespace GenderPayGap.WebUI.Models.Register
{
    [Serializable]
    public class OrganisationViewModel
    {
        public bool PINSent;
        public bool PINExpired;
        public bool ManualRegistration { get; set; }=true;
        public string BackAction { get; set; }

        public OrganisationViewModel()
        {

        }

        [Required(AllowEmptyStrings=false)]
        public SectorTypes? SectorType { get; set; }

        [Required]
        [StringLength(100,ErrorMessage = "You must enter an employers name or company number between 3 and 100 characters in length",MinimumLength = 3)]
        [DisplayName("Search")]
        public string SearchText { get; set; }

        public PagedResult<EmployerRecord> Employers { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100), MinLength(3)]
        public string Name { get; set; }
        public string CompanyNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Address1 { get; set; }

        [MaxLength(100)]
        public string Address2 { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayName("Town or City")]
        [MaxLength(100)]
        public string Address3 { get; set; }
        public string Country { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(20)]
        public string PostCode { get; set; }
        public string PoBox { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactFirstName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactLastName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactJobTitle { get; set; }

        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string ContactOrganisation { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        public string ContactEmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Phone]
        public string ContactPhoneNumber { get; set; }

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


        public int SelectedEmployerIndex { get; set; }

        public EmployerRecord SelectedEmployer
        {
            get
            {
                if (SelectedEmployerIndex > -1 && SelectedEmployerIndex < Employers.Results.Count)
                    return Employers.Results[SelectedEmployerIndex];
                return null;
            }
        }

        public int EmployerStartIndex
        {
            get
            {
                if (Employers==null || Employers.Results == null || Employers.Results.Count < 1) return 1;
                return ((Employers.CurrentPage * Employers.PageSize) - Employers.PageSize) + 1;
            }
        }
        public int EmployerEndIndex
        {
            get
            {
                if (Employers==null || Employers.Results == null || Employers.Results.Count < 1) return 1;
                return EmployerStartIndex + Employers.Results.Count-1;
            }
        }
        public int PagerStartIndex
        {
            get
            {
                if (Employers==null || Employers.PageCount <= 5) return 1;
                if (Employers.CurrentPage < 4) return 1;
                if (Employers.CurrentPage + 2 > Employers.PageCount) return Employers.PageCount - 4;

                return Employers.CurrentPage - 2;
            }
        }
        public int PagerEndIndex
        {
            get
            {
                if (Employers == null) return 1;
                if (Employers.PageCount <= 5) return Employers.PageCount;
                if (PagerStartIndex + 4 > Employers.PageCount) return Employers.PageCount;
                return PagerStartIndex + 4;
            }
        }

        public string ReviewCode { get; set; }
        public string CancellationReason { get; set; }
    }
    
}