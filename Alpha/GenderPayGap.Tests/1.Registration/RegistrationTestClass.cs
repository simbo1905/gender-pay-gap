using GenderPayGap.Controllers;
using GenderPayGap.Models;
using GenderPayGap.Models.GpgDatabase;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using System.Web;
using System.Security.Principal;
using System.Web.Routing;

namespace GenderPayGap.Tests.Registeration
{

    class MockHttpContext : HttpContextBase
    {
        private readonly IPrincipal _user = new GenericPrincipal(new GenericIdentity("2"), null /* roles */);
        public override IPrincipal User
        {
            get { return _user; }
            set { base.User = value; }
        }
    }


    [TestFixture]
    public class RegistrationTestClass : AssertionHelper
    {
        private User user = null;
        private RegisterController controller = null;

        //public RegisterController SetupContext(long userid = 0)
        //{
        //    return null;
        //}

        private static RegisterController GetRegisterController()
        {
            RegisterController controller = new RegisterController();
            controller.ControllerContext = new ControllerContext()
            {
                Controller = controller, RequestContext = new RequestContext(new MockHttpContext(), new RouteData())
            };
            return controller;
        }


        [SetUp]
        [Description("Setup variables for class and test methods of this class")]
        public void Setup()
        {
            controller = GetRegisterController();
            bool resultAuth = controller.Authorise();

            ConfirmViewModel model = new ConfirmViewModel();
            ViewResult resultRegPost = controller.Confirm(model) as ViewResult;
            ViewResult resultRegGet = controller.Confirm(null, 0) as ViewResult;

            //user
            user = controller.GetCurrentUser();

        }

        [Test, Order(1)]
        [Description("Verify that the user is not already logged in")]
        public void VerifyUserIsNotLoggedIn()
        {
            // Arrange
            var expected = user;

            // Act
            User actual = null;

            // Assert
            Assert.That(actual == expected, "Error Message");

        }

        [Test, Order(2)]
        [Description("Verify that the user does not exist")]
        public void VerifyRegisteringUserDoesNotExist()
        {
            // TODO: Add your test code here
            // Arrange
            //Setup controller with mock httpcontext and no user logged in
           var controller = GetRegisterController();

            // Act
            // call the controller
            //var controller = new RegisterController();
            var result = controller.Index() as ViewResult;
            var model = result.Model as RegisterViewModel;

            // Assert
            //Check the ReturnResult.Model all fields are empty
            Assert.Multiple(() =>
            {
                Assert.That(string.IsNullOrWhiteSpace(model.IdentityProvider), "IdentityProvider address should be empty");
                Assert.That(string.IsNullOrWhiteSpace(model.EmailAddress), "Email address should be empty");
                Assert.That(string.IsNullOrWhiteSpace(model.ConfirmEmailAddress), "Confirm Email address should be empty");
                Assert.That(string.IsNullOrWhiteSpace(model.FirstName), "FirstName address should be empty");
                Assert.That(string.IsNullOrWhiteSpace(model.LastName), "LastName address should be empty");
                Assert.That(string.IsNullOrWhiteSpace(model.JobTitle), "JobTitle address should be empty");
                Assert.That(string.IsNullOrWhiteSpace(model.Password), "Password address should be empty");
                Assert.That(string.IsNullOrWhiteSpace(model.ConfirmPassword), "ConfirmPassword address should be empty");
            });
        }

        [Test, Order(3)]
        [Description("Verify that all fields are empty hence not populated")]
        public void VerifyFormFieldsAreEmpty(RegisterViewModel modelEmpty)
        {
            // TODO: Add your test code here
            // Arrange
            //Add dummy data form form
            // formEmpty.Add()

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test, Order(3)]
        [Description("Verify that all fields are now populated")]
        public void VerifyFormFieldsInputs(FormCollection formFilled)
        {
            // TODO: Add your test code here
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("Verify that password criteria/rule has been used to create a password")]
        public void VerifyPasswordRule(FormCollection formPassword)
        {
            // Arrange
            //use regular expression for validation
            var expected = user.Password; //"Password thst matches this regexp rule";

            // Act
            User actual = new User();
            actual.Password = formPassword.Get("PasswordKey");

            // Assert
            Assert.That(actual.Password == expected, "Error Message");
        }

        [Test]
        [Description("Verify that all fields are empty hence not populated")]
        public void VerifyPasswordAndConfirmPasswordEquality(FormCollection formPassword)
        {
            // Arrange
            User actual = new User();
            var expected = formPassword.Get("PasswordConfirmKey");

            // Act
            //use regular expression for validation
            actual.Password = user.Password; //"Password thst matches this regexp rule";

            // Assert
            Assert.That(actual.Password == expected, "Error Message");
        }

        [Test]
        [Description("Verify that button verifies and validates all fields before registering user")]
        public void VerifyContinueButton(FormCollection formPassword)
        {
            // TODO: Add your test code here
            // Arrange

            // Act

            // Assert
            //Verify all fields
            //verify confirmation email is sent
            //verifu confirmation email is received ?
            //verify confirmation page

            Assert.That(false, "Error Message");
        }

        public void TestModelIsEmptywhenUserNotLoggedIn()
        {
            controller = GetRegisterController();
        }

        public void TestModelIsCorrectAndFilledWhenUserLoggedIn()
        {
            controller = GetRegisterController();
        }
    }

    

}
