using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GpgDB;
using GpgDB.Models.GpgDatabase;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using static GpgDB.Models.GpgDatabase.Organisation;

namespace GenderPayGap.WebUI.Models
{

    public class RegisterViewModel
    {
        public RegisterViewModel()
        {

        }

        public RegisterViewModel(User currentUser)
        {
            if (currentUser != null)
            {
                this.FirstName = currentUser.Firstname;
                this.LastName = currentUser.Lastname;
                this.JobTitle = currentUser.JobTitle;
                this.EmailAddress = currentUser.EmailAddress;
                this.EmailAddress = currentUser.EmailAddress;
                this.Password = currentUser.Password;
                this.ConfirmPassword = currentUser.Password;
            }
        }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Confirm your email address")]
        [Compare("EmailAddress", ErrorMessage = "The email address and confirmation do not match.")]
        public string ConfirmEmailAddress { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

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
        public string EmailAddress { get; set; }
    }

    public class OrganisationViewModel
    {
        public bool PINSent;

        public OrganisationViewModel()
        {

        }

        public OrganisationViewModel(Organisation organisation)
        {
            if (organisation != null)
            {
                this.SectorType = organisation.SectorType;
                this.OrganisationRef = organisation.OrganisationRef;
            }
        }

        [Required]
        [EnumDataType(typeof(SectorTypes), ErrorMessage = "You must select the type of your organisation")]
        public SectorTypes? SectorType { get; set; }

        [Required]
        [StringLength(100,ErrorMessage = "You must enter an employers name or company number between 3 and 100 characters in length",MinimumLength = 3)]
        [DisplayName("Search")]
        public string SearchText { get; set; }

        public string OrganisationRef { get; set; }
        public string OrganisationName { get; set; }

        public long OrganisationId { get; set; }

        public string OrganisationAddress { get; set; }
        public string OrganisationAddressHtml { get; set; }
        public string UserName { get; set; }
        public string UserTitle { get; set; }
        public long UserId { get; set; }

        public string ConfirmUrl { get; set; }
        public long PIN { get; set; }
    }

    public class ConfirmViewModel
    {
        public ConfirmViewModel()
        {

        }

        [Display(Name = "Enter pin")]
        public long PIN { get; set; }
        public string ConfirmCode { get; set; }
        public string Default { get; set; }

        public bool confirmed = false;

    }

}