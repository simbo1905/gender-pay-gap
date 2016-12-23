using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GenderPayGap;
using Extensions;
using GenderPayGap.Models;
using Newtonsoft.Json;
using IdentityServer3.Core;

namespace GenderPayGap.Controllers
{
    public class RegisterController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {

            //The user can then go through the process of changing their details and email then sending another verification email

            var currentUser = GetCurrentUser();
            var model = new Models.RegisterViewModel(currentUser);

            if (currentUser == null)
            {
                model.EmailAddress = Models.GpgDatabase.User.GetUserClaim(User, Constants.ClaimTypes.Email);
                model.FirstName= Models.GpgDatabase.User.GetUserClaim(User, Constants.ClaimTypes.GivenName);
                model.LastName= Models.GpgDatabase.User.GetUserClaim(User, Constants.ClaimTypes.FamilyName);
            }
            model.ConfirmEmailAddress = model.EmailAddress;
            if (currentUser!=null) ViewData["currentUser"] = currentUser;
            
            //The user can then go through the process of changing their details and email then sending another verification email
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(Models.RegisterViewModel model)
        {
            //TODO validate the submitted fields
            if (!ModelState.IsValid)return View(model);
            model.EmailAddress = model.EmailAddress.ToLower();

            var currentUser = GetCurrentUser();

            if (currentUser == null) currentUser = GpgDatabase.Default.User.FirstOrDefault(u => u.EmailAddress == model.EmailAddress);

            if (currentUser == null) currentUser = new Models.GpgDatabase.User();

            if (currentUser.UserId==0)
            {
                //Check the email doesnt already exist
                if (GpgDatabase.Default.User.Any(u => u.EmailAddress==model.EmailAddress))
                {
                    ModelState.AddModelError("EmailAddress", "A user with this email already exists");
                    return View(model);
                }
            }

            else if (currentUser.EmailVerifiedDate>DateTime.MinValue)
            {
                //Go to the verification process
                return RedirectToAction("Verify", new { code = currentUser.EmailVerifyCode });
            }

            //Save the submitted fields
            currentUser.Firstname = model.FirstName;
            currentUser.Lastname = model.LastName;
            currentUser.JobTitle = model.JobTitle;
            currentUser.EmailAddress = model.EmailAddress;
            currentUser.Password = model.Password;
            currentUser.EmailVerifiedDate = null;

            //Save the user to DB
            if (currentUser.UserId==0)GpgDatabase.Default.User.Add(currentUser);
            GpgDatabase.Default.SaveChanges();

            var authProviderId = Models.GpgDatabase.User.GetUserClaim(User, Constants.ClaimTypes.IdentityProvider);
            var tokenIdentifier = Models.GpgDatabase.User.GetUserClaim(User, Constants.ClaimTypes.ExternalProviderUserId);

            if (authProviderId.EqualsI("google"))
            {
                var token = GpgDatabase.Default.UserTokens.FirstOrDefault(ut=>ut.AuthProviderId==authProviderId);
                if (token == null)
                {
                    token = new UserToken()
                    {
                        UserId= currentUser.UserId,
                        AuthProviderId = authProviderId,
                        TokenIdentifier = tokenIdentifier,
                        Created = DateTime.Now
                    };
                    GpgDatabase.Default.UserTokens.Add(token);

                }
                if (model.EmailAddress == Models.GpgDatabase.User.GetUserClaim(User, Constants.ClaimTypes.Email))
                    currentUser.EmailVerifiedDate = DateTime.Now;

            }
            GpgDatabase.Default.SaveChanges();

            var verifyCode = Encryption.EncryptQuerystring(currentUser.UserId.ToString());
            if (currentUser.EmailVerifiedDate > DateTime.MinValue)
                return RedirectToAction("Organisation",new {code=verifyCode });

            //Send a verification link to the email address
            try
            {
                if (!GovNotifyAPI.SendVerifyEmail(currentUser.EmailAddress, verifyCode))
                    throw new Exception("Could not send verification email. Please try again later.");

                currentUser.EmailVerifyCode = verifyCode;
                currentUser.EmailVerifySendDate = DateTime.Now;
                GpgDatabase.Default.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.Message.EqualsI("This Email Address is not registered with Gov Notify."))
                    ModelState.AddModelError("EmailAddress", ex.Message);
                else
                    ModelState.AddModelError(string.Empty, ex.Message);
            }

            //Set the verification link and save it to the db
            ViewData["currentUser"] = currentUser;

            //Pass the verify url back to the view for confirmation
            model.VerifyUrl = string.IsNullOrWhiteSpace(currentUser.EmailVerifyCode) ? null : GovNotifyAPI.GetVerifyUrl(currentUser.EmailVerifyCode);

            //Prompt user to click verification link
            return View(model);
        }


        [HttpGet]
        public ActionResult Verify(string code=null)
        {
            if (string.IsNullOrWhiteSpace(code))code = Request.Url.Query.TrimStartI(" ?");

            //Load the user from the verification code
            var currentUser = GpgDatabase.Default.User.FirstOrDefault(u=>u.EmailVerifyCode==code);

            var model = new VerifyViewModel();

            //Show an error if the code doesnt exist in db
            if (currentUser == null)
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }

            if (currentUser.EmailVerifySendDate<DateTime.Now.AddDays(-6))
            {
                //TODO Resend verification code and prompt user
                ModelState.AddModelError("", "This verification link has expired. A new link has been sent to " + currentUser.EmailAddress);
                return View(model);
            }

            try
            {
                code = Encryption.DecryptQuerystring(code);
            }
            catch 
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }

            //Show an error if the code doesnt match the userId
            if (currentUser.UserId != code.ToLong())
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }

            //Set the user as verified
            currentUser.EmailVerifiedDate = DateTime.Now;

            //Save the current user
            GpgDatabase.Default.SaveChanges();
            ViewData["currentUser"] = currentUser;

            //Take the user through the process of lookup address and send pin code

            model.UserId = currentUser.UserId;
            return View(model);
        }

        [HttpPost]
        public ActionResult Verify(VerifyViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public ActionResult Organisation(string code=null)
        {
            if (string.IsNullOrWhiteSpace(code)) code = Request.Url.Query.TrimStartI(" ?");

            //Load the user from the verification code
            var currentUser = GpgDatabase.Default.User.FirstOrDefault(u => u.EmailVerifyCode == code);

            var model = new OrganisationViewModel();
            
            //Show an error if the code doesnt exist in db
            if (currentUser == null)
            {
                ModelState.AddModelError("", "Invalid request");
                return View(model);
            }

            try
            {
                code = Encryption.DecryptQuerystring(code);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }

            //Show an error if the code doesnt match the userId
            if (currentUser.UserId != code.ToLong())
            {
                ModelState.AddModelError("", "Invalid email verification code");
                return View(model);
            }
            model.UserId = currentUser.UserId;

            if (currentUser != null) ViewData["currentUser"] = currentUser;

            return View(model);
        }

        [HttpPost]
        public ActionResult Organisation(OrganisationViewModel model)
        {
            if (model == null || model.UserId==0)
            {
                ModelState.AddModelError("", "Invalid request");
                return View(model);
            }

            //TODO validate the submitted fields
            if (!ModelState.IsValid) return View(model);

            var currentUser = GetCurrentUser();
            if (currentUser == null && model.UserId > 0) currentUser = GpgDatabase.Default.User.Find(model.UserId);


            if (model.OrganisationType != Models.GpgDatabase.Organisation.OrgTypes.Unknown)
            {
                if (string.IsNullOrWhiteSpace(model.OrganisationRef))
                {
                    switch (model.OrganisationType)
                    {
                        case Models.GpgDatabase.Organisation.OrgTypes.Company:
                            ModelState.AddModelError("OrganisationRef", "You must enter your company number");
                            break;
                        case Models.GpgDatabase.Organisation.OrgTypes.Charity:
                            ModelState.AddModelError("OrganisationRef", "You must enter your charity number");
                            break;
                        case Models.GpgDatabase.Organisation.OrgTypes.Government:
                            ModelState.AddModelError("OrganisationRef", "You must enter your department reference");
                            break;
                    }
                    return View(model);
                }
                else if (string.IsNullOrWhiteSpace(model.OrganisationName))
                {
                    var org=GpgDatabase.Default.Organisation.FirstOrDefault(o=>o.OrganisationType==model.OrganisationType && o.OrganisationRef== model.OrganisationRef);
                    //Lookup the company details
                    var company = CompaniesHouseAPI.Lookup(model.OrganisationRef);

                    if (org == null) org = new Organisation();
                    
                    //Save the new company                        
                    org.OrganisationType = model.OrganisationType;
                    org.OrganisationRef = model.OrganisationRef;
                    org.OrganisationName = company.company_name;
                    if (org.OrganisationId==0)GpgDatabase.Default.Organisation.Add(org);
                    GpgDatabase.Default.SaveChanges();

                    var address = GpgDatabase.Default.OrganisationAddress.FirstOrDefault(o => o.OrganisationId == org.OrganisationId);
                    if (address==null) address = new OrganisationAddress();
                    address.OrganisationId = org.OrganisationId;
                    address.Address1 = company.registered_office_address.address_line_1;
                    address.Address2 = company.registered_office_address.address_line_2;
                    address.Address3 = company.registered_office_address.locality;
                    address.Country = company.registered_office_address.country;
                    address.PostCode = company.registered_office_address.postal_code;
                    if (address.OrganisationAddressId==0)GpgDatabase.Default.OrganisationAddress.Add(address);
                    GpgDatabase.Default.SaveChanges();
                    
                    model.OrganisationId = org.OrganisationId;
                    model.OrganisationName = org.OrganisationName;
                    model.OrganisationAddress = address.GetAddress();
                }
                else if (string.IsNullOrWhiteSpace(model.ConfirmUrl))
                {
                    //TODO Send the PIN and confirm when sent
                    var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == model.OrganisationId && uo.UserId == model.UserId);
                    if (userOrg == null)
                    {
                        userOrg = new Models.GpgDatabase.UserOrganisation()
                        {
                            UserId = model.UserId,
                            OrganisationId = model.OrganisationId,
                            Created = DateTime.Now
                        };
                        GpgDatabase.Default.UserOrganisations.Add(userOrg);
                        GpgDatabase.Default.SaveChanges();
                    }

                    //Send a PIN link to the email address
                    try
                    {
                        var pin = Numeric.Rand(0,999999);
                        if (!GovNotifyAPI.SendPinInPost(currentUser.Fullname + " ("+currentUser.JobTitle + ")",currentUser.EmailAddress, pin.ToString()))
                            throw new Exception("Could not send PIN in the POST. Please try again later.");

                        //Send a confirmation link to the email address
                        var confirmCode = Encryption.EncryptQuerystring(string.Format("{0}:{1}", userOrg.UserId, userOrg.OrganisationId));
                        if (!GovNotifyAPI.SendConfirmEmail(currentUser.EmailAddress, confirmCode))
                            throw new Exception("Could not send confirmation email. Please try again later.");

                        userOrg.PINCode = pin;
                        userOrg.PINSentDate = DateTime.Now;
                        userOrg.ConfirmCode = confirmCode;
                        GpgDatabase.Default.SaveChanges();
                        model.ConfirmUrl = GovNotifyAPI.GetConfirmUrl(confirmCode);
                        model.PIN = pin;

                        model.UserName = currentUser.Fullname;
                        model.UserTitle = currentUser.JobTitle;
                        model.OrganisationAddressHtml = model.OrganisationAddress.ReplaceI(", ","<br/>");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Confirm(string code = null, long pin=0)
        {
            UserOrganisation userOrg=null;
            var currentUser = GetCurrentUser();
            if (string.IsNullOrWhiteSpace(code))
            {
                if (currentUser == null)
                {
                    ModelState.AddModelError("", "Invalid user");
                    return View(code);
                }
                userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(uo => uo.UserId == currentUser.UserId && uo.PINSentDate > DateTime.MinValue && uo.PINConfirmedDate == null);
                if (userOrg!=null)code = userOrg.ConfirmCode;
            }
            if (string.IsNullOrWhiteSpace(code)) code = Request.Url.Query.TrimStartI(" ?");

            //Load the user from the verification code
            if (userOrg == null) userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(u => u.ConfirmCode == code);

            var model = new ConfirmViewModel();
            model.Default = pin>0 ? pin.ToString() : null;
            model.ConfirmCode = code;

            //Show an error if the code doesnt exist in db
            if (userOrg == null)
            {
                ModelState.AddModelError("", "Invalid confirmation link");
                return View(model);
            }

            var user = GpgDatabase.Default.User.Find(userOrg.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid confirmation link");
                return View(model);
            }

            if (currentUser!=null && currentUser.UserId != user.UserId)
            {
                ModelState.AddModelError("", "Invalid confirmation link");
                return View(model);
            }

            if (userOrg.PINSentDate < DateTime.Now.AddDays(-6))
            {
                //TODO Resend verification code and prompt user
                ModelState.AddModelError("", "This confirmation link has expired. A new link has been sent to " + currentUser.EmailAddress);
                return View(model);
            }

            try
            {
                code = Encryption.DecryptQuerystring(code);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid confirmation code");
                return View(model);
            }

            //Show an error if the code doesnt match the userId
            if (!string.Format("{0}:{1}", userOrg.UserId, userOrg.OrganisationId).EqualsI(code,Server.UrlDecode(code)))
            {
                ModelState.AddModelError("", "Invalid confirmation code");
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Confirm(ConfirmViewModel model)
        {
            var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(u => u.ConfirmCode == model.ConfirmCode);

            if (model.PIN != userOrg.PINCode)
            {
                ModelState.AddModelError("PIN", "Invalid PIN code");
                return View(model);
            }
            userOrg.PINConfirmedDate = DateTime.Now;
            model.confirmed = true;
            //Save the current user
            GpgDatabase.Default.SaveChanges();
            return View(model);
        }
    }
}