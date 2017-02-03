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
        #region Test: Action methods return Views
        [Test]
        [Description("Index action should call the index View")]
        public void IndexActionReturnsIndexView()
        {
            //TDD:
            // Arrange
            QueryController controller = new QueryController();

            // Act
            var result = (ViewResult)controller.Index();

            // Assert
            //Positive Test:
            //Negative Test:
            Assert.That(result, Is.EqualTo("Index"), "Error Message");
        }

        [Test]
        [Description("Start action should call the start View")]
        public void StartActionReturnsStartView()
        {
            //TDD:
            // Arrange
            QueryController controller = new QueryController();

            // Act
            ViewResult result = controller.Start() as ViewResult;

            // Assert
            //Positive Test:
            //Negative Test:
            Assert.That(result, Is.EqualTo("Start"), "Error Message");
        }

        [Test]
        [Description("Search action should call the search View")]
        public void SearchActionReturnSearchView()
        {
            //Add HTTPOST test

            //TDD:
            // Arrange
            QueryController controller = new QueryController();

            // Act
            ViewResult result = controller.Search() as ViewResult;

            // Assert
            Assert.That(result, Is.EqualTo("Search"), "Error Message");
        }

        [Test]
        [Description("Sectors action should call the sectors View")]
        public void SectorsActionReturnSectorsView()
        {
            //TDD:
            // Arrange
            QueryController controller = new QueryController();

            // Act
            ViewResult result = controller.Sectors() as ViewResult;

            // Assert
            Assert.That(result, Is.EqualTo("Sectors"), "Error Message");
        }

        [Test]
        [Description("Download action should call the download View")]
        public void DownloadActionReturnsDownloadView()
        {
            //TDD:
            // Arrange
            QueryController controller = new QueryController();

            // Act
            ViewResult result = controller.Download() as ViewResult;

            // Assert
            Assert.That(result, Is.EqualTo("Download"), "Error Message");
        } 
        #endregion
    }
}
