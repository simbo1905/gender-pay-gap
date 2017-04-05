using NUnit.Framework;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.Models.SqlDatabase;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Submit;

namespace GenderPayGap.Tests.Submission
{


    [TestFixture]
    public class SubmissionTest
    {

        //TODO For non Positive Tests:
        //TODO shouldnt you be checking that all the model fields are returned correctly
        //TODO remember when checking modelstate.isvalid=true there should be no need to check individual modelstate erorrs but when checking  checking modelstate.isvalid=valid you should be checking all the errors are exactly right with no more and no less errors than expected

        [SetUp]
        public void Setup() {}

        #region Return
        
        //[Test]
        [Description("")]
        public void EnterCalculations_UserNotLoggedIn_RedirectToLoginPage()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
            //var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var controller = TestHelper.GetController<SubmitController>(1);

            //Act
            //var result = controller.xyz() as RedirectToRouteResult;
            var result = controller.EnterCalculations() as RedirectToRouteResult;

            //Assert
            Assert.Null(result, "Should have redirected");
            // Assert.Null(, "Should have redirected to index page");

        }

      //  [Test]
        [Description("If a user has a return in the database load that return and verify its existence")]
        public void EnterCalculations_UserHasReturn_ShowExistingReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
            //simulated return from mock db
            var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var model = new ReturnViewModel()
                            {
                                ReturnId = 1,
                                SectorType = SectorTypes.Private
                            };


            //Add a return to the mock repo to simulate one in the database
            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);
            // controller.bind();

            controller.StashModel(model);

            //Act:
            var result = (ViewResult)controller.EnterCalculations();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 1, "ReturnId expected '1' as a value");
            Assert.That(resultModel.OrganisationId == 1, "OrganisationId expected '1' as a value");

        }

        //[Test]
        [Description("If a user does not have a return existing in the database, a new one should be created and verified with default values")]
        public void EnterCalculations_UserHasNoReturn_ShowNewPrivateSectorReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            var PrivateAccountingDate = WebUI.Properties.Settings.Default.PrivateAccountingDate; //new DateTime(2017, 4, 5);
            //controller.Bind(model);


            //Act:
            var result = (ViewResult)controller.EnterCalculations();
            var resultModel = result.Model as ReturnViewModel;

            //Assert:
            Assert.That(resultModel.ReturnId == 0, "ReturnId expected 0");
            Assert.That(resultModel.OrganisationId == 1, "Organisation Id ");
            Assert.That(resultModel.AccountingDate == PrivateAccountingDate, "Private sector Return start date expected");
        }

        //[Test]
        [Description("If a user does not have a return existing in the database, a new one should be created and verified with default values")]
        public void EnterCalculations_UserHasNoReturn_ShowNewPublicSectorReturn()
        {
            // Arrange:
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
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


        #region Negative Tests
        //[Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public void EnterCalculations_EmptyFields_ShowAllErrors()
        {
            //ARRANGE:
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            //empty model without values
            var model = new ReturnViewModel();

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //ACT:
            var result = controller.EnterCalculations(model) as ViewResult;

            //ASSERT:
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

        //[Test]
        [Description("EnterCalculations should fail when any field is outside of the minimum allowed range of valid values")]
        public void EnterCalculations_MinInValidValues_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            decimal minOutOfRangeValue = -201;

            var PrivateAccountingDate = WebUI.Properties.Settings.Default.PrivateAccountingDate;

            var model = new ReturnViewModel()
            {
                AccountingDate = PrivateAccountingDate,
                DiffMeanBonusPercent = minOutOfRangeValue,
                DiffMeanHourlyPayPercent = minOutOfRangeValue,
                DiffMedianBonusPercent = minOutOfRangeValue,
                DiffMedianHourlyPercent = minOutOfRangeValue,
                FemaleLowerPayBand = minOutOfRangeValue,
                FemaleMedianBonusPayPercent = minOutOfRangeValue,
                FemaleMiddlePayBand = minOutOfRangeValue,
                FemaleUpperPayBand = minOutOfRangeValue,
                FemaleUpperQuartilePayBand = minOutOfRangeValue,
                MaleLowerPayBand = minOutOfRangeValue,
                MaleMedianBonusPayPercent = minOutOfRangeValue,
                MaleMiddlePayBand = minOutOfRangeValue,
                MaleUpperPayBand = minOutOfRangeValue,
                MaleUpperQuartilePayBand = minOutOfRangeValue,
                SectorType = SectorTypes.Private,
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


                //TODO instead of checking ModelState.Isvalid we should now be checking for exact error message on each field is than this in ErrorConfig - this can be done later but we must start doing this from now on
            });
        }

        //[Test]
        [Description("EnterCalculations should fail when any field is outside of the maximum allowed range of valid values")]
        public void EnterCalculations_MaxInValidValues_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "register");

            decimal maxOutOfRangeValue = 201M;

            var model = new ReturnViewModel()
            {
                DiffMeanBonusPercent = maxOutOfRangeValue,
                DiffMeanHourlyPayPercent = maxOutOfRangeValue,
                DiffMedianBonusPercent = maxOutOfRangeValue,
                DiffMedianHourlyPercent = maxOutOfRangeValue,
                FemaleLowerPayBand = maxOutOfRangeValue,
                FemaleMedianBonusPayPercent = maxOutOfRangeValue,
                FemaleMiddlePayBand = maxOutOfRangeValue,
                FemaleUpperPayBand = maxOutOfRangeValue,
                FemaleUpperQuartilePayBand = maxOutOfRangeValue,
                MaleLowerPayBand = maxOutOfRangeValue,
                MaleMedianBonusPayPercent = maxOutOfRangeValue,
                MaleMiddlePayBand = maxOutOfRangeValue,
                MaleUpperPayBand = maxOutOfRangeValue,
                MaleUpperQuartilePayBand = maxOutOfRangeValue
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

            //TODO again we need to check for exact error messages from config -> ignore for now
        }

        #endregion

        #region Positive Tests
        [Test]
        [Description("Ensure that EnterCalculations passes when all zero values are entered in all/any of the fields as zero is a valid value")]
        public void EnterCalculations_ZeroValidValueInFields_NoError()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            string returnurl = "CheckData";
            var PrivateAccountingDate = WebUI.Properties.Settings.Default.PrivateAccountingDate;
            decimal zero = 0;

            var model = new ReturnViewModel()
            {
                AccountingDate = PrivateAccountingDate,
                DiffMeanBonusPercent = zero,
                DiffMeanHourlyPayPercent = zero,
                DiffMedianBonusPercent = zero,
                DiffMedianHourlyPercent = zero,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = zero,
                FemaleMiddlePayBand = 60,
                FemaleUpperPayBand = 70,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = zero,
                MaleMiddlePayBand = 40,
                MaleUpperPayBand = 30,
                MaleUpperQuartilePayBand = 50,
                SectorType = SectorTypes.Private
            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //Act
            var result = controller.EnterCalculations(model, returnurl) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // Assert
            //DONE:Since it was stashed no need to check the fields as it is exactly what it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel as ReturnViewModel, "Unstashed model is Invalid Expected ReturnViewModel");
            Assert.That(result.RouteValues["action"].ToString() == "CheckData", "Expected a RedirectToRouteResult to CheckData");

            Assert.NotNull(result, "Expected RedirectResult");
            var x = controller.ViewData.Model as ReturnViewModel;

            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(resultModel.AccountingDate == model.AccountingDate, "Input value does not match model");
                Assert.That(resultModel.Address == model.Address, "Input value does not match model");
                Assert.That(resultModel.CompanyLinkToGPGInfo == model.CompanyLinkToGPGInfo, "Input value does not match model");
                Assert.That(resultModel.DiffMeanBonusPercent == model.DiffMeanBonusPercent, "Input value does not match model");
                Assert.That(resultModel.DiffMeanHourlyPayPercent == model.DiffMeanHourlyPayPercent, "Input value does not match model");
                Assert.That(resultModel.DiffMedianBonusPercent == model.DiffMedianBonusPercent, "Input value does not match model");
                Assert.That(resultModel.DiffMedianHourlyPercent == model.DiffMedianHourlyPercent, "Input value does not match model");
                Assert.That(resultModel.FemaleLowerPayBand == model.FemaleLowerPayBand, "Input value does not match model");
                Assert.That(resultModel.FemaleMedianBonusPayPercent == model.FemaleMedianBonusPayPercent, "Input value does not match model");
                Assert.That(resultModel.FemaleMiddlePayBand == model.FemaleMiddlePayBand, "Input value does not match model");
                Assert.That(resultModel.FemaleUpperPayBand == model.FemaleUpperPayBand, "Input value does not match model");
                Assert.That(resultModel.FemaleUpperQuartilePayBand == model.FemaleUpperQuartilePayBand, "Input value does not match model");
                Assert.That(resultModel.MaleLowerPayBand == model.MaleLowerPayBand, "Input value does not match model");
                Assert.That(resultModel.MaleMiddlePayBand == model.MaleMiddlePayBand, "Input value does not match model");

                Assert.That(resultModel.MaleUpperPayBand == model.MaleUpperPayBand, "Input value does not match model");
                Assert.That(resultModel.MaleUpperQuartilePayBand == model.MaleUpperQuartilePayBand, "Input value does not match model");
                Assert.That(resultModel.OrganisationId == model.OrganisationId, "Input value does not match model");
                Assert.That(resultModel.OrganisationName == model.OrganisationName, "Input value does not match model");
                Assert.That(resultModel.ReturnId == model.ReturnId, "Input value does not match model");
                Assert.That(resultModel.ReturnUrl == model.ReturnUrl, "Input value does not match model");
                Assert.That(resultModel.Sector == model.Sector, "Input value does not match model");
                Assert.That(resultModel.SectorType == model.SectorType, "Input value does not match model");
                Assert.That(resultModel.FirstName == model.FirstName, "Input value does not match model");
                Assert.That(resultModel.JobTitle == model.JobTitle, "Input value does not match model");
                Assert.That(resultModel.LastName == model.LastName, "Input value does not match model");
            });
        }

      //  [Ignore("This test needs fixing")]
        [Test]
        [Description("EnterCalculations should succeed when all fields have valid values")]
        public void EnterCalculations_ValidValueInFields_NoErrors()
        {
            //ARRANGE:
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            string returnurl = "CheckData";
            var PrivateAccountingDate = WebUI.Properties.Settings.Default.PrivateAccountingDate;
            decimal validValue = 100M;

            var model = new ReturnViewModel()
            {
                AccountingDate = PrivateAccountingDate,
                DiffMeanBonusPercent = validValue,
                DiffMeanHourlyPayPercent = validValue,
                DiffMedianBonusPercent = validValue,
                DiffMedianHourlyPercent = validValue,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = validValue,
                FemaleMiddlePayBand = 60,
                FemaleUpperPayBand = 70,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = validValue,
                MaleMiddlePayBand = 40,
                MaleUpperPayBand = 30,
                MaleUpperQuartilePayBand = 50,
                SectorType = SectorTypes.Private

            };


            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //ACT:
            var result = controller.EnterCalculations(model, returnurl) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //ASSERT:
            //DONE:Since it was stashed no need to check the fields as it is exactly what it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel as ReturnViewModel, "Unstashed model is Invalid Expected ReturnViewModel");
            Assert.That(result.RouteValues["action"].ToString() == "CheckData", "Expected a RedirectToRouteResult to CheckData");

            Assert.NotNull(result, "Expected RedirectResult");
            var x = controller.ViewData.Model as ReturnViewModel;

            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.Multiple(() =>
            {
                Assert.NotNull(result, "Expected ViewResult");
                Assert.That(resultModel.AccountingDate       == model.AccountingDate, "Input value does not match model");
                Assert.That(resultModel.Address              == model.Address, "Input value does not match model");
                Assert.That(resultModel.CompanyLinkToGPGInfo == model.CompanyLinkToGPGInfo, "Input value does not match model");
                Assert.That(resultModel.DiffMeanBonusPercent == model.DiffMeanBonusPercent, "Input value does not match model");
                Assert.That(resultModel.DiffMeanHourlyPayPercent == model.DiffMeanHourlyPayPercent, "Input value does not match model");
                Assert.That(resultModel.DiffMedianBonusPercent == model.DiffMedianBonusPercent, "Input value does not match model");
                Assert.That(resultModel.DiffMedianHourlyPercent == model.DiffMedianHourlyPercent, "Input value does not match model");
                Assert.That(resultModel.FemaleLowerPayBand == model.FemaleLowerPayBand, "Input value does not match model");
                Assert.That(resultModel.FemaleMedianBonusPayPercent == model.FemaleMedianBonusPayPercent, "Input value does not match model");
                Assert.That(resultModel.FemaleMiddlePayBand == model.FemaleMiddlePayBand, "Input value does not match model");
                Assert.That(resultModel.FemaleUpperPayBand == model.FemaleUpperPayBand, "Input value does not match model");
                Assert.That(resultModel.FemaleUpperQuartilePayBand == model.FemaleUpperQuartilePayBand, "Input value does not match model");
                Assert.That(resultModel.MaleLowerPayBand == model.MaleLowerPayBand, "Input value does not match model");
                Assert.That(resultModel.MaleMiddlePayBand == model.MaleMiddlePayBand, "Input value does not match model");

                Assert.That(resultModel.MaleUpperPayBand  == model.MaleUpperPayBand, "Input value does not match model");
                Assert.That(resultModel.MaleUpperQuartilePayBand == model.MaleUpperQuartilePayBand, "Input value does not match model");
                Assert.That(resultModel.OrganisationId    == model.OrganisationId, "Input value does not match model");
                Assert.That(resultModel.OrganisationName  == model.OrganisationName, "Input value does not match model");
                Assert.That(resultModel.ReturnId == model.ReturnId, "Input value does not match model");
                Assert.That(resultModel.ReturnUrl == model.ReturnUrl, "Input value does not match model");
                Assert.That(resultModel.Sector == model.Sector, "Input value does not match model");
                Assert.That(resultModel.SectorType == model.SectorType, "Input value does not match model");
                Assert.That(resultModel.FirstName == model.FirstName, "Input value does not match model");
                Assert.That(resultModel.JobTitle == model.JobTitle, "Input value does not match model");
                Assert.That(resultModel.LastName == model.LastName, "Input value does not match model");
            });
        }

      
        [Test]
        [Description("EnterCalculations should fail when any field is outside of the minimum allowed range of valid values")]
        public void EnterCalculations_MinValidValues_NoErrors()
        {
            //ARRANGE:
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            string returnurl = "CheckData";
            var PrivateAccountingDate = WebUI.Properties.Settings.Default.PrivateAccountingDate;
            decimal minValidValue = 200M; //-200.9M;
            decimal maleEquiValue   = 50;
            decimal femaleEquiValue = 50;

            var model = new ReturnViewModel()
            {
                AccountingDate               = PrivateAccountingDate,
                DiffMeanBonusPercent         = minValidValue,
                DiffMeanHourlyPayPercent     = minValidValue,
                DiffMedianBonusPercent       = minValidValue,
                DiffMedianHourlyPercent      = minValidValue,
                FemaleLowerPayBand           = femaleEquiValue,
                FemaleMedianBonusPayPercent  = minValidValue,
                FemaleMiddlePayBand          = femaleEquiValue,
                FemaleUpperPayBand           = femaleEquiValue,
                FemaleUpperQuartilePayBand   = femaleEquiValue,
                MaleLowerPayBand             = maleEquiValue,
                MaleMedianBonusPayPercent    = minValidValue,
                MaleMiddlePayBand            = maleEquiValue,
                MaleUpperPayBand             = maleEquiValue,
                MaleUpperQuartilePayBand     = maleEquiValue,
                SectorType                   = SectorTypes.Private
            };


            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //ACT:
            var result = controller.EnterCalculations(model, returnurl) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //ASSERT:
            //DONE:Since it was stashed no need to check the fields as it is exactly what it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel as ReturnViewModel, "Unstashed model is Invalid Expected ReturnViewModel");
            Assert.That(result.RouteValues["action"].ToString() == "CheckData", "Expected a RedirectToRouteResult to CheckData");

            // Assert.Multiple(() =>
            // {
            //Assert.NotNull(result, "Expected ViewResult");
            //Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");

            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),        true, "Expected DiffMeanBonusPercent failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"),    true, "Expected DiffMeanHourlyPayPercent failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),      true, "Expected DiffMedianBonusPercent failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),     true, "Expected DiffMedianHourlyPercent failure");

            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleLowerPayBand"),          true, "Expected FemaleLowerPayBand failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), true, "Expected FemaleMedianBonusPayPercent failure");                                                                         
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),         true, "Expected FemaleMiddlePayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperPayBand"),          true, "Expected FemaleUpperPayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),  true, "Expected FemaleUpperQuartilePayBand  failure");                                                                            
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleLowerPayBand"),            true, "Expected MaleLowerPayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),   true, "Expected MaleMedianBonusPayPercent  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMiddlePayBand"),           true, "Expected MaleMiddlePayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperPayBand"),            true, "Expected MaleUpperPayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),    true, "Expected MaleUpperQuartilePayBand  failure");
            // });
        }

        //[Ignore("This test needs fixing")]
        [Test]
        [Description("EnterCalculations should fail when any field is outside of the maximum allowed range of valid values")]
        public void EnterCalculations_MaxValidValues_NoErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            string returnurl = "CheckData";
            var PrivateAccountingDate = WebUI.Properties.Settings.Default.PrivateAccountingDate;
            decimal maxValidValue = 200.9M;
            decimal maleEquiValue   = 50;
            decimal femaleEquiValue = 50;

            var model = new ReturnViewModel()
            {
                AccountingDate               = PrivateAccountingDate,
                DiffMeanBonusPercent         = maxValidValue,
                DiffMeanHourlyPayPercent     = maxValidValue,
                DiffMedianBonusPercent       = maxValidValue,
                DiffMedianHourlyPercent      = maxValidValue,
                FemaleLowerPayBand           = femaleEquiValue,
                FemaleMedianBonusPayPercent  = maxValidValue,
                FemaleMiddlePayBand          = femaleEquiValue,
                FemaleUpperPayBand           = femaleEquiValue,
                FemaleUpperQuartilePayBand   = femaleEquiValue,
                MaleLowerPayBand             = maleEquiValue,
                MaleMedianBonusPayPercent    = maxValidValue,
                MaleMiddlePayBand            = maleEquiValue,
                MaleUpperPayBand             = maleEquiValue,
                MaleUpperQuartilePayBand     = maleEquiValue,
                SectorType                   = SectorTypes.Private

            };


            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //Act
            var result = controller.EnterCalculations(model, returnurl) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // Assert
            //DONE:Since it was stashed no need to check the fields as it is exactly what it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel as ReturnViewModel, "Unstashed model is Invalid Expected ReturnViewModel");
            Assert.That(result.RouteValues["action"].ToString() == "CheckData", "Expected a RedirectToRouteResult to CheckData");

            // Assert.Multiple(() =>
            // {
            //Assert.NotNull(result, "Expected ViewResult");
            //Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");

            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),        true, "Expected DiffMeanBonusPercent failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"),    true, "Expected DiffMeanHourlyPayPercent failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),      true, "Expected DiffMedianBonusPercent failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),     true, "Expected DiffMedianHourlyPercent failure");

            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleLowerPayBand"),          true, "Expected FemaleLowerPayBand failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), true, "Expected FemaleMedianBonusPayPercent failure");                                                                         
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),         true, "Expected FemaleMiddlePayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperPayBand"),          true, "Expected FemaleUpperPayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),  true, "Expected FemaleUpperQuartilePayBand  failure");                                                                            
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleLowerPayBand"),            true, "Expected MaleLowerPayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),   true, "Expected MaleMedianBonusPayPercent  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleMiddlePayBand"),           true, "Expected MaleMiddlePayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperPayBand"),            true, "Expected MaleUpperPayBand  failure");
            //Assert.AreEqual(result.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),    true, "Expected MaleUpperQuartilePayBand  failure");
            // });
        }

        //TODO Test needed for fields are now using regex to ensure only 1 decimal place

        [Test]
        [Description("Ensure the Enter Calculations form returns an existing return if there is one and the loaded model of the return is valid")]
        public void EnterCalculations_VerifyActionReturns_ValidReturnModel()
        {
            //ARRANGE:
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //return in the db
            var @return = new Return()
            {
                ReturnId = 1,
                OrganisationId = 1,
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

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);

            var model = new ReturnViewModel();
            controller.StashModel(model);

            //ACT:
            var result = controller.EnterCalculations() as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.That(result != null && result is ViewResult, "Expected returned ViewResult object not to be null  or incorrect resultType returned");
            Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");
            Assert.IsNotNull(resultModel, "Expected returned ReturnViewModel object not to be null");
            Assert.That(resultModel is ReturnViewModel, "Expected Model to be of type ReturnViewModel");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.NotNull(resultModel.DiffMeanBonusPercent == @return.DiffMeanBonusPercent, "DiffMeanBonusPercent:Expected a null or empty field");
            Assert.NotNull(resultModel.DiffMeanHourlyPayPercent == @return.DiffMeanHourlyPayPercent, "DiffMeanHourlyPayPercent:Expected a null  or empty field");
            Assert.NotNull(resultModel.DiffMedianBonusPercent == @return.DiffMedianBonusPercent, "DiffMedianBonusPercent:Expected a null  or empty field");
            Assert.NotNull(resultModel.DiffMedianHourlyPercent == @return.DiffMedianHourlyPercent, "DiffMedianHourlyPercent:Expected a null  or empty field");
            Assert.NotNull(resultModel.FemaleLowerPayBand == @return.FemaleLowerPayBand, "FemaleLowerPayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.FemaleMedianBonusPayPercent == @return.FemaleMedianBonusPayPercent, "FemaleMedianBonusPayPercent:Expected a null  or empty field");
            Assert.NotNull(resultModel.FemaleMiddlePayBand == @return.FemaleMiddlePayBand, "FemaleMiddlePayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.FemaleUpperPayBand == @return.FemaleUpperPayBand, "FemaleUpperPayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.FemaleUpperQuartilePayBand == @return.FemaleUpperQuartilePayBand, "FemaleUpperQuartilePayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.MaleLowerPayBand == @return.MaleLowerPayBand, "MaleLowerPayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.MaleMedianBonusPayPercent == @return.MaleMedianBonusPayPercent, "MaleMedianBonusPayPercent:Expected a null  or empty field");
            Assert.NotNull(resultModel.MaleMiddlePayBand == @return.MaleMiddlePayBand, "MaleMiddlePayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.MaleUpperPayBand == @return.MaleUpperPayBand, "MaleUpperPayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.MaleUpperQuartilePayBand == @return.MaleUpperQuartilePayBand, "MaleUpperQuartilePayBand:Expected a null  or empty field");

            Assert.NotNull(resultModel.FirstName == @return.FirstName, "FirstName:Expected a null  or empty field");
            Assert.NotNull(resultModel.LastName == @return.LastName, "LastName:Expected a null  or empty field");
            Assert.NotNull(resultModel.JobTitle == @return.JobTitle, "JobTitle:Expected a null  or empty field");

            Assert.NotNull(resultModel.CompanyLinkToGPGInfo == @return.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfo:Expected a null  or empty field");
        }

        [Test]
        [Description("Ensure the EnterCalculations formreturns an existing return if there is one for the current user ")]
        public void EnterCalculations_VerifyActionReturns_AnExistingReturn()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            var @return = new Return() { ReturnId = 1, OrganisationId = organisation.OrganisationId, Organisation = organisation, CompanyLinkToGPGInfo = "https://www.test.com" };

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

        [Test]
        [Description("Ensure the EnterCalculations form is returned for the current user ")]
        public void EnterCalculations_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now /*, EmailVerifyHash = code.GetSHA512Checksum()*/ };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

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
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result != null && result is ViewResult, "Expected viewResult  or incorrect resultType returned");
            Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            //TODO you should be checking the returned model is empty
            Assert.Null(resultModel.DiffMeanBonusPercent, "DiffMeanBonusPercent:Expected a null or empty field");
            Assert.Null(resultModel.DiffMeanHourlyPayPercent, "DiffMeanHourlyPayPercent:Expected a null  or empty field");
            Assert.Null(resultModel.DiffMedianBonusPercent, "DiffMedianBonusPercent:Expected a null  or empty field");
            Assert.Null(resultModel.DiffMedianHourlyPercent, "DiffMedianHourlyPercent:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleLowerPayBand, "FemaleLowerPayBand:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleMedianBonusPayPercent, "FemaleMedianBonusPayPercent:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleMiddlePayBand, "FemaleMiddlePayBand:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleUpperPayBand, "FemaleUpperPayBand:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleUpperQuartilePayBand, "FemaleUpperQuartilePayBand:Expected a null  or empty field");
            Assert.Null(resultModel.MaleLowerPayBand, "MaleLowerPayBand:Expected a null  or empty field");
            Assert.Null(resultModel.MaleMedianBonusPayPercent, "MaleMedianBonusPayPercent:Expected a null  or empty field");
            Assert.Null(resultModel.MaleMiddlePayBand, "MaleMiddlePayBand:Expected a null  or empty field");
            Assert.Null(resultModel.MaleUpperPayBand, "MaleUpperPayBand:Expected a null  or empty field");
            Assert.Null(resultModel.MaleUpperQuartilePayBand, "MaleUpperQuartilePayBand:Expected a null  or empty field");


        }

      
        [Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public void EnterCalculations_Post_Success_PrivateSector()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = "";

            var model = new ReturnViewModel()
            {
                AccountingDate = (DateTime)WebUI.Properties.Settings.Default["PrivateAccountingDate"],
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 0.0M,
                DiffMeanHourlyPayPercent = 0,
                DiffMedianBonusPercent = 0,
                DiffMedianHourlyPercent = 0,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 60,
                FemaleUpperQuartilePayBand = 80,
                FirstName = null,
                LastName = null,
                JobTitle = null,
                MaleLowerPayBand = 90,
                MaleMedianBonusPayPercent = 0,
                MaleMiddlePayBand = 70,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 20,
                OrganisationId = organisation.OrganisationId,
                SectorType = SectorTypes.Private,
                ReturnId = 0,
            };

            //TODO line above is wrong as you should be setting the fields to null not zero

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

           //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.EnterCalculations(model, returnurl) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();


            //TODO this test is completely wrong you should be cheking the all the fields are invalid in the modelstate

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "PersonResponsible", "Expected a RedirectToRouteResult to PersonResponsible");

            // See if there are anymore asserts that can be done for a redirect here.

            Assert.Multiple(() =>
            {
                Assert.NotNull(resultModel is ReturnViewModel, "Expected ReturnViewModel");

                Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

                Assert.That(controller.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),     "Expected DiffMeanBonusPercent failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"), "Expected DiffMeanHourlyPayPercent failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),   "Expected DiffMedianBonusPercent failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),  "Expected DiffMedianHourlyPercent failure");
                                
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleLowerPayBand"),          "Expected FemaleLowerPayBand failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), "Expected FemaleMedianBonusPayPercent failure");          
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),         "Expected FemaleMiddlePayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleUpperPayBand"),          "Expected FemaleUpperPayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),  "Expected FemaleUpperQuartilePayBand  failure");
                               
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleLowerPayBand"),            "Expected MaleLowerPayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),   "Expected MaleMedianBonusPayPercent  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleMiddlePayBand"),           "Expected MaleMiddlePayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleUpperPayBand"),            "Expected MaleUpperPayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),    "Expected MaleUpperQuartilePayBand  failure");
            });
        }

        [Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public void EnterCalculations_Post_Success_PublicSector()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Public };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = "";

            var model = new ReturnViewModel()
            {
                AccountingDate = (DateTime)WebUI.Properties.Settings.Default["PrivateAccountingDate"],
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 0,
                DiffMeanHourlyPayPercent = 0,
                DiffMedianBonusPercent = 0,
                DiffMedianHourlyPercent = 0,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 60,
                FemaleUpperQuartilePayBand = 80,
                FirstName = null,
                LastName = null,
                JobTitle = null,
                MaleLowerPayBand = 90,
                MaleMedianBonusPayPercent = 0,
                MaleMiddlePayBand = 70,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 20,
                OrganisationId = organisation.OrganisationId,
                SectorType = SectorTypes.Public,
                ReturnId = 0,
            };

            //TODO line above is wrong as you should be setting the fields to null not zero

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.EnterCalculations(model, returnurl) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "EmployerWebsite", "Expected a RedirectToRouteResult to EmployerWebsite");

            //TODO This line is wrong as we should be returning the same view since model state was invalid
           
            // See if there are anymore asserts that can be done for a redirect here.

            Assert.Multiple(() =>
            {
                Assert.NotNull(resultModel is ReturnViewModel, "Expected ReturnViewModel");

                Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

                Assert.That(controller.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"), "Expected DiffMeanBonusPercent failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"), "Expected DiffMeanHourlyPayPercent failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"), "Expected DiffMedianBonusPercent failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"), "Expected DiffMedianHourlyPercent failure");

                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleLowerPayBand"), "Expected FemaleLowerPayBand failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"), "Expected FemaleMedianBonusPayPercent failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"), "Expected FemaleMiddlePayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleUpperPayBand"), "Expected FemaleUpperPayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"), "Expected FemaleUpperQuartilePayBand  failure");

                Assert.That(controller.ViewData.ModelState.IsValidField("MaleLowerPayBand"), "Expected MaleLowerPayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"), "Expected MaleMedianBonusPayPercent  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleMiddlePayBand"), "Expected MaleMiddlePayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleUpperPayBand"), "Expected MaleUpperPayBand  failure");
                Assert.That(controller.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"), "Expected MaleUpperQuartilePayBand  failure");
            });
        }
        #endregion

        #endregion


        #region Person Responsible

        #region Positive Tests
        [Test]
        [Description("Ensure the Person Responsible form is returned for the current user ")]
        public void PersonResponsible_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

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
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.That(result != null && result is ViewResult, " Expected a viewResult or Incorrect resultType returned");
            Assert.That(result.Model is ReturnViewModel, "Incorrect model type returned");
            Assert.That(result.ViewName == "PersonResponsible", "Incorrect view returned");

            //TODO wrong should be checking its invalid: for negative tests yes.
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            //DONE should be checking each field for exact error message in modelstate
            Assert.Null(resultModel.FirstName, "FirstName:Expected a null  or empty field");
            Assert.Null(resultModel.LastName, "LastName:Expected a null  or empty field");
            Assert.Null(resultModel.JobTitle, "JobTitle:Expected a null  or empty field");

        }

        [Test]
        [Description("Ensure that Person Responsible form is filled and sent successfully ")]
        public void PersonResponsible_Post_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = "EmployerWebsite";

            var PrivateAccountingDate = WebUI.Properties.Settings.Default.PrivateAccountingDate;

            var model = new ReturnViewModel()
            {
                AccountingDate = (DateTime)WebUI.Properties.Settings.Default["PrivateAccountingDate"],
                CompanyLinkToGPGInfo = "http://www.test.com",
                DiffMeanBonusPercent = 20,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 20,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 20,
                FemaleMedianBonusPayPercent = 20,
                FemaleMiddlePayBand = 20,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 20,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 20,
                MaleMedianBonusPayPercent = 20,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 20,
                MaleUpperQuartilePayBand = 20,
                OrganisationId = organisation.OrganisationId,
            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.PersonResponsible(model, returnurl) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            Assert.NotNull(resultModel as ReturnViewModel, "Unstashed model is Invalid Expected ReturnViewModel");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "EmployerWebsite", "Expected a RedirectToRouteResult to EmployerWebsite");

            //TODO you are not checking here for model state is invalid
            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");
            //DONE you should be checking modelstate.isvalid and each modelstate error
            //DONE you should be checking only the exact failed fields show and error message
            Assert.That(controller.ViewData.ModelState.IsValidField("FirstName"), "Model is Invalid");
            Assert.That(controller.ViewData.ModelState.IsValidField("LastName"), "Model is Invalid");
            Assert.That(controller.ViewData.ModelState.IsValidField("Title"), "Model is Invalid");

            //TODO you should be checking each error message is exact as per confilg file
        }

        //[Test]
        [Description("Ensure the PersonResponsible fails when any field is empty")]
        public void PersonResponsible_EmptyFields_ShowAllErrors()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

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

               Assert.AreEqual(result.ViewData.ModelState.IsValid, false, "Expected a valid Model");

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
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

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
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };


            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "register");

            var model = new ReturnViewModel()
            {
                JobTitle = "Director",
                FirstName = "MyFirstName",
                LastName = "MyLastName"
            };

            var command = "";

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.bind();

            //Act:
            var result = controller.PersonResponsible(model, command) as ViewResult;

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

        #region Negative Tests

        #endregion

        #endregion


        #region CompanyLinkToGPGInfo

        #region Positive Tests
        [Test]
        [Description("Ensure the employer Website form is returned for the current user ")]
        public void EmployerWebsite_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

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
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.That(result != null && result.GetType() == typeof(ViewResult), "Expected an object other than null or Incorrect resultType returned");
            Assert.That(result.ViewName == "EmployerWebsite", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected a ReturnViewModel object, null object is returned");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
            Assert.That(result.ViewData.ModelState.IsValidField("CompanyLinkToGPGInfo"), "Expected CompanyLinkToGPGInfo value is malformed or incorrect format");
            Assert.Null(resultModel.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfo:Expected a null  or empty field");
        }

        [Test]
        [Description("Ensure that employer Website form is filled and sent successfully when there is no value as it is optional")]
        public void EmployerWebsite_Post_Without_CompanyLinkToGPGInfoValue_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            var model = new ReturnViewModel()
            {
                AccountingDate = (DateTime)WebUI.Properties.Settings.Default["PrivateAccountingDate"],
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
        [Description("Ensure that employer Website form is filled and sent successfully when its field value is a valid url value")]
        public void EmployerWebsite_Post_With_CompanyLinkToGPGInfoValue_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            var model = new ReturnViewModel()
            {
                AccountingDate = (DateTime)WebUI.Properties.Settings.Default["PrivateAccountingDate"],
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 10,
                DiffMedianBonusPercent = 10,
                DiffMedianHourlyPercent = 10,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 10,
                FemaleMiddlePayBand = 10,
                FemaleUpperPayBand = 10,
                FemaleUpperQuartilePayBand = 10,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 10,
                MaleMedianBonusPayPercent = 10,
                MaleMiddlePayBand = 10,
                MaleUpperPayBand = 10,
                MaleUpperQuartilePayBand = 10,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 10,
            };

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(model);

            //controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.EmployerWebsite(model) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.RouteValues["action"].ToString() == "CheckData", "Expected a RedirectToRouteResult to CheckData");

            //DONE:Since it was stashed no need to check the fields as it is exactlywhat it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel, "unstashed model is Invalid");

            //DONE you should be checking modelstate.isvalid and each modelstate error
            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");
            Assert.That(controller.ViewData.ModelState.IsValidField("CompanyLinkToGPGInfo"), "value for CompanyLinkToGPGInfo is malformed or incorrect format");
            Assert.That(resultModel.CompanyLinkToGPGInfo.StartsWith("http://"), "Expected CompanyLinkToGPGInfoLink URL Prefix:'http://' ");

            //Assert.NotNull(resultModel.DiffMeanBonusPercent         == resultModel.DiffMeanBonusPercent,        "DiffMeanBonusPercent:Expected a null or empty field");
            //Assert.NotNull(resultModel.DiffMeanHourlyPayPercent     == resultModel.DiffMeanHourlyPayPercent,    "DiffMeanHourlyPayPercent:Expected a null  or empty field");
            //Assert.NotNull(resultModel.DiffMedianBonusPercent       == resultModel.DiffMedianBonusPercent,      "DiffMedianBonusPercent:Expected a null  or empty field");
            //Assert.NotNull(resultModel.DiffMedianHourlyPercent      == resultModel.DiffMedianHourlyPercent,     "DiffMedianHourlyPercent:Expected a null  or empty field");
            //Assert.NotNull(resultModel.FemaleLowerPayBand           == resultModel.FemaleLowerPayBand,          "FemaleLowerPayBand:Expected a null  or empty field");
            //Assert.NotNull(resultModel.FemaleMedianBonusPayPercent  == resultModel.FemaleMedianBonusPayPercent, "FemaleMedianBonusPayPercent:Expected a null  or empty field");
            //Assert.NotNull(resultModel.FemaleMiddlePayBand          == resultModel.FemaleMiddlePayBand,         "FemaleMiddlePayBand:Expected a null  or empty field");
            //Assert.NotNull(resultModel.FemaleUpperPayBand           == resultModel.FemaleUpperPayBand,          "FemaleUpperPayBand:Expected a null  or empty field");
            //Assert.NotNull(resultModel.FemaleUpperQuartilePayBand   == resultModel.FemaleUpperQuartilePayBand,  "FemaleUpperQuartilePayBand:Expected a null  or empty field");
            //Assert.NotNull(resultModel.MaleLowerPayBand             == resultModel.MaleLowerPayBand,            "MaleLowerPayBand:Expected a null  or empty field");
            //Assert.NotNull(resultModel.MaleMedianBonusPayPercent    == resultModel.MaleMedianBonusPayPercent,   "MaleMedianBonusPayPercent:Expected a null  or empty field");
            //Assert.NotNull(resultModel.MaleMiddlePayBand            == resultModel.MaleMiddlePayBand,           "MaleMiddlePayBand:Expected a null  or empty field");
            //Assert.NotNull(resultModel.MaleUpperPayBand             == resultModel.MaleUpperPayBand,            "MaleUpperPayBand:Expected a null  or empty field");
            //Assert.NotNull(resultModel.MaleUpperQuartilePayBand     == resultModel.MaleUpperQuartilePayBand,    "MaleUpperQuartilePayBand:Expected a null  or empty field");

            //Assert.NotNull(resultModel.FirstName                    == resultModel.FirstName,                   "FirstName:Expected a null  or empty field");
            //Assert.NotNull(resultModel.LastName                     == resultModel.LastName,                    "LastName:Expected a null  or empty field");
            //Assert.NotNull(resultModel.JobTitle                     == resultModel.JobTitle,                    "JobTitle:Expected a null  or empty field");

            //Assert.NotNull(resultModel.CompanyLinkToGPGInfo         == resultModel.CompanyLinkToGPGInfo,        "CompanyLinkToGPGInfo:Expected a null  or empty field");
        }




        //I dont think this test is neccesary as the above does the same thing this just does the same but in opposite
        [Test]
        [Description("Verify that a bad url link with the improper web protocol prefix is not validated or allowed")]
        public void EmployerWebsite_VerifyGPGInfoLink_BadURL_Link()
        {
            //Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };
            // var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            var controller = TestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            //controller.Bind(model);


            var model = new ReturnViewModel()
            {
                CompanyLinkToGPGInfo = "http:www.//google.com"
            };

            //Act
            var result = controller.EmployerWebsite(model) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //ASSERT
            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");
            Assert.That(controller.ViewData.ModelState.IsValidField("CompanyLinkToGPGInfo"), "value for CompanyLinkToGPGInfo is malformed or incorrect format");


        }

        [Test]
        [Description("Verify an existing GPGInfo Link is what is returned")]
        public void EmployerWebsite_VerifyGPGInfoLink_WhatYouPutIn_IsWhatYouGetOut()
        {
            //ARRANGE:
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "0" };

            //mock return with CompanyLinkToGPGInfo in the DB
            var @return = new Return() { ReturnId = 1, OrganisationId = organisation.OrganisationId, Organisation = organisation, CompanyLinkToGPGInfo = "http://www.test.com" };

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

        #region Negative Tests

        #endregion

        #endregion


        #region Review

        #region Positive Tests
        [Test]
        [Description("Ensure the Check Data form is returned for the current user ")]
        public void CheckData_Get_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            //Stash an object to pass in for this.ClearStash()
            var model = new ReturnViewModel()
                {
                  DiffMeanBonusPercent = 12,
                  DiffMeanHourlyPayPercent = 14,
                  DiffMedianBonusPercent = 12,
                  DiffMedianHourlyPercent = 43,
                  FemaleLowerPayBand = 23,
                  FemaleMedianBonusPayPercent = 21,
                  FemaleMiddlePayBand = 16,
                  FemaleUpperPayBand = 17,
                  FemaleUpperQuartilePayBand = 41,
                  MaleLowerPayBand = 12,
                  MaleMedianBonusPayPercent = 11,
                  MaleMiddlePayBand = 56,
                  MaleUpperPayBand = 33,
                  MaleUpperQuartilePayBand = 42,
                  OrganisationId = 1,
                  OrganisationName = "test org name",
                  ReturnId = 1,
                };

            var controller = TestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            controller.StashModel(model);

            //ACT:
            var result = controller.CheckData() as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.That(result != null && result.GetType() == typeof(ViewResult), " Incorrect resultType returned");//TODO redundant again due to previous line
            Assert.That(result.ViewName == "CheckData", "Incorrect view returned");
            Assert.NotNull(resultModel as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(resultModel != null && resultModel.GetType() == typeof(ReturnViewModel), "Expected ReturnViewModel or Incorrect viewModel returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        //why is ReturnID = 0, Investigate!
        [Test]
        [Description("Ensure that CheckData form has all previous form values correct and validated and sent successfully")]
        public void CheckData_Post_Success()
        {
            // Arrange
            var user = new User() { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = DateTime.Now };
            var organisation = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation() { OrganisationId = organisation.OrganisationId, Organisation = organisation, UserId = 1, PINConfirmedDate = DateTime.Now, PINHash = "1" };

            //mock return existing in the DB
            var @return = new ReturnViewModel() { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "http://www.test.com" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            //mock entered 'return' at review CheckData view
            var model = new ReturnViewModel()
            {
                AccountingDate = (DateTime)WebUI.Properties.Settings.Default["PrivateAccountingDate"],
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
            var result = controller.CheckData(model) as RedirectToRouteResult;
            //var resultModel = result.Model as ReturnViewModel;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //DONE this should just return the correct record with returnid=1
            var resultDB = (controller.DataRepository.GetAll<Return>().FirstOrDefault(r => r.ReturnId == 1));

            // ASSERT:
            //3.Check that the result is not null
            Assert.That(result != null && result.GetType() == typeof(RedirectToRouteResult), "Expected RedirectToRouteResult or Incorrect resultType returned");
            Assert.That(result.RouteValues["action"].ToString() == "SubmissionComplete", "Incorrect view returned");

            Assert.That(resultModel  != null && resultModel.GetType() == typeof(ReturnViewModel), "Expected ReturnViewModelis null or Incorrect resultType returned");

            //Check the Model State
            //Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
            

            // get the data from the mock database and assert it is there
            Assert.That(model.CompanyLinkToGPGInfo == resultModel.CompanyLinkToGPGInfo, "expected: entered companyLinkToGPGInfo is what is saved in db");

            //TODO this is wrong - you should be checking the model values you passed in have been saved exactly in resultDB in a new record and not in the old one since it has changed

            //TODO you should also do a test that if no changes saved no new record is recreated

        }

        #endregion

        #region Negative Tests

        #endregion

        #endregion












    }
}
