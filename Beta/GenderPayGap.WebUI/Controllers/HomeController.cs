using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GenderPayGap.Models.SqlDatabase;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Autofac;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Properties;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Home")]
    [Route("{action}")]
    public class HomeController : BaseController
    {
        #region Initialisation
        public HomeController() : base() { }
        public HomeController(IContainer container) : base(container) { }


        /// <summary>
        /// This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [Route("Init")]
        public ActionResult Init()
        {
#if DEBUG
            MvcApplication.Log.WriteLine("Home Controller Initialised");
#endif
            return new EmptyResult();
        }
        #endregion
        [Route("~/")]
        public ActionResult Redirect()
        {
            if (WasController("Register"))return RedirectToAction("AboutYou","Register");
            if (WasController("Submit"))return RedirectToAction("EnterCalculations", "Submit");
            return RedirectToAction("SearchResults","Viewing");
        }

        [Route("SignOut")]
        public ActionResult SignOut()
        {
            Session.Abandon();
            Request.GetOwinContext().Authentication.SignOut(new AuthenticationProperties { RedirectUri = Url.Action("EnterCalculations", "Submit",null,"https") });
            return RedirectToAction("EnterCalculations","Submit");
        }

        [Route("TimeOut")]
        public ActionResult TimeOut()
        {
            Session.Abandon();
            Request.GetOwinContext().Authentication.SignOut(new AuthenticationProperties { RedirectUri = Url.Action("EnterCalculations","Submit", null, "https") });
            return null;
        }

        #region TEST CODE ONLY
#if DEBUG || TEST

        [HttpGet]
        [Route]
        [Route("Index")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("Execute")]
        public ActionResult Execute()
        {
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        [Route("Execute")]
        public ActionResult Execute(string command)
        {
            var userId = User.GetUserId();
            switch (command)
            {
                case "SignIn":
                    return new HttpUnauthorizedResult();
                case "DeleteOrganisations":
                    DbContext.DeleteOrganisations(userId);
                    break;
                case "DeleteReturns":
                    DbContext.DeleteReturns(userId);
                    break;
                case "DeleteAccount":
                    DbContext.DeleteAccount(userId);
                    Session.Abandon();
                    Request.GetOwinContext().Authentication.SignOut();
                    break;
                case "CreateTestData":
                    CreateTestData();
                    break;
                case "ClearDatabase":
                    DbContext.Truncate();

                    if (MvcApplication.FileRepository.GetDirectoryExists(Settings.Default.DownloadsLocation))
                        MvcApplication.FileRepository.DeleteFiles(Settings.Default.DownloadsLocation);

                    //Refresh the repository
                    DataRepository = null;

                    if (User.Identity.IsAuthenticated)
                    {
                        Session.Abandon();
                        Request.GetOwinContext().Authentication.SignOut();
                    }
                    break;
            }
            return RedirectToAction("Index");
        }

        void CreateTestData(int recordCount = 500, int yearCount = 3)
        {
            var organisations = DataRepository.GetAll<Organisation>();
            var sicCodes = DataRepository.GetAll<SicCode>();
            while (organisations.Count() < recordCount)
            {
                var sector = Numeric.Rand(0, 1) == 0 ? SectorTypes.Private : SectorTypes.Public;
                string searchText = Text.AlphabetChars[Numeric.Rand(0, 1)].ToString();
                var organisation = new Organisation();
                PagedResult<EmployerRecord> result;
                if (sector == SectorTypes.Public)
                {
                    try
                    {
                        result = PublicSectorRepository.Search(searchText, Numeric.Rand(1, recordCount), 10);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (result == null || result.RowCount == 0 || result.Results == null || result.Results.Count == 0) continue;
                    foreach (var employer in result.Results)
                    {
                        if (organisations.Any(o => o.OrganisationName == employer.Name)) continue;
                        organisation = new Organisation();
                        organisation.OrganisationName = employer.Name;
                        organisation.SectorType = sector;
                        organisation.Status = OrganisationStatuses.Active;
                        organisation.StatusDetails = "TEST DATA";
                        DataRepository.Insert(organisation);
                        DataRepository.SaveChanges();

                        DataRepository.Insert(new OrganisationAddress()
                        {
                            OrganisationId = organisation.OrganisationId,
                            Address1 = "Address line " + organisation.OrganisationId,
                            Address3 = "City" + organisation.OrganisationId,
                            Country = "Country" + organisation.OrganisationId,
                            PostCode = "Post Code" + organisation.OrganisationId,
                            Status = AddressStatuses.Active
                        });

                        DataRepository.Insert(new OrganisationSicCode()
                        {
                            OrganisationId = organisation.OrganisationId,
                            SicCodeId = 1
                        });
                    }
                }
                else if (sector == SectorTypes.Private)
                {
                    try
                    {
                        result = PrivateSectorRepository.Search(searchText, Numeric.Rand(1, recordCount), 10);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (result == null || result.RowCount == 0 || result.Results == null || result.Results.Count == 0) continue;
                    foreach (var employer in result.Results)
                    {
                        if (organisations.Any(o => o.OrganisationName == employer.Name)) continue;
                        organisation = new Organisation();
                        organisation.OrganisationName = employer.Name;
                        organisation.SectorType = sector;
                        organisation.Status = OrganisationStatuses.Active;
                        organisation.StatusDetails = "TEST DATA";
                        organisation.PrivateSectorReference = employer.CompanyNumber;
                        DataRepository.Insert(organisation);
                        DataRepository.SaveChanges();

                        DataRepository.Insert(new OrganisationAddress()
                        {
                            OrganisationId = organisation.OrganisationId,
                            Address1 = employer.Address1,
                            Address2 = employer.Address2,
                            Address3 = employer.Address3,
                            Country = employer.Country,
                            PostCode = employer.PostCode,
                            PoBox = employer.PoBox,
                            Status = AddressStatuses.Active
                        });

                        employer.SicCodes = PrivateSectorRepository.GetSicCodes(employer.CompanyNumber);

                        foreach (var sicCode in employer.SicCodes.SplitI())
                        {
                            var code = sicCode.ToInt32();
                            if (!sicCodes.Any(s => s.SicCodeId == code))
                            {
                                MvcApplication.Log.WriteLine($"Invalid SIC code '{code}' received from companies house");
                                continue;
                            }
                            if (organisation.OrganisationSicCodes.Any(a => a.SicCodeId == code)) continue;
                            DataRepository.Insert(new OrganisationSicCode()
                            {
                                OrganisationId = organisation.OrganisationId,
                                SicCodeId = code
                            });
                        }
                        DataRepository.SaveChanges();
                    }
                }
            }

            foreach (var organisation in organisations.ToList())
            {
                for (var year = DateTime.Now.Year; year >= (DateTime.Now.Year - yearCount); year--)
                {
                    var returns = organisation.Returns.ToList();
                    var accountingDate = GetAccountYearStartDate(organisation.SectorType, year);
                    var @return = returns.FirstOrDefault(r => r.AccountingDate == accountingDate);
                    if (@return != null) continue;
                    DataRepository.Insert(new Return()
                    {
                        OrganisationId = organisation.OrganisationId,
                        AccountingDate = accountingDate,
                        Status = ReturnStatuses.Submitted,
                        StatusDate = accountingDate.AddDays(Numeric.Rand(0, 365)),
                        CompanyLinkToGPGInfo = "https://" + organisation.OrganisationName.ReplaceI(" ", "") + ".co.uk/GPGData",
                        DiffMeanBonusPercent = Numeric.Rand(0, 100),
                        DiffMeanHourlyPayPercent = Numeric.Rand(0, 100),
                        DiffMedianBonusPercent = Numeric.Rand(0, 100),
                        DiffMedianHourlyPercent = Numeric.Rand(0, 100),
                        FemaleLowerPayBand = Numeric.Rand(0, 100),
                        FemaleMedianBonusPayPercent = Numeric.Rand(0, 100),
                        FemaleMiddlePayBand = Numeric.Rand(0, 100),
                        FemaleUpperPayBand = Numeric.Rand(0, 100),
                        FemaleUpperQuartilePayBand = Numeric.Rand(0, 100),
                        FirstName = "Firstname" + organisation.OrganisationId,
                        LastName = "Lastname" + organisation.OrganisationId,
                        JobTitle = "Jobtitle " + organisation.OrganisationId,
                        MaleLowerPayBand = Numeric.Rand(0, 100),
                        MaleMedianBonusPayPercent = Numeric.Rand(0, 100),
                        MaleUpperQuartilePayBand = Numeric.Rand(0, 100),
                        MaleMiddlePayBand = Numeric.Rand(0, 100),
                        MaleUpperPayBand = Numeric.Rand(0, 100)
                    });
                }
            }
            DataRepository.SaveChanges();
        }
#endif
        #endregion

    }
}