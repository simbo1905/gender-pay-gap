using GenderPayGap.Controllers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GenderPayGap.Tests._7.Navigation.Controllers
{
    [TestFixture]
    public class ReturnControllerTest
    {
        [Test]
        [Description("This test is to verify that the user successfully logged in")]
        public void VerifyLoginPassed()
        {
            Assert.That(false, "Error Message");
        }

        [Test]
        public void Index()
        {
            //TDD:
            // Arrange
            ReturnController controller = new ReturnController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            //Assert.That(result != null, "Error Message");

            //Negative Test:
            Assert.That(result == null, "Error Message");
        }
    }
}
