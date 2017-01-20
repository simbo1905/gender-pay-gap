using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GenderPayGap.Tests.Login
{
    [TestFixture]
    public class LoginTest
    {
        //TODO: Use the Constrain Model only

       // Asserts

        [Test]
        [Description("This test is to verify that feedback link links to/opens the feed back page")]
        public void VerifyFeedBackLink()
        {
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("This test is to verify that register link  links to/ opens the registeration page")]
        public void VerifyRegisterLink()
        {
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("When user clicks on the register link from Sign-in, user is taken to register page (1 of 6)")]
        public void RegisterLinkValidation()
        {
            // TODO: Add your test code here

            // Arrange
            // const string returnUrl = "/Register";

            // Act
            //var actual = _sut.GetResult(returnUrl) as RedirectResult;

            // Assert
            // Assert.That(actual, Is.Not.Null);
            // Assert.That(actual.Url, Is.EqualTo("/Register"));
            Assert.That(false, "Error Message");

        }

        [Test]
        [Description("This test is to verify that the email field is empty")]
        public void VerifyEmailFieldIsEmpty()
        {
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("This test is to verify that password field is empty")]
        public void VerifyPasswordFieldIsEmpty()
        {
            Assert.That(false, "Error Message");
        }

       
        [Test]
        [Description("This test is to verify that the user failed to / successfully logged in")]
        public void VerifyLogin()
        {
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("This test is to verify that the forgotten password link links to/ open appropriate page")]
        public void VerifyForgottenPasswordLink()
        {
            Assert.That(false, "Error Message");
        }
       



    }
}
