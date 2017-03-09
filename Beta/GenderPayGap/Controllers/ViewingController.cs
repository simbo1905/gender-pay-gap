﻿using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using System.IO;
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
using Microsoft.Ajax.Utilities;
using Thinktecture.IdentityModel.Extensions;

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

            var newSectors = sectors.SplitI(string.Empty).ToList();

            if (search!=model.SearchText || model.Employers==null || model.Employers.RowCount==0 || model.Employers.CurrentPage != page || model.Year != year || !model.OldSectors.EqualsI(newSectors))
            {
                model.SearchText = search;

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

            string pattern = searchText?.ToLower();
            var hasSector = sectors.Any();

            //using (DataRepository.BeginTransaction(IsolationLevel.Snapshot))
            {
                IQueryable<Return> searchResults;
                if (!string.IsNullOrWhiteSpace(searchText) && hasSector)
                    searchResults = DataRepository.GetAll<Return>().Where(r =>
                        ((r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)) &&
                        r.Organisation.OrganisationName.ToLower().Contains(pattern) &&
                        r.Organisation.OrganisationSicCodes.Any(sic => sectors.Contains(sic.SicCode.SicSectionId)))
                        .OrderBy(r => r.Organisation.OrganisationName);
                else if (!string.IsNullOrWhiteSpace(searchText) && !hasSector)
                    searchResults = DataRepository.GetAll<Return>().Where(r =>
                    ((r.AccountingDate == privateAccountingDate && r.Organisation.SectorType == SectorTypes.Private) || (r.AccountingDate == publicAccountingDate && r.Organisation.SectorType == SectorTypes.Public)) && 
                    r.Organisation.OrganisationName.ToLower().Contains(pattern))
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
                result.Results = searchResults.ToList().Select(r => r.Organisation.ToEmployerRecord()).Page(pageSize,page).ToList();
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

            var oldSearchText = model.SearchText;
            if (command == "search")
            {
                model.SearchText = m.SearchText.Trim();

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

            var selectedSectors = m.NewSectors.ToDelimitedString(string.Empty);

            //If search text, sectors or page changed then redirect to search page
            if (oldSearchText != m.SearchText)
                nextPage = 1;

            if (oldSearchText != model.SearchText || nextPage != model.Employers.CurrentPage || selectedSectors != model.NewSectors.ToDelimitedString(string.Empty))
            {
                model.SearchText = oldSearchText;
                return RedirectToAction("SearchResults", new { search = m.SearchText, page=nextPage, sectors = selectedSectors });
            }

            //Otherwise show the same results
            return View("SearchResults", model);
        }
         
        [HttpGet]
        [Route("download")]
        public ActionResult Download(string year)
        {
            //Get the latest return accounting date
            var returnDates = DataRepository.GetAll<Return>().Where(r=>r.Status==ReturnStatuses.Submitted).Select(r=>r.AccountingDate).Distinct().ToList();

            //Ensure we have a directory
            var downloadsLocation = FileSystem.ExpandLocalPath(Settings.Default.DownloadsLocation);
            if (!Directory.Exists((downloadsLocation))) downloadsLocation.CreateTree();

            var directory =new DirectoryInfo(downloadsLocation);
            string filePattern;
            foreach (var returnDate in returnDates)
            {
                directory.Refresh();

                //If another server is already in process of creating a file then skip
                var tmpfilePattern = $"GPGData_{returnDate.Year}-{returnDate.Year + 1}.tmp";
                var tempfile = directory.GetFiles(tmpfilePattern).FirstOrDefault();
                if (tempfile!=null && tempfile.Exists) continue;

                filePattern = $"GPGData_{returnDate.Year}-{returnDate.Year+1}_*.csv";
                var file = directory.GetFiles(filePattern).FirstOrDefault();

                //Skip if the file already exists and is newer than 1 hour or older than 1 year
                if (file != null && (file.LastWriteTime.AddHours(1) >= DateTime.Now || file.LastWriteTime.AddYears(1) <= DateTime.Now))
                    continue;
                tempfile=new FileInfo(Path.Combine(directory.FullName,tmpfilePattern));
                try
                {
                    int count = 0;
                    using (var textWriter = tempfile.CreateText())
                    {
                        var downloadData = DataRepository.GetAll<Return>().Where(r => r.AccountingDate.Year == returnDate.Year).OrderBy(r => r.Organisation.OrganisationName).ToList();
                        var records = downloadData.Select(r => r.ToDownloadRecord());
                        using (var writer = new CsvWriter(textWriter))
                        {
                            writer.WriteRecords(records);
                        }
                        count = downloadData.Count();
                    }
                    try
                    {
                        //Delete the old file if it exists
                        if (file!=null && file.Exists) file.Delete();
                        //Generate a new file name
                        var newFilePath = Path.Combine(directory.FullName, $"GPGData_{returnDate.Year}-{returnDate.Year + 1}_{count}.csv");

                        //If the new file name exists then delete it
                        if (System.IO.File.Exists(newFilePath)) System.IO.File.Delete(newFilePath);

                        //Rename the temp file to the new filename
                        tempfile.MoveTo(Path.Combine(directory.FullName,newFilePath));
                    }
                    catch (Exception ex)
                    {
                    }
                }
                finally
                {
                    
                }
            }

            directory.Refresh();
            var model=new DownloadViewModel();
            model.Downloads=new List<DownloadViewModel.Download>();
            filePattern = $"GPGData_????-????_*.csv";
            foreach (var file in directory.GetFiles(filePattern))
            {
                var download = new DownloadViewModel.Download();

                download.Title = Path.GetFileNameWithoutExtension(file.Name).AfterFirst("GPGData_");
                download.Count = download.Title.AfterLast("_");
                download.Title = download.Title.BeforeLast("_");
                download.Extension = file.Extension.TrimI(".");
                download.Size = Numeric.FormatFileSize(file.Length);
                download.Url = Url.Action("DownloadData", new {year = download.Title.BeforeFirst("-")});
                model.Downloads.Add(download);
            }

            //Sort downloadsby descending year
            model.Downloads=model.Downloads.OrderByDescending(d=>d.Title).ToList();

            //Return the view with the model
            return View("Download",model);
        }

        [HttpGet]
        [Route("download-data")]
        public ActionResult DownloadData(int year)
        {
            //Ensure we have a directory
            var downloadsLocation = FileSystem.ExpandLocalPath(Settings.Default.DownloadsLocation);
            if (!Directory.Exists((downloadsLocation))) return new HttpNotFoundResult("There are no GPG data files");
            var directory = new DirectoryInfo(downloadsLocation);

            //Ensure we have a file
            var filePattern = $"GPGData_{year}-{year + 1}_*.csv";
            var file = directory.GetFiles(filePattern).FirstOrDefault();
            if (file==null || !file.Exists) return new HttpNotFoundResult("Cannot find GPG data file for year: " + year);
            //Get the public and private accounting dates for the specified year

            //TODO log download

            //Setup the http response
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = $"UK Gender Pay Gap Data - {year} to {year + 1}.csv",
                Inline = true,
            };
            Response.AppendHeader("Content-Disposition", contentDisposition.ToString());

            // Buffer response so that page is sent
            // after processing is complete.
            Response.BufferOutput = true;

            //Return the data
            return File(file.FullName, "text/csv");
        }

        [HttpGet]
        [Route("employer-details")]
        public ActionResult EmployerDetails(int index)
        {
            return View("EmployerDetails");
        }
    }
}
