using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models;
using NUnit.Framework;
using System.Web.Mvc;
using GenderPayGap.Models.SqlDatabase;
using System;

namespace GenderPayGap.Tests.Registeration
{
    [TestFixture]
    public class RegistrationTestClass : AssertionHelper
    {


        [SetUp]
        [Description("Setup variables for class and test methods of this class")]
        public void Setup()
        {

        }

        [Test, Order(1)]
        [Description("Ensure the register.Index action the user is not already logged in")]
        public void EnsureRegisterControllerReturned()
        {
            // Arrange
            var user = new User() { UserId = 1,EmailVerifiedDate=DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userorganisation = new UserOrganisation() { OrganisationId = 1, UserId=1 };

            var controller = TestHelper.GetController<RegisterController>(1,user,organisation,userorganisation);

            // Act
            var result= controller.Step1();

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
        }

        [Test, Order(2)]
        [Description("Verify that the user does not exist")]
        public void VerifyRegisteringUserDoesNotExist()
        {
            // TODO: Add your test code here
            // Arrange
            //Setup controller with mock httpcontext and no user logged in
           var controller = TestHelper.GetController<RegisterController>();

            // Act
            // call the controller
            //var controller = new RegisterController();
            var result = controller.Step1() as ViewResult;
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
            //var expected = user.Password; //"Password thst matches this regexp rule";

            // Act
            //User actual = new User();
            //actual.Password = formPassword.Get("PasswordKey");

            // Assert
            //Assert.That(actual.Password == expected, "Error Message");
        }

        [Test]
        [Description("Verify that all fields are empty hence not populated")]
        public void VerifyPasswordAndConfirmPasswordEquality(FormCollection formPassword)
        {
            // Arrange
            //User actual = new User();
            var expected = formPassword.Get("PasswordConfirmKey");

            // Act
            //use regular expression for validation
            //actual.Password = user.Password; //"Password thst matches this regexp rule";

            // Assert
            //Assert.That(actual.Password == expected, "Error Message");
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
            //controller = GetRegisterController();
        }

        public void TestModelIsCorrectAndFilledWhenUserLoggedIn()
        {
            //controller = GetRegisterController();
        }
    }

    

}
