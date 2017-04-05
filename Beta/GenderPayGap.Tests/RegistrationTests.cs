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
using GenderPayGap.Core.Classes;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Properties;

namespace GenderPayGap.Tests
{
    [TestFixture]
    public class RegistrationTests : AssertionHelper
    {
        #region Test user is enrolled 
  //    [Test]
        [Description("Ensure IdentityNotMappedException thrown when bad user Id")]
        public void Step1_IdentityNotMapped_ThrowException()
        {
            // Arrange
            var controller = TestHelper.GetController<RegisterController>(2);

            // Act

            // Assert
            Assert.Throws<IdentityNotMappedException>(() => controller.AboutYou(), "Expected IdentityNotMappedException");
        }

  //    [Test]
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


  //    [Test]
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
            var result = controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not verified your email address.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("Step2", "Register"), "Invalid error action");

        }

  //    [Test]
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
            var result = controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You did not verified your email address within the allowed time.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("Step2", "Register"), "Invalid error action");
        }

  //    [Test]
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
            var result = controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not verified your email address.", "Invalid error description");
            Assert.Null(model.ActionUrl, "Invalid error action");

        }

  //    [Test]
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
            var result = controller.AboutYou() as ViewResult;
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

  //    [Test]
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
            var result = controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not been sent a PIN in the post.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("SendPIN", "Register"), "Invalid error action");

        }

  //    [Test]
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
            var result = controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You did not confirm the PIN sent to you in the post in the allowed time.", "Invalid error description");
            Assert.That(model.ActionUrl == controller.Url.Action("ConfirmPIN", "Register"), "Invalid error action");
        }

  //    [Test]
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
            var result = controller.AboutYou() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "Error", "Incorrect view returned");
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.Title == "Incomplete Registration", "Invalid error title");
            Assert.That(model.Description == "You have not confirmed the PIN sent to you in the post.", "Invalid error description");
            Assert.Null(model.ActionUrl, "Invalid error action");

        }

   //   [Test]
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
            var result = controller.AboutYou() as ViewResult;
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
  //    [Test]
        [Description("Ensure the Step1 action returns an empty form when there is no user logged in")]
        public void Step1_NotLoggedIn_ShowEmptyForm()
        {
            // Arrange
            var controller = TestHelper.GetController<RegisterController>();

            // Act
            var result = controller.AboutYou() as ViewResult;
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

  //    [Test]
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
            var result = controller.AboutYou(model) as ViewResult;
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

  //    [Test]
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
            var result = controller.AboutYou(model) as RedirectToRouteResult;

            //check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step2", "Redirected to the wrong view");

            //check that the model stashed preserved with the redirect is equal to what is expected the Arrange values here
            //Retreive the model stashed preserved with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //Check that the unstashed model is not null
            Assert.NotNull(model, "Expected RegisterViewModel");

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

  //    [Test]
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
            var result = controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Email compare should have failed");
        }

  //   [Test]
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
            var result = controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password compare should have failed");
        }

  //    [Test]
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
            var result = controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Short password compare should have failed");
        }

  //    [Test]
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
            var result = controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password containing 'password' should have failed");
        }

  //    [Test]
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
            var result = controller.AboutYou(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password expression should have failed");
        }

        #endregion

        #region Test enrollment step 1 - send verification email
        #endregion

        #region Test enrollment step 1 - verify email
        #endregion


       

       // [Test]
        [Description("Ensure the Step1 succeeds and gets a new registration form for newly authorized users to register")]
        public void AboutYou_Get_RegistrationComplete_Success()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            var user = new User() { UserId = 0 };

            var routeData = new RouteData();
            routeData.Values.Add("action", "AboutYou");
            routeData.Values.Add("Controller", "register");

            //Stash an object to pass in for  this.ClearStash()
            var controller = TestHelper.GetController<RegisterController>(0, routeData, user = null);

            //ACT:
            var result = controller.AboutYou() as RedirectToRouteResult;

            //ASSERT:
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "Expected User registration to be complete");
        }

        #region AboutYou

        #region Positive tests
        [Test]
        [Description("Ensure that a new registration form is returned for a user to register")]
        public void AboutYou_Get_NewRegistration_Success()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            var user = new User() { UserId = 0 };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AboutYou");
            routeData.Values.Add("Controller", "Register");

            //Stash an object to pass in for this.ClearStash()
            //var model = new RegisterViewModel();
            var controller = TestHelper.GetController<RegisterController>(0, routeData, user = null/*, model*/);
            //controller.StashModel(model);

            //ACT:
            var result = controller.AboutYou() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "AboutYou", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(RegisterViewModel), "Expected RegisterViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

        }

        [Test]
        [Description("Ensure that the new registration form filled with correct values is sent successfully when all fields values are valid")]
        public void AboutYou_Post_Success()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            //var user = new User() { UserId = 0 };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AboutYou");
            routeData.Values.Add("Controller", "Register");


            //1.Arrange the test setup variables
            var model = new RegisterViewModel()
            {
                EmailAddress = "test@hotmail.com",
                ConfirmEmailAddress = "test@hotmail.com",
                FirstName = "Kingsley",
                LastName = "Eweka",
                JobTitle = "Dev",
                Password = "K1ngsl3y3w3ka",
                ConfirmPassword = "K1ngsl3y3w3ka"
            };

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AboutYou(model) as RedirectToRouteResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "VerifyEmail", "Expected a RedirectToRouteResult to VerifyEmail");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model, "Expected RegisterViewModel");

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
        #endregion

        #region Negative tests

        [Test]
        [Description("Ensure the Step1 fails when a user does not exist in the db")]
        public void AboutYou_Get_UnAuthorisedUser_Fail()
        {
            //not really working see comment below on routedata
            //ARRANGE:
            //create a user who does not exist in the db
            var user = new User() { UserId = 0 };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1"); //AboutYou fix here to make sure HttpUnauthorizedResult is returned!
            routeData.Values.Add("controller", "register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            //ACT:
            var result = controller.AboutYou() as ActionResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected an ActionResult object");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.GetType() == typeof(HttpUnauthorizedResult), "expected: user should not have been authorised");
        }

        #endregion

        #endregion

        #region VerifyEmail

        #region Positive tests
        [Test]
        [Description("Ensure the Step2 succeeds when is verified and an email is sent")]
        public void VerifyEmail_Get_ViewResult_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var code = "abcdefg";
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = null, EmailVerifySendDate = null, EmailVerifyHash = code.GetSHA512Checksum(), Status = UserStatuses.New, Organisations = null };

            var verifiedModel = new VerifyViewModel() { Sent = true };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "VerifyEmail");
            routeData.Values.Add("Controller", "Register");

            //simulate a model to stash
            var model = new RegisterViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.ConfirmEmailAddress = "test@hotmail.com";
            model.FirstName = "Kingsley";
            model.LastName = "Eweka";
            model.JobTitle = "Dev";
            model.Password = "K1ngsl3y3w3ka";
            model.ConfirmPassword = "K1ngsl3y3w3ka";

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, verifiedModel);
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.VerifyEmail(Encryption.EncryptQuerystring(code)) as ViewResult;

            var resultModel = result.Model as VerifyViewModel;

            result.ViewData.ModelState.Clear();
            resultModel.Sent = (controller.DataRepository.GetAll<VerifyViewModel>().FirstOrDefault(v => v.Sent)).Sent;

            //ASSERT:
            //Ensure confirmation view is returned
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "VerifyEmail", "Incorrect view returned");

            //Ensure the model is not null and it is correct
            Assert.NotNull(result.Model as VerifyViewModel, "Expected VerifyViewModel");
            Assert.That(result.ViewData.ModelState.IsValid, " Model is not valid");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("EmailAddress"), "Email is not a match or is invalid");

            //ensure user is marked as verified
            Assert.AreEqual(resultModel.Sent, true, "Expected VerifyViewModel");
        }


        [Test]
        [Description("Ensure the Step2 succeeds when all fields are good")]
        public void VerifyEmail_Get_RedirectResult_Success() //Registration complete
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var code = "abcdefg";
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = null, EmailVerifyHash = code.GetSHA512Checksum() };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set the user up as if finished step1 which is email known etc but not sent
            var routeData = new RouteData();
            routeData.Values.Add("Action", "VerifyEmail");
            routeData.Values.Add("Controller", "Register");

            var model = new VerifyViewModel();

            //var controller = TestHelper.GetController<RegisterController>();
            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.VerifyEmail(Encryption.EncryptQuerystring(code)) as RedirectToRouteResult;

            //ASSERT:
            //Check the user is return the confirmation view
            //Check the user verifcation is now marked as sent
            //Check a verification has been set against user 
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "Registration is not complete!");

        }

        [Test]
        [Description("Ensure the Step2 user verification succeeds")]
        public void VerifyEmail_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = null };

            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "VerifyEmail");
            routeData.Values.Add("Controller", "Register");

            //ARRANGE:
            //1.Arrange the test setup variables
            var model = new VerifyViewModel();
            model.EmailAddress = "test@hotmail.com";
            model.Resend = false;

            //Set model as if email

            // model.Sent = true;
            model.UserId = 1;

            // model.WrongCode = false;

            //var controller = TestHelper.GetController<RegisterController>();
            var controller = TestHelper.GetController<RegisterController>(1, routeData, user/*, userOrganisation*/);
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.VerifyEmail(model) as RedirectToRouteResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            // Assert.That(result.RouteValues["action"].ToString() == "Step3", "");
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "Registration is incomplete");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            //  var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            //  Assert.NotNull(model, "Expected RegisterViewModel");
        }
        #endregion

        #region Negative tests

        #endregion

        #endregion

        #region OrganisationType

        #region Positive tests

        [Test]
        [Description("Ensure the Organisation type form is returned for the current user ")]
        public void OrganisationType_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            //ACT:
            var result = controller.OrganisationType() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "OrganisationType", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Private Sector:Ensure the Organisation type form is confirmed and sent successfully")]
        public void OrganisationType_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };
            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel()
                            {
                                ManualRegistration = false,
                                SectorType = SectorTypes.Private
                            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user/*, userOrganisation, organisation*/);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.OrganisationType(model) as RedirectToRouteResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "OrganisationSearch", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(model.SectorType == SectorTypes.Private, true, "Expected equal object entities success");
            Assert.AreEqual(model.ManualRegistration == unStashedmodel.ManualRegistration, true, "Expected equal object entities success");

            
        }

        [Test]
        [Description("Public Sector:Ensure the Organisation type form is confirmed and sent successfully")]
        public void OrganisationType_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel()
                            {
                                ManualRegistration = false,
                                SectorType = SectorTypes.Public
                            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.OrganisationType(model) as RedirectToRouteResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "OrganisationSearch", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(model.SectorType == SectorTypes.Public, true, "Expected equal object entities success");
        }
        #endregion

        #region Negative tests

        #endregion

        #endregion

        #region OrganisationSearch

        #region Positive tests
        [Test]
        [Description("Ensure the Organisation search form is returned for the current user ")]
        public void OrganisationSearch_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel() { ManualRegistration = false };
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.OrganisationSearch() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "OrganisationSearch", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            // var controller = TestHelper.GetController<RegisterController>();
            // controller.PublicSectorRepository.Insert(new EmployerRecord());
        }

       // [Ignore("This test needs fixing")]
        [Test]
        [Description("Ensure that organisation search form has a search text in its field sent successfully and a a matching record is returned")]
        public void OrganisationSearch_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };
            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
            
            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            //search text in model
            var model = new OrganisationViewModel()
                            {
                                Employers = new PagedResult<EmployerRecord>() { },
                                SearchText = "smith ltd",
                                ManualRegistration = false,
                                SectorType = SectorTypes.Private,
                                CompanyNumber = "456GT657",
                                Country = "UK",
                                PostCode = "nw1 5re"
                            };


            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //insert  some records into the db...
            controller.PrivateSectorRepository.Insert( new EmployerRecord() { Name = "acme inc", Address1 = "123", Address2 = "EverGreen Terrace",
                                                           CompanyNumber = "123QA432", CompanyStatus = "Active",  Country = "UK", PostCode = "e12 3eq" }
                                                     );

            controller.PrivateSectorRepository.Insert( new EmployerRecord() { Name = "smith ltd", Address1 = "45", Address2 = "iverson rd",
                                                           CompanyNumber = "456GT657", CompanyStatus = "Active", Country = "UK", PostCode = "nw1 5re" }
                                                     );

             controller.PrivateSectorRepository.Insert( new EmployerRecord() { Name = "smith & Wes ltd", Address1 = "45", Address2 = "iverson rd",
                                                           CompanyNumber = "456GT657", CompanyStatus = "Active", Country = "UK", PostCode = "nw1 5re" }
                                                     );

             controller.PrivateSectorRepository.Insert( new EmployerRecord() { Name = "smithers and sons ltd", Address1 = "45", Address2 = "iverson rd",
                                                            CompanyNumber = "456GT657", CompanyStatus = "Active", Country = "UK", PostCode = "nw1 5re" }
                                                     );

            controller.PrivateSectorRepository.Insert( new EmployerRecord() { Name = "excetera ltd", Address1 = "123", Address2 = "Venice avenue ",
                                                           CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "w1 9eaz" }
                                                     );

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.OrganisationSearch(model) as RedirectToRouteResult;

            //3.If the redirection successfull retrieve the model stash sent with the redirect.
            //returned from the MockPrivateEmployerRepository db then stashed and then unstashed
            var resultUnStashedModel = controller.UnstashModel<OrganisationViewModel>();

            //check that the search returned a match in the db
            //var sResult     = controller.DataRepository.GetAll<OrganisationViewModel>().Where(o => o.CompanyNumber == resultModel.CompanyNumber);
            //var pagedResult =  controller.PrivateSectorRepository.Search(model.SearchText, 1, Settings.Default.EmployerPageSize);

            //ASSERT:
            //4.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "ChooseOrganisation", "Redirected to the wrong view");

            //5.check that the stashed model with the redirect is not null.
            Assert.NotNull(resultUnStashedModel, "Expected OrganisationViewModel");

            //check that the model stashed matched what was unstanshed entity wise.
            Assert.AreEqual(model == resultUnStashedModel, true, "Expected equal object entities success");

            //6.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.Multiple(() =>
            {
                Assert.That(resultUnStashedModel.ManualRegistration    ==  model.ManualRegistration      , "No matching record with: ManualRegistration    found");            
                Assert.That(resultUnStashedModel.BackAction            ==  model.BackAction              , "No matching record with: BackAction            found");      
                Assert.That(resultUnStashedModel.SelectedEmployerIndex ==  model.SelectedEmployerIndex   , "No matching record with: SelectedEmployerIndex found");      
                Assert.That(resultUnStashedModel.Name                  ==  model.Name                    , "No matching record with: Name                  found");      
                Assert.That(resultUnStashedModel.CompanyNumber         ==  model.CompanyNumber           , "No matching record with: CompanyNumber         found");      
                Assert.That(resultUnStashedModel.Address1              ==  model.Address1                , "No matching record with: Address1              found");      
                Assert.That(resultUnStashedModel.Address2              ==  model.Address2                , "No matching record with: Address2              found");      
                Assert.That(resultUnStashedModel.Address3              ==  model.Address3                , "No matching record with: Address3              found");      
                Assert.That(resultUnStashedModel.Country               ==  model.Country                 , "No matching record with: Country               found");      
                Assert.That(resultUnStashedModel.PostCode              ==  model.PostCode                , "No matching record with: PostCode              found");
                Assert.That(resultUnStashedModel.PoBox                 == model.PoBox                    , "No matching record with: PoBox                 found");     
                Assert.AreEqual(resultUnStashedModel.SectorType        == SectorTypes.Private,     true  , "Expected equal object entities success");
             });
        }

        [Test]
        [Description("Ensure the Step4 succeeds when all fields are good")]
        public void OrganisationSearch_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel()
                            {
                                Employers = new PagedResult<EmployerRecord>() { },
                                SearchText = "5 Boroughs Partnership NHS Foundation Trust",
                                ManualRegistration = false,
                                SectorType = SectorTypes.Public
                            };


            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //insert  some records into the db...
            controller.PublicSectorRepository.Insert(new EmployerRecord() { Name = "2Gether NHS Foundation Trust",                EmailPatterns = "nhs.uk" });
            controller.PublicSectorRepository.Insert(new EmployerRecord() { Name = "5 Boroughs Partnership NHS Foundation Trust", EmailPatterns = "nhs.uk" });
            controller.PublicSectorRepository.Insert(new EmployerRecord() { Name = "Abbots Langley Parish Council",               EmailPatterns = "abbotslangley-pc.gov.uk" });
            controller.PublicSectorRepository.Insert(new EmployerRecord() { Name = "Aberdeen City Council",                       EmailPatterns = "aberdeencityandshire-sdpa.gov.uk" });
            controller.PublicSectorRepository.Insert(new EmployerRecord() { Name = "Aberdeenshire Council",                       EmailPatterns = "aberdeenshire.gov.uk" });

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.OrganisationSearch(model) as RedirectToRouteResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "ChooseOrganisation", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Public, true, "Expected equal object entities success");

        }
        #endregion

        #region Negative tests

        #endregion

        #endregion

        #region ChooseOrganisation

        #region Positive tests
        [Test]
        [Description("Ensure the Choose Organisation form is returned for the current user to choose an organisation")]
        public void ChooseOrganisation_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "ChooseOrganisation");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel() { ManualRegistration = false };
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.ChooseOrganisation() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "ChooseOrganisation", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Ensure that the new Choose Organisation form is selected and sent successfully when all fields values are valid")]
        public void ChooseOrganisation_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "ChooseOrganisation");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { Name = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "123QA10", CompanyStatus = "Active", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { Name = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "123QA11", CompanyStatus = "Active", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { Name = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "123QA12", CompanyStatus = "Active", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { Name = "Trax ltd",  Address1 = "13", Address2 = "EverGreen Terrace", CompanyNumber = "123QA13", CompanyStatus = "Active", Country = "UK", PostCode = "sw2  5gh" },
                    new EmployerRecord() { Name = "Exant ltd", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "123QA14", CompanyStatus = "Active", Country = "UK", PostCode = "se2  2bh" },
                    new EmployerRecord() { Name = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "123QA15", CompanyStatus = "Active", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { Name = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "123QA16", CompanyStatus = "Active", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { Name = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "123QA17", CompanyStatus = "Active", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { Name = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "123QA18", CompanyStatus = "Active", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { Name = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "123QA19", CompanyStatus = "Active", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { Name = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "123QA20", CompanyStatus = "Active", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { Name = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "123QA21", CompanyStatus = "Active", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { Name = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "123QA22", CompanyStatus = "Active", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { Name = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "123QA23", CompanyStatus = "Active", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { Name = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "123QA24", CompanyStatus = "Active", Country = "UK", PostCode = "sw1  5ml" },
                }
            };

            //change recordNum to test each record: 
            int recordNum = 4;
            string command = "employer_" + recordNum;

            var model = new OrganisationViewModel()
                            {
                                SearchText = "Trax",
                                Employers = employerResult,
                                ManualRegistration = false,
                                SectorType = SectorTypes.Private,
                            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.ChooseOrganisation(model, command) as RedirectToRouteResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "ConfirmOrganisation", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Private, true, "Expected equal object entities success");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
        }

        /// <summary>
        /// Emailpattern matching organisation matching:
        /// </summary>
        [Test]
        [Description("Ensure that when Choose Organisation form is selected and sent successfully")]
        public void ChooseOrganisation_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@abbotslangley-pc.gov.uk", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "ChooseOrganisation");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { Name="2Gether NHS Foundation Trust",                EmailPatterns = "nhs.uk" },
                    new EmployerRecord() { Name="5 Boroughs Partnership NHS Foundation Trust", EmailPatterns = "nhs.uk" },
                    new EmployerRecord() { Name="Abbots Langley Parish Council",               EmailPatterns = "abbotslangley-pc.gov.uk" },
                    new EmployerRecord() { Name="Aberdeen City Council",                       EmailPatterns = "aberdeencityandshire-sdpa.gov.uk" },
                    new EmployerRecord() { Name="Aberdeenshire Council",                       EmailPatterns = "aberdeenshire.gov.uk" },
                    new EmployerRecord() { Name="Aberford &amp; District Parish Council",      EmailPatterns = "aberford-pc.gov.uk" },
                    new EmployerRecord() { Name="Abergavenny Town Council",                    EmailPatterns = "AbergavennyTownCouncil.gov.uk" },
                    new EmployerRecord() { Name="Aberporth Community Council",                 EmailPatterns = "aberporthcommunitycouncil.gov.uk" },
                    new EmployerRecord() { Name="Abertilly and Llanhilleth Community Council", EmailPatterns = "abertilleryandllanhilleth-wcc.gov.uk" },
                    new EmployerRecord() { Name="Aberystwyth Town Council",                    EmailPatterns = "aberystwyth.gov.uk" },
                    new EmployerRecord() { Name="Abingdon Town Council",                       EmailPatterns = "abingdon.gov.uk" },
                    new EmployerRecord() { Name="Academies Enterprise Trust",                  EmailPatterns = "" },
                    new EmployerRecord() { Name="Academy Transformation Trust",                EmailPatterns = "" },
                    new EmployerRecord() { Name="Account NI DFP",                              EmailPatterns = "accountni.gov.uk" },
                    new EmployerRecord() { Name="Accountant in Bankruptcy",                    EmailPatterns = "aib.gov.uk" }
                }
            };

            //change recordNum to test each record: 
            //use 0 for email to be authorised 
            //use 1 for non authourised email
            int recordNum = 2;
            string command = "employer_" + recordNum;
          
          // test
          //  bool IsAuthourisedEmail = employerResult.Results[recordNum].IsAuthorised(employerResult.Results[recordNum].EmailPatterns);

            var model = new OrganisationViewModel()
                            {
                                Employers = employerResult,
                                SelectedEmployerIndex = recordNum,
                                SearchText = "Abbots Langley Parish Council",
                                ManualRegistration = false,
                                SectorType = SectorTypes.Public
                            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            var result = controller.ChooseOrganisation(model, command) as RedirectToRouteResult;
            //retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "AddOrganisation", "Redirected to the wrong view");
            Assert.NotNull(unStashedmodel, "Expected OrganisationViewModel");
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Public, true, "Expected equal object entities success");
        }



        /// <summary>
        /// Emailpattern not matching and organisation not matching:
        /// </summary>
        [Test]
        [Description("Public Manual Journey: Ensure that when Choose Organisation form is selected and email pattern and organisation does not matched, user is redirected to the add organisation form")]
        public void ChooseOrganisation_Post_PublicSector_NoEmailMatch_NoOrgMatch_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "ChooseOrganisation");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { Name="2Gether NHS Foundation Trust",                EmailPatterns = "nhs.uk" },
                    new EmployerRecord() { Name="5 Boroughs Partnership NHS Foundation Trust", EmailPatterns = "nhs.uk" },
                    new EmployerRecord() { Name="Abbots Langley Parish Council",               EmailPatterns = "abbotslangley-pc.gov.uk" },
                    new EmployerRecord() { Name="Aberdeen City Council",                       EmailPatterns = "aberdeencityandshire-sdpa.gov.uk" },
                    new EmployerRecord() { Name="Aberdeenshire Council",                       EmailPatterns = "aberdeenshire.gov.uk" },
                    new EmployerRecord() { Name="Aberford &amp; District Parish Council",      EmailPatterns = "aberford-pc.gov.uk" },
                    new EmployerRecord() { Name="Abergavenny Town Council",                    EmailPatterns = "AbergavennyTownCouncil.gov.uk" },
                    new EmployerRecord() { Name="Aberporth Community Council",                 EmailPatterns = "aberporthcommunitycouncil.gov.uk" },
                    new EmployerRecord() { Name="Abertilly and Llanhilleth Community Council", EmailPatterns = "abertilleryandllanhilleth-wcc.gov.uk" },
                    new EmployerRecord() { Name="Aberystwyth Town Council",                    EmailPatterns = "aberystwyth.gov.uk" },
                    new EmployerRecord() { Name="Abingdon Town Council",                       EmailPatterns = "abingdon.gov.uk" },
                    new EmployerRecord() { Name="Academies Enterprise Trust",                  EmailPatterns = "" },
                    new EmployerRecord() { Name="Academy Transformation Trust",                EmailPatterns = "" },
                    new EmployerRecord() { Name="Account NI DFP",                              EmailPatterns = "accountni.gov.uk" },
                    new EmployerRecord() { Name="Accountant in Bankruptcy",                    EmailPatterns = "aib.gov.uk" },

                    //EmployerRecord 15: Emailpattern not matching and organisation not matching:
                    new EmployerRecord() { Name="",   EmailPatterns = "" }
                }
            };

            //change recordNum to test each record: 
            int recordNum = 15;
            string command = "employer_" + recordNum;

            var model = new OrganisationViewModel()
            {
                Employers = employerResult,
                SelectedEmployerIndex = recordNum,
                SearchText = "text that will not be found",
                ManualRegistration = true,
                SectorType = SectorTypes.Public
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            var result = controller.ChooseOrganisation(model, command) as RedirectToRouteResult;
            //retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "AddOrganisation", "Redirected to the wrong view");
            Assert.NotNull(unStashedmodel, "Expected OrganisationViewModel");
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Public, true, "Expected equal object entities success");
        }
        
        #endregion

        #region Negative tests

        #endregion

        #endregion

        #region  Add Organisation - Public Sector

        /// <summary>
        /// Public Sector: Add organisaton address
        /// </summary>
        [Test]
        [Description("Ensure the Add Organisation form is returned for the current user to add an organisation")]
        public void AddOrganisation_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddOrganisation");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel()
            { 
                ManualRegistration = false,
                SectorType = SectorTypes.Public
            };

            controller.StashModel(orgModel);

            //ACT:
            var result = controller.AddOrganisation() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "AddOrganisation", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Ensure that the new Add Address form is filled and sent successfully when all fields values are valid")]
        public void AddOrganisation_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddOrganisation");
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel()
            { 
                Name = "Acme ltd",
                Address1 = "123",
                Address3 = "WestMinster",
                PostCode = "W1A 2ED",
                SelectedEmployerIndex = 0,
                SearchText = "smith",
                ManualRegistration = false,
                SectorType = SectorTypes.Public
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddOrganisation(model) as RedirectToRouteResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "ConfirmOrganisation", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Public, true, "Expected equal object entities success");
        }

        #endregion

        #region ManualRegistration: Add organisation - Private Sector 

        [Test]
        [Description("Private Manual Journey Choose your org in private sector in manual reg mode returns the Add Organisation view ")]
        public void AddOrganisation_Get_PrivateSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddOrganisation");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();
            
            var model = new OrganisationViewModel()
            {
               // Employers = employerResult, //0 record returned
                ManualRegistration = true, 
                SectorType = SectorTypes.Private
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
           // controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddOrganisation() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "AddOrganisation", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(OrganisationViewModel), "Expected OrganisationViewModel or Incorrect resultType returned");
        }

        [Test]
        [Description("Private Manual journey: Ensure that the new AddOrganisation form is filled and sent successfully when all fields values are valid")]
        public void AddOrganisation_Post_PrivateSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddOrganisation");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            //use the include and exclude functions here to save typing
            var model = new OrganisationViewModel()
            {
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                CompanyNumber = "wetrw1234fg",
                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactOrganisation = "test Organisation",
                ContactPhoneNumber = "79000 000 000",
                Country = "United Kingdom",
                Name = "Acme ltd",
                PINExpired = false,
                PINSent = false,
                PoBox = "",
                PostCode = "W1 5qr",
                ReviewCode = "",
                SearchText = "Searchtext",
                SelectedEmployerIndex = -1,
                BackAction = "",
                CancellationReason = "",
              //Employers = employerResult, //0 record returned
                ManualRegistration = true, //already set to true by default, this is for reference
                SectorType = SectorTypes.Private
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddOrganisation(model) as RedirectToRouteResult;

            //ASSERTS:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "AddContact", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(model.Address1              == unStashedmodel.Address1              , true, "Expected Address1              success");
                Assert.AreEqual(model.Address2              == unStashedmodel.Address2              , true, "Expected Address2              success");
                Assert.AreEqual(model.Address3              == unStashedmodel.Address3              , true, "Expected Address3              success");
                Assert.AreEqual(model.CompanyNumber         == unStashedmodel.CompanyNumber         , true, "Expected CompanyNumber         success");
                Assert.AreEqual(model.ContactEmailAddress   == unStashedmodel.ContactEmailAddress   , true, "Expected ContactEmailAddress   success");
                Assert.AreEqual(model.ContactFirstName      == unStashedmodel.ContactFirstName      , true, "Expected ContactFirstName      success");
                Assert.AreEqual(model.ContactLastName       == unStashedmodel.ContactLastName       , true, "Expected ContactLastName       success");
                Assert.AreEqual(model.ContactJobTitle       == unStashedmodel.ContactJobTitle       , true, "Expected ContactJobTitle       success");
                Assert.AreEqual(model.ContactOrganisation   == unStashedmodel.ContactOrganisation   , true, "Expected ContactOrganisation   success");
                Assert.AreEqual(model.ContactPhoneNumber    == unStashedmodel.ContactPhoneNumber    , true, "Expected ContactPhoneNumber    success");
                Assert.AreEqual(model.Country               == unStashedmodel.Country               , true, "Expected Country               success");
                Assert.AreEqual(model.Name                  == unStashedmodel.Name                  , true, "Expected Name                  success");
                Assert.AreEqual(model.PINExpired            == unStashedmodel.PINExpired            , true, "Expected PINExpired            success");
                Assert.AreEqual(model.PINSent               == unStashedmodel.PINSent               , true, "Expected PINSent               success");
                Assert.AreEqual(model.PoBox                 == unStashedmodel.PoBox                 , true, "Expected PoBox                 success");
                Assert.AreEqual(model.PostCode              == unStashedmodel.PostCode              , true, "Expected PostCode              success");
                Assert.AreEqual(model.ReviewCode            == unStashedmodel.ReviewCode            , true, "Expected ReviewCode            success");
                Assert.AreEqual(model.SearchText            == unStashedmodel.SearchText            , true, "Expected SearchText            success");
                Assert.AreEqual(model.SelectedEmployerIndex == unStashedmodel.SelectedEmployerIndex , true, "Expected SelectedEmployerIndex success");
                Assert.AreEqual(model.BackAction            == unStashedmodel.BackAction            , true, "Expected BackAction            success");
                Assert.AreEqual(model.CancellationReason    == unStashedmodel.CancellationReason    , true, "Expected CancellationReason    success");
                Assert.AreEqual(model.Employers             == unStashedmodel.Employers             , true, "Expected Employers             success");
                Assert.AreEqual(model.ManualRegistration    == unStashedmodel.ManualRegistration    , true, "Expected ManualRegistration    success");
                Assert.AreEqual(model.SectorType            == unStashedmodel.SectorType            , true, "Expected SectorType            success");

              });
           //8.verify that it was private sector was selected
            Assert.AreEqual(model.SectorType == SectorTypes.Private, true, "Expected equal object entities success");
        }

        #endregion

        #region ManualRegistration: Add contact - Private Sector 

        [Test]
        [Description("Private Manual Journey: ensure Add Contact form is returned successfully to the user")]
        public void AddContact_Get_PrivateSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddContact");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            var model = new OrganisationViewModel()
            {
                Employers = employerResult,
                ManualRegistration = true,
                SectorType = SectorTypes.Private
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            // controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "AddContact", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(OrganisationViewModel), "Expected OrganisationViewModel or Incorrect resultType returned");

        }

        //[Ignore("This test needs fixing")]
        [Test]
        [Description("Private Manual Journey: ensure Add Contact form is filled and sent successfully")]
        public void AddContact_Post_PrivateSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddContact");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            var model = new OrganisationViewModel()
            {
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                CompanyNumber = "wetrw1234fg",
                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactOrganisation = "test Organisation",
                ContactPhoneNumber = "79000 000 000",
                Country = "United Kingdom",
                Name = "Acme ltd",
                PINExpired = false,
                PINSent = false,
                PoBox = "",
                PostCode = "W1 5qr",
                ReviewCode = "",
                SearchText = "Searchtext",
                //   SelectedEmployerIndex = -1,
                BackAction = "",
                CancellationReason = "",
                Employers = employerResult,
                ManualRegistration = true,
                SectorType = SectorTypes.Private
            };


            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact(model) as RedirectToRouteResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "ConfirmOrganisation", "Redirected to the wrong view");
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();
            Assert.NotNull(model, "Expected OrganisationViewModel");
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
        }

        #endregion


        #region ManualRegistration: Add organisation - Public Sector 

        [Test]
        [Description("Public Manual Journey Choose your org in private sector in manual reg mode returns the Add Organisation view")]
        public void AddOrganisation_Get_PublicSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddOrganisation");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            var model = new OrganisationViewModel()
            {
                Employers = employerResult,
                ManualRegistration = true,
                SectorType = SectorTypes.Public
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddOrganisation() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "AddOrganisation", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(OrganisationViewModel), "Expected OrganisationViewModel or Incorrect resultType returned");
        }

        [Test]
        [Description("Public Manual journey: Ensure that the new AddOrganisation form is filled and sent successfully when all fields values are valid")]
        public void AddOrganisation_Post_PublicSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddOrganisation");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>()
                {
                    //new EmployerRecord() { Name = "", EmailPatterns = "" } 
                };

            var model = new OrganisationViewModel()
            {
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                CompanyNumber = "wetrw1234fg",
                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactOrganisation = "test Organisation",
                ContactPhoneNumber = "79000 000 000",
                Country = "United Kingdom",
                Name = "Acme ltd",
                PINExpired = false,
                PINSent = false,
                PoBox = "",
                PostCode = "W1 5qr",
                ReviewCode = "",
                SearchText = "Searchtext",
           //   SelectedEmployerIndex = -1,
                BackAction = "",
                CancellationReason = "",
                Employers = employerResult,
                ManualRegistration = true, 
                SectorType = SectorTypes.Public
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            var result = controller.AddOrganisation(model) as RedirectToRouteResult;
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERTS:
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "AddContact", "Redirected to the wrong view");
            Assert.NotNull(model, "Expected OrganisationViewModel");
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
            Assert.Multiple(() =>
            {
                Assert.AreEqual(model.Address1              == unStashedmodel.Address1              , true, "Expected Address1              success");
                Assert.AreEqual(model.Address2              == unStashedmodel.Address2              , true, "Expected Address2              success");
                Assert.AreEqual(model.Address3              == unStashedmodel.Address3              , true, "Expected Address3              success");
                Assert.AreEqual(model.CompanyNumber         == unStashedmodel.CompanyNumber         , true, "Expected CompanyNumber         success");
                Assert.AreEqual(model.ContactEmailAddress   == unStashedmodel.ContactEmailAddress   , true, "Expected ContactEmailAddress   success");
                Assert.AreEqual(model.ContactFirstName      == unStashedmodel.ContactFirstName      , true, "Expected ContactFirstName      success");
                Assert.AreEqual(model.ContactLastName       == unStashedmodel.ContactLastName       , true, "Expected ContactLastName       success");
                Assert.AreEqual(model.ContactJobTitle       == unStashedmodel.ContactJobTitle       , true, "Expected ContactJobTitle       success");
                Assert.AreEqual(model.ContactOrganisation   == unStashedmodel.ContactOrganisation   , true, "Expected ContactOrganisation   success");
                Assert.AreEqual(model.ContactPhoneNumber    == unStashedmodel.ContactPhoneNumber    , true, "Expected ContactPhoneNumber    success");
                Assert.AreEqual(model.Country               == unStashedmodel.Country               , true, "Expected Country               success");
                Assert.AreEqual(model.Name                  == unStashedmodel.Name                  , true, "Expected Name                  success");
                Assert.AreEqual(model.PINExpired            == unStashedmodel.PINExpired            , true, "Expected PINExpired            success");
                Assert.AreEqual(model.PINSent               == unStashedmodel.PINSent               , true, "Expected PINSent               success");
                Assert.AreEqual(model.PoBox                 == unStashedmodel.PoBox                 , true, "Expected PoBox                 success");
                Assert.AreEqual(model.PostCode              == unStashedmodel.PostCode              , true, "Expected PostCode              success");
                Assert.AreEqual(model.ReviewCode            == unStashedmodel.ReviewCode            , true, "Expected ReviewCode            success");
                Assert.AreEqual(model.SearchText            == unStashedmodel.SearchText            , true, "Expected SearchText            success");
                Assert.AreEqual(model.SelectedEmployerIndex == unStashedmodel.SelectedEmployerIndex , true, "Expected SelectedEmployerIndex success");
                Assert.AreEqual(model.BackAction            == unStashedmodel.BackAction            , true, "Expected BackAction            success");
                Assert.AreEqual(model.CancellationReason    == unStashedmodel.CancellationReason    , true, "Expected CancellationReason    success");
                Assert.AreEqual(model.Employers             == unStashedmodel.Employers             , true, "Expected Employers             success");
                Assert.AreEqual(model.ManualRegistration    == unStashedmodel.ManualRegistration    , true, "Expected ManualRegistration    success");
                Assert.AreEqual(model.SectorType            == unStashedmodel.SectorType            , true, "Expected SectorType            success");

              });
            Assert.AreEqual(model.SectorType == SectorTypes.Public, true, "Expected equal object entities success");
        }

        #endregion

        #region ManualRegistration: Add contact - Public Sector 

        [Test]
        [Description("Public Sector Manual Journey: ensure Add Contact form is returned successfully to the user")]
        public void AddContact_Get_PublicManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddContact");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            var model = new OrganisationViewModel()
            {
                Employers = employerResult,
                ManualRegistration = true,
                SectorType = SectorTypes.Public
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            // controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "AddContact", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(OrganisationViewModel), "Expected OrganisationViewModel or Incorrect resultType returned");

        }


        [Test]
        [Description("Public Manual:ensure Add Contact form is returned successfully to the user")]
        public void AddContact_Post_PublicSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "AddContact");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();


            var model = new OrganisationViewModel()
            {
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                CompanyNumber = "wetrw1234fg",
                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactOrganisation = "test Organisation",
                ContactPhoneNumber = "79000 000 000",
                Country = "United Kingdom",
                Name = "Acme ltd",
                PINExpired = false,
                PINSent = false,
                PoBox = "",
                PostCode = "W1 5qr",
                ReviewCode = "",
                SearchText = "Searchtext",
                //   SelectedEmployerIndex = -1,
                BackAction = "",
                CancellationReason = "",
                Employers = employerResult,
                ManualRegistration = true,
                SectorType = SectorTypes.Public
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact(model) as RedirectToRouteResult;

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "ConfirmOrganisation", "Redirected to the wrong view");
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();
            Assert.NotNull(model, "Expected OrganisationViewModel");
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
        }

        #endregion




        #region Confirm

        #region Positive tests

        [Test]
        [Description("Ensure the Confirm Organisation form is returned for the current user to confirm an organisation selection")]
        public void ConfirmOrganisation_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "ConfirmOrganisation");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel(){ ManualRegistration = false };

            controller.StashModel(orgModel);

            //ACT:
            var result = controller.ConfirmOrganisation() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "ConfirmOrganisation", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        #region Private sector confirm organisation
       
        [Test]
        [Description("Private Sector Journey: Ensure the Confirm Organisation form is returned for the current user to confirm an organisation selection")]
        public void ConfirmOrganisation_Get_PrivateSector_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "ConfirmOrganisation");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel()
            {
                ManualRegistration = false,
                SectorType = SectorTypes.Private
            };

            controller.StashModel(orgModel);

            //ACT:
            var result = controller.ConfirmOrganisation() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "ConfirmOrganisation", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Private Sector Journey: Ensure the Confirm Organisation form is confirmed and sent successfully")]
        public void ConfirmOrganisation_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            ICollection<OrganisationSicCode> OrgSicCodeList = new List<OrganisationSicCode>();

            //this might not be neccesary anymore.
            //do
            //{
            //    int count = 0;
            //    OrgSicCodeList.Add( 
            //                         new OrganisationSicCode()
            //                        {
            //                            SicCodeId = count++,
            //                            SicCode = new SicCode()
            //                            { SicCodeId = count++,
            //                              Description  = "SicCode description {0} " + count++,
            //                              SicSection = new SicSection()
            //                              {
            //                                    Created = DateTime.Now,
            //                                    Description = "SicSection description {0}" + count++,
            //                                    SicCodes = new List<SicCode>()
            //                                    {
            //                                     new SicCode{ SicCodeId = 1, SicSectionId = "SSID1", SicSection = new SicSection(), Description = "" },
            //                                     new SicCode{ SicCodeId = 2, SicSectionId = "SSID2", SicSection = new SicSection(), Description = "" },
            //                                     new SicCode{ SicCodeId = 3, SicSectionId = "SSID3", SicSection = new SicSection(), Description = "" },
            //                                    },
            //                                    SicSectionId = count++.ToString()
            //                               },
            //                               SicSectionId =  "" + count++ + "",
            //                               Created = DateTime.Now
            //                           }
            //                        }
            //                       );
            //}
            //while (OrgSicCodeList.Count < 14);

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "ConfirmOrganisation");
            routeData.Values.Add("Controller", "Register");

            //this might not be neccesary anymore.
            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { Name = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "123QA10", SicCodes = OrgSicCodeList.Where(s => s.SicCodeId == 1).ToString(), CompanyStatus = "Active", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { Name = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "123QA11", SicCodes = OrgSicCodeList.Where(s => s.SicCodeId == 2).ToString(), CompanyStatus = "Active", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { Name = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "123QA12", SicCodes = OrgSicCodeList.Where(s => s.SicCodeId == 3).ToString(), CompanyStatus = "Active", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { Name = "Trax ltd",  Address1 = "13", Address2 = "EverGreen Terrace", CompanyNumber = "123QA13", SicCodes = OrgSicCodeList.Where(s => s.SicCodeId == 4).ToString(), CompanyStatus = "Active", Country = "UK", PostCode = "sw2  5gh" },
                }
            };

            var model = new OrganisationViewModel()
            {
                Employers = employerResult,
                ManualRegistration = false,
                SelectedEmployerIndex = 1,
                SectorType = SectorTypes.Private,
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //insert some employee records with sicCodes into the db...
            controller.PrivateSectorRepository.Insert(new EmployerRecord()
            {
                Name = "acme inc",
                Address1 = "123",
                Address2 = "EverGreen Terrace",
                CompanyNumber = "123QA432",
                CompanyStatus = "Active",
                Country = "UK",
                PostCode = "e12 3eq",
                SicCodes = "41100,41201,41202"
            }
                                                     );

            controller.PrivateSectorRepository.Insert(new EmployerRecord()
            {
                Name = "smith ltd",
                Address1 = "45",
                Address2 = "iverson rd",
                CompanyNumber = "123QA11",
                CompanyStatus = "Active",
                Country = "UK",
                PostCode = "nw1 5re",
                SicCodes = "42110,42130,42210"
            }
                                                     );

            controller.PrivateSectorRepository.Insert(new EmployerRecord()
            {
                Name = "smith & Wes ltd",
                Address1 = "45",
                Address2 = "iverson rd",
                CompanyNumber = "456GT657",
                CompanyStatus = "Active",
                Country = "UK",
                PostCode = "nw1 5re",
                SicCodes = "42220,42910,42990"
            }
                                                    );

            controller.PrivateSectorRepository.Insert(new EmployerRecord()
            {
                Name = "smithers and sons ltd",
                Address1 = "45",
                Address2 = "iverson rd",
                CompanyNumber = "956GT237",
                CompanyStatus = "Active",
                Country = "UK",
                PostCode = "nw1 5re",
                SicCodes = "43110,43120,43130"
            }
                                                    );

            controller.PrivateSectorRepository.Insert(new EmployerRecord()
            {
                Name = "excetera ltd",
                Address1 = "123",
                Address2 = "Venice avenue ",
                CompanyNumber = "910QA0942",
                CompanyStatus = "Active",
                Country = "UK",
                PostCode = "w1 9eaz",
                SicCodes = "43210,43220,43290"
            }
                                                     );

            //insert some sicCodes into the db for employee records sicCodes to match...
            // controller.DataRepository.Insert(OrgSicCodeList);
            controller.DataRepository.Insert(new SicCode { SicCodeId = 42110, SicSectionId = "SSID1", SicSection = new SicSection(), Description = "" } );
            controller.DataRepository.Insert(new SicCode { SicCodeId = 42130, SicSectionId = "SSID2", SicSection = new SicSection(), Description = "" } );
            controller.DataRepository.Insert(new SicCode { SicCodeId = 42210, SicSectionId = "SSID3", SicSection = new SicSection(), Description = "" } );

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.ConfirmOrganisation(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "PINSent", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model, "Expected OrganisationViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here

            //8.verify that it was private sector was selected
            Assert.AreEqual(model.SectorType == SectorTypes.Private, true, "Expected equal object entities success");
        }

        #endregion

        #region Public sector confirm organisation
        [Test]
        [Description("Ensure the ConfirmOrganisation succeeds when all fields are good")]
        public void ConfirmOrganisation_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = DateTime.Now };

            //create an existing organisation model in the db
            var organisation = new Organisation() { OrganisationId = 1 };

            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "ConfirmOrganisation");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { Name="2Gether NHS Foundation Trust",                EmailPatterns = "nhs.uk" },
                    new EmployerRecord() { Name="5 Boroughs Partnership NHS Foundation Trust", EmailPatterns = "nhs.uk" },
                    new EmployerRecord() { Name="Abbots Langley Parish Council",               EmailPatterns = "abbotslangley-pc.gov.uk" },
                    new EmployerRecord() { Name="Aberdeen City Council",                       EmailPatterns = "aberdeencityandshire-sdpa.gov.uk" },
                    new EmployerRecord() { Name="Aberdeenshire Council",                       EmailPatterns = "aberdeenshire.gov.uk" },
                    new EmployerRecord() { Name="Aberford &amp; District Parish Council",      EmailPatterns = "aberford-pc.gov.uk" },
                    new EmployerRecord() { Name="Abergavenny Town Council",                    EmailPatterns = "AbergavennyTownCouncil.gov.uk" },
                    new EmployerRecord() { Name="Aberporth Community Council",                 EmailPatterns = "aberporthcommunitycouncil.gov.uk" },
                    new EmployerRecord() { Name="Abertilly and Llanhilleth Community Council", EmailPatterns = "abertilleryandllanhilleth-wcc.gov.uk" },
                    new EmployerRecord() { Name="Aberystwyth Town Council",                    EmailPatterns = "aberystwyth.gov.uk" },
                    new EmployerRecord() { Name="Abingdon Town Council",                       EmailPatterns = "abingdon.gov.uk" },
                    new EmployerRecord() { Name="Academies Enterprise Trust",                  EmailPatterns = "" },
                    new EmployerRecord() { Name="Academy Transformation Trust",                EmailPatterns = "" },
                    new EmployerRecord() { Name="Account NI DFP",                              EmailPatterns = "accountni.gov.uk" },
                    new EmployerRecord() { Name="Accountant in Bankruptcy",                    EmailPatterns = "aib.gov.uk" }
                }
            };

            //model to be saved.
            var model = new OrganisationViewModel()
            {
                Employers = employerResult,
                ManualRegistration = false,
                SelectedEmployerIndex = 1,
                SectorType = SectorTypes.Public
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user/*, userOrganisation, organisation*/);

            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.ConfirmOrganisation(model) as RedirectToRouteResult;

            //this should just return the correct record with organisationid=1
            var resultDBModel = (controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == 1));

            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model, "Expected OrganisationViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here

            //8.verify that it was private sector was selected
            Assert.AreEqual(model.SectorType == SectorTypes.Public, true, "Expected equal object entities success");
        }

        #endregion

        #endregion

        #region Negative tests

        #endregion

        #endregion
    }
}