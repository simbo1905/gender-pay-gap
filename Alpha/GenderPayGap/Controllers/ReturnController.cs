using GenderPayGap.Models;
using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GenderPayGap.Controllers
{
    [Authorize]
    public class ReturnController : BaseController
    {
        //Get: Return
        [HttpGet]
        public ActionResult Index()
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            var currentUser = GetCurrentUser();
            var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            var model = GpgDatabase.Default.Return.FirstOrDefault(r => r.OrganisationId == userOrg.UserId);
            if (model==null)model = new Return();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(Return @return)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            return View(@return);
        }

        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create(Return @return)
        {
            if (!Authorise())
                return RedirectToAction("Index", "Register");

            return View(@return);
        }

        [HttpPost]
        public ActionResult Authoriser(Return @return)
        {
            if (!Authorise())
                return RedirectToAction("Index", "Register");

            return View(@return);
        }

        [HttpPost]
        public ActionResult DataConfirm(Return @return)
        {
            if (!Authorise())
                return RedirectToAction("Index", "Register");

            return View(@return);
        }

        [HttpGet]
        public ActionResult SendConfirmed()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendConfirmed(Return @return)
        {
            if (@return.ReturnId == 0)
            {
                var currentUser = GetCurrentUser();
                var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(uo => uo.UserId == currentUser.UserId);
                @return.OrganisationId = userOrg.OrganisationId;
                GpgDatabase.Default.Return.Add(@return);
            }
            @return.AccountingDate = DateTime.Now;
            GpgDatabase.Default.SaveChanges();
            return RedirectToAction("SendConfirmed");
        }

        // GET: Return/Details/5
        public ActionResult DataConfirm(int id = 1)
        {
            var qid = GpgDatabase.Default.Return.Find(id);
            return View(qid);
        }

        public ActionResult Details(int id /*= 1*/)
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
        //public ActionResult Authoriser()
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
