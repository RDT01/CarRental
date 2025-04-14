using System.Linq;
using System.Web.Mvc;
using Project1.Models;

namespace Project1.Controllers
{
    public class HomeController : Controller
    {
        private VehiclesContext db = new VehiclesContext();

        public ActionResult Index(string searchString)
        {
            // Render the full page with vehicles
            if (string.IsNullOrEmpty(searchString))
            {
                var vehicles = db.Vehicles.ToList();
                return View(vehicles);
            }

            // If a search term is provided, return filtered vehicles as JSON
            var filteredVehicles = db.Vehicles
                                      .Where(v => v.Make.Contains(searchString) || v.Location.Contains(searchString))
                                      .ToList();
            return Json(filteredVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}
