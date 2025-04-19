using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Project1.Models
{
    [Table("VehicleTbl")]
    public class Vehicles
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Host { get; set; }

        [StringLength(100)]
        public string Make { get; set; }

        [StringLength(100)]
        public string Model { get; set; }

        public decimal Price { get; set; }
        public int Mileage { get; set; }

        [StringLength(100)]
        public string Gas { get; set; }

        public int Doors { get; set; }
        public int Seats { get; set; }

        [StringLength(255)]
        public string ImagePath { get; set; }
    }
}


