using GenderPayGap.WebUI.Controllers;
using GenderPayGap.Models.SqlDatabase;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GenderPayGap.Tests.Navigation.Controllers
{
    [TestFixture]
    public class ReturnControllerTest
    {

        private ReturnController controller;
        private  Return myReturn;
        private User user;

        #region Helper Methods
        public void UserLoggedIn(User _user)
        {

        }

        public void UserLoggedNotIn(User _user)
        {

        } 

        #endregion

        [SetUp]
        public void Setup()
        {
            controller = new ReturnController();
            myReturn = new Return();

            //Mock Request.Url.AbsoluteUri 
            HttpRequest httpRequest      = new HttpRequest("", "http://mySomething", "");
            StringWriter stringWriter    = new StringWriter();
            HttpResponse httpResponse    = new HttpResponse(stringWriter);
            HttpContext httpContextMock  = new HttpContext(httpRequest, httpResponse);
            controller.ControllerContext = new ControllerContext(new HttpContextWrapper(httpContextMock), new RouteData(), controller);
            string methodContext = controller.HttpContext.Request.HttpMethod;
        }

        [Test]
        [Description("This test is to verify that the user successfully logged in")]
        public void VerifyLoginPassed()
        {
            // TDD:
            // Arrange
            
            // Act

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(false, "Error Message");
        }

        #region Test: Action methods return Views
        [Test]
        [Description("Test to validate index view")]
        public void IndexActionReturnsIndexView()
        {
            // TDD:
            // Arrange
             controller = new ReturnController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(result, Is.EqualTo("Index"), "Error Message");
           
        }

        [Test]
        [TestCase("1")]
        [Description("Test to validate Create view")]
        public void CreateActionReturnsCreateView()
        {

            //Add HTTPOST test for Create in return
            
	


            // TDD:
            // Arrange
            controller = new ReturnController();
            myReturn = new Return();

            // Act
            var result = controller.Step1() as ViewResult;
            //var resultWithSuppliedModel = controller.Create(myReturn) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(result, Is.EqualTo("Create"),  "Error Message");
            //Assert.That(resultWithSuppliedModel, Is.EqualTo("Create"), "Error Message");
        }

        [Test]
        [TestCase("1")]
        [Description("Test to validate Authoriser view")]
        public void Authoriser()
        {
            //Add HTTPOST test for Authoriser

            // TDD:
            // Arrange
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();

            // Act
            //ViewResult result = controller.Authoriser(myReturn) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            //Assert.That(result == null, "Error Message");
        }

        [Test]
        [Description("Test to validate Confirmed")]
        public void Confirm()
        {
            //Add HTTPOST test for Confirm in return
            // TDD:
            // Arrange
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();

            // Act
            var resultWithDefaultID = controller.Step3() as ViewResult;
            //var resultWithSuppliedID = controller.Confirm(1) as ViewResult;
            //var resultWithSuppliedModel = controller.Confirm(myReturn) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            Assert.That(resultWithDefaultID == null, "Error Message");
            //Assert.That(resultWithSuppliedID == null, "Error Message");
            //Assert.That(resultWithSuppliedModel == null, "Error Message");
        }

        [Test]
        [Description("Test to Validate SendConfirmed")]
        public void SendConfirm()
        {
            //Add HTTPOST test for Create in return

            // TDD:
            // Arrange
            ReturnController controller = new ReturnController();
            Return myReturn = new Return();

            // Act
            //var resultWithDefaultID = controller.Step4() as ViewResult;
            //var resultWithSuppliedID = controller.SendConfirmed(1) as ViewResult;
            //var resultWithSuppliedModel = controller.SendConfirmed(myReturn) as ViewResult;

            // Assert
            // TODO: RED GREEN REFACTOR
            // Negative Test:
            //Assert.That(resultWithDefaultID == null, "Error Message");
            //Assert.That(resultWithSuppliedID == null, "Error Message");
            //Assert.That(resultWithSuppliedModel == null, "Error Message");
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

        [HttpGet]
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(null)]
        [Description("Details action should call the details View")]
        public void DetailsActionReturnsDetailsView( int param)
        {
            var controller = new ReturnController();
            var result = controller.Details(param) as ViewResult;
            Assert.That(result.ViewName, Is.EqualTo("Details"));
        }
        #endregion
    }
}
