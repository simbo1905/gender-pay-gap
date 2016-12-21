using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GenderPayGap.Models.GpgEntityModel;
using System.Security.Claims;

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
        [Authorize]
        public ActionResult GpgWizardView()
        {
            //This is how to get the UserID facebook willbe a ling string or our id will be a simple number
            
            //TODO IF the user is not in the database redirect to /Register
            //TODO If the user is not verified (clicked email verification link) redirect to /Register/Verify
            //TODO If the user is not active (confirmed via PIN) then then redirect to /Register/Confirm
            //Load the organisation for the active user
            //Find the Return for the current year
            //If no return then create a new one
            //If it has already been submitted then show a warning and submitted data
            //if the return has not been submitted then load for editing
            return View();
        }


        // GET: Return/Details/5
        public ActionResult Details(long? id = 4)
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
    }
}


