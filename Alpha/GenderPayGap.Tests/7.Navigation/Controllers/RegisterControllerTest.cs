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
    public class RegisterControllerTest
    {
        [Test]
        public void TestMethod()
        {
            // TODO: Add your test code here
            //Assert.Pass("Your first passing test");
            Assert.That("Your first passing test", Is.EqualTo(true));
        }


        // Test Action Methods and Views
        [Test]
        public void Index()
        {
            //TDD:
            // Arrange
            RegisterController controller = new RegisterController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            //Assert.That(result != null, "Error Message");

            //Negative Test:
            Assert.That(result == null, "Error Message");
        }

        public void Index( object model)
        {

        }

        public void Verify(string code=null)
        {

        }

        public void Verify()
        {

        }
    }
}
