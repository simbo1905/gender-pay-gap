using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Extensions;
using GenderPayGap.Models.SqlDatabase;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Models
{

    public class RegisterViewModel
    {
        public RegisterViewModel()
        {

        }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Confirm your email address")]
        [Compare("EmailAddress", ErrorMessage = "The email address and confirmation do not match.")]
        public string ConfirmEmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Password]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string VerifyUrl { get; internal set; }

        public string IdentityProvider { get; set; }
    }

    public class VerifyViewModel
    {
        public VerifyViewModel()
        {

        }

        public bool Expired { get; set; }
        public bool Verified { get; set; }
        public bool Retry { get; set; }
        public bool Resend { get; set; }
        public string EmailAddress { get; set; }
        public bool Sent { get; internal set; }
        public bool WrongCode { get; internal set; }
    }

    public class OrganisationViewModel
    {
        public bool PINSent;
        public bool PINExpired;

        public OrganisationViewModel()
        {

        }

        public OrganisationViewModel(int pageSize)
        {
            EmployerPageSize = pageSize;
        }

        [Required]
        [EnumDataType(typeof(SectorTypes), ErrorMessage = "You must select the type of your organisation")]
        public SectorTypes? SectorType { get; set; }

        [Required]
        [StringLength(100,ErrorMessage = "You must enter an employers name or company number between 3 and 100 characters in length",MinimumLength = 3)]
        [DisplayName("Search")]
        public string SearchText { get; set; }

        public List<EmployerRecord> Employers { get; set; }

        public int SelectedEmployerIndex { get; set; }

        public int EmployerRecords { get; set; }

        public int EmployerCurrentPage { get; set; } 

        public int EmployerPageSize { get; set; }=10;

        public int EmployerPages
        {
            get
            {
                return (int) Math.Ceiling((double) EmployerRecords / EmployerPageSize); 
            }
        }
        public int EmployerStartIndex
        {
            get
            {
                if (Employers == null || Employers.Count < 1) return 1;
                return ((EmployerCurrentPage * EmployerPageSize) - EmployerPageSize) + 1;
            }
        }
        public int EmployerEndIndex
        {
            get
            {
                if (Employers == null || Employers.Count < 1) return 1;
                return EmployerStartIndex + Employers.Count-1;
            }
        }
        public int PagerStartIndex
        {
            get
            {
                if (EmployerPages <= 5) return 1;
                if (EmployerCurrentPage < 4) return 1;
                if (EmployerCurrentPage + 2 > EmployerPages) return EmployerPages-4;

                return EmployerCurrentPage -2;
            }
        }
        public int PagerEndIndex
        {
            get
            {
                if (EmployerPages <=5) return EmployerPages;
                if (PagerStartIndex + 4 > EmployerPages) return EmployerPages;
                return PagerStartIndex + 4;
            }
        }
    }

    [Serializable]
    public class EmployerRecord
    {
        //Public Sector
        public string OrgName { get; set; }
        public string EmailPatterns { get; set; }

        public string CompanyNumber { get; set; }
        public string CompanyStatus { get; set; }
        public string Name { get; internal set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string PoBox { get; set; }

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
    }

    public class CompleteViewModel
    {
        public CompleteViewModel()
        {

        }
        [Required(AllowEmptyStrings=false,ErrorMessage="You must enter a PIN code")]
        [Display(Name = "Enter pin")]
        [Pin()]
        public string PIN { get; set; }
        public bool AllowResend { get; set; }
        public string Remaining { get; set; }
    }

}