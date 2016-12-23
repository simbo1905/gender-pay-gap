﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using GenderPayGap.Models.GpgDatabase;
using Thinktecture.IdentityModel.Mvc;

namespace GenderPayGap.Controllers
{
    [Authorize]
    public class ReturnController : BaseController
    {

        // GET: Return
        public ActionResult Index()
        {

            if (!Authorise()) return RedirectToAction("Index", "Register");

            return View(/*GpgDatabase.Default.Return.ToList()*/);
        }


        //Get: Return
        public ActionResult GpgWizardView()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Details([Bind(Include = "ReturnId,DiffMeanHourlyPayPercent,DiffMedianHourlyPercent,DiffMeanBonusPercent,DiffMedianBonusPercent,MaleMedianBonusPayPercent,FemaleMedianBonusPayPercent,MaleLowerPayBand,FemaleLowerPayBand,MaleMiddlePayBand,FemaleMiddlePayBand,MaleUpperPayBand,FemaleUpperPayBand,MaleUpperQuartilePayBand,FemaleUpperQuartilePayBand,CompanyLinkToGPGInfo,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified,JobTitle,FirstName,LastName")] Return @return)
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
            Return @return = GpgDatabase.Default.Return.Find(id);
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
        public ActionResult Create([Bind(Include = "ReturnId,DiffMeanHourlyPayPercent,DiffMedianHourlyPercent,DiffMeanBonusPercent,DiffMedianBonusPercent,MaleMedianBonusPayPercent,FemaleMedianBonusPayPercent,MaleLowerPayBand,FemaleLowerPayBand,MaleMiddlePayBand,FemaleMiddlePayBand,MaleUpperPayBand,FemaleUpperPayBand,MaleUpperQuartilePayBand,FemaleUpperQuartilePayBand,CompanyLinkToGPGInfo,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified,JobTitle,FirstName,LastName")] Return @return)
        {
            if (ModelState.IsValid)
            {
                GpgDatabase.Default.Return.Add(@return);
                GpgDatabase.Default.SaveChanges();
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
            Return @return = GpgDatabase.Default.Return.Find(id);
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
        public ActionResult Edit([Bind(Include = "ReturnId,DiffMeanHourlyPayPercent,DiffMedianHourlyPercent,DiffMeanBonusPercent,DiffMedianBonusPercent,MaleMedianBonusPayPercent,FemaleMedianBonusPayPercent,MaleLowerPayBand,FemaleLowerPayBand,MaleMiddlePayBand,FemaleMiddlePayBand,MaleUpperPayBand,FemaleUpperPayBand,MaleUpperQuartilePayBand,FemaleUpperQuartilePayBand,CompanyLinkToGPGInfo,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified,JobTitle,FirstName,LastName")] Return @return)
        {
            if (ModelState.IsValid)
            {
                GpgDatabase.Default.Entry(@return).State = EntityState.Modified;
                GpgDatabase.Default.SaveChanges();
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
            Return @return = GpgDatabase.Default.Return.Find(id);
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
            Return @return = GpgDatabase.Default.Return.Find(id);
            GpgDatabase.Default.Return.Remove(@return);
            GpgDatabase.Default.SaveChanges();
            return RedirectToAction("Index");
        }

        
        //GET
        public ActionResult PersonRespCreate()
        {
            return View();
        }

        // POST: Return/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PersonRespCreate([Bind(Include = "ReturnId,DiffMeanHourlyPayPercent,DiffMedianHourlyPercent,DiffMeanBonusPercent,DiffMedianBonusPercent,MaleMedianBonusPayPercent,FemaleMedianBonusPayPercent,MaleLowerPayBand,FemaleLowerPayBand,MaleMiddlePayBand,FemaleMiddlePayBand,MaleUpperPayBand,FemaleUpperPayBand,MaleUpperQuartilePayBand,FemaleUpperQuartilePayBand,CompanyLinkToGPGInfo,CurrentStatus,CurrentStatusDate,CurrentStatusDetails,Created,Modified,JobTitle,FirstName,LastName")] Return @return)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    GpgDatabase.Default.Return.Add(@return);
                    GpgDatabase.Default.SaveChanges();
                    // return RedirectToAction("Index");
                }

                return View(@return);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
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
