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
        HomeController controller;

        #region Test: Action methods return Views
        [Test]
        [Description("Index action should call the index View")]
        public void IndexActionReturnsIndexView()
        {
            //Arrange
            controller = new HomeController();
            //Act
            ViewResult result = controller.Index() as ViewResult;
            //Assert
            Assert.That(result.ViewName, Is.EqualTo("index"));
        }
        #endregion



    }
}
