using System.Linq;
using System.Web.Mvc;
using Project1.Models;
using Project1.Services; 

namespace Project1.Controllers
{
    public class HomeController : Controller
    {
        private VehiclesContext db = new VehiclesContext();

        public ActionResult Index(string searchString, int page = 1, int pageSize = 20)
        {
            CarApiUpdater.UpdateVehiclePrices(db);

            if (string.IsNullOrEmpty(searchString))
            {
                var vehicles = db.Vehicles.ToList();
                return View(vehicles);
            }

            var query = db.Vehicles
                          .Where(v => v.Make.Contains(searchString) || v.Model.Contains(searchString));

            var totalCount = query.Count();
            var totalPages = (int)System.Math.Ceiling((double)totalCount / pageSize);

            var pagedResults = query
                                .OrderBy(v => v.Id)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .Select(v => new
                                {
                                    v.Id,
                                    v.Make,
                                    v.Model,
                                    v.Price,
                                    v.ImagePath
                                })
                                .ToList();

            return Json(new { vehicles = pagedResults, currentPage = page, totalPages = totalPages }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            return View();
        }
        public ActionResult Questions()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
    }
}
