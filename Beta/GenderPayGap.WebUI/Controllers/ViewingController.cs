using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.Models.SqlDatabase;
using Autofac;
using CsvHelper;
using Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Properties;
using GenderPayGap.Core.Classes;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Models.Submit;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Viewing")]
    [Route("{action}")]
    public class ViewingController : BaseController
    {

        #region Initialisation

        public ViewingController() : base()
        {
            
        }
        public ViewingController(IContainer container) : base(container) { }

        /// <summary>
        /// This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [Route("Init")]
        public ActionResult Init()
        {
#if DEBUG
            MvcApplication.InfoLog.WriteLine("Viewing Controller Initialised");
#endif
            return new EmptyResult();
        }
        #endregion

        public string LastSearch
        {
            get { return Session["LastSearch"] as string; }
            set { Session["LastSearch"] = value; }
        }

        [Route]
        [OutputCache(CacheProfile = "RedirectIndex")]
        public ActionResult Redirect()
        {
            return RedirectToAction("SearchResults");
        }

        [HttpGet]
        [Route("search-results")]
        [OutputCache(CacheProfile = "SearchResults")]
        public ActionResult SearchResults(string search = null, IEnumerable<string> s = null, int p = 1, int z = 0, int y = 0)
        {
            var model = new SearchViewModel();

            //Make sure we know all the sic sectors
            var list = new List<SearchViewModel.SicSection>();
            foreach (var sector in DataRepository.GetAll<SicSection>().OrderBy(sic => sic.SicSectionId))
            {
                list.Add(new SearchViewModel.SicSection()
                {
                    SicSectionId = sector.SicSectionId,
                    Description = sector.Description = sector.Description.BeforeFirst(";")
                });
            }
            model.AllSectors = list.AsEnumerable();

            var newSectors = s.ToDelimitedString(string.Empty);
            if (z < 1) z = Settings.Default.EmployerPageSize;
            if (y < 1) y = DateTime.Now.Year;
            if (search != model.search || model.Employers == null || model.Employers.RowCount == 0 || model.Employers.CurrentPage != p || model.y != y || (model.s==null || !model.s.ToDelimitedString(string.Empty).EqualsI(newSectors)))
            {
                //Make sure we can load employers from session
                var searchSectors = s?.Where(sc=>!string.IsNullOrWhiteSpace(sc)).ToList();
                var allSectors = model.AllSectors.Select(sic => sic.SicSectionId).ToDelimitedString(string.Empty);
                if (newSectors.EqualsI(allSectors)) searchSectors = new List<string>();
                model.Employers = Search(search, searchSectors, p, Settings.Default.EmployerPageSize, y);
                model.search = search;
                model.p = p;
                model.z = z;
                model.y = y;
                model.s = s;
                LastSearch = Request.Url.PathAndQuery;
                
                var sources = new List<Core.Classes.SelectedItem>();
                foreach (var sector in model.AllSectors)
                {
                    sources.Add(new SelectedItem()
                    {
                         Key = sector.SicSectionId,
                         Text = sector.Description,
                         Value=sector.SicSectionId
                    });
                }
                model.SectorSources = sources;
            }

            return View("SearchResults", model);
        }

        private PagedResult<EmployerRecord> Search(string searchText, List<string> sectors = null, int page = -1, int pageSize = -1, int year = -1)
        {

            //Get the public and private accounting dates for the specified or current year 
            var publicAccountingDate = GetAccountYearStartDate(SectorTypes.Public, year);
            var privateAccountingDate = GetAccountYearStartDate(SectorTypes.Private, year);

            var result = new PagedResult<EmployerRecord>
            {
                RowCount = 0,
                Results = new List<EmployerRecord>()
            };

            string pattern = searchText?.ToLower();
            var hasSector = sectors!=null && sectors.Any();

            
            //using (DataRepository.BeginTransaction(IsolationLevel.Snapshot))
            {
                IQueryable<Return> searchResults;
                if (!string.IsNullOrWhiteSpace(searchText) && hasSector)
                    searchResults = DataRepository.GetAll<Return>().Where(r => r.Status==ReturnStatuses.Submitted && r.Organisation.Status == OrganisationStatuses.Active &&
                        ((r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)) &&
                        r.Organisation.OrganisationName.ToLower().Contains(pattern) &&
                        r.Organisation.OrganisationSicCodes.Any(sic => sectors.Contains(sic.SicCode.SicSectionId)))
                        .OrderBy(r => r.Organisation.OrganisationName);
                else if (!string.IsNullOrWhiteSpace(searchText) && !hasSector)
                    searchResults = DataRepository.GetAll<Return>().Where(r => r.Status == ReturnStatuses.Submitted && r.Organisation.Status == OrganisationStatuses.Active &&
                    ((r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)) &&
                    r.Organisation.OrganisationName.ToLower().Contains(pattern))
                    .OrderBy(r => r.Organisation.OrganisationName);
                else if (string.IsNullOrWhiteSpace(searchText) && hasSector)
                    searchResults = DataRepository.GetAll<Return>().Where(r => r.Status == ReturnStatuses.Submitted && r.Organisation.Status == OrganisationStatuses.Active &&
                        ((r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)) &&
                        r.Organisation.OrganisationSicCodes.Any(sic => sectors.Contains(sic.SicCode.SicSectionId)))
                        .OrderBy(r => r.Organisation.OrganisationName);
                else
                    searchResults = DataRepository.GetAll<Return>().Where(r => r.Status == ReturnStatuses.Submitted && r.Organisation.Status == OrganisationStatuses.Active &&
                        (r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public))
                        .OrderBy(r => r.Organisation.OrganisationName);

                result.RowCount = searchResults.Count();
                
                //Pick a random page if none specified
                if (page==0)
                    page = 1;
                else if (page < 0)
                    page = Numeric.Rand(1, result.PageCount);

                result.Results = searchResults.ToList().Select(r => r.Organisation.ToEmployerRecord()).Page(pageSize, page).ToList();
            }

            result.CurrentPage = page;
            result.PageSize = pageSize;
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            return result;
        }


        [HttpGet]
        [Route("download")]
        [OutputCache(CacheProfile = "Download")]
        public ActionResult Download()
        {
            //Get the latest return accounting date
            var returnYears = DataRepository.GetAll<Return>().Where(r => r.Status == ReturnStatuses.Submitted).Select(r => r.AccountingDate.Year).Distinct().ToList();

            //Ensure we have a directory
            if (!MvcApplication.FileRepository.GetDirectoryExists(Settings.Default.DownloadsLocation)) MvcApplication.FileRepository.CreateDirectory(Settings.Default.DownloadsLocation);

            string filePattern;
            foreach (var year in returnYears)
            {
                //If another server is already in process of creating a file then skip

                filePattern = $"GPGData_{year}-{year + 1}_*.csv";
                var file = MvcApplication.FileRepository.GetFiles(Settings.Default.DownloadsLocation,filePattern).FirstOrDefault();

                //Skip if the file already exists and is newer than 1 hour or older than 1 year
                if (file != null)
                {
                    var lastWriteTime = MvcApplication.FileRepository.GetLastWriteTime(file);
                    if (lastWriteTime.AddHours(1) >= DateTime.Now || lastWriteTime.AddYears(1) <= DateTime.Now)
                        continue;
                }

                var tempfile = new FileInfo(System.IO.Path.GetTempFileName());
                try
                {
                    var count = 0;
                    using (var textWriter = tempfile.CreateText())
                    {
                        var downloadData = DataRepository.GetAll<Return>().Where(r => r.AccountingDate.Year == year).OrderBy(r => r.Organisation.OrganisationName).ToList();
                        var records = downloadData.Where(r=> !r.Organisation.OrganisationName.StartsWithI(MvcApplication.TestPrefix)).Select(r => r.ToDownloadRecord());
                        using (var writer = new CsvWriter(textWriter))
                        {
                            writer.WriteRecords(records);
                        }
                        count = downloadData.Count;
                    }

                    try
                    {
                        //Generate a new file name
                        var newFilePath = MvcApplication.FileRepository.GetFullPath(Path.Combine(Settings.Default.DownloadsLocation,$"GPGData_{year}-{year + 1}_{count}.csv"));
                        MvcApplication.FileRepository.Write(newFilePath,tempfile);

                        //Delete the old file if it exists
                        if (file != null && MvcApplication.FileRepository.GetFileExists(file) && !file.EqualsI(newFilePath))
                            MvcApplication.FileRepository.DeleteFile(file);
                    }
                    catch (Exception ex)
                    {
                        MvcApplication.ErrorLog.WriteLine(ex.Message);
                    }
                }
                finally
                {
                    System.IO.File.Delete(tempfile.FullName);
                }
            }

            var model = new DownloadViewModel();
            model.Downloads = new List<DownloadViewModel.Download>();
            filePattern = $"GPGData_????-????_*.csv";
            foreach (var file in MvcApplication.FileRepository.GetFiles(Settings.Default.DownloadsLocation,filePattern))
            {
                var download = new DownloadViewModel.Download();

                download.Title = Path.GetFileNameWithoutExtension(file).AfterFirst("GPGData_");
                download.Count = download.Title.AfterLast("_");
                download.Title = download.Title.BeforeLast("_");
                download.Extension = Path.GetExtension(file).TrimI(".");
                download.Size = Numeric.FormatFileSize(MvcApplication.FileRepository.GetFileSize(file));
                download.Url = Url.Action("DownloadData", new { year = download.Title.BeforeFirst("-") });
                model.Downloads.Add(download);
            }

            //Sort downloadsby descending year
            model.Downloads = model.Downloads.OrderByDescending(d => d.Title).ToList();

            //Return the view with the model
            return View("Download", model);
        }

        [HttpGet]
        [Route("download-data")]
        [OutputCache(CacheProfile = "DownloadData")]
        public ActionResult DownloadData(int year=0)
        {
            if (year == 0)
            {
                var ret= DataRepository.GetAll<Return>().OrderBy(a=>Guid.NewGuid()).FirstOrDefault(r => r.Status == ReturnStatuses.Submitted);
                if (ret != null) year = ret.AccountingDate.Year;
            }

            //Ensure we have a directory
            if (!MvcApplication.FileRepository.GetDirectoryExists(Settings.Default.DownloadsLocation)) return new HttpNotFoundResult("There are no GPG data files");

            //Ensure we have a file
            var filePattern = $"GPGData_{year}-{year + 1}_*.csv";
            var file = MvcApplication.FileRepository.GetFiles(Settings.Default.DownloadsLocation,filePattern).FirstOrDefault();
            if (file == null || !MvcApplication.FileRepository.GetFileExists(file)) return new HttpNotFoundResult("Cannot find GPG data file for year: " + year);
            //Get the public and private accounting dates for the specified year

            //TODO log download

            //Setup the http response
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = $"UK Gender Pay Gap Data - {year} to {year + 1}.csv",
                Inline = true,
            };
            Response.AppendHeader("Content-Disposition", contentDisposition.ToString());

            //cache old files for 1 day
            if (MvcApplication.FileRepository.GetLastWriteTime(file).AddMonths(12)<DateTime.Now)Response.Cache.SetExpires(DateTime.Now.AddDays(1));

            // Buffer response so that page is sent
            // after processing is complete.
            Response.BufferOutput = true;

            //Return the data
            return Content(MvcApplication.FileRepository.Read(file), "text/csv");
        }

        [HttpGet]
        [Route("employer-details")]
        [OutputCache(CacheProfile = "EmployerDetails")]
        public ActionResult EmployerDetails(string id=null, string view=null)
        {
            //Make sure we have a view
            if (string.IsNullOrWhiteSpace(view))
                view = "hourly-rate";
            else
                view = view.ToLower();

            Organisation org=null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                long orgId = 0;
                if (id.EqualsI("-1", "random", "rand"))
                    org = DataRepository.GetAll<Organisation>().OrderBy(a => Guid.NewGuid()).FirstOrDefault();
                else
                {
                    try
                    {
                        id = Encryption.DecryptQuerystring(id);
                        orgId = id.ToInt64();
                    }
                    catch (Exception ex)
                    {
                        MvcApplication.ErrorLog.WriteLine("Cannot decrypt organisation id from querystring");
                        return View("CustomError", new ErrorViewModel(400));
                    }

                    if (orgId > 0) org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == orgId);
                }
            }
            else if (User.Identity.IsAuthenticated)
            {
                //Do not cache page 
       //         Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
        //        Response.Cache.SetValidUntilExpires(false);
         //       Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
           //     Response.Cache.SetCacheability(HttpCacheability.NoCache);
          //      Response.Cache.SetNoStore();

                //TODO Load the current users details
                var currentUser = DataRepository.FindUser(User);
                if (currentUser != null)
                {
                    var userOrg = DataRepository.GetUserOrg(currentUser);
                    if (userOrg!=null)org = userOrg.Organisation;
                }
            }

            if (org==null)return RedirectToAction("SearchResults");

            var expectStartDate = GetAccountYearStartDate(org.SectorType);

            var @return = DataRepository.GetAll<Return>().OrderByDescending
                (r => r.AccountingDate).FirstOrDefault(r => r.OrganisationId == org.OrganisationId && r.AccountingDate == expectStartDate && r.Status == ReturnStatuses.Submitted);

            if (@return== null)
            {
                //TODO
                return RedirectToAction("SearchResults");
            }

            var model = new ReturnViewModel();
            model.SectorType = org.SectorType;
            model.ReturnId = @return.ReturnId;
            model.OrganisationId = @return.OrganisationId;
            model.DiffMeanBonusPercent = @return.DiffMeanBonusPercent;
            model.DiffMeanHourlyPayPercent = @return.DiffMeanHourlyPayPercent;
            model.DiffMedianBonusPercent = @return.DiffMedianBonusPercent;
            model.DiffMedianHourlyPercent = @return.DiffMedianHourlyPercent;
            model.FemaleLowerPayBand = @return.FemaleLowerPayBand;
            model.FemaleMedianBonusPayPercent = @return.FemaleMedianBonusPayPercent;
            model.FemaleMiddlePayBand = @return.FemaleMiddlePayBand;
            model.FemaleUpperPayBand = @return.FemaleUpperPayBand;
            model.FemaleUpperQuartilePayBand = @return.FemaleUpperQuartilePayBand;
            model.MaleLowerPayBand = @return.MaleLowerPayBand;
            model.MaleMedianBonusPayPercent = @return.MaleMedianBonusPayPercent;
            model.MaleMiddlePayBand = @return.MaleMiddlePayBand;
            model.MaleUpperPayBand = @return.MaleUpperPayBand;
            model.MaleUpperQuartilePayBand = @return.MaleUpperQuartilePayBand;
            model.JobTitle = @return.JobTitle;
            model.FirstName = @return.FirstName;
            model.LastName = @return.LastName;
            model.CompanyLinkToGPGInfo = @return.CompanyLinkToGPGInfo;
            model.AccountingDate = @return.AccountingDate;
            model.Address = org.ActiveAddress.GetAddress();
            model.OrganisationName = org.OrganisationName;
            model.Sector = org.GetSicSectors(",<br/>");
            model.ReturnUrl = string.IsNullOrWhiteSpace(id) ? null : LastSearch;
            switch (view)
            {
                default:
                    return View("HourlyRate", model);
                case "pay-quartiles":
                    return View("PayQuartiles", model);
                case "bonus-pay":
                    return View("BonusPay", model);
            }
        }
    }
}
