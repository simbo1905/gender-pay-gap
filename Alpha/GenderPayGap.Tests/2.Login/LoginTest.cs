using GenderPayGap.Models.GpgDatabase;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GenderPayGap.Tests.Login
{
    [TestFixture]
    public class LoginTest
    {

        #region Helper Method
        private HttpContext GetHttpContext()
        {
            throw new NotImplementedException();
        }

        private User GetLoggedInUser()
        {
            throw new NotImplementedException();
        }
        #endregion

        [SetUp]
        public void Setup()
        {
            //this.SetupTest(session => // the NH/EF session to attach the objects to
            //{
            //    var user = new UserAccount("Mr", "Joe", "Bloggs");
            //    session.Save(user);
            //    this.UserID = user.UserID;
            //});

            //Helper Methods
            HttpContext httpContext = GetHttpContext();
            User currUser = GetLoggedInUser();
        }




        //TODO: Use the Constrain Model only

        // Asserts

        [Test]
        [TestCase("www.feedbacklink.com")] //surveyMonkey
        [Description("This test is to verify that feedback link links to/opens the feed back page")]
        public void VerifyFeedBackLink(string link)
        {
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test]
        [TestCase("https://localhost:44371/Register")]
        [Description("This test is to verify When user clicks on the register link from Sign-in, user is taken to register page (1 of 6)")]
        public void VerifyRegisterLink()
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
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("This test is to verify that password field is empty")]
        public void VerifyPasswordFieldIsEmpty()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

       
        [Test]
        [Description("This test is to verify that the user failed to / successfully logged in")]
        public void VerifyLogin()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("This test is to verify that the forgotten password link links to/ open appropriate page")]
        public void VerifyForgottenPasswordLink()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(false, "Error Message");
        }


    }
}
