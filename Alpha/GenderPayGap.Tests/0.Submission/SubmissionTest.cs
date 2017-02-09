
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

namespace GenderPayGap.Tests.Submission
{

    //************************RED GREEN REFACTOR***********************************


    public class MockHttpContext : Mock<HttpContextBase>
    {
        private readonly IPrincipal _user = new GenericPrincipal(new GenericIdentity("2"), null /* roles */);

        public  IPrincipal User
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
            var c = TestHelper.BuildContainerIoC(null);
            var controller = TestHelper.GetController<ReturnController>(0, null);
        }

        //Class Variables
        //Mock<TestEF_DBRepository> dbRepository = null;
        TestEF_DBRepository dbRepository = null;
        ReturnController returnController = TestHelper.GetController<ReturnController>(0, null);
        Mock<ControllerContext> contextMock = null;
        MockHttpContext httpContextMock = null;
        IPrincipal principal = null;
        User currUser = null;
        User loggedInUser = null;
        User loggedOutUser = null;


       
        #region Helper Methods


        [Description("Get the current user")]
        private User GetCurrentUser()
        {
            User result = null;
            MockHttpContext context = new MockHttpContext();
            var user = long.Parse(context.User.Identity.Name);

            //do look up on the mock repository
            //dbRepository
            User mockUser = dbRepository.GetUserByID(2);
            if (user == mockUser.UserId)
            {
                result  = mockUser;
            }
            
            return result;
        }


        [Description("Get the mock controller context")]
        private Mock<ControllerContext> GetControllerContext()
        {
           var contextMock = new Mock<ControllerContext>();
           return contextMock;
        }


        [Description("Get the mock Http context")]
        private MockHttpContext GetHttpContextMock()
        {
            var httpContextMock = new MockHttpContext();
            return httpContextMock;
        }

        [Description("User not logged in")]
        private User GetLoggedInUser()
        {
            HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(String.Empty), new string[0]);
            return (User)HttpContext.Current.User;
        }

        [Description("User logged in")]
        private User GetLoggedoutUser()
        {
            HttpContext.Current.User = new GenericPrincipal( new GenericIdentity("2"), new string[0]);
            return (User)HttpContext.Current.User;
        }
        #endregion




        [SetUp]
        public void Setup()
        {
            //Call required helper methods

            //dbRepository = new Mock<TestEF_DBRepository>();
            dbRepository = new TestEF_DBRepository();
            returnController = new ReturnController();
            contextMock      = GetControllerContext();
            httpContextMock  = GetHttpContextMock();
            currUser         = GetCurrentUser();
            principal        = (IPrincipal)GetCurrentUser();
            loggedInUser     = GetLoggedInUser();
            loggedOutUser    = GetLoggedInUser();

        }

        //PAGE LOAD TEST
        //View Load Test:
        //1.User clicks the start page


        //HTTPGET
        [Test]
        [Description("Start page should call the create action should call the create view")]
        public void StartButtonCallsCreateActionLoadsReturnView()
        {
            // Arrange
            returnController = new ReturnController();

            // Act
            //Mock user Authorisation here
            httpContextMock.Setup(x => x.User).Returns( new GenericPrincipal(principal.Identity, null));

            contextMock.Setup(ctx => ctx.HttpContext).Returns(httpContextMock.Object);
            returnController.ControllerContext = contextMock.Object;

            //Act
            var result = (ViewResult)returnController.Create();

            // Assert
            Assert.That(result, Is.EqualTo("Create"), "Error Message");
            
        }


        
        [Test]
        [Description("Start page should call the create action should call the create view")]
        public void CreateActionValidateReturnModelValue()
        {
            // Arrange
            returnController = new ReturnController();

            // Act
            //Mock user Authorisation here
            httpContextMock.Setup(x => x.User).Returns(new GenericPrincipal(principal.Identity, null));

            contextMock.Setup(ctx => ctx.HttpContext).Returns(httpContextMock.Object);
            returnController.ControllerContext = contextMock.Object;

            //Act
            var result = (ViewResult)returnController.Create();
            var returnModel = (Return)result.Model;

            // Assert
            Assert.That(returnModel.MaleLowerPayBand, Is.EqualTo(0.0), "Error Message");
            Assert.That(returnModel.MaleMedianBonusPayPercent, Is.EqualTo("Create"), "Error Message");
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

            Assert.That(returnModel.JobTitle, Is.EqualTo("Director Of Operations"), "Error Message");
            Assert.That(returnModel.FirstName, Is.EqualTo("Kingsley"), "Error Message");
            Assert.That(returnModel.LastName, Is.EqualTo("MagnusEweka"), "Error Message");

            Assert.That(returnModel.AccountingDate, Is.EqualTo("Create"), "Error Message");
            Assert.That(returnModel.CompanyLinkToGPGInfo, Is.EqualTo("Create"), "Error Message");
            Assert.That(returnModel.Modified, Is.EqualTo(""), "Error Message");
            Assert.That(returnModel.Created, Is.EqualTo(""), "Error Message");
            Assert.That(returnModel.CurrentStatus, Is.EqualTo(""), "Error Message");
            Assert.That(returnModel.CurrentStatusDate, Is.EqualTo(""), "Error Message");
            Assert.That(returnModel.CurrentStatusDetails, Is.EqualTo("Create"), "Error Message");

            Assert.That(returnModel.Organisation, Is.EqualTo(""), "Error Message");

            Assert.That(returnModel.OrganisationId, Is.EqualTo(""), "Error Message");
            Assert.That(returnModel.ReturnId, Is.EqualTo(""), "Error Message");
        }




        //HTTPPOST
        [Test]
        [Description("POST create action should call the create view with model")]
        public void StartButtonCallsCreateActionLoadsReturnView( Return model)
        {
            //TDD:
            // Arrange
            ReturnController controller = new ReturnController();

            // Act
            var result = (ViewResult)controller.Create();

            // Assert
            Assert.That(result, Is.EqualTo("Create"), "Error Message");
        }


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

    }
}
