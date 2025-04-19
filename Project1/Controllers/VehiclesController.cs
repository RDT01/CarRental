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
using CsvHelper.Configuration.Attributes;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace Project1.Controllers
{
    public class VehiclesController : Controller
    {
        private VehiclesContext db = new VehiclesContext(); // Database context instance

        public ActionResult Index(string searchString)
        {
            var vehicles = db.Vehicles.AsQueryable();

            // If a search term is provided, filter by Make or Model
            if (!String.IsNullOrEmpty(searchString))
                vehicles = vehicles.Where(v => v.Make.Contains(searchString) || v.Model.Contains(searchString));

            return View(vehicles.ToList());
        }

        [HttpGet]
        public JsonResult SearchVehicles(string searchString, int page = 1, int pageSize = 20)
        {
            var query = db.Vehicles.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(v => v.Make.Contains(searchString) || v.Model.Contains(searchString));

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var vehicles = query
                .OrderBy(v => v.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new
                {
                    v.Id,
                    v.Host,
                    v.Make,
                    v.Model,
                    v.Price,
                    v.ImagePath
                })
                .ToList();

            return Json(new
            {
                vehicles,
                currentPage = page,
                totalPages = totalPages
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var vehicle = db.Vehicles.Find(id);
            if (vehicle == null) return HttpNotFound();

            return View(vehicle);
        }

        [AdminAuthorize]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult Create(Vehicles vehicle, HttpPostedFileBase VehicleImage)
        {
            if (ModelState.IsValid)
            {
                // Save uploaded image file
                if (VehicleImage != null && VehicleImage.ContentLength > 0)
                {
                    string imagesDirectory = Server.MapPath("~/Images");
                    if (!Directory.Exists(imagesDirectory)) Directory.CreateDirectory(imagesDirectory);

                    string fileName = "image_" + Guid.NewGuid() + Path.GetExtension(VehicleImage.FileName);
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

        [AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var vehicle = db.Vehicles.Find(id);
            if (vehicle == null) return HttpNotFound();

            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult Edit(int id, [Bind(Include = "Id,Host,Make,Model,Price,Mileage,Gas,Doors,Seats,ImagePath")] Vehicles vehicle, HttpPostedFileBase VehicleImage)
        {
            if (ModelState.IsValid)
            {
                // Replace existing image if new one is uploaded
                if (VehicleImage != null && VehicleImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(VehicleImage.FileName);
                    string extension = Path.GetExtension(VehicleImage.FileName);
                    fileName += "_" + Guid.NewGuid().ToString().Substring(0, 8) + extension;

                    string imagesDirectory = Server.MapPath("~/Images");
                    if (!Directory.Exists(imagesDirectory)) Directory.CreateDirectory(imagesDirectory);

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

        [AdminAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var vehicle = db.Vehicles.Find(id);
            if (vehicle == null) return HttpNotFound();

            return View(vehicle);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult DeleteConfirmed(int id)
        {
            var vehicle = db.Vehicles.Find(id);
            db.Vehicles.Remove(vehicle);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [AdminAuthorize]
        public ActionResult Import() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult PreviewImport(HttpPostedFileBase csvFile)
        {
            if (csvFile == null || csvFile.ContentLength == 0 ||
                !Path.GetExtension(csvFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Please upload a valid .csv file.");
                return View();
            }

            var headers = new List<string>();
            var rows = new List<List<string>>();
            string tempPath = Path.Combine(Server.MapPath("~/App_Data"), "csv_preview_" + Guid.NewGuid() + ".csv");

            // Copy uploaded file to temp location for later re-use
            using (var fileStream = System.IO.File.Create(tempPath))
            {
                csvFile.InputStream.Seek(0, SeekOrigin.Begin);
                csvFile.InputStream.CopyTo(fileStream);
            }

            using (var reader = new StreamReader(tempPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    Delimiter = ","
                };

                using (var csv = new CsvReader(reader, config))
                {
                    if (csv.Read())
                    {
                        csv.ReadHeader();
                        headers = csv.HeaderRecord?.ToList() ?? new List<string>();
                    }

                    // Read only the first 5 rows for preview
                    int previewCount = 0;
                    while (csv.Read() && previewCount < 5)
                    {
                        var row = new List<string>();
                        for (int i = 0; i < headers.Count; i++)
                        {
                            row.Add(csv.GetField(i));
                        }
                        rows.Add(row);
                        previewCount++;
                    }
                }
            }

            var model = new CsvPreviewViewModel
            {
                Headers = headers,
                PreviewRows = rows,
                TempFilePath = tempPath
            };

            TempData["CsvFilePath"] = tempPath; // Save temp file path for use in next step

            return View("PreviewImport", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult ImportMappedCsv(FormCollection form)
        {
            System.Diagnostics.Debug.WriteLine("ImportMappedCsv hit");

            var mappings = Request.Form.GetValues("ColumnMappings")?.ToList();
            var filePath = TempData["CsvFilePath"] as string;

            System.Diagnostics.Debug.WriteLine("Mappings:");
            if (mappings != null)
            {
                for (int i = 0; i < mappings.Count; i++)
                    System.Diagnostics.Debug.WriteLine($"[{i}] {mappings[i]}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Mappings is null");
            }

            System.Diagnostics.Debug.WriteLine($"FilePath: {filePath}");
            System.Diagnostics.Debug.WriteLine($"Exists: {System.IO.File.Exists(filePath)}");

            if (mappings == null || string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "CSV file or mappings were not provided.";
                return RedirectToAction("Import");
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                Delimiter = ","
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                var headers = csv.HeaderRecord.ToList();

                while (csv.Read())
                {
                    var vehicle = new Vehicles { Host = "Mapped CSV" };

                    for (int i = 0; i < mappings.Count; i++)
                    {
                        var field = mappings[i];
                        if (string.IsNullOrWhiteSpace(field)) continue;

                        try
                        {
                            string value = csv.GetField(i);

                            // Dynamically assign values based on mapping
                            switch (field)
                            {
                                case "Make": vehicle.Make = value; break;
                                case "Model": vehicle.Model = value; break;
                                case "Price": if (decimal.TryParse(value, out var price)) vehicle.Price = price; break;
                                case "Mileage": if (int.TryParse(value, out var mileage)) vehicle.Mileage = mileage; break;
                                case "Gas": vehicle.Gas = value; break;
                                case "Doors": if (int.TryParse(value, out var doors)) vehicle.Doors = doors; break;
                                case "Seats": if (int.TryParse(value, out var seats)) vehicle.Seats = seats; break;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing field {field}: {ex.Message}");
                        }
                    }

                    db.Vehicles.Add(vehicle);
                }
                System.Diagnostics.Debug.WriteLine($"Importing {db.Vehicles.Local.Count} vehicles");
                db.SaveChanges();
            }

            System.IO.File.Delete(filePath); // Clean up temp file
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AdminAuthorize]
        public JsonResult UploadImage(IEnumerable<HttpPostedFileBase> VehicleImages)
        {
            List<string> uploadedPaths = new List<string>();
            string imagesDirectory = Server.MapPath("~/Images");
            if (!Directory.Exists(imagesDirectory)) Directory.CreateDirectory(imagesDirectory);

            if (VehicleImages != null)
            {
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
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

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
            if (string.IsNullOrEmpty(fileName)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string filePath = Path.Combine(Server.MapPath("~/Images"), fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        [HttpPost]
        public ActionResult StartCheckout(List<Vehicles> cart)
        {
            if (cart == null || !cart.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Session["Cart"] = cart; // Save cart to session
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Checkout()
        {
            var cart = Session["Cart"] as List<Vehicles>;
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(FormCollection form)
        {
            return RedirectToAction("ConfirmCheckout");
        }

        public ActionResult ConfirmCheckout() => View();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // Used for displaying finalized rental info
        public class RentalItem
        {
            public int Id { get; set; }
            public string Make { get; set; }
            public string Model { get; set; }
            public decimal Price { get; set; }
            public DateTime RentalDate { get; set; }
        }

        // Represents vehicle from CSV file using headers
        public class CsvVehicle
        {
            [Name("Make Name")] public string Make { get; set; }
            [Name("Model Name")] public string Model { get; set; }
            [Name("Trim Msrp")] public decimal Price { get; set; }
            [Name("Body Doors")] public int? Doors { get; set; }
            [Name("Body Seats")] public int? Seats { get; set; }
            [Name("Mileage Combined Mpg")] public int? Mileage { get; set; }
            [Name("Engine Fuel Type")] public string Gas { get; set; }
        }

        // CsvHelper mapping class
        public sealed class CsvVehicleMap : ClassMap<CsvVehicle>
        {
            public CsvVehicleMap()
            {
                Map(m => m.Make).Name("Make Name");
                Map(m => m.Model).Name("Model Name");
                Map(m => m.Price).Name("Trim Msrp");
                Map(m => m.Doors).Name("Body Doors");
                Map(m => m.Seats).Name("Body Seats");
                Map(m => m.Mileage).Name("Mileage Combined Mpg");
                Map(m => m.Gas).Name("Engine Fuel Type");
            }
        }
    }
}
