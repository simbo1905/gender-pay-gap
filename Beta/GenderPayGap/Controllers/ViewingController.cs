using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using GenderPayGap.Models.SqlDatabase;
using Autofac;
using Castle.Components.DictionaryAdapter;
using CsvHelper;
using Extensions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Properties;
using IsolationLevel = System.Data.IsolationLevel;
using GenderPayGap.Core.Classes;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Models.Register;

namespace GenderPayGap.WebUI.Controllers
{
    [RoutePrefix("Viewing")]
    [Route("{action}")]
    public class ViewingController : BaseController
    {

        #region Initialisation
        public ViewingController():base(){ }
        public ViewingController(IContainer container): base(container){ }

        /// <summary>
        /// This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [Route("Init")]
        public ActionResult Init()
        {
#if DEBUG
            MvcApplication.Log.WriteLine("Viewing Controller Initialised");
#endif
            return new EmptyResult();
        }
        #endregion

        [Route]
        public ActionResult Redirect()
        {
            return RedirectToAction("SearchResults");
        }

        [HttpGet]
        [Route("search-results")]

        public ActionResult SearchResults(string search = null,int year=0, int page=1,string sectors=null)
        {
            var model = this.UnstashModel<SearchViewModel>() ?? new SearchViewModel();

            //Make sure we know all the sic sectors
            if (model.AllSectors == null) model.AllSectors = DataRepository.GetAll<SicSection>().OrderBy(s => s.SicSectionId);

            foreach (var sector in model.AllSectors)
                sector.Description = sector.Description.BeforeFirst(";");

            var newSectors = sectors.SplitI().ToList();

            if (model.Employers==null || model.Employers.CurrentPage != page || model.Year != year || !model.OldSectors.EqualsI(newSectors))
            {
                //Make sure we can load employers from session
                model.Employers = Search(search, newSectors.EqualsI(model.AllSectors) ? new List<string>() : newSectors, page, Settings.Default.EmployerPageSize, year);

                model.OldSectors = model.NewSectors?.ToList();
                model.NewSectors = newSectors;

                model.SectorSources=new SelectList(model.AllSectors,nameof(SicSection.SicSectionId),nameof(SicSection.Description));
                this.StashModel(model);
            }

            return View("SearchResults", model);
        }

        public PagedResult<EmployerRecord> Search(string searchText, List<string> sectors=null, int page=-1, int pageSize=-1, int year=-1)
        {

            //Get the public and private accounting dates for the specified or current year 
            var publicAccountingDate = GetAccountYearStartDate(SectorTypes.Public, year);
            var privateAccountingDate = GetAccountYearStartDate(SectorTypes.Private, year);

            var result = new PagedResult<EmployerRecord>
            {
                RowCount = 0,
                Results = new List<EmployerRecord>()
            };

            string pattern = $"%{searchText}%";
            var hasSector = sectors.Any();

            //using (DataRepository.BeginTransaction(IsolationLevel.Snapshot))
            {
                IQueryable<Return> searchResults;
                if (!string.IsNullOrWhiteSpace(searchText) && hasSector)
                    searchResults = DataRepository.GetAll<Return>().Where(r =>
                        ((r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)) &&
                        r.Organisation.OrganisationName.Like(pattern) &&
                        r.Organisation.OrganisationSicCodes.Any(sic => sectors.Contains(sic.SicCode.SicSectionId)))
                        .OrderBy(r => r.Organisation.OrganisationName);
                else if (!string.IsNullOrWhiteSpace(searchText) && !hasSector)
                    searchResults = DataRepository.GetAll<Return>().Where(r =>
                        ((r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)) &&
                        r.Organisation.OrganisationName.Like(pattern))
                        .OrderBy(r => r.Organisation.OrganisationName);
                else if (string.IsNullOrWhiteSpace(searchText) && hasSector)
                    searchResults = DataRepository.GetAll<Return>().Where(r =>
                        ((r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)) &&
                        r.Organisation.OrganisationSicCodes.Any(sic => sectors.Contains(sic.SicCode.SicSectionId)))
                        .OrderBy(r => r.Organisation.OrganisationName);
                else
                    searchResults = DataRepository.GetAll<Return>().Where(r =>
                        (r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public))
                        .OrderBy(r => r.Organisation.OrganisationName);

                result.RowCount = searchResults.Count();
                result.Results = searchResults.ToList().Select(r => r.Organisation.ToEmployerRecord()).ToList();
            }

            result.CurrentPage = page;
            result.PageSize = pageSize;
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("search-results")]
        public ActionResult SearchResults(SearchViewModel m, string command)
        {
            //Make sure we can load employers from session
            var model = this.UnstashModel<SearchViewModel>();
            if (model == null) return View("CustomError", new ErrorViewModel(1118));
            
            var nextPage = model.Employers.CurrentPage;

            if (command == "search")
            {
                m.SearchText = m.SearchText.Trim();

                if (string.IsNullOrWhiteSpace(model.SearchText))
                {
                    AddModelError(3019, "SearchText");
                    this.CleanModelErrors<SearchViewModel>();
                    return View("SearchResults", model);
                }
                if (model.SearchText.Length < 3 || model.SearchText.Length > 100)
                {
                    AddModelError(3007, "SearchText");
                    this.CleanModelErrors<SearchViewModel>();
                    return View("SearchResults", model);
                }
            }
            else if (command == "pageNext")
            {
                if (nextPage >= model.Employers.PageCount)
                    throw new Exception("Cannot go past last page");
                nextPage++;
            }
            else if (command == "pagePrev")
            {
                if (nextPage <= 1)
                    throw new Exception("Cannot go before previous page");
                nextPage--;
            }
            else if (command.StartsWithI("page_"))
            {
                var page = command.AfterFirst("page_").ToInt32();
                if (page < 1 || page > model.Employers.PageCount)
                    throw new Exception("Invalid page selected");

                if (page != nextPage)
                {
                    nextPage = page;
                }
            }

            ModelState.Clear();

            var selectedSectors = model.SectorSources.SelectedValues.ToString();

            //If search text, sectors or page changed then redirect to search page
            if (m.SearchText!=model.SearchText || nextPage!=model.Employers.CurrentPage || selectedSectors!=model.NewSectors.ToDelimitedString())
                return RedirectToAction("SearchResults", new { searchText=model.SearchText, page=nextPage, sectors = selectedSectors });

            //Otherwise show the same results
            return View("SearchResults", model);
        }
         
        [HttpGet]
        [Route("download")]
        public ActionResult Download(string year)
        {
            return View("Download");
        }

        [HttpGet]
        [Route("download-data")]
        public ActionResult DownloadData(int year)
        {
            //Get the public and private accounting dates for the specified year
            var publicAccountingDate = GetAccountYearStartDate(SectorTypes.Public, year);
            var privateAccountingDate = GetAccountYearStartDate(SectorTypes.Private, year);

            //TODO log download

            //Setup the http response
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = $"UK GenderPayGap Data - {year} to {year + 1}.csv",
                Inline = true,
            };
            Response.AppendHeader("Content-Disposition", contentDisposition.ToString());

            // Buffer response so that page is sent
            // after processing is complete.
            Response.BufferOutput = true;

            //Only use a snapshot of the data
            using (DataRepository.BeginTransaction(IsolationLevel.Snapshot))
            {
                //Write to the response output
                using (var writer = new CsvWriter(Response.Output))
                {
                    //Write all the records for the specified year
                    writer.WriteRecords(DataRepository.GetAll<Return>().Where(r => (r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)).OrderBy(r => r.Organisation.OrganisationName));        
                }
            }

            //Return the data
            return File(Response.OutputStream, "text/csv");
        }

        [HttpGet]
        [Route("employer-details")]
        public ActionResult EmployerDetails(int index)
        {
            return View("EmployerDetails");
        }
    }
}
