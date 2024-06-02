using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Polyclinic.Models.data
{
    [Table("AspNetUserRoles")]
    public partial class AspNetUserRole
    {
        [Key]
        [Required]
        public string UserId { get; set; }

        public AspNetUser AspNetUser { get; set; }

        [Key]
        [Required]
        public string RoleId { get; set; }

        public AspNetRole AspNetRole { get; set; }

    }
}