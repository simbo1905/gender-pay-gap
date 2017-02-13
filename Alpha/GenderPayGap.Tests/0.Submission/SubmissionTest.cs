
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using System.Web;
using System.Security.Principal;
using System.Web.Routing;
using System.Security.Claims;
using GenderPayGap.Tests.DBRespository;

using GenderPayGap.Tests;
using GenderPayGap.WebUI.Controllers;
using GpgDB.Models.GpgDatabase;
using System.Text.RegularExpressions;

namespace GenderPayGap.Tests.Submission
{

    //************************RED GREEN REFACTOR***********************************


    public class MockHttpContext : Mock<HttpContextBase>
    {
        private readonly IPrincipal _user = new GenericPrincipal(new GenericIdentity("2"), null /* roles */);

        public IPrincipal User
        {
            get { return _user; }
            set { User = value; }
        }
    }


    [TestFixture]
    public class SubmissionTest
    {

        [SetUp]
        public void Setup()
        {
            //Call required helper methods

            ////dbRepository = new Mock<TestEF_DBRepository>();
            //dbRepository = new TestEF_DBRepository();
            //returnController = new ReturnController();
            //contextMock      = GetControllerContext();
            //httpContextMock  = GetHttpContextMock();
            //currUser         = GetCurrentUser();
            //principal        = (IPrincipal)GetCurrentUser();
            //loggedInUser     = GetLoggedInUser();
            //loggedOutUser    = GetLoggedInUser();

        }

        #region Examples
        [Test]
        public void TestRegEx1()
        {
            string input = "0.0";
            string pattern = "^[0-9]([.][0-9]{1,1})?$"; //Pattern of "0.0" decimal format and place

            //Assert.IsTrue(Regex.IsMatch(input, pattern));
            Assert.That(Regex.IsMatch(input, pattern), "Error Message");
        }
        #endregion

        //PAGE LOAD TEST
        //View Load Test:
        //1.User clicks the start page

        [Test]
        [Description("GPG Return button should call the Index Action result method return the index View and redirects to: https://localhost:44371/Return")]
        public void GPGButtonCallsIndexActionLoadsIndexView()
        {
            // Arrange
            var controller = TestHelper.GetController<ReturnController>();

            //Act
            var result = (ViewResult)controller.Index();

            // Assert
            Assert.That(result.ViewName, Is.EqualTo("Index"), "Error Message");
        }

        //HTTPGET
        [Test]
        [Description("Start page should call the create action result")]
        public void StartButtonCallsCreateActionLoadsReturnView()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            //Act
            var result = (ViewResult)controller.Step1();

            // Assert
            Assert.That(result.ViewName, Is.EqualTo("Create"), "Error Message");

        }

        [Test]
        [Description("Create action result should load the return model view")]
        public void CreateActionLoadsReturnModelView()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);
            //Act
            var result = (ViewResult)controller.Step1();

            var model = result.Model as Return;

            // Assert
            Assert.IsNotNull(result.Model, "Error Message");
        }

        [Test]
        [Description("Start page should call the create action should call the create view")]
        public void CreateActionValidateReturnModelValues()
        {
            // Arrange
            // Mock user, Organisation, user organisation and Return
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return()
            {
                FirstName = null,
                JobTitle = null,
                LastName = null,
                AccountingDate = null,
                Created = DateTime.Now.Date,
                CurrentStatusDate = null,
                Modified = DateTime.Now.Date,
                CompanyLinkToGPGInfo = null,
                CurrentStatus = null,
                CurrentStatusDetails = null,
                Organisation = null,
                ReturnId = 1,
                OrganisationId = 1   //, ReturnStatuses
            };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation, @return);

            //Act
            var result = (ViewResult)controller.Step1();
            var returnModel = (Return)result.Model;

            // Assert
            Assert.That(returnModel.MaleLowerPayBand, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.MaleMedianBonusPayPercent, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.MaleMiddlePayBand, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.MaleUpperPayBand, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.MaleUpperQuartilePayBand, Is.EqualTo(0.0), "Error Message");

            Assert.That(returnModel.FemaleLowerPayBand, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.FemaleMedianBonusPayPercent, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.FemaleMiddlePayBand, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.FemaleUpperPayBand, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.FemaleUpperQuartilePayBand, Is.EqualTo(0.0), "Error Message");

            Assert.That(returnModel.DiffMeanBonusPercent, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.DiffMeanHourlyPayPercent, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.DiffMedianBonusPercent, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.DiffMedianHourlyPercent, Is.EqualTo(0.0), "Error Message");
            //Assert.That(returnModel.JobTitle, Is.EqualTo("Director Of Operations"), "Error Message");
            //Assert.That(returnModel.FirstName, Is.EqualTo("Kingsley"), "Error Message");
            //Assert.That(returnModel.LastName, Is.EqualTo("MagnusEweka"), "Error Message");
            Assert.That(returnModel.JobTitle, Is.EqualTo(@return.JobTitle), "Error Message");
            Assert.That(returnModel.FirstName, Is.EqualTo(@return.FirstName), "Error Message");
            Assert.That(returnModel.LastName, Is.EqualTo(@return.LastName), "Error Message");

            Assert.That(returnModel.AccountingDate, Is.EqualTo(@return.AccountingDate), "Error Message");
            Assert.That(((DateTime)(returnModel.Created)).Date, Is.EqualTo(@return.Created), "Error Message");
            Assert.That(returnModel.CurrentStatusDate, Is.EqualTo(@return.CurrentStatusDate), "Error Message");

            Assert.That(((DateTime)(returnModel.Modified)).Date, Is.EqualTo(@return.Modified), "Error Message");

            Assert.That(returnModel.CurrentStatus, Is.EqualTo(@return.CurrentStatus), "Error Message");
            Assert.That(returnModel.CurrentStatusDetails, Is.EqualTo(@return.CurrentStatusDetails), "Error Message");

            Assert.That(returnModel.Organisation, Is.EqualTo(@return.Organisation), "Error Message");

            Assert.That(returnModel.OrganisationId, Is.EqualTo(@return.OrganisationId), "Error Message");
            Assert.That(returnModel.ReturnId, Is.EqualTo(@return.ReturnId), "Error Message");
        }

        //HTTPPOST
        [Test]
        [Description("POST create action should call the create view with model")]
        public void StartButtonCallsCreateActionLoadsReturnView(Return model)
        {
            //TDD:
            // Arrange
            ReturnController controller = new ReturnController();

            // Act
            var result = (ViewResult)controller.Step1();

            // Assert
            Assert.That(result, Is.EqualTo("Create"), "Error Message");
        }

      



        #region When GPG Page Loads

        #region FeedBackLink for all Pages
        [Test]
        [Description("Verify the feedback link")]
        public void VerifyFeedBackLinkForAllPages()
        {


            // Arrange
            ReturnController controller = new ReturnController();

            // Act
            var result = (ViewResult)controller.Step1();

            // Assert
            Assert.That(result, Is.EqualTo("Create"), "Error Message");
        } 
        #endregion

        //Verify previous page that loaded this returns page
        [Test]
        [Description("Verify previous page that loaded this (returns) page")]
        public void VerifyPreviousPageThatLoadedThisReturnsPage()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation, @return);

            //Act
            var result = (ViewResult)controller.Step1();
            var prevReferrer = controller.Request.UrlReferrer.ToString();
            

            //Assert
            Assert.That(prevReferrer, Is.EqualTo("https://localhost:44371/"), "Error Message");
           
        }

        [Test]
        [Description("Verify current page that loaded this (returns) page")]
        public void VerifyCurrentPageThatLoadedThisReturnsPage()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation, @return);

            //Act
            var result = (ViewResult)controller.Step1();
            var currReferrer = controller.Request.Url.ToString();
            
            //Assert
            Assert.That(currReferrer, Is.EqualTo("https://localhost:44371/Return"), "Error Message");
        }



        //How to calculate you data page(verify link to that page)
        [Test]
        [Description
        ("verify loaded (HTTPGET) Empty fields test :-> validate that the fields are empty or have 0.0 except  *use regular expression")]
        public void VerifyGPGReturnFormFieldsAreEmptyWhenPageLoads()
        {

            {
                // Arrange
                // Mock user, Organisation, user organisation and Return
                var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
                var organisation = new Organisation() { OrganisationId = 0 };
                var userOrganisation = new UserOrganisation() { OrganisationId = 0, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
                var @return = new Return();
                var zeroVal = 0;
                
                var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

                //Act
                var result = (ViewResult)controller.Step1();
                var returnModel = (Return)result.Model;

                // Assert
                Assert.That(returnModel.MaleLowerPayBand, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.MaleMedianBonusPayPercent, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.MaleMiddlePayBand, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.MaleUpperPayBand, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.MaleUpperQuartilePayBand, Is.EqualTo(zeroVal), "Error Message");

                Assert.That(returnModel.FemaleLowerPayBand, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.FemaleMedianBonusPayPercent, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.FemaleMiddlePayBand, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.FemaleUpperPayBand, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.FemaleUpperQuartilePayBand, Is.EqualTo(zeroVal), "Error Message");

                Assert.That(returnModel.DiffMeanBonusPercent, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.DiffMeanHourlyPayPercent, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.DiffMedianBonusPercent, Is.EqualTo(zeroVal), "Error Message");
                Assert.That(returnModel.DiffMedianHourlyPercent, Is.EqualTo(zeroVal), "Error Message");
                
                Assert.That(returnModel.JobTitle, Is.EqualTo(null), "Error Message");
                Assert.That(returnModel.FirstName, Is.EqualTo(null), "Error Message");
                Assert.That(returnModel.LastName, Is.EqualTo(null), "Error Message");

                Assert.That(returnModel.AccountingDate, Is.EqualTo(@return.AccountingDate), "Error Message");

                Assert.That(((DateTime)(returnModel.Created)).Date, Is.EqualTo(((DateTime)(@return.Created)).Date), "Error Message");
                Assert.That(((DateTime)(returnModel.Modified)).Date, Is.EqualTo(((DateTime)(@return.Modified)).Date), "Error Message");

                Assert.That(returnModel.CurrentStatusDate, Is.EqualTo(null), "Error Message");

                Assert.That(returnModel.CurrentStatus, Is.EqualTo(null), "Error Message");
                Assert.That(returnModel.CurrentStatusDetails, Is.EqualTo(null), "Error Message");

                Assert.That(returnModel.Organisation, Is.EqualTo(null), "Error Message");

                Assert.That(returnModel.OrganisationId, Is.EqualTo(0), "Error Message");
                Assert.That(returnModel.ReturnId, Is.EqualTo(0), "Error Message");
            }
        }

        [Test]
        [Description("Verify all fields have one decimal place format")]
        public void ValidatedOneDecimalPlaceforAllNumericDataPointsFields()
        {
            // Arrange
            string pattern = "^[0-9]([.][0-9]{1,1})?$"; //Pattern of "0.0" decimal format and place should allow "000.0 as well"

            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            //Act
            var result = (ViewResult)controller.Step1();
            var returnModel = (Return)result.Model;

            //Assert
            Assert.That(Regex.IsMatch(returnModel.MaleLowerPayBand.ToString(),  pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.MaleMiddlePayBand.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.MaleUpperPayBand.ToString(),  pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.MaleMedianBonusPayPercent.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.MaleUpperQuartilePayBand.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.FemaleLowerPayBand.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.FemaleMedianBonusPayPercent.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.FemaleMiddlePayBand.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.FemaleUpperPayBand.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.FemaleUpperQuartilePayBand.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.DiffMeanBonusPercent.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.DiffMeanBonusPercent.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.DiffMeanHourlyPayPercent.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.DiffMedianBonusPercent.ToString(), pattern), "Error Message");
            Assert.That(Regex.IsMatch(returnModel.DiffMedianHourlyPercent.ToString(), pattern), "Error Message");

        }
        #endregion




        #region Submitting
        // test to validate all the data entered into the form:
        public void ValidteAllEnteredValuesIntoGPGForm()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation, @return);

            //Act
            var result = (ViewResult)controller.Step2(@return);

            //Assert

        }

        //verify when you click on the continue button->
        //Numeric validation test : -> validate one decimal place -> if a user enters a whole number it should automatically convert to 1 decimal place *use regular expression
        //verify all fields are numerical and have one decimal 0.0 place even if you put in a whole number* use regular expression
        //Range validation test :-> 200.00 to +200.00
        //loads up persone responsible page --> method that calls this page and view
        #endregion








        public void VerifyPreprendCompanyLinkToGPGInfo()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            //Act
            var result = (ViewResult)controller.Step1();
            var returnModel = (Return)result.Model;

            //Assert
            Assert.That(returnModel.CompanyLinkToGPGInfo, Is.EqualTo(""), "Error Message"); // if is empty
            Assert.That(returnModel.CompanyLinkToGPGInfo, Is.EqualTo(null), "Error Message"); // is null
            Assert.That(returnModel.CompanyLinkToGPGInfo, Is.EqualTo(@return.CompanyLinkToGPGInfo), "Error Message"); // and is prepended with https or the like
        }

    }
}
