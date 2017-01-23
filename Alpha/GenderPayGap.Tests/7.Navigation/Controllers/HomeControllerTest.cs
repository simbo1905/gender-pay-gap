using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using GenderPayGap;
using GenderPayGap.Controllers;

namespace GenderPayGap.Tests.Navigation.Controllers
{
    [TestFixture]
    public class HomeControllerTest : AssertionHelper
    {


        [Test]
        public void Index()
        {

            //TDD:

            // Arrange
            HomeController controller = new HomeController();
            //Assert.That<HomeController>(controller, null, "" );

            // Act
            ViewResult result = controller.Index() as ViewResult;

            //break these out into individual test methods to have one assert each
            // Asserts:
            //Positive Tests:
            //Assert.That(result != null, "");
            //Assert.That(result.ViewBag.Title == "Home Page", "The HomeController View Title is not set as 'Home Page'!");

            //Negative Tests:
            Assert.That(result == null, "");
            Assert.That(result.ViewBag.Title != "Home Page" || result.ViewBag.Title != "", "");
        }

    }
}
