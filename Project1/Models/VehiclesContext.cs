using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Project1.Models
{
    public class VehiclesContext:DbContext
    {
        public VehiclesContext() : base("VehiclesDBConnection") { }
        public DbSet<Vehicles> Vehicles { get; set; }
    }
}