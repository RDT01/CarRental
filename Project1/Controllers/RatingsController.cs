using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Project1.Filters;
using Project1.Models;

namespace Project1.Controllers
{
    public class RatingsController : Controller
    {
        private RatingsContext db = new RatingsContext();

        // GET: Ratings
        public ActionResult Index()
        {
            var ratings = db.Ratings.ToList();
            return View(ratings);
        }

        // GET: Ratings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ratings ratings = db.Ratings.Find(id);
            if (ratings == null)
            {
                return HttpNotFound();
            }
            return View(ratings);
        }

        // GET: Ratings/Create
        [AdminAuthorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Ratings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Cleanliness,Maintenance,Communication,Accuracy,Feedback")] Ratings ratings)
        {
            if (ModelState.IsValid)
            {
                db.Ratings.Add(ratings);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ratings);
        }

        // GET: Ratings/Edit/5
        [AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ratings ratings = db.Ratings.Find(id);
            if (ratings == null)
            {
                return HttpNotFound();
            }
            return View(ratings);
        }

        // POST: Ratings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Cleanliness,Maintenance,Communication,Accuracy,Feedback")] Ratings ratings)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ratings).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ratings);
        }

        // GET: Ratings/Delete/5
        [AdminAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ratings ratings = db.Ratings.Find(id);
            if (ratings == null)
            {
                return HttpNotFound();
            }
            return View(ratings);
        }

        // POST: Ratings/Delete/5
        [HttpPost, ActionName("Delete")]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ratings ratings = db.Ratings.Find(id);
            db.Ratings.Remove(ratings);
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
