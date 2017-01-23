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

namespace GenderPayGap.Tests.Navigation.Controllers
{
    [TestFixture]
    public class RegisterControllerTest
    {
        [SetUp]
        public void Setup()
        {
            RegisterController controller = new RegisterController();
            VerifyViewModel verifyViewModel = new VerifyViewModel();
        }

        // Test Action Methods and Views

        [Test]
        [Description("Test to validate Index view")]
        public void Index()
        {
            // TDD:
            // Arrange
            RegisterController controller = new RegisterController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(result == null, "Error Message");
        }

        [Test]
        [Description("Test to validate Verify view")]
        public void Verify()
        {
            // TDD:
            // Arrange
            RegisterController controller = new RegisterController();
            VerifyViewModel verifyViewModel = new VerifyViewModel();

            // Act
            var resultWithDefaultID = controller.Verify() as ViewResult;
            var resultWithSuppliedID = controller.Verify("") as ViewResult;
            var resultWithSuppliedModel = controller.Verify(verifyViewModel) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(resultWithDefaultID == null, "Error Message");
            Assert.That(resultWithSuppliedID == null, "Error Message");
            Assert.That(resultWithSuppliedModel == null, "Error Message");
        }

        [Test]
        [Description("Test to validate Organisation view")]
        public void Organisation()
        {
            // TDD:
            // Arrange
            RegisterController controller = new RegisterController();
            OrganisationViewModel orgViewModel = new OrganisationViewModel();

            // Act
            var resultWithDefaultID = controller.Organisation() as ViewResult;
            var resultWithSuppliedID = controller.Organisation("") as ViewResult;
            var resultWithSuppliedModel = controller.Organisation(orgViewModel) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(resultWithDefaultID == null, "Error Message");
            Assert.That(resultWithSuppliedID == null, "Error Message");
            Assert.That(resultWithSuppliedModel == null, "Error Message");
        }

        [Test]
        [Description("Test to validate Confirmed")]
        public void Confirm()
        {
            // TDD:
            // Arrange
            RegisterController controller = new RegisterController();
            ConfirmViewModel confirmViewModel = new ConfirmViewModel();

            // Act
            var resultWithDefaultID = controller.Confirm() as ViewResult;
            var resultWithSuppliedID = controller.Confirm("", 0) as ViewResult;
            var resultWithSuppliedModel = controller.Confirm(confirmViewModel) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(resultWithDefaultID == null, "Error Message");
            Assert.That(resultWithSuppliedID == null, "Error Message");
            Assert.That(resultWithSuppliedModel == null, "Error Message");
        }


    }
}
