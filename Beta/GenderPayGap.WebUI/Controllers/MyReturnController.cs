using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using GenderPayGap.Models;
using GenderPayGap.Models.GPGEntityModel;

namespace GenderPayGap.Controllers
{
    public class MyReturnController : Controller
    {
        //private MyDbContext db = new MyDbContext();
        private GpgDBEntities db = new GpgDBEntities();

        // GET: MyReturn
        public ActionResult Index()
        {
            return View();
        }

        // GET: MyReturn/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MyReturn/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MyReturn/Create
        [HttpPost]
        public ActionResult Create(Return myreturn)
        {
            try
            {
                db.Return.Add(myreturn);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception:" + e.InnerException + " " + "e.Message:" + e.Message);
                return View();
            }
        }

        // GET: MyReturn/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MyReturn/Edit/5
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
                return View();
            }
        }

        // GET: MyReturn/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MyReturn/Delete/5
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
                return View();
            }
        }
    }
}
