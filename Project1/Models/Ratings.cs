using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Project1.Models
{
    [Table("RatingTbl")]
    public class Ratings
    {
        [Key]
        public int Id { get; set; }

        [Required]

        public int Cleanliness { get; set; }
        public int Maintenance { get; set; }
        public int Communication { get; set; }
        public int Accuracy { get; set; }
        [StringLength(999)]
        public string Feedback { get; set; }
    }
}