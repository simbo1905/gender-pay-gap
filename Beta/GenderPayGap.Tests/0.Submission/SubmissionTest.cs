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
//using GenderPayGap.Tests.DBRespository;
using GenderPayGap.Tests;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.Models.SqlDatabase;
using System.Text.RegularExpressions;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Classes;
using System.Configuration;

namespace GenderPayGap.Tests.Submission
{


    [TestFixture]
    public class SubmissionTest
    {

        [SetUp]
        public void Setup() {}

        //[Test]
        [Description("")]
        public void Step1_UserNotLoggedIn_RedirectToLoginPage()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now,  PINHash = "0" };
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<SubmitController>(1);

            //Act
            //var result = controller.xyz() as RedirectToRouteResult;
            var result = controller.Step1() as RedirectToRouteResult;

            //Assert
            Assert.Null(result, "Should have redirected");
            // Assert.Null(, "Should have redirected to index page");

        }

     // [Test]
        [Description("If a user has a return in the database load that return and verify its existence")]
        public void Step1_UserHasReturn_ShowExistingReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash ="0" };
            //simulated return from mock db
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "step1");
            routeData.Values.Add("controller", "submit");

            //Add a return to the mock repo to simulate one in the database
            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);
           // controller.bind();

            //Act:
            var result = (ViewResult)controller.Step1();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 1, "ReturnId expected '1' as a value");
            Assert.That(resultModel.OrganisationId == 1, "OrganisationId expected '1' as a value");

        }

       // [Test]
        [Description("If a user does not have a return existing in the database, a new one should be created and verified with default values")]
        public void Step1_UserHasNoReturn_ShowNewPrivateSectorReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "step1");
            routeData.Values.Add("controller", "submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            var PrivateAccountingDate = new DateTime(2017, 4, 5);
            // controller.Bind(model);


            //Act:
            var result = (ViewResult)controller.Step1();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 0, "ReturnId expected 0");
            Assert.That(resultModel.OrganisationId == 1, "Organisation Id ");
            Assert.That(resultModel.AccountingDate == PrivateAccountingDate, "Private sector Return start date expected");
        }

       // [Test]
        [Description("If a user does not have a return existing in the database, a new one should be created and verified with default values")]
        public void Step1_UserHasNoReturn_ShowNewPublicSectorReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step1");
            routeData.Values.Add("Controller", "Submit");

            var PublicAccountingDate = new DateTime(2017, 3, 31);

            //Stash an object to pass in unStashModel()
            var model = new ReturnViewModel()
                            {
                                OrganisationId = organisation.OrganisationId, //0
                                AccountingDate = PublicAccountingDate
                            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);

            controller.StashModel(model);

            //Act:
            var result = controller.Step1() as ViewResult;
            // controller.Bind(model);
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 0, "ReturnId expected 0");
            Assert.That(resultModel.OrganisationId == 1, "Organisation Id ");
            Assert.That(resultModel.AccountingDate == PublicAccountingDate, "Public sector Return start date expected ");
        }

        [Test]
        [Description("Step1 should fail when any field is empty")]
        public void Step1_EmptyFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step1");
            routeData.Values.Add("Controller", "Submit");

            //empty model without values
            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step1", "Incorrect view returned");

                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"), false, "Expected DiffMeanBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"), false, "Expected DiffMeanHourlyPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"), false, "Expected DiffMedianBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"), false, "Expected DiffMedianHourlyPercent failure");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleLowerPayBand"), false, "Expected FemaleLowerPayBand failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), false, "Expected FemaleMedianBonusPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"), false, "Expected FemaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperPayBand"), false, "Expected FemaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"), false, "Expected FemaleUpperQuartilePayBand  failure");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleLowerPayBand"), false, "Expected MaleLowerPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"), false, "Expected MaleMedianBonusPayPercent  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMiddlePayBand"), false, "Expected MaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperPayBand"), false, "Expected MaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"), false, "Expected MaleUpperQuartilePayBand  failure");

            });
        }

     //   [Test]
        [Description("Ensure that Step1 passes when all zero values are entered in all/any of the fields as zero is a valid value")]
        public void Step1_ZeroValidValueInFields_NoError()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "step1");
            routeData.Values.Add("controller", "submit");

            decimal zero = 0;

            var model = new ReturnViewModel()
            {
                DiffMeanBonusPercent        = zero,
                DiffMeanHourlyPayPercent    = zero,
                DiffMedianBonusPercent      = zero,
                DiffMedianHourlyPercent     = zero,
                FemaleLowerPayBand          = zero,
                FemaleMedianBonusPayPercent = zero,
                FemaleMiddlePayBand         = zero,
                FemaleUpperPayBand          = zero,
                FemaleUpperQuartilePayBand  = zero,
                MaleLowerPayBand            = zero,
                MaleMedianBonusPayPercent   = zero,
                MaleMiddlePayBand           = zero,
                MaleUpperPayBand            = zero,
                MaleUpperQuartilePayBand    = zero
            };


            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step1", "Incorrect view returned");

                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),        true, "Expected DiffMeanBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"),    true, "Expected DiffMeanHourlyPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),      true, "Expected DiffMedianBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),     true, "Expected DiffMedianHourlyPercent failure");
                                                                                                        
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleLowerPayBand"),          true, "Expected FemaleLowerPayBand failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), true, "Expected FemaleMedianBonusPayPercent failure");                                                                         
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),         true, "Expected FemaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperPayBand"),          true, "Expected FemaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),  true, "Expected FemaleUpperQuartilePayBand  failure");                                                                            
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleLowerPayBand"),            true, "Expected MaleLowerPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),   true, "Expected MaleMedianBonusPayPercent  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMiddlePayBand"),           true, "Expected MaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperPayBand"),            true, "Expected MaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),    true, "Expected MaleUpperQuartilePayBand  failure");
                
            });
        }

      //  [Test]
        [Description("Step1 should succeed when all fields have valid values")]
        public void Step1_ValidValueInFields_NoError()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            decimal validValue = 100;

            var model = new ReturnViewModel()
            {
                DiffMeanBonusPercent        = validValue,
                DiffMeanHourlyPayPercent    = validValue,
                DiffMedianBonusPercent      = validValue,
                DiffMedianHourlyPercent     = validValue,
                FemaleLowerPayBand          = validValue,
                FemaleMedianBonusPayPercent = validValue,
                FemaleMiddlePayBand         = validValue,
                FemaleUpperPayBand          = validValue,
                FemaleUpperQuartilePayBand  = validValue,
                MaleLowerPayBand            = validValue,
                MaleMedianBonusPayPercent   = validValue,
                MaleMiddlePayBand           = validValue,
                MaleUpperPayBand            = validValue,
                MaleUpperQuartilePayBand    = validValue
            };


            var controller = TestHelper.GetController<SubmitController>();
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;
            
            // Assert
            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step1", "Incorrect view returned");

                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),        true, "Expected DiffMeanBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"),    true, "Expected DiffMeanHourlyPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),      true, "Expected DiffMedianBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),     true, "Expected DiffMedianHourlyPercent failure");
                                                                                                       
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleLowerPayBand"),          true, "Expected FemaleLowerPayBand failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), true, "Expected FemaleMedianBonusPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),         true, "Expected FemaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperPayBand"),          true, "Expected FemaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),  true, "Expected FemaleUpperQuartilePayBand  failure");
                                                                                                        
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleLowerPayBand"),            true, "Expected MaleLowerPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),   true, "Expected MaleMedianBonusPayPercent  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMiddlePayBand"),           true, "Expected MaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperPayBand"),            true, "Expected MaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),    true, "Expected MaleUpperQuartilePayBand  failure");
                
            });
        }

        [Test]
        [Description("Step1 should fail when any field is outside of the minimum allowed range of valid values")]
        public void Step1_MinInValidValues_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "step1");
            routeData.Values.Add("controller", "submit");

            decimal minOutOfRangeValue = -201M;

            var model = new ReturnViewModel()
            {
                DiffMeanBonusPercent        = minOutOfRangeValue,
                DiffMeanHourlyPayPercent    = minOutOfRangeValue,
                DiffMedianBonusPercent      = minOutOfRangeValue,
                DiffMedianHourlyPercent     = minOutOfRangeValue,
                FemaleLowerPayBand          = minOutOfRangeValue,
                FemaleMedianBonusPayPercent = minOutOfRangeValue,
                FemaleMiddlePayBand         = minOutOfRangeValue,
                FemaleUpperPayBand          = minOutOfRangeValue,
                FemaleUpperQuartilePayBand  = minOutOfRangeValue,
                MaleLowerPayBand            = minOutOfRangeValue,
                MaleMedianBonusPayPercent   = minOutOfRangeValue,
                MaleMiddlePayBand           = minOutOfRangeValue,
                MaleUpperPayBand            = minOutOfRangeValue,
                MaleUpperQuartilePayBand    = minOutOfRangeValue
            };


            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step1", "Incorrect view returned");

                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),        false, "Expected DiffMeanBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"),    false, "Expected DiffMeanHourlyPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),      false, "Expected DiffMedianBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),     false, "Expected DiffMedianHourlyPercent failure");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleLowerPayBand"),          false, "Expected FemaleLowerPayBand failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), false, "Expected FemaleMedianBonusPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),         false, "Expected FemaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperPayBand"),          false, "Expected FemaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),  false, "Expected FemaleUpperQuartilePayBand  failure");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleLowerPayBand"),            false, "Expected MaleLowerPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),   false, "Expected MaleMedianBonusPayPercent  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMiddlePayBand"),           false, "Expected MaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperPayBand"),            false, "Expected MaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),    false, "Expected MaleUpperQuartilePayBand  failure");

            });
        }

        [Test]
        [Description("Step1 should fail when any field is outside of the maximum allowed range of valid values")]
        public void Step1_MaxInValidValues_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "step1");
            routeData.Values.Add("controller", "register");

            decimal maxOutOfRangeValue = 201M;

            var model = new ReturnViewModel()
            {
                DiffMeanBonusPercent        = maxOutOfRangeValue,
                DiffMeanHourlyPayPercent    = maxOutOfRangeValue,
                DiffMedianBonusPercent      = maxOutOfRangeValue,
                DiffMedianHourlyPercent     = maxOutOfRangeValue,
                FemaleLowerPayBand          = maxOutOfRangeValue,
                FemaleMedianBonusPayPercent = maxOutOfRangeValue,
                FemaleMiddlePayBand         = maxOutOfRangeValue,
                FemaleUpperPayBand          = maxOutOfRangeValue,
                FemaleUpperQuartilePayBand  = maxOutOfRangeValue,
                MaleLowerPayBand            = maxOutOfRangeValue,
                MaleMedianBonusPayPercent   = maxOutOfRangeValue,
                MaleMiddlePayBand           = maxOutOfRangeValue,
                MaleUpperPayBand            = maxOutOfRangeValue,
                MaleUpperQuartilePayBand    = maxOutOfRangeValue
            };


            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            // Act
            var result = controller.Step1(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step1", "Incorrect view returned");

                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),        false, "Expected DiffMeanBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"),    false, "Expected DiffMeanHourlyPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),      false, "Expected DiffMedianBonusPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),     false, "Expected DiffMedianHourlyPercent failure");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleLowerPayBand"),          false, "Expected FemaleLowerPayBand failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), false, "Expected FemaleMedianBonusPayPercent failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),         false, "Expected FemaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperPayBand"),          false, "Expected FemaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),  false, "Expected FemaleUpperQuartilePayBand  failure");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleLowerPayBand"),            false, "Expected MaleLowerPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),   false, "Expected MaleMedianBonusPayPercent  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMiddlePayBand"),           false, "Expected MaleMiddlePayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperPayBand"),            false, "Expected MaleUpperPayBand  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),    false, "Expected MaleUpperQuartilePayBand  failure");

            });
        }


        [Test]
        [Description("Create action result should load the return model view")]
        public void Step1_VerifyActionReturns_ValidReturnModel()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step1");
            routeData.Values.Add("Controller", "Submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);

            var model = new ReturnViewModel();
            controller.StashModel(model);

            //Act
            var result = (ViewResult)controller.Step1();

            // Assert
            Assert.IsNotNull(result.Model, "Error Message");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Error Message");
        }

        [Test]
        [Description("Create action result should load the return model view")]
        public void Step1_VerifyActionReturns_AnExistingReturn()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            var @return = new Return() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "https://www.test.com" };

            var routeData = new RouteData();
            routeData.Values.Add("action", "Step1");
            routeData.Values.Add("controller", "Register");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);

            //Act
            //var result = (ViewResult)controller.Step1(@return);
            //var returnModel = result.Model as Return;

            //Assert
            //Assert.That(returnModel, Is.EqualTo(@return), "Error Message");
        }







        //Happy Path for Sumission Journey
        [Test]
        [Description("Step1 should fail when any field is empty")]
        public void Step1_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now /*, EmailVerifyHash = code.GetSHA512Checksum()*/ };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step1");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            //Stash an object to pass in for this.ClearStash()
            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            controller.StashModel(model);

            //ACT:
            var result = controller.Step1(returnurl) as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step1", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected RegisterViewModel");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }


        [Test]
        [Description("Step1 should fail when any field is empty")]
        public void Step1_Post_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step1");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = "";

            var PrivateAccountingDate = new DateTime(2017, 4, 4);
            //var x = ConfigurationManager.AppSettings["GpgApiScope"];
            // var x = ConfigurationManager.AppSettings["PrivateAccountingDate"];
          //does not work why cant i get to Application Settings??? but can get to AppSettings
            //var x = ConfigurationManager.ApplicationSettings["PrivateAccountingDate"];

            var model = new ReturnViewModel()
            {
                AccountingDate = PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 0,
                DiffMeanHourlyPayPercent = 0,
                DiffMedianBonusPercent = 0,
                DiffMedianHourlyPercent = 0,
                FemaleLowerPayBand = 0,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 0,
                FemaleUpperPayBand = 0,
                FemaleUpperQuartilePayBand = 0,
                FirstName = null,
                LastName = null,
                JobTitle = null,
                MaleLowerPayBand = 0,
                MaleMedianBonusPayPercent = 0,
                MaleMiddlePayBand = 0,
                MaleUpperPayBand = 0,
                MaleUpperQuartilePayBand = 0,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 0,
            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step1(model, returnurl) as RedirectToRouteResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step2", "Expected a RedirectToRouteResult to Step2");

            // See if there are anymore asserts that can be done for a redirect here.

            // Assert.That(result.ViewName == "Step4" || result.ViewName == "Step2", "Incorrect view returned");
            // Assert.That(result.ViewName == returnurl, "Expected ViewResult");

            //Assert.Multiple(() =>
            //{
            //    Assert.NotNull(result, "Expected ViewResult");
            //    Assert.That(result.ViewName == "Step1", "Incorrect view returned");

            //    Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"), false, "Expected DiffMeanBonusPercent failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"), false, "Expected DiffMeanHourlyPayPercent failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"), false, "Expected DiffMedianBonusPercent failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"), false, "Expected DiffMedianHourlyPercent failure");

            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleLowerPayBand"), false, "Expected FemaleLowerPayBand failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), false, "Expected FemaleMedianBonusPayPercent failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"), false, "Expected FemaleMiddlePayBand  failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperPayBand"), false, "Expected FemaleUpperPayBand  failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"), false, "Expected FemaleUpperQuartilePayBand  failure");

            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleLowerPayBand"), false, "Expected MaleLowerPayBand  failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"), false, "Expected MaleMedianBonusPayPercent  failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMiddlePayBand"), false, "Expected MaleMiddlePayBand  failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperPayBand"), false, "Expected MaleUpperPayBand  failure");
            //    Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"), false, "Expected MaleUpperQuartilePayBand  failure");

            //});
        }



        #region Person Responsible

        [Test]
        [Description("Step1 should fail when any field is empty")]
        public void Step2_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step2");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            //Stash an object to pass in for this.ClearStash()
            controller.StashModel(model);

            //ACT:
            var result = controller.Step2(returnurl) as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step2", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }


        [Test]
        [Description("Step1 should fail when any field is empty")]
        public void Step2_Post_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step2");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = "";

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            var model = new ReturnViewModel()
            {
                AccountingDate = PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 0,
                DiffMeanHourlyPayPercent = 0,
                DiffMedianBonusPercent = 0,
                DiffMedianHourlyPercent = 0,
                FemaleLowerPayBand = 0,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 0,
                FemaleUpperPayBand = 0,
                FemaleUpperQuartilePayBand = 0,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 0,
                MaleMedianBonusPayPercent = 0,
                MaleMiddlePayBand = 0,
                MaleUpperPayBand = 0,
                MaleUpperQuartilePayBand = 0,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 0,
            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step2(model, returnurl) as RedirectToRouteResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step3", "Expected a RedirectToRouteResult to Step3");

            // See if there are anymore asserts that can be done for a redirect here.
           
        }



       // [Test]
        [Description("Ensure the Step2 fails when any field is empty")]
        public void Step2_EmptyFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step2");
            routeData.Values.Add("Controller", "Submit");

            string emptyString = string.Empty;

            var model = new ReturnViewModel()
            {
                JobTitle = emptyString,
                FirstName = emptyString,
                LastName = emptyString
            };

            var command = "";

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.bind();

            //Act:
            var result = controller.Step2(model, command) as ViewResult; ;

            //Assert
            Assert.Multiple(() =>
           {
               Assert.NotNull(result, "Expected ViewResult");
               Assert.That(result.ViewName == "Step2", "Incorrect view returned");
               Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

               Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), false, "Expected JobTitle value other than empty strings failure");
               Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), false, "Expected FirstName value other than empty strings  failure");
               Assert.AreEqual(result.ViewData.ModelState.IsValidField("LasttName"), false, "Expected LasttName value other than empty strings  failure");
           });

        }

     //   [Test]
        [Description("Ensure the Step2 fails when any field is null")]
        public void Step2_NullFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("action", "step2");
            routeData.Values.Add("controller", "submit");



            string emptyString = null;

            var model = new ReturnViewModel()
            {
                JobTitle = emptyString,
                FirstName = emptyString,
                LastName = emptyString
            };

            var command = "";

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.bind();

            //Act:
            var result = controller.Step2(model, command) as ViewResult; ;

            //Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step2", "Incorrect view returned");
                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), false, "Expected JobTitle  value other than null failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), false, "Expected FirstName value other than null  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("LasttName"), false, "Expected LasttName value other than null  failure");
            });

        }

     //   [Test]
        [Description("Ensure the Step2 succeeds when all fields are filled in with valid values")]
        public void Step2_ValidFields_NoErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };


            var routeData = new RouteData();
            routeData.Values.Add("action", "step1");
            routeData.Values.Add("controller", "register");

            var model = new ReturnViewModel()
            {
                JobTitle  = "Director",
                FirstName = "MyFirstName",
                LastName  = "MyLastName"
            };

            var command = "";

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.bind();

            //Act:
            var result = controller.Step2(model, command) as ViewResult; ;

            //Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "Step2", "Incorrect view returned");
                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), true, "Expected JobTitle value other than empty strings success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), true, "Expected FirstName value other than empty strings  success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("LasttName"), true, "Expected LasttName value other than empty strings  success");
            });

        }
        #endregion


        #region CompanyLinkToGPGInfo
        [Test]
        [Description("Step3 should succeed when view is requested")]
        public void Step3_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step3");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            //Stash an object to pass in for this.ClearStash()
            controller.StashModel(model);

            //ACT:
            var result = controller.Step3(returnurl) as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step3", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Step3 should succeed its field is empty or null on View Post")]
        public void Step3_Post_Without_CompanyLinkToGPGInfoValue_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step3");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            var model = new ReturnViewModel()
            {
                AccountingDate = PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 0,
                DiffMeanHourlyPayPercent = 0,
                DiffMedianBonusPercent = 0,
                DiffMedianHourlyPercent = 0,
                FemaleLowerPayBand = 0,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 0,
                FemaleUpperPayBand = 0,
                FemaleUpperQuartilePayBand = 0,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 0,
                MaleMedianBonusPayPercent = 0,
                MaleMiddlePayBand = 0,
                MaleUpperPayBand = 0,
                MaleUpperQuartilePayBand = 0,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 0,
            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step3(model) as RedirectToRouteResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step4", "Expected a RedirectToRouteResult to Step3");

            // See if there are anymore asserts that can be done for a redirect here.

        }

        [Test]
        [Description("Step3 should succeed its field has value on View Post, no need to check validity of the value here, (due to client-side validation)")]
        public void Step3_Post_With_CompanyLinkToGPGInfoValue_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step3");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            var model = new ReturnViewModel()
            {
                AccountingDate = PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 0,
                DiffMeanHourlyPayPercent = 0,
                DiffMedianBonusPercent = 0,
                DiffMedianHourlyPercent = 0,
                FemaleLowerPayBand = 0,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 0,
                FemaleUpperPayBand = 0,
                FemaleUpperQuartilePayBand = 0,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 0,
                MaleMedianBonusPayPercent = 0,
                MaleMiddlePayBand = 0,
                MaleUpperPayBand = 0,
                MaleUpperQuartilePayBand = 0,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 0,
            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step3(model) as RedirectToRouteResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result as RedirectToRouteResult, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "Step4", "Expected a RedirectToRouteResult to Step3");

            // See if there are anymore asserts that can be done for a redirect here.

        }




        [Test]
        [Description("Verify that a good url link with the proper web protocol prefix is validated and allowed")]
        public void Step3_VerifyGPGInfoLink_GoodURL_Link()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
           // var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step3");
            routeData.Values.Add("Controller", "Submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);


            var model = new ReturnViewModel()
            {
                CompanyLinkToGPGInfo = "http://www.google.com" 
            };
           
            //Act
            var result = controller.Step3(model) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //Assert
            Assert.That(resultModel.CompanyLinkToGPGInfo.StartsWith("http://")  ||
                        resultModel.CompanyLinkToGPGInfo.StartsWith("https://") || 
                        resultModel.CompanyLinkToGPGInfo.StartsWith("ftp://"), 
                        "Expected CompanyLinkToGPGInfoLink should have one of the neccesary URL Prefix:'http://', 'https://' or 'ftp://' ");
        }

        //I dont think this test is neccesary as the above does the same thing this just does the same but in opposite
        [Test]
        [Description("Verify that a bad url link with the improper web protocol prefix is not validated or allowed")]
        public void Step3_VerifyGPGInfoLink_BadURL_Link()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
           // var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step3");
            routeData.Values.Add("Controller", "Submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);


            var model = new ReturnViewModel()
            {
                CompanyLinkToGPGInfo =  "http:www.//google.com" 
            };

            //Act
            var result = controller.Step3(model) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //Assert
            Assert.That((!resultModel.CompanyLinkToGPGInfo.StartsWith("http://")) ||
                        (!resultModel.CompanyLinkToGPGInfo.StartsWith("https://")) ||
                        (!resultModel.CompanyLinkToGPGInfo.StartsWith("ftp://")),
                        "Expected CompanyLinkToGPGInfoLink should have one of the neccesary URL Prefix:'http://', 'https://' or 'ftp://' ");
        }

        [Test]
        [Description("Verify an existing GPGInfo Link is what is returned")]
        public void Step3_VerifyGPGInfoLink_WhatYouPutIn_IsWhatYouGetOut()
        {
            //ARRANGE:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //mock return with CompanyLinkToGPGInfo in the DB
            var @return = new Return() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "http://www.test.com" };

            var routeData = new RouteData();
            routeData.Values.Add("action", "step3");
            routeData.Values.Add("controller", "submit");

            //mock entered return CompanyLinkToGPGInfo in the CompanyLinkToGPGInfo step3 view
            var model = new ReturnViewModel()
                            {
                                CompanyLinkToGPGInfo = "http://www.test.com"
                            };

            //added into the mock DB via mockRepository
            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);

            //ACT:
            var result = controller.Step3(model) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //ASSERT:
            Assert.That(resultModel.CompanyLinkToGPGInfo == model.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfoLink that was input by the user is not what is returned");
        }

        #endregion


        #region Review
        [Test]
        [Description("Step4 should fail when any field is empty")]
        public void Step4_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step4");
            routeData.Values.Add("Controller", "Submit");

            //Stash an object to pass in for this.ClearStash()
            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            controller.StashModel(model);

            //ACT:
            var result = controller.Step4() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step4", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("Step4 should fail when any field is empty")]
        public void Step4_Post_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //mock return existing in the DB
            var @return = new ReturnViewModel() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "http://www.test.com" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Step4");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            //mock entered return at review step4 view
            var model = new ReturnViewModel()
            {
                AccountingDate = PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 30,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 30,
                MaleMedianBonusPayPercent = 40,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 70,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 1
            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.Step4(model) as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            var resultDB = (controller.DataRepository.GetAll<Return>().FirstOrDefault( r => r.CompanyLinkToGPGInfo == "http://www.gov.uk"));

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "Step5", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            // get the data from te mock database and assert it is there
            Assert.That(model.CompanyLinkToGPGInfo == resultModel.CompanyLinkToGPGInfo, "expected entered companyLinkToGPGInfo is what is saved in db");
        }
        #endregion












    }
}
