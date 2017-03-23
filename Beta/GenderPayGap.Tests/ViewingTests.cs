using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models;
using GenderPayGap.Models.SqlDatabase;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using GenderPayGap.WebUI.Classes;
using System.Web.Routing;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Properties;
using GenderPayGap.WebUI.Models.Search;


namespace GenderPayGap.Tests
{
    [TestFixture]
    public class ViewingTests : AssertionHelper
    {
        #region search-results

        #region Positive tests

        [Test]
        [Description("")]
        public void SearchResults_Get_Success()
        {
            //ARRANGE:
            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "SearchResults");
            routeData.Values.Add("Controller", "Viewing");

            //Stash an object to pass in for this.ClearStash()
            var model = new SearchViewModel();

            var controller = TestHelper.GetController<ViewingController>(0, routeData /*, model*/);
            controller.StashModel(model);

            //ACT:
            var result = controller.SearchResults() as ViewResult;


            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "SearchResults", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(SearchViewModel), "Expected SearchViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

        }

        [Test]
        [Description("")]
        public void SearchResults_Post_Success()
        {
            //ARRANGE:
            SearchViewModel sModel = new SearchViewModel()
                                        {

                                        };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "SearchResults");
            routeData.Values.Add("Controller", "Viewing");

            //Stash an object to pass in for this.ClearStash()
            SearchViewModel model = new SearchViewModel() { };
            string command = "search"; //do additional tests for pageNext, pagePreview, page_ + pageNo:

            var controller = TestHelper.GetController<ViewingController>(0, routeData /*, model*/);
            controller.StashModel(sModel);

            //ACT:
            var result = controller.SearchResults(model, command) as ViewResult;


            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "SearchResults", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(RegisterViewModel), "Expected RegisterViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

        }

        #endregion

        #region Negative Tests.

        #endregion

        #endregion

        #region download
        [Test]
        [Description("")]
        public void Download_Get_Success()
        {
            //ARRANGE:
            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Download");
            routeData.Values.Add("Controller", "Viewing");

            //Stash an object to pass in for this.ClearStash()
            var model = new DownloadViewModel();

            var controller = TestHelper.GetController<ViewingController>(0, routeData /*, model*/);
            controller.StashModel(model);

            //ACT:
            var result = controller.Download() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Download", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(DownloadViewModel), "Expected DownloadViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

        }

        [Test]
        [Description("")]
        public void DownloadData_Get_Success()
        {
            //ARRANGE:
            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "DownloadData");
            routeData.Values.Add("Controller", "Viewing");

            //Stash an object to pass in for this.ClearStash()
            var model = new DownloadViewModel();

            var controller = TestHelper.GetController<ViewingController>(0, routeData /*, model*/);
            controller.StashModel(model);

            int year = 2016;
            //ACT:
            var result = controller.DownloadData(year) as ContentResult;
            //var contentResult = controller.DownloadData(year) as FileContentResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ContentResult), "Incorrect resultType returned");
            
            //Assert.That(result.FileDownloadName == "DownloadData", "Incorrect view returned");
           // Assert.That(result.Model != null && result.Model.GetType() == typeof(DownloadViewModel), "Expected SearchViewModel or Incorrect resultType returned");
           // Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }
        #endregion

        #region employer-details
        [Test]
        [Description("")]
        public  void EmployerDetails_Get_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "employer-details");
            routeData.Values.Add("Controller", "Viewing");

            //Stash an object to pass in for this.ClearStash()
            //var model = new SearchViewModel();

            var controller = TestHelper.GetController<ViewingController>(user.UserId, routeData, organisation /*, model*/);
            
            //controller.StashModel(model);

            string id = "1";
            string view = null;
            
            //ACT:
            var result = controller.EmployerDetails(organisation.OrganisationId.ToString(), null) as ViewResult;


            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "HourlyRate", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(SearchViewModel), "Expected SearchViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }
        #endregion
    }
}
