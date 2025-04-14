using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Project1.Models
{

    [Table("UserTbl")]
    public class Users
    {

        [Key]
        public int Id { get; set; }

        [Required]

        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(100)]
        public string Address { get; set; }
        [StringLength(15)]
        public string Phone { get; set; }
        [StringLength(100)]
        public string PasswordHash { get; set; }
        [StringLength(100)]
        public string Role { get; set; }

    }
}