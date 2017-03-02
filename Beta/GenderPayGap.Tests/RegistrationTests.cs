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
using GenderPayGap.WebUI.Classes;
using System.Web.Routing;
using Extensions;

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
            Assert.Throws<IdentityNotMappedException>(() => controller.Step1(), "Expected IdentityNotMappedException");
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
            Assert.That(model.Title == "Unauthorised Request", "Invalid error title");
        }


        [Test]
        [Description("Ensure registered users attempting to reregistered when no verify email is sent is prompted to resend")]
        public void Step1_NoVerifyUserReRegistering_ErrorWithStep2Link()
        {
            // Arrange
            var user = new User() { UserId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");


            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);

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
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now.AddDays(-7) };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);

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

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");


            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);

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
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now.AddDays(-3), EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");


            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not completed the registration process.", "Invalid error description");
            Assert.That(model.CallToAction == "Next Step: Select your organisation", "Invalid error call to action");
            Assert.That(model.ActionUrl == controller.Url.Action("Step2", "Register"), "Invalid error action");

        }

        [Test]
        [Description("Ensure users attempting to reregister when no PIN sent is prompted to send")]
        public void Step1_UserReRegisterNoPinSent_ErrorWithSendPINLink()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now, EmailVerifiedDate = DateTime.Now };
            var userOrg = new UserOrganisation() { UserId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, userOrg);

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
            var user = new User() { UserId = 1, EmailVerifySendDate = DateTime.Now, EmailVerifiedDate = DateTime.Now };
            var userOrg = new UserOrganisation() { UserId = 1, OrganisationId = 1, PINSentDate = DateTime.Now.AddMonths(-3) };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, userOrg);

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

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");


            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, userOrg);

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
            var userOrg = new UserOrganisation() { UserId = 1, OrganisationId = 1, PINSentDate = DateTime.Now.AddHours(-1), PINConfirmedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, userOrg);

            // Act
            var result = controller.Step1() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Registration Complete", "Invalid error title");
            Assert.That(model.Description == "You have already completed registration.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("Step1", "Submit"), "Invalid error action");
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
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("EmailAddress"), false, "Expected email failure");
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
            //ARRANGE:
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

            //ACT:
            //Get the result of the test
            var result = controller.Step1(model) as RedirectToRouteResult;

            //check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step2", "");

            //check that the model stashed preserved with the redirect is equal to what is expected the Arrange values here
            //Retreive the model stashed preserved with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //Check that the unstashed model is not null
            Assert.NotNull(model as RegisterViewModel, "Expected RegisterViewModel");

            //ASSERT:
            // Verify the values from the result that was stashed is equal tothat of the Arrange values here
            Assert.Multiple(() =>
            {
                Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
                Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
                Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
                Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
                Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
                Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
                Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
                Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
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
            Assert.That(!result.ViewData.ModelState.IsValid, "Email compare should have failed");
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

        [Test]
        [Description("Ensure the Step1 succeeds and gets a new registration form for newly authorized users to register")]
        public void Step1_Get_RegistrationComplete_Success()
        {
            //ARRANGE:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };


            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, organisation, userOrganisation);

            //ACT:
            var result = controller.Step1() as RedirectToRouteResult;

            //ASSERT:
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "Expected User registration to be complete");
        }

        //[Test]
        [Description("Ensure the Step1 succeeds and gets a new registration form for newly authorized users to register")]
        public void Step1_Get_NewRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");


            var controller = TestHelper.GetController<RegisterController>(0, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);

            //ACT:
            var result = controller.Step1() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.NotNull(result.Model as RegisterViewModel, "Expected RegisterViewModel");
            Assert.That(result.ViewName == "Step1", "Incorrect view returned");
        }

        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step1_Post_Success()
        {
            /************************************FOR POST_SUCCESS MAINLY REDIRECTTOROUTRESULT************************************/
            //ARRANGE:
            //1.Arrange the test setup variables
            //ACT:
            //2.Run and get the result of the test
            //3.Check that the result is not null
            //4.Check that the redirection went to the right url step.
            //5.If the redirection successfull retreive, the model stash sent with the redirect.
            //6.Check that the unstashed model is not null
            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            /****************************************************************************************************************/

            //ARRANGE:
            //1.Arrange the test setup variables
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

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step1(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step2", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as RegisterViewModel, "Expected RegisterViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.Multiple(() =>
            {
                Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
                Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
                Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
                Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
                Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
                Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
                Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
                Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            });
        }


        [Test]
        [Description("Ensure the Step2 succeeds when all fields are good")]
        public void Step2_Get_RedirectResult_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set the user up as if finished step1 which is email known etc but not sent

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step2");
            routeData.Values.Add("controller", "register");

            var model = new VerifyViewModel();

            //var controller = TestHelper.GetController<RegisterController>();
            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step2(string.Empty) as RedirectToRouteResult;

            //ASSERT:
            //Check the user is return the confirmation view
            //Check the user verifcation is now marked as sent
            //Check a verification has been set against user 
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "");
            //Assert.That(result.RouteValues["action"].ToString() == "Step2", ""); *** will be in the failing tests
        }


        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step2_Get_Step2_Get_ViewResult_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var code = "abcdefg";
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now, EmailVerifyHash = code.GetSHA512Checksum()};
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step2");
            routeData.Values.Add("controller", "register");

            //var model = new VerifyViewModel();

            //var controller = TestHelper.GetController<RegisterController>();
            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step2(Encryption.EncryptQuerystring(code)) as ViewResult;

            //ASSERT:
            //Ensure confimation view is returned
            //Ensure the model is correct
            //ensure uswr is marked as verified
            Assert.NotNull(result as ViewResult, "Expected ViewResult");
            Assert.NotNull(result.Model as VerifyViewModel, "Expected VerifyViewModel");
        }

        [Test]
        [Description("Ensure the Step2 user verification succeeds")]
        public void Step2_Post_Success()
        {

            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //SEt user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("action", "Step2");
            routeData.Values.Add("controller", "register");

            //ARRANGE:
            //1.Arrange the test setup variables
            var model = new VerifyViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.Expired = false;
            model.Resend = false;
            model.Retry = false;
            //Set model as if email

            // model.Sent = true;
            model.UserId = 1;
            model.Verified = true;
            // model.WrongCode = false;

            //var controller = TestHelper.GetController<RegisterController>();
            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step2(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
           // Assert.That(result.RouteValues["action"].ToString() == "Step3", "");
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as VerifyViewModel, "Expected RegisterViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.Multiple(() =>
            {
                //Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
                //Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
                //Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
                //Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
                //Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
                //Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
                //Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
                //Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            });
        }


        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step3_Get_Success()
        {

        }

        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step3_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var model = new OrganisationViewModel();
            //
            //

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step3(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step4", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as OrganisationViewModel, "Expected OrganisationViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.Multiple(() =>
            {
                //Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
                //Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
                //Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
                //Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
                //Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
                //Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
                //Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
                //Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            });
        }


        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step4_Get_Success()
        {
            var controller = TestHelper.GetController<RegisterController>();
            controller.PublicSectorRepository.Insert(new EmployerRecord());

        }

        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step4_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var model = new RegisterViewModel();
            //
            //

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step1(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step5", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as RegisterViewModel, "Expected RegisterViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.Multiple(() =>
            {
                Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
                Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
                Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
                Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
                Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
                Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
                Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
                Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            });
        }


        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step5_Get_Success()
        {

        }

        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step5_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var model = new RegisterViewModel();
            //
            //

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step1(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step6", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as RegisterViewModel, "Expected RegisterViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.Multiple(() =>
            {
                Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
                Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
                Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
                Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
                Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
                Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
                Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
                Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            });
        }



        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step6_Get_Success()
        {

        }

        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step6_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var model = new RegisterViewModel();
            //
            //

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step1(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step7", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as RegisterViewModel, "Expected RegisterViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.Multiple(() =>
            {
                Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
                Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
                Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
                Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
                Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
                Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
                Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
                Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            });
        }



        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step7_Get_Success()
        {

        }

        [Test]
        [Description("Ensure the Step1 succeeds when all fields are good")]
        public void Step7_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var model = new RegisterViewModel();
            //
            //

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step1(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step2", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as RegisterViewModel, "Expected RegisterViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.Multiple(() =>
            {
                Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
                Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
                Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
                Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
                Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
                Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
                Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
                Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            });
        }



    }
}