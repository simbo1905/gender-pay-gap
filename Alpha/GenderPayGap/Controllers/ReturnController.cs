using GenderPayGap.Models;
using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GenderPayGap.Controllers
{
    public class ReturnController : Controller
    {

        //Get: Return
        public ActionResult Submit()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Submit(Return @return)
        {
            int count = 2;
            try
            {
                // TODO: Add insert logic here
                @return.Organisation = new Organisation
                {
                    OrganisationId = ++count,
                    OrganisationName = "testOrg2" + count.ToString()
                };

                
                GpgDatabase.Default.Return.Add(@return);
                GpgDatabase.Default.SaveChanges();
                @return.AccountingDate = DateTime.Now;

                return RedirectToAction("SendConfirmed");
            }
            catch (Exception e)
            {
                return View(e);
            }
        }

        public ActionResult SendConfirmed()
        {
            return View();
        }

        // GET: Return/Details/5
        public ActionResult DataConfirm(int id)
        {
            var qid = GpgDatabase.Default.Return.Find(id);
            return View(qid);
        }

        public ActionResult Details(int id)
        {
            var qid = GpgDatabase.Default.Return.Find(id);
            return View(qid);
        }

        //// GET: Return/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}


        ////GET
        //public ActionResult PersonResponsibleCreate()
        //{
        //    return View();
        //}

        //public ActionResult SendConfirmed()
        //{
        //    return View();
        //}



        //// GET: Return
        ////public ActionResult Index()
        ////{
        ////    return View();
        ////}

        //// GET: Return/Details/5
        //public ActionResult Details(int id = 0)
        //{
        //   var qid = GpgDatabase.Default.Return.Find(id);
        //    return View(qid);
        //}


        //// POST: Return/Create
        //[HttpPost]
        //public ActionResult Create(Return model)
        //{
        //    try
        //    {


        //        // TODO: Add insert logic here

        //        GpgDatabase.Default.Return.Add(model);
        //        GpgDatabase.Default.SaveChanges();

        //        return RedirectToAction("Index");
        //    }
        //    catch(Exception e)
        //    {
        //        return View(e);
        //    }
        //}

        //// GET: Return/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Return/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Return/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Return/Delete/5
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
