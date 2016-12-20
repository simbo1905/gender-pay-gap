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
    public class ReturnController : Controller
    {
        private GpgDBEntitiesContext db = new GpgDBEntitiesContext();

        // GET: Return
        public ActionResult Index()
        {
            return View();
        }

        //Get: Return
        public ActionResult GpgWizardView()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Details(/*[Bind(Include = "ReturnId,DiffMeanHourlyPayPercent,DiffMedianHourlyPercent,DiffMeanBonusPercent,DiffMedianBonusPercent,MaleMedianBonusPayPercent,FemaleMedianBonusPayPercent,MaleLowerPayBand,FemaleLowerPayBand,MaleMiddlePayBand,FemaleMiddlePayBand,MaleUpperPayBand,FemaleUpperPayBand,MaleUpperQuartilePayBand,FemaleUpperQuartilePayBand,CompanyLinkToGPGInfo,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified")]*/ Return @return)
        {
            //Create(@return);

            //return View();

            return RedirectToAction("SendConfirmed");
            
        }

        // GET: Return/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Return @return = db.Return.Find(id);
            if (@return == null)
            {
                return HttpNotFound();
            }
            return View(@return);
        }

       
        // GET: Return/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Return/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReturnId,DiffMeanHourlyPayPercent,DiffMedianHourlyPercent,DiffMeanBonusPercent,DiffMedianBonusPercent,MaleMedianBonusPayPercent,FemaleMedianBonusPayPercent,MaleLowerPayBand,FemaleLowerPayBand,MaleMiddlePayBand,FemaleMiddlePayBand,MaleUpperPayBand,FemaleUpperPayBand,MaleUpperQuartilePayBand,FemaleUpperQuartilePayBand,CompanyLinkToGPGInfo,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified")] Return @return)
        {
            if (ModelState.IsValid)
            {
                db.Return.Add(@return);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(@return);
        }

        // GET: Return/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Return @return = db.Return.Find(id);
            if (@return == null)
            {
                return HttpNotFound();
            }
            return View(@return);
        }

        // POST: Return/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReturnId,DiffMeanHourlyPayPercent,DiffMedianHourlyPercent,DiffMeanBonusPercent,DiffMedianBonusPercent,MaleMedianBonusPayPercent,FemaleMedianBonusPayPercent,MaleLowerPayBand,FemaleLowerPayBand,MaleMiddlePayBand,FemaleMiddlePayBand,MaleUpperPayBand,FemaleUpperPayBand,MaleUpperQuartilePayBand,FemaleUpperQuartilePayBand,CompanyLinkToGPGInfo,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified")] Return @return)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@return).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(@return);
        }

        // GET: Return/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Return @return = db.Return.Find(id);
            if (@return == null)
            {
                return HttpNotFound();
            }
            return View(@return);
        }

        // POST: Return/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Return @return = db.Return.Find(id);
            db.Return.Remove(@return);
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


       //GET
        public ActionResult PersonRespCreate()
        {
            return View();
        }

        //GET
        public ActionResult PersonRespEdit()
        {
            return View();
        }

        public ActionResult SendConfirmed()
        {
            return View();
        }
       
    }
}


