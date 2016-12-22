using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.Models.GpgEntityModel;

namespace GenderPayGap.Controllers
{
    public class QueryController : Controller
    {
        private GpgDBEntitiesContext db = new GpgDBEntitiesContext();

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
            var organisation = db.Organisation.Include(o => o.Group).Include(o => o.OrganisationAddress);
            return View(organisation.ToList());
        }

        // GET: Query/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organisation organisation = db.Organisation.Find(id);
            if (organisation == null)
            {
                return HttpNotFound();
            }
            return View(organisation);
        }

        // GET: Query/Create
        public ActionResult Create()
        {
            ViewBag.GroupId = new SelectList(db.Group, "GroupId", "GroupRef");
            ViewBag.OrganisationId = new SelectList(db.OrganisationAddress, "OrganisationID", "Type");
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
                db.Organisation.Add(organisation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GroupId = new SelectList(db.Group, "GroupId", "GroupRef", organisation.GroupId);
            ViewBag.OrganisationId = new SelectList(db.OrganisationAddress, "OrganisationID", "Type", organisation.OrganisationId);
            return View(organisation);
        }

        // GET: Query/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organisation organisation = db.Organisation.Find(id);
            if (organisation == null)
            {
                return HttpNotFound();
            }
            ViewBag.GroupId = new SelectList(db.Group, "GroupId", "GroupRef", organisation.GroupId);
            ViewBag.OrganisationId = new SelectList(db.OrganisationAddress, "OrganisationID", "Type", organisation.OrganisationId);
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
                db.Entry(organisation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.GroupId = new SelectList(db.Group, "GroupId", "GroupRef", organisation.GroupId);
            ViewBag.OrganisationId = new SelectList(db.OrganisationAddress, "OrganisationID", "Type", organisation.OrganisationId);
            return View(organisation);
        }

        // GET: Query/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organisation organisation = db.Organisation.Find(id);
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
            Organisation organisation = db.Organisation.Find(id);
            db.Organisation.Remove(organisation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
