using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models;
using GenderPayGap.Database;
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
using GenderPayGap.WebUI.Models.Submit;

namespace GenderPayGap.Tests
{
    [TestFixture]
    public class ViewingTests : AssertionHelper
    {
        #region search-results

        #region Positive tests

        [Test]
        [Description("Ensure the Search Results form is returned for the current user")]
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

        //[Ignore("This test is ignored as Post HTTP is beig taking out")]
        [Test]
        [Description("Search Result:Ensure that search results form is filled and sent successfully and only If search text, sectors or page changed then redirect to search page")]
        public void SearchResults_Post_Success_Redirect()
        {
            //ARRANGE:
            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "SearchResults");
            routeData.Values.Add("Controller", "Viewing");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { Name = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "123QA10", CompanyStatus = "Active", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { Name = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "123QA11", CompanyStatus = "Active", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { Name = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "123QA12", CompanyStatus = "Active", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { Name = "Trax ltd",  Address1 = "13", Address2 = "EverGreen Terrace", CompanyNumber = "123QA13", CompanyStatus = "Active", Country = "UK", PostCode = "sw2  5gh" },
                    new EmployerRecord() { Name = "Exant ltd", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "123QA14", CompanyStatus = "Active", Country = "UK", PostCode = "se2  2bh" }
                }
            };

            //explicit set for testing.
            int currentPage = employerResult.CurrentPage = 0;
            
            SearchViewModel sModel = new SearchViewModel()
            {
                 Employers = employerResult,
                //TODO: is SearchText noe search? SearchText = "Acme",
                search = "Acme",
            };

            //Stash an object to pass in for this.ClearStash()
            SearchViewModel model = new SearchViewModel()
            {
                Employers = employerResult, //just Added
                search = "Acme"
            };

            string command = "search"; //do additional tests for pageNext, pagePreview, page_ + pageNo:

            var controller = TestHelper.GetController<ViewingController>(0, routeData /*, model*/);
            controller.StashModel(sModel);

            //ACT:
            var result = controller.SearchResults(model.search) as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "SearchResults", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(SearchViewModel), "Expected RegisterViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            var unStashedmodel = controller.UnstashModel<SearchViewModel>();
            Assert.NotNull(unStashedmodel, "Expected OrganisationViewModel");

          //  Assert.AreEqual(model == unStashedmodel, true, "Expected equal object entities success");
        }

        //[Ignore("This test is ignored as Post HTTP is beig taking out")]
        [Test]
        [Description("Search Result: if search text does not change then show the same results")]
        public void SearchResults_Post_Success_ViewResult()
        {
            //ARRANGE:
            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "SearchResults");
            routeData.Values.Add("Controller", "Viewing");

            var employerResult = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { Name = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "123QA10", CompanyStatus = "Active", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { Name = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "123QA11", CompanyStatus = "Active", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { Name = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "123QA12", CompanyStatus = "Active", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { Name = "Trax ltd",  Address1 = "13", Address2 = "EverGreen Terrace", CompanyNumber = "123QA13", CompanyStatus = "Active", Country = "UK", PostCode = "sw2  5gh" },
                    new EmployerRecord() { Name = "Exant ltd", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "123QA14", CompanyStatus = "Active", Country = "UK", PostCode = "se2  2bh" }
                }
            };

            //explicit set for testing.
            int currentPage = employerResult.CurrentPage = 0;

            SearchViewModel sModel = new SearchViewModel()
            {
                Employers = employerResult,
                //TODO: is SearchText noe search? SearchText = "Acme",
                search = "Acme",
                //TODO: what is this replaced with ? NewSectors = new List<string>() {  },
                 
            };

            //Stash an object to pass in for this.ClearStash()
            SearchViewModel model = new SearchViewModel()
            {
                Employers = employerResult,
                //TODO: is SearchText noe search? SearchText = "Acme",
                search = "Acme",

            };
            string command = "search"; //do additional tests for pageNext, pagePreview, page_ + pageNo:

            var controller = TestHelper.GetController<ViewingController>(0, routeData /*, model*/);
            controller.StashModel(sModel);

            //ACT:
            var result = controller.SearchResults(model.search/*, command*/) as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "SearchResults", "Incorrect view returned");
            Assert.That(result.Model != null && result.Model.GetType() == typeof(SearchViewModel), "Expected RegisterViewModel or Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        #endregion

        #region Negative Tests.

        #endregion

        #endregion

        #region download

        [Test]
        [Description("Ensure the Download view form is returned for the current user")]
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
        [Description("Ensure the Download Data view form is returned for the current user")]
        public void DownloadData_Get_Success()
        {
            //ARRANGE:
            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "DownloadData");
            routeData.Values.Add("Controller", "Viewing");

            //Stash an object to pass in for this.ClearStash()
            var model = new DownloadViewModel();

            var controller = TestHelper.GetController<ViewingController>(0, routeData);
            controller.StashModel(model);

            //insert a file in the file repository
            Settings.Default["DownloadsLocation"] = AppDomain.CurrentDomain.BaseDirectory;
            var download = Settings.Default.DownloadsLocation;
            

            int year = 2016;
            //ACT:
            var result = controller.DownloadData(year) as ContentResult;
            //var contentResult = controller.DownloadData(year) as FileContentResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ContentResult), "Incorrect resultType returned");
            Assert.NotNull(result.Content, "Incorrect resultType returned");

            //Assert.That(result.FileDownloadName == "DownloadData", "Incorrect view returned");
            // Assert.That(result.Model != null && result.Model.GetType() == typeof(DownloadViewModel), "Expected SearchViewModel or Incorrect resultType returned");
            // Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }
        #endregion

        #region employer-details
        [Test]
        [Description("Ensure the Employer Details form is returned for the current user, for the selected employer")]
        public  void EmployerDetails_Get_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };

            var organisation = new Organisation()
            {
                OrganisationId = 1,
                SectorType = SectorTypes.Private,
                Returns = new List<Return>() { new Return(), new Return(), new Return() },
                OrganisationName = "Acme",
                OrganisationAddresses = new List<OrganisationAddress>()
                {
                    new OrganisationAddress() { Address1 = "123",
                                                Address2 = "evergreen Terrace",
                                                Address3 = "Heathrow",
                                                County   = "Twickenham",
                                                PostCode = "Tw1 2sx",
                                                OrganisationId = 1,
                                                Status = AddressStatuses.Active
                                              }
                },
                Status = OrganisationStatuses.Active
               
            };

          //  var activeAddress = organisation.ActiveAddress; //.GetAddress();

             var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = user.UserId, User = user, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //return in the db
            var @return = new Return()
            {
                AccountingDate = Settings.Default.PrivateAccountingDate.AddYears(-1),
                Status = ReturnStatuses.Submitted,
                ReturnStatuses = new List<ReturnStatus>() { new ReturnStatus() },
                ReturnId = 1,

                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,

                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 10,
                DiffMedianBonusPercent = 10,
                DiffMedianHourlyPercent = 10,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 10,
                FemaleMiddlePayBand = 10,
                FemaleUpperPayBand = 10,
                FemaleUpperQuartilePayBand = 10,
                MaleLowerPayBand = 10,
                MaleMedianBonusPayPercent = 10,
                MaleMiddlePayBand = 10,
                MaleUpperPayBand = 10,
                MaleUpperQuartilePayBand = 10,

                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Dev",

                CompanyLinkToGPGInfo = "http:www.geo.gov.uk"
               
                
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "employer-details");
            routeData.Values.Add("Controller", "Viewing");

            //Stash an object to pass in for this.ClearStash()
            //var model = new SearchViewModel();

            var controller = TestHelper.GetController<ViewingController>(user.UserId, routeData, organisation, user, userOrganisation, @return /*, model*/);
            //controller.StashModel(model);

            //ACT:
            //NO ARGUMENT FOR ID AND VIEW TEST -> call to Employers details with no parameters
            string id       = null;
            string view     = null;
            var result      = controller.EmployerDetails(id, view) as ViewResult;
            var resultModel = result.Model as ReturnViewModel;
            Assert.That(result.ViewName == "HourlyRate", "Incorrect view returned");

            //HOURLYRATE TEST: -> call to Employers details  Id with empty view - default to hourly rate
            id = Encryption.EncryptQuerystring(organisation.OrganisationId.ToString());
            view = null;
            result = controller.EmployerDetails(id, view) as ViewResult;
            resultModel = result.Model as ReturnViewModel;
            
            Assert.That(result.ViewName == "HourlyRate", "Incorrect view returned");

            //HOURLYRATE TEST: -> call to Employers details  Id and hourly-rate View
            id = Encryption.EncryptQuerystring(organisation.OrganisationId.ToString());
            view = "Hourly-Rate";
            result = controller.EmployerDetails(id, view) as ViewResult;
            resultModel = result.Model as ReturnViewModel;
            Assert.That(result.ViewName == "HourlyRate", "Incorrect view returned");

            //PAYQUARTILE TEST -> call to Employers details with value for Id and hourly-rate View parameters
             id          = Encryption.EncryptQuerystring(organisation.OrganisationId.ToString());
            view        = "Pay-Quartiles";
            result      = controller.EmployerDetails(id, view) as ViewResult;
            resultModel = result.Model as ReturnViewModel;
            Assert.That(result.ViewName == "PayQuartiles", "Incorrect view returned");

            //BONUS PAY TEST -> call to Employers details with no parameters
            id = Encryption.EncryptQuerystring(organisation.OrganisationId.ToString());
            view = "Bonus-Pay";
            result = controller.EmployerDetails(id, view) as ViewResult;
            resultModel = result.Model as ReturnViewModel;
            Assert.That(result.ViewName == "BonusPay", "Incorrect view returned");

            //ASSERT:
            //Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
            Assert.That(result != null && result is ViewResult, "Expected returned ViewResult object not to be null  or incorrect resultType returned");
            Assert.IsNotNull(resultModel, "Expected returned SearchViewModel object not to be null");
            Assert.That(resultModel is ReturnViewModel, "Expected Model to be of type ReturnViewModel");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.That(resultModel.DiffMeanBonusPercent == @return.DiffMeanBonusPercent, "DiffMeanBonusPercent:Expected a matching value in return from db");
            Assert.That(resultModel.DiffMeanHourlyPayPercent == @return.DiffMeanHourlyPayPercent, "DiffMeanHourlyPayPercent:Expected a matching value in return from db");
            Assert.That(resultModel.DiffMedianBonusPercent == @return.DiffMedianBonusPercent, "DiffMedianBonusPercent:Expected a matching value in return from db");
            Assert.That(resultModel.DiffMedianHourlyPercent == @return.DiffMedianHourlyPercent, "DiffMedianHourlyPercent:Expected a matching value in return from db");
            Assert.That(resultModel.FemaleLowerPayBand == @return.FemaleLowerPayBand, "FemaleLowerPayBand:Expected a matching value in return from db");
            Assert.That(resultModel.FemaleMedianBonusPayPercent == @return.FemaleMedianBonusPayPercent, "FemaleMedianBonusPayPercent:Expected a matching value in return from db");
            Assert.That(resultModel.FemaleMiddlePayBand == @return.FemaleMiddlePayBand, "FemaleMiddlePayBand:Expected a matching value in return from db");
            Assert.That(resultModel.FemaleUpperPayBand == @return.FemaleUpperPayBand, "FemaleUpperPayBand:Expected a matching value in return from db");
            Assert.That(resultModel.FemaleUpperQuartilePayBand == @return.FemaleUpperQuartilePayBand, "FemaleUpperQuartilePayBand:Expected a matching value in return from db");
            Assert.That(resultModel.MaleLowerPayBand == @return.MaleLowerPayBand, "MaleLowerPayBand:Expected a matching value in return from db");
            Assert.That(resultModel.MaleMedianBonusPayPercent == @return.MaleMedianBonusPayPercent, "MaleMedianBonusPayPercent:Expected a matching value in return from db");
            Assert.That(resultModel.MaleMiddlePayBand == @return.MaleMiddlePayBand, "MaleMiddlePayBand:Expected a matching value in return from db");
            Assert.That(resultModel.MaleUpperPayBand == @return.MaleUpperPayBand, "MaleUpperPayBand:Expected a matching value in return from db");
            Assert.That(resultModel.MaleUpperQuartilePayBand == @return.MaleUpperQuartilePayBand, "MaleUpperQuartilePayBand:Expected a matching value in return from db");
                   
            Assert.That(resultModel.FirstName == @return.FirstName, "FirstName:Expected a matching value in return from db");
            Assert.That(resultModel.LastName == @return.LastName, "LastName:Expected a matching value in return from db");
            Assert.That(resultModel.JobTitle == @return.JobTitle, "JobTitle:Expected a matching value in return from db");
                  
            Assert.That(resultModel.CompanyLinkToGPGInfo == @return.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfo:Expected a matching value in return from db");
        }

        #endregion
    }
}
