using GenderPayGap.Controllers;
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
    public class QueryControllerTest
    {
        [Test]
        [Description("This test is to verify that the user successfully logged in")]
        public void Index()
        {
            //TDD:
            // Arrange
            QueryController controller = new QueryController();

            // Act
            var result = (ViewResult)controller.Index();

            // Assert
            //Positive Test:
            //Assert.That(result != null, "Error Message");


            //Negative Test:
            Assert.That(result == null, "Error Message");
        }


        [Test]
        public void Start()
        {
            //TDD:
            // Arrange
            QueryController controller = new QueryController();

            // Act
            ViewResult result = controller.Start() as ViewResult;

            // Assert
            //Positive Test:
            //Assert.That(result != null, "Error Message");
            

            //Negative Test:
            Assert.That(result == null, "Error Message");
        }
    }
}
