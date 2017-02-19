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
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode ="0" };
            //simulated return from mock db
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            //Add a return to the mock repo to simulate one in the database
            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation, @return);
           // controller.bind();

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
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "0" };

            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);
           // controller.Bind(model);


            //Act:
            var result = (ViewResult)controller.Step1();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 0, "ReturnId expected 0");
            Assert.That(resultModel.OrganisationId == 1, "Organisation Id ");
        }




        [Test]
        [Description("Ensure that Step1 fails when any field is empty")]
        public void Step1_EmptyFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "1" };

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


            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);
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
        [Description("Ensure the Step1 succeeds when all fields have valid values")]
        public void Step1_ValidValueInFields_NoError()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "1" };

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
            //controller.Bind(model);

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
        [Description("Ensure that Step1 fails when any field is outside of the minimum allowed range of valid values")]
        public void Step1_MinInValidValues_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "1" };

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


            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);
            //controller.Bind(model);

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
        [Description("Ensure that Step1 fails when any field is outside of the maximum allowed range of valid values")]
        public void Step1_MaxInValidValues_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "1" };

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


            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);
            //controller.Bind(model);

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




        #region Person Responsible
        [Test]
        [Description("Ensure the Step2 fails when any field is empty")]
        public void Step2_EmptyFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "1" };

            string emptyString = string.Empty;

            var model = new ReturnViewModel()
            {
                JobTitle = emptyString,
                FirstName = emptyString,
                LastName = emptyString
            };

            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);
            //controller.bind();

            //Act:
            var result = controller.Step2(model) as ViewResult; ;

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

        [Test]
        [Description("Ensure the Step2 fails when any field is null")]
        public void Step2_NullFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "1" };

            string emptyString = null;

            var model = new ReturnViewModel()
            {
                JobTitle = emptyString,
                FirstName = emptyString,
                LastName = emptyString
            };

            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);
            //controller.bind();

            //Act:
            var result = controller.Step2(model) as ViewResult; ;

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

        [Test]
        [Description("Ensure the Step2 succeeds when all fields are filled in with valid values")]
        public void Step2_ValidFields_NoErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "1" };

            var model = new ReturnViewModel()
            {
                JobTitle  = "Director",
                FirstName = "MyFirstName",
                LastName  = "MyLastName"
            };

            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);
            //controller.bind();

            //Act:
            var result = controller.Step2(model) as ViewResult; ;

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
        [Description("Verify that a bad url link is not allowed or validated")]
        public void Step3_VerifyGPGInfoLink_BadURL_Link()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "0" };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);

            var model = new ReturnViewModel()
            {
                CompanyLinkToGPGInfo = "http:www.//google.com" 
                //controller.Bind(model);
            };
           
            //Act
            var result = (ViewResult)controller.Step3(model, string.Empty);
            var returnModel = result.Model as Return;

            //Assert
            Assert.That(returnModel.CompanyLinkToGPGInfo == model.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfoLink should have the neccesary URL Prefix, 'http://' /'https:// and 'ftp://''");

        }

        [Test]
        [Description("Verify an existing GPGInfo Link is what is returned")]
        public void Step3_VerifyGPGInfoLink_WhatYouPutIn_IsWhatYouGetOut()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "0" };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var model = new ReturnViewModel();
            model.CompanyLinkToGPGInfo = "http://www.test.com";

            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);

            //Act
            var result = (ViewResult)controller.Step3(model, string.Empty);
            //controller.Bind(model);
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
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "0" };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);
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
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINCode = "0" };
            var @return = new Return() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "https://www.test.com" };

            var controller = TestHelper.GetController<SubmitController>(1, user, organisation, userOrganisation);

            //Act
            //var result = (ViewResult)controller.Step1(@return);
            //var returnModel = result.Model as Return;

            // Assert
            // Assert.That(returnModel, Is.EqualTo(@return), "Error Message");
        }



        



    }
}
