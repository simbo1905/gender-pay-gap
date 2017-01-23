using GenderPayGap.Controllers;
using GenderPayGap.Models.GpgDatabase;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GenderPayGap.Tests.Navigation.Controllers
{
    [TestFixture]
    public class ReturnControllerTest
    {
        [SetUp]
        public void Setup()
        {
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();
        }

        [Test]
        [Description("This test is to verify that the user successfully logged in")]
        public void VerifyLoginPassed()
        {
            //Note: FEEDBACK Link is not ready yet. So this cannot be Tested
            
            // TDD:
            // Arrange
            
            // Act

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(false, "Error Message");
        }

        [Test]
        [Description("Test to validate index view")]
        public void Index()
        {
            // TDD:
            // Arrange
             var controller = new ReturnController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(result == null, "Error Message");
           
        }

        [Test]
        [TestCase("1")]
        [Description("Test to validate Create view")]
        public void Create()
        {
            // TDD:
            // Arrange
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();

            // Act
            var result = controller.Create() as ViewResult;
            var resultWithSuppliedModel = controller.Create(myReturn) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(result == null, "Error Message");
            Assert.That(resultWithSuppliedModel == null, "Error Message");
        }

        [Test]
        [TestCase("1")]
        [Description("Test to validate Authoriser view")]
        public void Authoriser()
        {
            // TDD:
            // Arrange
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();

            // Act
            ViewResult result = controller.Authoriser(myReturn) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(result == null, "Error Message");
        }

        [Test]
        [Description("Test to validate Confirmed")]
        public void Confirm()
        {
            // TDD:
            // Arrange
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();

            // Act
            var resultWithDefaultID = controller.Confirm() as ViewResult;
            var resultWithSuppliedID = controller.Confirm(1) as ViewResult;
            var resultWithSuppliedModel = controller.Confirm(myReturn) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(resultWithDefaultID == null, "Error Message");
            Assert.That(resultWithSuppliedID == null, "Error Message");
            Assert.That(resultWithSuppliedModel == null, "Error Message");
        }

        [Test]
        [Description("Test to Validate SendConfirmed")]
        public void SendConfirm()
        {
            // TDD:
            // Arrange
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();

            // Act
            var resultWithDefaultID = controller.SendConfirmed() as ViewResult;
            var resultWithSuppliedID = controller.SendConfirmed(1) as ViewResult;
            var resultWithSuppliedModel = controller.SendConfirmed(myReturn) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(resultWithDefaultID == null, "Error Message");
            Assert.That(resultWithSuppliedID == null, "Error Message");
            Assert.That(resultWithSuppliedModel == null, "Error Message");
        }

        [Test]
        [Description("Test to Validate Details of the record")]
        public void Details()
        {
            // TDD:
            // Arrange
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();

            // Act
            var resultWithDefaultID = controller.Details() as ViewResult;
            var resultWithSuppliedID = controller.Details(1) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(resultWithDefaultID == null, "Error Message");
            Assert.That(resultWithSuppliedID == null, "Error Message");
        }

    }
}
