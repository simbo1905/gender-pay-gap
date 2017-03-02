using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.Models.SqlDatabase;
using GenderPayGap.WebUI.Models;
using Extensions;
using Autofac;

using GenderPayGap;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Controllers
{
    public class QueryController : BaseController
    {
        public QueryController():base(){ }
    public QueryController(IContainer container): base(container){ }

    public ActionResult Start()
        {
            return View();
        }

        public ActionResult Search(string query = null)
        {
            var model = new SearchViewModel();
            if (!string.IsNullOrWhiteSpace(query))
            {
                //model.Results = GpgDatabase.Default.Organisation.Where(o => o.OrganisationName.ToLower().Contains(query.ToLower())).ToArray();
                model.Results = DataRepository.GetAll<Organisation>().Where(o => o.OrganisationName.ToLower().Contains(query.ToLower())).ToArray();

                //var x = model.Search;
                //model.Results = GpgDatabase.Default.Organisation.Where(o => o.OrganisationName.ToLower().Contains(model.Search.ToLower())).ToArray();
                //model.Results = GpgDatabase.Default.Organisation.Where(p => p.OrganisationName.ToLower() == model.Search.ToLower()).ToArray();
                //model.Results = (from o in GpgDatabase.Default.Organisation
                //                 where (o.OrganisationName.ContainsI(model.Search))
                //                 orderby o.OrganisationName
                //                 select o).ToArray();

            }


            return View(model);
        }

        [HttpPost]
        public ActionResult Search(SearchViewModel model)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(model.Search))
            {
                return RedirectToAction("Search", new { query = model.Search });
            }
            else if (ModelState.IsValid && string.IsNullOrWhiteSpace(model.Search))
            {
                model.Results = DataRepository.GetAll<Organisation>().Select(o => o).ToArray();
            }

            this.CleanModelErrors<SearchViewModel>();
            return View(model);
        }
         

        public ActionResult Sectors()
        {
            return View();
        }

        public ActionResult Download()
        {
            return View();
        }


        // GET: Query
        public ActionResult Index()
        {
            return View("Search");
        }

        // GET: Query/Details/5
        public ActionResult Details(int id)
        {
            return View("Search");
        }

        // GET: Query/Create
        public ActionResult Create()
        {
            return View("Search");
        }

        // POST: Query/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Search");
            }
        }

        // GET: Query/Edit/5
        public ActionResult Edit(int id)
        {
            return View("Search");
        }

        // POST: Query/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Search");
            }
        }

        // GET: Query/Delete/5
        public ActionResult Delete(int id)
        {
            return View("Search");
        }

        // POST: Query/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Search");
            }
        }
    }
}
