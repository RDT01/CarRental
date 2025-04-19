using Project1.Models;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace Project1.Services
{
    public class CarApiUpdater
    {
        public class CarApiRow
        {
            [Name("Make Name")]
            public string Make_Name { get; set; }

            [Name("Body Doors")]
            public int Body_Doors { get; set; }

            [Name("Body Seats")]
            public int Body_Seats { get; set; }

            [Name("Trim Msrp")]
            public string Trim_Msrp { get; set; }
        }

        public static void UpdateVehiclePrices(VehiclesContext db)
        {
            var filePath = HostingEnvironment.MapPath("~/App_Data/carapi-opendatafeed-sample.csv");
            if (!File.Exists(filePath)) return;

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var apiRows = csv.GetRecords<CarApiRow>().ToList();
                var vehicles = db.Vehicles.ToList();

                foreach (var vehicle in vehicles)
                {
                    var match = apiRows.FirstOrDefault(r =>
                        !string.IsNullOrWhiteSpace(r.Make_Name) &&
                        r.Make_Name.Trim().ToLower() == vehicle.Make.Trim().ToLower() &&
                        r.Body_Doors == vehicle.Doors &&
                        r.Body_Seats == vehicle.Seats
                    );

                    if (match != null && decimal.TryParse(match.Trim_Msrp, out decimal parsedPrice))
                    {
                        vehicle.Price = parsedPrice;
                    }
                }

                db.SaveChanges();
            }
        }
    }
}
