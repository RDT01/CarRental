using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Project1.Models
{
    public class RatingsContext : DbContext
    {
        public RatingsContext() : base("RatingsDBConnection") { }
        public DbSet<Ratings> Ratings { get; set; }
    }
}