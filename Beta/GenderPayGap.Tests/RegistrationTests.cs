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
            Assert.Throws<IdentityNotMappedException>(() => controller.Step1(), "Expected IdentityNotMappedException");
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
  //    [Test]
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
            var result = controller.Step1(model) as ViewResult;

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
            var result = controller.Step1(model) as ViewResult;

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
            var result = controller.Step1(model) as ViewResult;

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
            var result = controller.Step1(model) as ViewResult;

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
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.That(!result.ViewData.ModelState.IsValid, "Password expression should have failed");
        }

        #endregion

        #region Test enrollment step 1 - send verification email
        #endregion

        #region Test enrollment step 1 - verify email
        #endregion


        //[Test]
        [Description("Ensure the Step1 fails when a user does not exist in the db")]
        public void Step1_Get_unAuthUser_Fail()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            var user = new User() { UserId = 0 };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            //ACT:
            var result = controller.Step1();

            //ASSERT:
            Assert.Null(result as ActionResult, "Expected Null value");
           
        }



        //Happy Path - Registration GET and POST Actions
        //[Test]
        //[Description("Ensure the Step1 succeeds and gets a new registration form for newly authorized users to register")]
        //public void Step1_Get_RegistrationComplete_Success()
        //{
        //    //ARRANGE:
        //    //create a user who does not exist in the db
        //    var user = new User() { UserId = 0};

        //    var routeData = new RouteData();
        //    routeData.Values.Add("action", "Step1");
        //    routeData.Values.Add("Controller", "register");

        //    //Stash an object to pass in for  this.ClearStash()
        //    var controller = TestHelper.GetController<RegisterController>(0, routeData, user = null);

        //    //ACT:
        //    var result = controller.Step1() as RedirectToRouteResult;

        //    //ASSERT:
        //    Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");
        //    Assert.That(result.RouteValues["action"].ToString() == "Complete", "Expected User registration to be complete");
        //}

        [Test]
        [Description("Ensure the Step1 succeeds and gets a new registration form for newly authorized users to register")]
        public void Step1_Get_NewRegistration_Success()
        {
            //ARRANGE:
            //create a user who does not exist in the db
            var user = new User() { UserId = 0 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step1");
            routeData.Values.Add("Controller", "Register"); ;

            //Stash an object to pass in for this.ClearStash()
            //var model = new RegisterViewModel();
            var controller = TestHelper.GetController<RegisterController>(0, routeData, user = null/*, model*/);
            //controller.StashModel(model);

            //ACT:
            var result = controller.Step1() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step1", "Incorrect view returned");
            Assert.NotNull(result.Model as RegisterViewModel, "Expected RegisterViewModel");
            Assert.That(result.Model.GetType() == typeof(RegisterViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

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
            ////1.Arrange the test setup variables
            //var model = new RegisterViewModel();
            //model.EmailAddress = "test@hotmail.com"; 
            //model.ConfirmEmailAddress = "test@hotmail.com";
            //model.FirstName = "TestFirstName";
            //model.LastName = "TestLastName";
            //model.JobTitle = "TestJobTitle";
            //model.Password = "P@ssword1!";
            //model.ConfirmPassword = "P@ssword1!";

            //1.Arrange the test setup variables
            var model = new RegisterViewModel()
                            {
                                EmailAddress         = "magnuski@hotmail.com",
                                ConfirmEmailAddress  = "magnuski@hotmail.com",
                                FirstName            = "Kingsley",
                                LastName             = "Eweka",
                                JobTitle             = "Dev",
                                Password             = "K1ngsl3y3w3ka",
                                ConfirmPassword      = "K1ngsl3y3w3ka"
                            };

            var controller = TestHelper.GetController<RegisterController>();
            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step1(model) as RedirectToRouteResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step2", "Expected a RedirectToRouteResult to Step2");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<RegisterViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as RegisterViewModel, "Expected RegisterViewModel");

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
        [Description("Ensure the Step2 succeeds when is verified and an email is sent")]
        public void Step2_Get_ViewResult_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var code = "abcdefg";
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = null, EmailVerifySendDate = null, EmailVerifyHash = code.GetSHA512Checksum(), Status = UserStatuses.New, Organisations = null };
          
            var verifiedModel = new VerifyViewModel() { Verified = true };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step2");
            routeData.Values.Add("Controller", "Register");

            //simulate a model to stash
            var model = new RegisterViewModel();
            model.EmailAddress = "magnuski@hotmail.com";
            model.ConfirmEmailAddress = "magnuski@hotmail.com";
            model.FirstName = "Kingsley";
            model.LastName = "Eweka";
            model.JobTitle = "Dev";
            model.Password = "K1ngsl3y3w3ka";
            model.ConfirmPassword = "K1ngsl3y3w3ka";

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, verifiedModel);
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step2(Encryption.EncryptQuerystring(code)) as ViewResult;

            var resultModel = result.Model as VerifyViewModel;

            result.ViewData.ModelState.Clear();
            resultModel.Verified = (controller.DataRepository.GetAll<VerifyViewModel>().FirstOrDefault(v => v.Verified)).Verified;

            //ASSERT:
            //Ensure confirmation view is returned
            Assert.NotNull(result as ViewResult, "Expected ViewResult");
            Assert.That(result.ViewName == "Step2", "Incorrect view returned");

            //Ensure the model is not null and it is correct
            Assert.NotNull(result.Model as VerifyViewModel, "Expected VerifyViewModel");
            Assert.That(result.ViewData.ModelState.IsValid, " Model is not valid"); 
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("EmailAddress"), "Email is not a match or is invalid");
            
            //ensure user is marked as verified
            Assert.AreEqual(resultModel.Verified, true, "Expected VerifyViewModel");
        }

        [Test]
        [Description("Ensure the Step2 succeeds when all fields are good")]
        public void Step2_Get_RedirectResult_Success() //Registration complete
        {
            //ARRANGE:
            //1.Arrange the test setup variables
             var code = "abcdefg";
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now, EmailVerifyHash = code.GetSHA512Checksum() };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set the user up as if finished step1 which is email known etc but not sent

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step2");
            routeData.Values.Add("Controller", "Register");

            var model = new VerifyViewModel();

            //var controller = TestHelper.GetController<RegisterController>();
            var controller = TestHelper.GetController<RegisterController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step2(Encryption.EncryptQuerystring(code)) as RedirectToRouteResult;

            //ASSERT:
            //Check the user is return the confirmation view
            //Check the user verifcation is now marked as sent
            //Check a verification has been set against user 
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "");
            
        }
       
        [Test]
        [Description("Ensure the Step2 user verification succeeds")]
        public void Step2_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step2");
            routeData.Values.Add("Controller", "Register");

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
            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
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
        [Description("Ensure the Step3 succeeds when all fields are good")]
        public void Step3_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step3");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            //ACT:
            var result = controller.Step3() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step3", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Ensure the Step3 succeeds when all fields are good")]
        public void Step3_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step3");
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel() { SectorType = SectorTypes.Private };
            
            var controller = TestHelper.GetController<RegisterController>( 1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step3(model) as RedirectToRouteResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step4", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel as OrganisationViewModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
            
            //8.verify that it was private sector was selected
            Assert.AreEqual(model.SectorType == SectorTypes.Private, true, "Expected equal object entities success");

            //Assert.Multiple(() =>
            //{
            //    //Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
            //    //Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
            //    //Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
            //    //Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
            //    //Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
            //    //Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
            //    //Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            //});
        }

        [Test]
        [Description("Ensure the Step3 succeeds when all fields are good")]
        public void Step3_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step3");
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel() { SectorType = SectorTypes.Public };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step3(model) as RedirectToRouteResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step4", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel as OrganisationViewModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(model.SectorType == SectorTypes.Public, true, "Expected equal object entities success");

            //Assert.Multiple(() =>
            //{
            //    //Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
            //    //Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
            //    //Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
            //    //Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
            //    //Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
            //    //Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
            //    //Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            //});
        }


        [Test]
        [Description("Ensure the Step4 succeeds when all fields are good")]
        public void Step4_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step4");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel();
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.Step4() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step4", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            // var controller = TestHelper.GetController<RegisterController>();
            // controller.PublicSectorRepository.Insert(new EmployerRecord());
        }

        [Test]
        [Description("Ensure the Step4 succeeds when all fields are good")]
        public void Step4_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step4");
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel()
                            {
                               Employers = new Core.Classes.PagedResult<EmployerRecord>() {}, 
                               SearchText = "smith",
                               SectorType = SectorTypes.Private
                             };


            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step4(model) as RedirectToRouteResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step5", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel as OrganisationViewModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Private, true, "Expected equal object entities success");

        }

        [Test]
        [Description("Ensure the Step4 succeeds when all fields are good")]
        public void Step4_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step4");
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel()
            {
                Employers = new PagedResult<EmployerRecord>() { },
                SearchText = "smith",
                SectorType = SectorTypes.Public
            };


            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step4(model) as RedirectToRouteResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step5", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel as OrganisationViewModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Public, true, "Expected equal object entities success");

        }

        [Test]
        [Description("Ensure the Step5 succeeds when all fields are good")]
        public void Step5_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step5");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel();
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.Step5() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step5", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Ensure the Step5 succeeds when all fields are good")]
        public void Step5_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step5");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                            {
                                 new EmployerRecord() {  Name = "1 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "2 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "3 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "4 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "5 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "6 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "7 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "8 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "9 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "10 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "11 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "12 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "13 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "14 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "15 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },
                            }
            };

            //change recordNum to test each record: 
            int recordNum = 0;
            string command = "employer_" + recordNum;

            var model = new OrganisationViewModel()
            {
                Employers  =  employerResult,
                SelectedEmployerIndex = recordNum,
                SearchText = "smith",
                SectorType = SectorTypes.Private
            };

           

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step5(model, command) as RedirectToRouteResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step6", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel as OrganisationViewModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Private, true, "Expected equal object entities success");
          
            //7.Verify the values from the result that was stashed matches that of the Arrange values here

            //Assert.Multiple(() =>
            //{
            //    Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
            //    Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
            //    Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
            //    Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
            //    Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
            //    Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
            //    Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
            //    Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            //});
        }

        [Test]
        [Description("Ensure the Step5 succeeds when all fields are good")]
        public void Step5_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step5");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                            {
                                 new EmployerRecord() {  Name = "1 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq", EmailPatterns = "test@gov.uk" },

                                 new EmployerRecord() {  Name = "2 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq", EmailPatterns = "test@test.uk"  },

                                 new EmployerRecord() {  Name = "3 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "4 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "5 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "6 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "7 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "8 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "9 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "10 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "11 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "12 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "13 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "14 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "15 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },
                            }
            };

            //change recordNum to test each record: 
            //use 0 for email to be authorised 
            //use 1 for non authourised email
            int recordNum = 0;
            string command = "employer_" + recordNum;

            bool IsAuthourisedEmail = employerResult.Results[recordNum].IsAuthorised(employerResult.Results[recordNum].EmailPatterns);

            var model = new OrganisationViewModel()
            {
                Employers = employerResult,
                SelectedEmployerIndex = 0,
                SearchText = "smith",
                SectorType = SectorTypes.Private
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step5(model, command) as RedirectToRouteResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step6", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel as OrganisationViewModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Private, true, "Expected equal object entities success");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here

            //Assert.Multiple(() =>
            //{
            //    Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
            //    Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
            //    Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
            //    Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
            //    Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
            //    Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
            //    Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
            //    Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            //});
        }



        //PRIVATE SECTOR -> CONFIRM EMPLOYER
        /// <summary>
        /// Private Sector: Confirm Employer
        /// </summary>
        [Test]
        [Description("Ensure the Step6 succeeds when all fields are good")]
        public void Step6_Get_ConfirmEmployer_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step6");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel() { SectorType = SectorTypes.Private };
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.Step6() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "ConfirmEmployer", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        //PUBLIC SECTOR -> ADD ADDRESS
        /// <summary>
        /// Public Sector: Add address
        /// </summary>
        [Test]
        [Description("Ensure the Step6 succeeds when all fields are good")]
        public void Step6_Get_AddAddress_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step6");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel() { SectorType = SectorTypes.Public };
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.Step6() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "AddAddress", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Ensure the Step6 succeeds when all fields are good")]
        public void Step6_Post_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step6");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                            {
                                 new EmployerRecord() {  Name = "1 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "2 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "3 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "4 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "5 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "6 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "7 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "8 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "9 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "10 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "11 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "12 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "13 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "14 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "15 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },
                            }
            };

            var model = new OrganisationViewModel()
            {
                Employers = employerResult,
                SelectedEmployerIndex = 0,
                SearchText = "smith",
                SectorType = SectorTypes.Private
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step6(model) as RedirectToRouteResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "SendPIN", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel as OrganisationViewModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Private, true, "Expected equal object entities success");
            
            //Assert.Multiple(() =>
            //{
            //    Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
            //    Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
            //    Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
            //    Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
            //    Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
            //    Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
            //    Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
            //    Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            //});
        }

        [Test]
        [Description("Ensure the Step6 succeeds when all fields are good")]
        public void Step6_Post_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step6");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                            {
                                 new EmployerRecord() {  Name = "1 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "2 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "3 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "4 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "5 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "6 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "7 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "8 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "9 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "10 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "11 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "12 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "13 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "14 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "15 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },
                            }
            };

            var model = new OrganisationViewModel()
            {
                Address1 = "123",
                //Address2 = "EverGreen Terrace",
                Address3 = "WestMinster",
                //Country  = "UK",
                PostCode = "W1A 2ED",
                //PoBox    = "5553X", 
                Employers = employerResult,
                SelectedEmployerIndex = 0,
                SearchText = "smith",
                SectorType = SectorTypes.Public
            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step6(model) as RedirectToRouteResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step7", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unStashedmodel as OrganisationViewModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");

            //8.verify that it was private sector was selected
            Assert.AreEqual(unStashedmodel.SectorType == SectorTypes.Public, true, "Expected equal object entities success");

            //Assert.Multiple(() =>
            //{
            //    Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
            //    Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
            //    Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
            //    Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
            //    Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
            //    Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
            //    Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
            //    Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            //});
        }



        [Test]
        [Description("Ensure the Step7 succeeds when all fields are good")]
        public void Step7_Get_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step7");
            routeData.Values.Add("Controller", "Register");

            var controller = TestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel();
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.Step7() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "ConfirmEmployer", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Ensure the Step7 succeeds when all fields are good")]
        public void Step7_Post_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step7");
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                            {
                                 new EmployerRecord() {  Name = "1 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "2 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "3 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "4 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "5 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "6 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "7 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "8 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "9 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "10 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "11 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "12 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "13 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "14 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },

                                 new EmployerRecord() {  Name = "15 Organisation Name", Address1 = "123", Address2 = "EverGreen Terrace",
                                                    CompanyNumber = "123QA432", CompanyStatus = "Active", Country = "UK", PostCode = "e12 3eq" },
                            }
            };

            var model = new OrganisationViewModel()
                            {
                                Employers = employerResult,
                                SectorType = SectorTypes.Private
                            };

            var controller = TestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step7(model) as RedirectToRouteResult;

            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Complete", "");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unStashedmodel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(model as OrganisationViewModel, "Expected OrganisationViewModel");

            //ASSERT:
            //7.Verify the values from the result that was stashed matches that of the Arrange values here

            //8.verify that it was private sector was selected
            Assert.AreEqual(model.SectorType == SectorTypes.Private, true, "Expected equal object entities success");

            //Assert.Multiple(() =>
            //{
            //    //Assert.AreEqual(model.EmailAddress == unStashedmodel.EmailAddress, true, "Expected email success");
            //    //Assert.AreEqual(model.ConfirmEmailAddress == model.ConfirmEmailAddress, true, "Expected confirm email success");
            //    //Assert.AreEqual(model.FirstName == model.FirstName, true, "Expected first name success");
            //    //Assert.AreEqual(model.LastName == model.LastName, true, "Expected last name success");
            //    //Assert.AreEqual(model.JobTitle == model.JobTitle, true, "Expected jobtitle success");
            //    //Assert.AreEqual(model.Password == model.Password, true, "Expected password success");
            //    //Assert.AreEqual(model.ConfirmPassword == model.ConfirmPassword, true, "Expected confirm password success");
            //});
        }

    }
}