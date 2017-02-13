
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


        public SubmissionTest()
        {
            //var c = TestHelper.BuildContainerIoC(null);
            //var controller = TestHelper.GetController<ReturnController>(0, null);
        }

        //Class Variables
        //Mock<TestEF_DBRepository> dbRepository = null;
        //TestEF_DBRepository dbRepository = null;
        //ReturnController returnController = null; // TestHelper.GetController<ReturnController>(0, null);
        //Mock<ControllerContext> contextMock = null;
        //MockHttpContext httpContextMock = null;
        //IPrincipal principal = null;
        //User currUser = null;
        //User loggedInUser = null;
        //User loggedOutUser = null;
        #region Helper Methods
        //[Description("Get the current user")]
        //private User GetCurrentUser()
        //{
        //    User result = null;
        //    MockHttpContext context = new MockHttpContext();
        //    var user = long.Parse(context.User.Identity.Name);

        //    //do look up on the mock repository
        //    //dbRepository
        //    User mockUser = dbRepository.GetUserByID(2);
        //    if (user == mockUser.UserId)
        //    {
        //        result = mockUser;
        //    }

        //    return result;
        //}

        //[Description("Get the mock controller context")]
        //private Mock<ControllerContext> GetControllerContext()
        //{
        //    var contextMock = new Mock<ControllerContext>();
        //    return contextMock;
        //}

        //[Description("Get the mock Http context")]
        //private MockHttpContext GetHttpContextMock()
        //{
        //    var httpContextMock = new MockHttpContext();
        //    return httpContextMock;
        //}

        //[Description("User not logged in")]
        //private User GetLoggedInUser()
        //{
        //    HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(String.Empty), new string[0]);
        //    return (User)HttpContext.Current.User;
        //}

        //[Description("User logged in")]
        //private User GetLoggedoutUser()
        //{
        //    HttpContext.Current.User = new GenericPrincipal(new GenericIdentity("2"), new string[0]);
        //    return (User)HttpContext.Current.User;
        //}
        #endregion


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
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            //Act
            var result = controller.Create();

            // Assert
            Assert.That(result, Is.EqualTo("Create"), "Error Message");

        }

        [Test]
        [Description("Create action result should load the return model view")]
        public void CreateActionLoadsReturnModelView()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);
            //Act
            var result = (ViewResult)controller.Create();

            // Assert
            Assert.That(result.Model.ToString(), Is.EqualTo("GpgDB.Models.GpgDatabase.Return"), "Error Message");
        }

        [Test]
        [Description("Start page should call the create action should call the create view")]
        public void CreateActionValidateReturnModelValue()
        {
            // Arrange
            // Mock user, Organisation, user organisation and Return
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return()
            {
                MaleLowerPayBand = 0.0M,
                MaleMedianBonusPayPercent = 0.0M,
                MaleMiddlePayBand = 0.0M,
                MaleUpperPayBand = 0.0M,
                MaleUpperQuartilePayBand = 0.0M,
                FemaleLowerPayBand = 0.0M,
                FemaleMedianBonusPayPercent = 0.0M,
                FemaleMiddlePayBand = 0.0M,
                FemaleUpperPayBand = 0.0M,
                FemaleUpperQuartilePayBand = 0.0M,
                DiffMeanBonusPercent = 0.0M,
                DiffMeanHourlyPayPercent = 0.0M,
                DiffMedianBonusPercent = 0.0M,
                DiffMedianHourlyPercent = 0.0M,
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
            var result = (ViewResult)controller.Create();
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
            var result = (ViewResult)controller.Create();

            // Assert
            Assert.That(result, Is.EqualTo("Create"), "Error Message");
        }

        #region Hidden Section
        //Model is Empty Test:
        //Verify model is empty and form fields are empty test
        //2.Verify the model does not load any data into the form fields
        public void ModelMustReturnNullOrEmpty()
        {

        }

        //User Logged in Test:
        //if user is not logged in redirect to login page test
        //3.page check a database for the user if user Iprinciple is not set or null, redirect to login page
        public void RedirectUserToLoginPageIfUserNotLogged()
        {

        }

        //User exception mock testing
        //4.if principle is set and principle not exist in the mock, throw an exception
        public void IfPrincipleIsSetAndIsNotInMock()
        {

        }

        //User unconfirmed:
        //5.if user exist in the database but have not been verified / confirmed(confirmed email)
        public void IfUserExistInDBAndNotBeenConfirmed()
        {

        }

        //User unassociated:
        //6.if user exist in the database but have not been been associated with an organisation
        public void IfUserExistInDBAndNotBeenAssociatedWithAnOrganisation()
        {

        }
        #endregion

        #region Example
        [Test]
        public void TestRegEx1()
        {
            string input = "0.0";
            string pattern = "^[0-9]([.][0-9]{1,1})?$"; //Pattern of "0.0" decimal format and place

            //Assert.IsTrue(Regex.IsMatch(input, pattern));
            Assert.That(Regex.IsMatch(input, pattern), "Error Message");
        }
        #endregion



        #region When GPG Page Loads
        //Verify user is logged in from above
        //Tests for the gpg return form fields page:

        //verify the feedback link
       
        [Test]
        [Description("Verify the feedback link")]
        public void VerifyFeedBackLinkForAllPages()
        {

            Assert.That(false, "Error Message");
        }

        #region Guidance Test
        //Verify the link for the gender paygap guidance:
        [Test]
        [Description("How to enter your figures ( verify link to that page)")]
        public void VerifyGenderPayGapGuidanceLink()
        {

            Assert.That(false, "Error Message");
        } 
       
        [Test]
        [Description("How to calculate you data page( verify link to that page")]
        public void VerifyHowToEnterFiguresLink()
        {

            Assert.That(false, "Error Message");
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
            var prevReferrer = controller.Request.UrlReferrer.ToString();
            var result = (ViewResult)controller.Create();


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
            var currReferrer = controller.Request.Url.ToString();
            var result = (ViewResult)controller.Create();

            //Assert
            Assert.That(currReferrer, Is.EqualTo("https://localhost:44371/Return"), "Error Message");
        }

        //How to calculate you data page(verify link to that page)
        [Test]
        [Description
        ("verify loaded (HTTPGET) Empty fields test :-> validate that the fields are empty or have 0.0 except  *use regular expression")]
        public void VerifyGPGReturnFormFieldsAreEmptyWhenPageLoads(FormCollection form)
        {

            Assert.That(false, "Error Message");
        }

        //Numeric validation test : -> validate one decimal place -> 
        //if a user enters a whole number it should automatically convert to 1 decimal place *use regular expression
        [Test]
        [Description("Verify all fields have one decimal place format")]
        public void ValidatedOneDecimalPlaceforAllNumericDataPointsFields()
        {
            // Arrange
            string pattern = "^[0-9]([.][0-9]{1,1})?$"; //Pattern of "0.0" decimal format and place

            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            //Act
            var result = (ViewResult)controller.Create();
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
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);
            //Act
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
            var result = (ViewResult)controller.Create();
            var returnModel = (Return)result.Model;

            //Assert
            Assert.That(returnModel.CompanyLinkToGPGInfo, Is.EqualTo(""), "Error Message"); // if is empty
            Assert.That(returnModel.CompanyLinkToGPGInfo, Is.EqualTo(null), "Error Message"); // is null
            Assert.That(returnModel.CompanyLinkToGPGInfo, Is.EqualTo(@return.CompanyLinkToGPGInfo), "Error Message"); // and is prepended with https or the like
        }

    }
}
