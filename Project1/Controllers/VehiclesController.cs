using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Project1.Models;
using Project1.Filters;

namespace Project1.Controllers
{
    public class VehiclesController : Controller
    {
        private VehiclesContext db = new VehiclesContext();

        // GET: Vehicles
        public ActionResult Index(string searchString)
        {
            var vehicles = db.Vehicles.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(v => v.Make.Contains(searchString) || v.Location.Contains(searchString));
            }

            return View(vehicles.ToList());
        }

        // AJAX Search Handler
        [HttpGet]
        public JsonResult SearchVehicles(string searchString)
        {
            var results = db.Vehicles
                .Where(v => v.Make.Contains(searchString) || v.Location.Contains(searchString))
                .Select(v => new
                {
                    v.Id,
                    v.Host,
                    v.Make,
                    v.Location,
                    v.Price,
                    v.ImagePath
                })
                .ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }


        // GET: Vehicles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicles vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        // GET: Vehicles/Create
        [AdminAuthorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Vehicles vehicle, HttpPostedFileBase VehicleImage)
        {
            if (ModelState.IsValid)
            {
                if (VehicleImage != null && VehicleImage.ContentLength > 0)
                {
                    string imagesDirectory = Server.MapPath("~/Images");
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }

                    string fileName = "image_" + Guid.NewGuid().ToString() + Path.GetExtension(VehicleImage.FileName);
                    string path = Path.Combine(imagesDirectory, fileName);

                    VehicleImage.SaveAs(path);
                    vehicle.ImagePath = "Images/" + fileName;
                }

                db.Vehicles.Add(vehicle);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        [AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicles vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "Id,Host,Make,Location,Price,Mileage,Gas,Doors,Seats,ImagePath")] Vehicles vehicle, HttpPostedFileBase VehicleImage)
        {
            if (ModelState.IsValid)
            {
                if (VehicleImage != null && VehicleImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(VehicleImage.FileName);
                    string extension = Path.GetExtension(VehicleImage.FileName);
                    fileName = fileName + "_" + Guid.NewGuid().ToString().Substring(0, 8) + extension;

                    string imagesDirectory = Server.MapPath("~/Images");
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }

                    string path = Path.Combine(imagesDirectory, fileName);
                    VehicleImage.SaveAs(path);

                    vehicle.ImagePath = "Images/" + fileName;
                }

                db.Entry(vehicle).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        [AdminAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicles vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vehicles vehicle = db.Vehicles.Find(id);
            db.Vehicles.Remove(vehicle);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ==========================
        // AJAX File Upload Methods
        // ==========================

        [HttpPost]
        [AdminAuthorize]
        public JsonResult UploadImage(IEnumerable<HttpPostedFileBase> VehicleImages)
        {
            List<string> uploadedPaths = new List<string>();

            if (VehicleImages != null)
            {
                string imagesDirectory = Server.MapPath("~/Images");
                if (!Directory.Exists(imagesDirectory))
                {
                    Directory.CreateDirectory(imagesDirectory);
                }

                foreach (var file in VehicleImages)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(imagesDirectory, fileName);
                        file.SaveAs(filePath);
                        uploadedPaths.Add("/Images/" + fileName);
                    }
                }
            }

            return Json(uploadedPaths);
        }



        [HttpGet]
        public JsonResult ListFiles()
        {
            string imagesDirectory = Server.MapPath("~/Images");
            if (!Directory.Exists(imagesDirectory))
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var files = Directory.GetFiles(imagesDirectory)
                .Select(filePath => new
                {
                    FileName = Path.GetFileName(filePath),
                    DownloadUrl = Url.Content("~/Images/" + Path.GetFileName(filePath))
                })
                .ToList();

            return Json(files, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AdminAuthorize]
        public ActionResult DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string filePath = Path.Combine(Server.MapPath("~/Images"), fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult StartCheckout(List<Vehicles> cart)
        {
            if (cart == null || !cart.Any())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Session["Cart"] = cart;
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        // GET: Vehicles/Checkout
        public ActionResult Checkout()
        {
            var cart = Session["Cart"] as List<Vehicles>;
            return View(cart);
        }

        // POST: Vehicles/Checkout (Redirects to confirmation)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(FormCollection form)
        {
            return RedirectToAction("ConfirmCheckout");
        }

        // GET: Vehicles/ConfirmCheckout
        public ActionResult ConfirmCheckout()
        {
            return View(); // Just show the confirmation page
        }

        // Rental model (create this in your Models folder)
        public class RentalItem
        {
            public int Id { get; set; }
            public string Make { get; set; }
            public string Location { get; set; }
            public decimal Price { get; set; }
            public DateTime RentalDate { get; set; }
        }

    }
}
