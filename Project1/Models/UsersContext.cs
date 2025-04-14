using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Project1.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext() : base("UsersDBConnection") { }
        public DbSet<Users> Users { get; set; }
    }
}