using GenderPayGap.Controllers;
using GenderPayGap.Models;
using GenderPayGap.Models.GpgDatabase;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GenderPayGap.Tests.Navigation.Controllers
{
    [TestFixture]
    public class RegisterControllerTest
    {

        RegisterController controller;
        VerifyViewModel verifyViewModel;

      [SetUp]
        public void Setup()
        {
             controller = new RegisterController();
             verifyViewModel = new VerifyViewModel();
        }

        public void createContext()
        {
            //Mock Request.Url.AbsoluteUri 
            HttpRequest httpRequest = new HttpRequest("", "http://mySomething", "");
            StringWriter stringWriter = new StringWriter();
            HttpResponse httpResponse = new HttpResponse(stringWriter);
            HttpContext httpContextMock = new HttpContext(httpRequest, httpResponse);
            controller.ControllerContext = new ControllerContext(new HttpContextWrapper(httpContextMock), new RouteData(), controller);
            string methodContext = controller.HttpContext.Request.HttpMethod;
        }


        #region Test: Action methods return Views
        [Test]
        [Description("Test to validate Index view")]
        public void IndexActionReturnsIndexView()
        {

            //Add HTTPPOST logic here 

            // TDD:
            // Arrange
            RegisterController controller = new RegisterController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(result, Is.EqualTo("Index"), "Error Message");
        }

        [Test]
        [Description("Test to validate Verify view")]
        public void Verify()
        {
            //Add HTTPPOST logic here

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
            //Add HTTPPOST logic here

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
            //Add HTTPPOST logic here

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
        #endregion


    }
}
