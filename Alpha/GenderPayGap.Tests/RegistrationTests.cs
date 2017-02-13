using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models;
using GpgDB.Models.GpgDatabase;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GenderPayGap.Tests
{
    [TestFixture]
    public class RegistrationTests : AssertionHelper
    {
        #region Test user is enrolled 
        [Test]
        [Description("Ensure IdentityNotMappedException thrown when bad user Id")]
        public void RegisterStep1_IdentityNotMapped_ThrowException()
        {
            // Arrange
            var controller = TestHelper.GetController<RegisterController>(2);
            
            // Act

            // Assert
            Assert.Throws<IdentityNotMappedException>(()=>controller.Step1(), "Expected IdentityNotMappedException");
        }

        [Test]
        [Description("Ensure IdentityNotMapped action returns error view")]
        public void RegisterStep1_IdentityNotMapped_ReturnsView()
        {
            // Arrange
            var controller = TestHelper.GetController<RegisterController>(1);

            // Act
            var result = controller.IdentityNotMapped() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title== "Unauthorised Request", "Invalid error title");
        }


        [Test]
        [Description("Ensure registered users attempting to reregistered when no verify email is sent is prompted to resend")]
        public void RegisterStep1_NoVerifyUserReRegistering_ShowErrorWithReverifyLink()
        {
            // Arrange
            var user = new User() { UserId = 1};

            var controller = TestHelper.GetController<RegisterController>(1, user);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not verified your email address.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("Verify", "Register"), "Invalid error action");
            
        }

        [Test]
        [Description("Ensure registered users attempting to reregistered when old verify email is sent is prompted to resend")]
        public void RegisterStep1_OldVerifyUserReRegistering_ShowErrorWithReverifyLink()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifySendDate=DateTime.Now.AddDays(-7) };

            var controller = TestHelper.GetController<RegisterController>(1, user);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You did not verified your email address within the allowed time.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("Verify", "Register"), "Invalid error action");
        }

        [Test]
        [Description("Ensure registered users attempting to reregistered when verify email is recently sent is prompted to check email bit not allowed to resend")]
        public void RegisterStep1_RecentVerifyUserReRegistering_ShowErrorWithNoReverifyLink()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now.AddHours(-1) };

            var controller = TestHelper.GetController<RegisterController>(1, user);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not verified your email address.", "Invalid error description");
            Assert.Null(model.ActionUrl, "Invalid error action");

        }

        [Test]
        [Description("Ensure users who have verified email but not setup an organisation prompted to continue to that step")]
        public void RegisterStep1_VerifiedUserReRegisteringNoOrg_ShowErrorWithContinueRegisterLink()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now.AddDays(-3),EmailVerifiedDate = DateTime.Now};

            var controller = TestHelper.GetController<RegisterController>(1, user);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not completed the registration process.", "Invalid error description");
            Assert.That(model.CallToAction == "Next Step: Select your organisation","Invalid error call to action");
            Assert.That(model.ActionUrl == controller.Url.Action("Organisation", "Register"), "Invalid error action");

        }



        [Test]
        [Description("Ensure users attempting to reregister when no PIN sent is prompted to send")]
        public void RegisterStep1_NoPinUserReRegistering_ShowErrorWithSendLink()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now, EmailVerifiedDate = DateTime.Now };
            var userOrg =new UserOrganisation() { UserId = 1, OrganisationId = 1};

            var controller = TestHelper.GetController<RegisterController>(1, user,userOrg);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not been sent a PIN in the post.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("Confirm", "Register"), "Invalid error action");

        }

        [Test]
        [Description("Ensure users attempting to reregister when old PIN is sent is prompted to resend")]
        public void RegisterStep1_OldPINReRegister_ShowErrorWithResendLink()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now,EmailVerifiedDate = DateTime.Now};
            var userOrg = new UserOrganisation() { UserId = 1, OrganisationId = 1,PINSentDate = DateTime.Now.AddMonths(-3)};

            var controller = TestHelper.GetController<RegisterController>(1, user,userOrg);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You did not confirm the PIN sent to you in the post in the allowed time.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("Confirm", "Register"), "Invalid error action");
        }

        [Test]
        [Description("Ensure users attempting to reregister when verify email is recently sent is prompted to check mail but not allowed to resend")]
        public void RegisterStep1_RecentPINReRegister_ShowErrorWithNoResendLink()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now, EmailVerifiedDate = DateTime.Now };
            var userOrg = new UserOrganisation() { UserId = 1, OrganisationId = 1, PINSentDate = DateTime.Now.AddHours(-1) };

            var controller = TestHelper.GetController<RegisterController>(1, user,userOrg);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not confirmed the PIN sent to you in the post.", "Invalid error description");
            Assert.Null(model.ActionUrl, "Invalid error action");

        }

        [Test]
        [Description("Ensure registered users attempting to reregister are prompted to submit")]
        public void RegisterStep1_ReRegister_PromptToSubmit()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now, EmailVerifiedDate = DateTime.Now };
            var userOrg = new UserOrganisation() { UserId = 1, OrganisationId = 1, PINSentDate = DateTime.Now.AddHours(-1), PINConfirmedDate = DateTime.Now};

            var controller = TestHelper.GetController<RegisterController>(1, user, userOrg);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Registration Complete", "Invalid error title");
            Assert.That(model.Description == "You have already completed registration.", "Invalid error description");
            Assert.That(model.ActionUrl==controller.Url.Action("Create", "Return"), "Invalid error action");
        }
        #endregion

        #region Test start of enrollment
        [Test]
        [Description("Ensure the ++;.Step1 action returns an empty form when there is no user logged in")]
        public void RegisterStep1_NotLoggedIn_ShowEmptyForm()
        {
            // Arrange
            var controller = TestHelper.GetController<RegisterController>();

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as RegisterViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Step1", "Incorrect view returned");
            Assert.NotNull(model, "Expected RegisterViewModel");
            Assert.Null(model.EmailAddress, "Expected empty email address");
            Assert.Null(model.FirstName, "Expected empty first name");
            Assert.Null(model.LastName, "Expected empty last name");
            Assert.Null(model.JobTitle, "Expected empty job title");
        }
        #endregion

        #region Test enrollment step 1 - send verification email
        #endregion

        #region Test enrollment step 1 - verify email
        #endregion


    }
}