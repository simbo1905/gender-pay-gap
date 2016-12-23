using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.Models.GpgDatabase;

namespace GenderPayGap.Controllers
{
    public class QueryController : Controller
    {
        

        public ActionResult Start()
        {
            return View();
        }

        public ActionResult Search()
        {
            return View();
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
            // var organisation = GpgDatabase.Default.Organisation.Include(o => o).Include(o => o.OrganisationAddress);
            //return View(organisation.ToList());
            return View();
        }

        // GET: Query/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organisation organisation = GpgDatabase.Default.Organisation.Find(id);
            if (organisation == null)
            {
                return HttpNotFound();
            }
            return View(organisation);
        }

        // GET: Query/Create
        public ActionResult Create()
        {
            ViewBag.OrganisationId = new SelectList(GpgDatabase.Default.OrganisationAddress, "OrganisationID", "Type");
            return View();
        }

        // POST: Query/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrganisationId,GroupId,URN,OrganisationName,OrganisationType,OrganisationPhase,OrganisationPolicy,OrganisationDescription,Capacity,Population,Phone,Email,Web,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified")] Organisation organisation)
        {
            if (ModelState.IsValid)
            {
                GpgDatabase.Default.Organisation.Add(organisation);
                GpgDatabase.Default.SaveChanges();
                return RedirectToAction("Index");
            }

            
            ViewBag.OrganisationId = new SelectList(GpgDatabase.Default.OrganisationAddress, "OrganisationID", "Type", organisation.OrganisationId);
            return View(organisation);
        }

        // GET: Query/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organisation organisation = GpgDatabase.Default.Organisation.Find(id);
            if (organisation == null)
            {
                return HttpNotFound();
            }
           
            ViewBag.OrganisationId = new SelectList(GpgDatabase.Default.OrganisationAddress, "OrganisationID", "Type", organisation.OrganisationId);
            return View(organisation);
        }

        // POST: Query/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrganisationId,GroupId,URN,OrganisationName,OrganisationType,OrganisationPhase,OrganisationPolicy,OrganisationDescription,Capacity,Population,Phone,Email,Web,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified")] Organisation organisation)
        {
            if (ModelState.IsValid)
            {
                GpgDatabase.Default.Entry(organisation).State = EntityState.Modified;
                GpgDatabase.Default.SaveChanges();
                return RedirectToAction("Index");
            }
            
            ViewBag.OrganisationId = new SelectList(GpgDatabase.Default.OrganisationAddress, "OrganisationID", "Type", organisation.OrganisationId);
            return View(organisation);
        }

        // GET: Query/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organisation organisation = GpgDatabase.Default.Organisation.Find(id);
            if (organisation == null)
            {
                return HttpNotFound();
            }
            return View(organisation);
        }

        // POST: Query/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Organisation organisation = GpgDatabase.Default.Organisation.Find(id);
            GpgDatabase.Default.Organisation.Remove(organisation);
            GpgDatabase.Default.SaveChanges();
            return RedirectToAction("Index");
        }

       
    }
}
