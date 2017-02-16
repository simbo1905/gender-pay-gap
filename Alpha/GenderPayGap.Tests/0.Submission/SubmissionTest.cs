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
using GenderPayGap.WebUI.Models;

namespace GenderPayGap.Tests.Submission
{


    [TestFixture]
    public class SubmissionTest
    {

        [SetUp]
        public void Setup()
        {

        }


        //[Test]
        //[Description("")]
        //public void Step1_UserNotLoggedIn_RedirectToLoginPage()
        //{
        //    // Arrange
        //    var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
        //    var organisation = new Organisation() { OrganisationId = 1 };
        //    var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
        //    //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

        //    var controller = TestHelper.GetController<ReturnController>(1);

        //    //Act
        //    //var result = controller.xyz() as RedirectToRouteResult;
        //    var result = controller.Step1() as RedirectToRouteResult;

        //    //Assert
        //    Assert.Null(result, "Should have redirected");
        //   // Assert.Null(, "Should have redirected to index page");

        //}

        [Test]
        [Description("If a user has a return in the database load that return and verify its existence")]
        public void Step1_UserHasReturn_ShowExistingReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            //simulated return from mock db
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            //Add a return to the mock repo to simulate one in the database
            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation, @return);

            //Act:
            var result = (ViewResult)controller.Step1();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 1, "ReturnId expected '1' as a value");
            Assert.That(resultModel.OrganisationId == 1, "OrganisationId expected '1' as a value");

        }

        [Test]
        [Description("If a user does not have a return existing in the database, a new one should be created and verified with default values")]
        public void Step1_UserHasNoReturn_ShowNewReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            //Act:
            var result = (ViewResult)controller.Step1();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 0, "ReturnId expected 0");
            Assert.That(resultModel.OrganisationId == 1, "Organisation Id ");
        }


        #region CompanyLinkToGPGInfo

        [Test]
        [Description("Verify that a bad url link is not allowed or validated")]
        public void VerifyGPGInfoLink_BadURL_Link()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            var model = new ReturnViewModel()
            {
                CompanyLinkToGPGInfo = "http:www.//google.com" //put wrong deliberately!!!!
            };
           
            //Act
            var result = (ViewResult)controller.Step3(model);
            var returnModel = result.Model as Return;

            //Assert
            Assert.That(returnModel.CompanyLinkToGPGInfo == model.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfoLink should have the neccesary URL Prefix, 'http://' /'https:// and 'ftp://''");

        }

        [Test]
        [Description("Verify an existing GPGInfo Link is what is returned")]
        public void VerifyGPGInfoLink_WhatYouPutIn_IsWhatYouGetOut()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            var model = new ReturnViewModel()
            {
                CompanyLinkToGPGInfo = "http://www.test.com"
            };
           
            //Act
            var result = (ViewResult)controller.Step3(model);
            var returnModel = result.Model as Return;

            //Assert
            Assert.That(returnModel.CompanyLinkToGPGInfo == model.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfoLink that was input by the user is not what is returned");

        }
        #endregion


        [Test]
        [Description("Create action result should load the return model view")]
        public void VerifyStep1ActionReturnsAValidReturnModel()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);
            //Act
            var result = (ViewResult)controller.Step1();
            var model = result.Model as ReturnViewModel;

            // Assert
            Assert.IsNotNull(result.Model, "Error Message");
        }
       
        [Test]
        [Description("Create action result should load the return model view")]
        public void VerifyStep1ActionReturnsAnExistingReturn()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = 0 };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "https://www.test.com" };

            var controller = TestHelper.GetController<ReturnController>(1, user, organisation, userOrganisation);

            //Act
            //var result = (ViewResult)controller.Step1(@return);
            //var returnModel = result.Model as Return;

            // Assert
            // Assert.That(returnModel, Is.EqualTo(@return), "Error Message");
        }







    }
}
