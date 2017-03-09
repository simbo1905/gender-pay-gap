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
using GenderPayGap.WebUI.Models.Submit;

namespace GenderPayGap.Tests.Submission
{


    [TestFixture]
    public class SubmissionTest
    {

        [SetUp]
        public void Setup() {}

        //[Test]
        [Description("")]
        public void EnterCalculations_UserNotLoggedIn_RedirectToLoginPage()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now,  PINHash = "0" };
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<SubmitController>(1);

            //Act
            //var result = controller.xyz() as RedirectToRouteResult;
            var result = controller.EnterCalculations() as RedirectToRouteResult;

            //Assert
            Assert.Null(result, "Should have redirected");
            // Assert.Null(, "Should have redirected to index page");

        }

     // [Test]
        [Description("If a user has a return in the database load that return and verify its existence")]
        public void EnterCalculations_UserHasReturn_ShowExistingReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash ="0" };
            //simulated return from mock db
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            //Add a return to the mock repo to simulate one in the database
            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);
           // controller.bind();

            //Act:
            var result = (ViewResult)controller.EnterCalculations();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 1, "ReturnId expected '1' as a value");
            Assert.That(resultModel.OrganisationId == 1, "OrganisationId expected '1' as a value");

        }

       // [Test]
        [Description("If a user does not have a return existing in the database, a new one should be created and verified with default values")]
        public void EnterCalculations_UserHasNoReturn_ShowNewPrivateSectorReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            var PrivateAccountingDate = new DateTime(2017, 4, 5);
            // controller.Bind(model);


            //Act:
            var result = (ViewResult)controller.EnterCalculations();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 0, "ReturnId expected 0");
            Assert.That(resultModel.OrganisationId == 1, "Organisation Id ");
            Assert.That(resultModel.AccountingDate == PrivateAccountingDate, "Private sector Return start date expected");
        }

       // [Test]
        [Description("If a user does not have a return existing in the database, a new one should be created and verified with default values")]
        public void EnterCalculations_UserHasNoReturn_ShowNewPublicSectorReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
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
            var result = controller.EnterCalculations() as ViewResult;
            // controller.Bind(model);
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 0, "ReturnId expected 0");
            Assert.That(resultModel.OrganisationId == 1, "Organisation Id ");
            Assert.That(resultModel.AccountingDate == PublicAccountingDate, "Public sector Return start date expected ");
        }

        [Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public void EnterCalculations_EmptyFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            //empty model without values
            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            // Act
            var result = controller.EnterCalculations(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");

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
        [Description("Ensure that EnterCalculations passes when all zero values are entered in all/any of the fields as zero is a valid value")]
        public void EnterCalculations_ZeroValidValueInFields_NoError()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
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
            var result = controller.EnterCalculations(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");

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
        [Description("EnterCalculations should succeed when all fields have valid values")]
        public void EnterCalculations_ValidValueInFields_NoError()
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
            var result = controller.EnterCalculations(model) as ViewResult;
            
            // Assert
            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");

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
        [Description("EnterCalculations should fail when any field is outside of the minimum allowed range of valid values")]
        public void EnterCalculations_MinInValidValues_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            decimal minOutOfRangeValue = -201;

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
            var result = controller.EnterCalculations(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");

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


                //TODO instead of checking ModelState.Isvalid we should now be checking for exact error message on each field is than this in ErrorConfig - this can be done later but we must start doing this from now on
            });
        }

        [Test]
        [Description("EnterCalculations should fail when any field is outside of the maximum allowed range of valid values")]
        public void EnterCalculations_MaxInValidValues_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
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
            var result = controller.EnterCalculations(model) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");

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

            //TODO again we need to check for exact error messages from config
        }

        //TODO Test needed for fields are now using regex to ensure only 1 decimal place

        [Test]
        [Description("Create action result should load the return model view")]
        public void EnterCalculations_VerifyActionReturns_ValidReturnModel()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);

            var model = new ReturnViewModel();
            controller.StashModel(model);

            //Act
            var result = (ViewResult)controller.EnterCalculations();

            // Assert
            Assert.IsNotNull(result.Model, "Error Message");
            Assert.That(result.Model is ReturnViewModel, "Error Message");

            //TODO you should be checking here that returned model values match those expected
        }

        [Test]
        [Description("Create action result should load the return model view")]
        public void EnterCalculations_VerifyActionReturns_AnExistingReturn()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            var @return = new Return() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "https://www.test.com" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Register");

            string returnurl = null;

            //Stash an object to unStash()
            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);

            controller.StashModel(model);

            //ACT:
            var result = controller.EnterCalculations(returnurl) as ViewResult;
            var returnModel = result.Model as ReturnViewModel;

            //Assert
            Assert.NotNull(result, "Expected ViewResult");
            //TODO you arent checking the returned model at all
            //TODO again you should be checking the returned model has correct values and is for the correct user and org and userorg
        }

        //Happy Path for Sumission Journey
        [Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public void EnterCalculations_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now /*, EmailVerifyHash = code.GetSHA512Checksum()*/ };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            //Stash an object to unStash()
            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            controller.StashModel(model);

            //ACT:
            var result = controller.EnterCalculations(returnurl) as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result is ViewResult, "Incorrect resultType returned"); //TODO this is redundant as previous line doe this same check
            Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected RegisterViewModel");
            Assert.That(result.Model is ReturnViewModel, "Incorrect resultType returned"); //TODO again this is redundant due to previous line
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            //TODO you should be checking the returned model is empty
        }


        [Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public void EnterCalculations_Post_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
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

            //TODO line above is wrong as you should be setting the fields to null not zero

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.EnterCalculations(model, returnurl) as RedirectToRouteResult;

            //TODO this test is completely wrong you should be cheking the all the fields are invalid in the modelstate

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            //TODO This line is wrong as we should be returning as View since the modelstate was invalid

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "PersonResponsible", "Expected a RedirectToRouteResult to PersonResponsible");
            //TODO This line is wrong as we should be returning the same view since model state was invalid
            //TODO Also note public sector orgs skip person responsible step and instead go to companylink step but then only on succcess

            // See if there are anymore asserts that can be done for a redirect here.

            // Assert.That(result.ViewName == "CheckData" || result.ViewName == "PersonResponsible", "Incorrect view returned");
            // Assert.That(result.ViewName == returnurl, "Expected ViewResult");

            //Assert.Multiple(() =>
            //{
            //    Assert.NotNull(result, "Expected ViewResult");
            //    Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");

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
        [Description("EnterCalculations should fail when any field is empty")]
        public void PersonResponsible_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            //Stash an object to pass in for this.ClearStash()
            controller.StashModel(model);

            //ACT:
            var result = controller.PersonResponsible(returnurl) as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result is ViewResult, "Incorrect resultType returned");//TODO redundant due to previous line
            Assert.That(result.ViewName == "PersonResponsible", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.Model  is ReturnViewModel, "Incorrect resultType returned"); //TODO again redundant due to previous step
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");//TODO wrong should be checking its invalid

            //TODO should be checking each field for exact error message in modelstate
        }


        [Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public void PersonResponsible_Post_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
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

            //TODO again above line is wrong as you should be setting nulllable field values to null

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.PersonResponsible(model, returnurl) as RedirectToRouteResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "EmployerWebsite", "Expected a RedirectToRouteResult to EmployerWebsite");

            // See if there are anymore asserts that can be done for a redirect here.
           //TODO you are not checking here for model state is invalid
           //TODO you should be checking only the exact failed fields show and error message
           //TODO you should be checking each error message is exact as per confilg file
        }

       // [Test]
        [Description("Ensure the PersonResponsible fails when any field is empty")]
        public void PersonResponsible_EmptyFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
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
            var result = controller.PersonResponsible(model, command) as ViewResult; ;

            //Assert
            Assert.Multiple(() =>
           {
               Assert.NotNull(result, "Expected ViewResult");
               Assert.That(result.ViewName == "PersonResponsible", "Incorrect view returned");
               Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

               Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), false, "Expected JobTitle value other than empty strings failure");
               Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), false, "Expected FirstName value other than empty strings  failure");
               Assert.AreEqual(result.ViewData.ModelState.IsValidField("LasttName"), false, "Expected LasttName value other than empty strings  failure");

               //TODO you should also be checking modelstate has no other errors to ensure other fields are being retained after postback and not causing errors
           });

        }

     //   [Test]
        [Description("Ensure the PersonResponsible fails when any field is null")]
        public void PersonResponsible_NullFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("action", "PersonResponsible");
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
            var result = controller.PersonResponsible(model, command) as ViewResult; ;

            //Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "PersonResponsible", "Incorrect view returned");
                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), false, "Expected JobTitle  value other than null failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), false, "Expected FirstName value other than null  failure");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("LasttName"), false, "Expected LasttName value other than null  failure");
            });
            //TODO you should also be checking modelstate has no other errors to ensure other fields are being retained after postback and not causing errors

        }

        //   [Test]
        [Description("Ensure the PersonResponsible succeeds when all fields are filled in with valid values")]
        public void PersonResponsible_ValidFields_NoErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };


            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
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
            var result = controller.PersonResponsible(model, command) as ViewResult; ;

            //Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(result.ViewName == "PersonResponsible", "Incorrect view returned");
                Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");

                Assert.AreEqual(result.ViewData.ModelState.IsValidField("JobTitle"), true, "Expected JobTitle value other than empty strings success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("FirstName"), true, "Expected FirstName value other than empty strings  success");
                Assert.AreEqual(result.ViewData.ModelState.IsValidField("LasttName"), true, "Expected LasttName value other than empty strings  success");
            });
            //TODO you should also be checking modelstate has no other errors to ensure other fields are being retained after postback and not causing errors

        }
        #endregion


        #region CompanyLinkToGPGInfo
        [Test]
        [Description("EmployerWebsite should succeed when view is requested")]
        public void EmployerWebsite_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            //Stash an object to pass in for this.ClearStash()
            controller.StashModel(model);

            //ACT:
            var result = controller.EmployerWebsite(returnurl) as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");//TODO redundate due to previous line
            Assert.That(result.ViewName == "EmployerWebsite", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            //TODO you shouold be checking all model fields are correct too
        }

        [Test]
        [Description("EmployerWebsite should succeed its field is empty or null on View Post")]
        public void EmployerWebsite_Post_Without_CompanyLinkToGPGInfoValue_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
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
            var result = controller.EmployerWebsite(model) as RedirectToRouteResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "CheckData", "Expected a RedirectToRouteResult to CheckData");

            // See if there are anymore asserts that can be done for a redirect here.
            //TODO you should be checking modelstate.isvalid and also that all other fields dont fail in modelstate

        }

        [Test]
        [Description("EmployerWebsite should succeed its field has value on View Post, no need to check validity of the value here, (due to client-side validation)")]
        public void EmployerWebsite_Post_With_CompanyLinkToGPGInfoValue_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
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
            var result = controller.EmployerWebsite(model) as RedirectToRouteResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "CheckData", "Expected a RedirectToRouteResult to CheckData");

            // See if there are anymore asserts that can be done for a redirect here.
            //TODO you should be checking all fields here are in facts valid as well as modelstate.isvalid
        }




        [Test]
        [Description("Verify that a good url link with the proper web protocol prefix is validated and allowed")]
        public void EmployerWebsite_VerifyGPGInfoLink_GoodURL_Link()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
           // var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);


            var model = new ReturnViewModel()
            {
                CompanyLinkToGPGInfo = "http://www.google.com" 
            };
           
            //Act
            var result = controller.EmployerWebsite(model) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //Assert
            Assert.That(resultModel.CompanyLinkToGPGInfo.StartsWith("http://")  ||
                        resultModel.CompanyLinkToGPGInfo.StartsWith("https://") || 
                        resultModel.CompanyLinkToGPGInfo.StartsWith("ftp://"), 
                        "Expected CompanyLinkToGPGInfoLink should have one of the neccesary URL Prefix:'http://', 'https://' or 'ftp://' ");

            //TODO why are you checking for ftp http when you set it to https
            //TODO this is exaclty the same test as previous
            //TODO you should be checking modelstate.isvalid and each modelstate error
        }

        //I dont think this test is neccesary as the above does the same thing this just does the same but in opposite
        [Test]
        [Description("Verify that a bad url link with the improper web protocol prefix is not validated or allowed")]
        public void EmployerWebsite_VerifyGPGInfoLink_BadURL_Link()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
           // var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);


            var model = new ReturnViewModel()
            {
                CompanyLinkToGPGInfo =  "http:www.//google.com" 
            };

            //Act
            var result = controller.EmployerWebsite(model) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //Assert
            Assert.That((!resultModel.CompanyLinkToGPGInfo.StartsWith("http://")) ||
                        (!resultModel.CompanyLinkToGPGInfo.StartsWith("https://")) ||
                        (!resultModel.CompanyLinkToGPGInfo.StartsWith("ftp://")),
                        "Expected CompanyLinkToGPGInfoLink should have one of the neccesary URL Prefix:'http://', 'https://' or 'ftp://' ");

            //TODO again this is the wrong assert - you should be just checking that modelstate.isvalid and no other modelstate errors except for exact weblink field
        }

        [Test]
        [Description("Verify an existing GPGInfo Link is what is returned")]
        public void EmployerWebsite_VerifyGPGInfoLink_WhatYouPutIn_IsWhatYouGetOut()
        {
            //ARRANGE:
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //mock return with CompanyLinkToGPGInfo in the DB
            var @return = new Return() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "http://www.test.com" };

            var routeData = new RouteData();
            routeData.Values.Add("action", "EmployerWebsite");
            routeData.Values.Add("controller", "submit");

            //mock entered return CompanyLinkToGPGInfo in the CompanyLinkToGPGInfo EmployerWebsite view
            var model = new ReturnViewModel()
                            {
                                CompanyLinkToGPGInfo = "http://www.test.com"
                            };

            //added into the mock DB via mockRepository
            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);

            //ACT:
            var result = controller.EmployerWebsite(model) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //ASSERT:
            Assert.That(resultModel.CompanyLinkToGPGInfo == model.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfoLink that was input by the user is not what is returned");

            //TODO not really a valid test as there is no code which changes this - you should maybe just be checking there are no modelstate errors but then its a repeat test of one you did earlier
            //TODO also your not checking for the correct redirectresult and the rest of the model the correct model - why just test one field remains unchanged?
        }

        #endregion


        #region Review
        [Test]
        [Description("CheckData should fail when any field is empty")]
        public void CheckData_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            //Stash an object to pass in for this.ClearStash()
            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            controller.StashModel(model);

            //ACT:
            var result = controller.EnterCalculations() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");//TODO redundant again due to previous line
            Assert.That(result.ViewName == "CheckData", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");//TODO redundant due to previous line
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            //TODO shouldnt you be checking that all the model fields are returned correctly

            //TODO remember when checking modelstate.isvalid=true there should be no need to check individual modelstate erorrs but when checking  checking modelstate.isvalid=valid you should be checking all the errors are exactly right with no more and no less errors than expected
        }

        [Test]
        [Description("CheckData should fail when any field is empty")]
        public void CheckData_Post_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //mock return existing in the DB
            var @return = new ReturnViewModel() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "http://www.test.com" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            //mock entered return at review CheckData view
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
            var result = controller.CheckData(model) as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            var resultDB = (controller.DataRepository.GetAll<Return>().FirstOrDefault( r => r.CompanyLinkToGPGInfo == "http://www.gov.uk"));//TODO this should just return the correct record with returnid=1

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");//TODO redundant due to previous line
            Assert.That(result.ViewName == "SubmissionComplete", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");//Again redundant
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            // get the data from te mock database and assert it is there
            Assert.That(model.CompanyLinkToGPGInfo == resultModel.CompanyLinkToGPGInfo, "expected entered companyLinkToGPGInfo is what is saved in db");
            //TODO this is wrong - you should be checking the model values you passed in have been saved exactly in resultDB in a new record and not in the old one since it has changed
            //TODO you should also do a test that if no changes saved no new record is recreated

        }
        #endregion












    }
}
