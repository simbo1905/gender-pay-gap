using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models;
using GenderPayGap.Models.SqlDatabase;
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
        public void Step1_IdentityNotMapped_ThrowException()
        {
            // Arrange
            var controller = TestHelper.GetController<RegisterController>(2);
            
            // Act

            // Assert
            Assert.Throws<IdentityNotMappedException>(()=>controller.Step1(), "Expected IdentityNotMappedException");
        }

        [Test]
        [Description("Ensure IdentityNotMapped action returns error view")]
        public void Step1_IdentityNotMapped_ReturnsView()
        {
            // Arrange
            var controller = TestHelper.GetController<ErrorController>(1);

            // Act
            var result = controller.Default(404) as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title== "Unauthorised Request", "Invalid error title");
        }


        [Test]
        [Description("Ensure registered users attempting to reregistered when no verify email is sent is prompted to resend")]
        public void Step1_NoVerifyUserReRegistering_ErrorWithStep2Link()
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
            Assert.That(model.ActionUrl == controller.Url.Action("Step2", "Register"), "Invalid error action");
            
        }

        [Test]
        [Description("Ensure registered users attempting to reregistered when old verify email is sent is prompted to resend")]
        public void Step1_OldVerifyUserReRegistering_ShowErrorWithReverifyLink()
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
            Assert.That(model.ActionUrl == controller.Url.Action("Step2", "Register"), "Invalid error action");
        }

        [Test]
        [Description("Ensure registered users attempting to reregistered when verify email is recently sent is prompted to check email bit not allowed to resend")]
        public void Step1_RecentVerifyUserReRegistering_ShowErrorWithNoReverifyLink()
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
        public void Step1_VerifiedUserReRegisteringNoOrg_ShowErrorWithContinueRegisterLink()
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
            Assert.That(model.ActionUrl == controller.Url.Action("Step2", "Register"), "Invalid error action");

        }



        [Test]
        [Description("Ensure users attempting to reregister when no PIN sent is prompted to send")]
        public void Step1_UserReRegisterNoPinSent_ErrorWithSendPINLink()
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
            Assert.That(model.ActionUrl == controller.Url.Action("SendPIN", "Register"), "Invalid error action");

        }

        [Test]
        [Description("Ensure users attempting to reregister when old PIN is sent is prompted to resend")]
        public void Step1_OldPINReRegister_ShowErrorWithResendLink()
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
            Assert.That(model.ActionUrl == controller.Url.Action("ConfirmPIN", "Register"), "Invalid error action");
        }

        [Test]
        [Description("Ensure users attempting to reregister when verify email is recently sent is prompted to check mail but not allowed to resend")]
        public void Step1_RecentPINReRegister_ShowErrorWithNoResendLink()
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
        public void Step1_ReRegister_PromptToSubmit()
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
            Assert.That(model.ActionUrl==controller.Url.Action("Step1", "Submit"), "Invalid error action");
        }
        #endregion

        #region Test start of enrollment
        [Test]
        [Description("Ensure the Step1 action returns an empty form when there is no user logged in")]
        public void Step1_NotLoggedIn_ShowEmptyForm()
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

        [Test]
        [Description("Ensure the Step1 fails when any field is empty")]
        public void Step1_EmptyFields_ShowAllErrors()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "";
            model.ConfirmEmailAddress = " ";
            model.FirstName = "";
            model.LastName = "";
            model.JobTitle = "";
            model.Password = "";
            model.ConfirmPassword = " ";


            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;
            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step1", "Incorrect view returned");
                Assert.NotNull(result.Model as RegisterViewModel, "Expected RegisterViewModel");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("EmailAddress"), false,"Expected email failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("ConfirmEmailAddress"), false, "Expected confirm email failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), false, "Expected first name failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("LastName"), false, "Expected last name failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), false, "Expected jobtitle failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("Password"), false, "Expected password failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("ConfirmPassword"), false, "Expected confirm password failure");
            });
        }

        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step1_GoodFields_NoError()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "P@ssword1!";
            model.ConfirmPassword = "P@ssword1!";

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step1", "Incorrect view returned");
                Assert.NotNull(result.Model as RegisterViewModel, "Expected RegisterViewModel");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("EmailAddress"), true, "Expected email success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("ConfirmEmailAddress"), true, "Expected confirm email success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), true, "Expected first name success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("LastName"), true, "Expected last name success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), true, "Expected jobtitle success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("Password"), true, "Expected password success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("ConfirmPassword"), true, "Expected confirm password success");
            });
        }

        [Test]
        [Description("Ensure the Step1 fails when email and confirmation mismatch")]
        public void Step1_EmailMismatch_ShowError()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test1@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "P@ssword11!";
            model.ConfirmPassword = "P@ssword11!";

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid,"Email compare should have failed");
        }

        [Test]
        [Description("Ensure the Step1 fails when password and confirmation dont match")]
        public void Step1_PasswordMismatch_ShowError()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "P@ssword1!";
            model.ConfirmPassword = "P@ssword11!";

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password compare should have failed");
        }

        [Test]
        [Description("Ensure the Step1 fails when password is too short")]
        public void Step1_ShortPassword_ShowError()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "Passwor";
            model.ConfirmPassword = "Passwor";

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Short password compare should have failed");
        }

        [Test]
        [Description("Ensure the Step1 fails when password contains 'password'")]
        public void Step1_PasswordContainsPassword_ShowError()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "Password1";
            model.ConfirmPassword = "Password1";

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password containing 'password' should have failed");
        }

        [Test]
        [Description("Ensure the Step1 fails when a bad password expression is entered")]
        public void Step1_BadPasswordExpression_ShowError()
        {
            // Arrange
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "TestFirstName";
            model.LastName = "TestLastName";
            model.JobTitle = "TestJobTitle";
            model.Password = "p@ssword";
            model.ConfirmPassword = "P@ssword";

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password expression should have failed");
        }

        #endregion

        #region Test enrollment step 1 - send verification email
        #endregion

        #region Test enrollment step 1 - verify email
        #endregion


    }
}