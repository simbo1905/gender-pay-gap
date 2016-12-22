using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Models.GpgDatabase;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using static GenderPayGap.Models.GpgDatabase.Organisation;

namespace GenderPayGap.Models
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

    }

    public class VerifyViewModel
    {
        public VerifyViewModel()
        {

        }

        public long UserId { get; set; }
    }

    public class OrganisationViewModel
    {
        public OrganisationViewModel()
        {

        }

        public OrganisationViewModel(Organisation organisation)
        {
            if (organisation != null)
            {
                this.OrganisationType = organisation.OrganisationType;
                this.OrganisationRef = organisation.OrganisationRef;
            }
        }

        public OrgTypes OrganisationType { get; set; }

        public string OrganisationRef { get; set; }

        public string OrganisationName { get; set; }

        public string OrganisationAddress { get; set; }
        public long UserId { get; set; }
    }

}