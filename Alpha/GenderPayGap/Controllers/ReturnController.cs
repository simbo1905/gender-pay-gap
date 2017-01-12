using Extensions;
using GenderPayGap.Models;
using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GenderPayGap.Controllers
{
    public class ReturnController : BaseController
    {
        //Get: Return
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            var currentUser = GetCurrentUser();
            var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(uo => uo.UserId == currentUser.UserId);
            var model = GpgDatabase.Default.Return.FirstOrDefault(r => r.OrganisationId == userOrg.OrganisationId);
            if (model == null) model = new Return();
            model.OrganisationId = userOrg.OrganisationId;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(Return model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            if (!ModelState.IsValid) return View(model);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Authoriser(Return model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            if (Request.UrlReferrer.PathAndQuery.ContainsI("Create") && string.IsNullOrWhiteSpace(model.FirstName) && string.IsNullOrWhiteSpace(model.LastName) && string.IsNullOrWhiteSpace(model.JobTitle))
                ModelState.Clear();

            if (!ModelState.IsValid)
                return View(model);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Confirm(Return model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");
            if (!ModelState.IsValid) return View(model);
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public ActionResult SendConfirmed(long id = 0)
        {
            try
            {
                if (id < 1)
                {
                    return View(id);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            if (!Authorise()) return RedirectToAction("Index", "Register");
            return View(id);
        }

        [Authorize]
        [HttpPost]
        public ActionResult SendConfirmed(Return model)
        {
            if (!Authorise()) return RedirectToAction("Index", "Register");

            var original = GpgDatabase.Default.Return.Find(model.ReturnId);
            if (original == null)
            {
                var currentUser = GetCurrentUser();
                var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(uo => uo.UserId == currentUser.UserId);
                model.OrganisationId = userOrg.OrganisationId;
                GpgDatabase.Default.Return.Add(model);
            }
            else
            {
                GpgDatabase.Default.Entry(original).CurrentValues.SetValues(model);
            }
            model.Organisation = GpgDatabase.Default.Organisation.Find(model.OrganisationId);
            model.AccountingDate = DateTime.Now;
            try
            {
                GpgDatabase.Default.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            return RedirectToAction("SendConfirmed",new { id=model.ReturnId});
        }

        // GET: Return/Details/5
        public ActionResult Confirm(int id = 1)
        {
            var qid = GpgDatabase.Default.Return.Find(id);
            return View(qid);
        }

        public ActionResult Details(int id = 1)
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
