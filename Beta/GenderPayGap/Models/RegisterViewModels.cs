using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Extensions;
using GenderPayGap.Models.SqlDatabase;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.Core.Classes;

namespace GenderPayGap.WebUI.Models
{

    public class RegisterViewModel
    {
        public RegisterViewModel()
        {

        }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Job title")]
        public string JobTitle { get; set; }

        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        [Display(Name = "Email address")]
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

        public long UserId { get; set; }
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

        [Required]
        [EnumDataType(typeof(SectorTypes), ErrorMessage = "You must select the type of your organisation")]
        public SectorTypes? SectorType { get; set; }

        [Required]
        [StringLength(100,ErrorMessage = "You must enter an employers name or company number between 3 and 100 characters in length",MinimumLength = 3)]
        [DisplayName("Search")]
        public string SearchText { get; set; }

        public PagedResult<EmployerRecord> Employers { get; set; }

        [MaxLength(100)]
        public string Address1 { get; set; }
        [MaxLength(100)]
        public string Address2 { get; set; }
        [MaxLength(100)]
        public string Address3 { get; set; }
        public string Country { get; set; }
        [MaxLength(20)]
        public string PostCode { get; set; }
        public string PoBox { get; set; }

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
    }

    [Serializable]
    public class EmployerRecord
    {
        public string CompanyNumber { get; set; }
        public string CompanyStatus { get; set; }
        public string Name { get; /*internal*/ set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Address1 { get; set; }
        [MaxLength(100)]
        public string Address2 { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Address3 { get; set; }
        public string Country { get; set; }
        [Required(AllowEmptyStrings =false)]
        [MaxLength(20)]
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