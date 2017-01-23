using GenderPayGap.Controllers;
using GenderPayGap.Models;
using GenderPayGap.Models.GpgDatabase;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GenderPayGap.Tests.Registeration
{
    [TestFixture]
    public class RegistrationTestClass : AssertionHelper
    {
       private User user = null;
       private RegisterController reg = null;

        [SetUp]
        [Description("")]
        public void Setup()
        {
            reg = new RegisterController();
            bool resultAuth = reg.Authorise();

            ConfirmViewModel model = new ConfirmViewModel();
            ViewResult resultRegPost = reg.Confirm(model) as ViewResult;
            ViewResult resultRegGet = reg.Confirm(null, 0) as ViewResult;

           //user
           user = reg.GetCurrentUser();

        }

        [Test]
        [TestCase("https://localhost:44371/Return")]
        [Description("When user clicks on the Sign in link, user is taken to Sign in page")]
        public void SignInLinkValidation()
        {
            // TODO: Add your test code here
            //Use the Constrain Model only
            //verify that you are navigated to the sign-in page
            
            // Arrange

            // Act
           
            // Assert
            Assert.That(false, "Error Message");
           

        }

        [Test, Order(1)]
        [Description("Verify that the user is not already logged in")]
        public void VerifyUserIsNotLoggedIn()
        {
            // TODO: Add your test code here
            // Arrange

            // Act

            // Assert
            //Assert.That(false, "Error Message");
            Assert.That(user != null, "Error Message");
        }

        [Test, Order(2)]
        [Description("Verify that the user does not exist")]
        public void VerifyRegisteringUserDoesNotExist()
        {
            // TODO: Add your test code here
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test, Order(3)]
        [Description("Verify that all fields are empty hence not populated")]
        public void VerifyUserFormFieldsAreEmpty()
        {
            // TODO: Add your test code here
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test, Order(3)]
        [Description("Verify that all fields are empty hence not populated")]
        public void VerifyUserFormFieldsInputs()
        {
            // TODO: Add your test code here
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("Verify that all fields are empty hence not populated")]
        public void VerifyPasswordRule()
        {
            // TODO: Add your test code here
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("Verify that all fields are empty hence not populated")]
        public void VerifyPasswordAndConfirmPasswordEquality()
        {
            // TODO: Add your test code here
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }


        [Test]
        [Description("Verify that button verifies and validates all fields before registering user")]
        public void VerifyContinueButton()
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

    }
}
