using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Polyclinic.Models.data
{
    [Table("Doctor")]
    public partial class Doctor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Surname { get; set; }

        public string Name { get; set; }

        public string Forename { get; set; }

        public string Phone { get; set; }

        public long? IdGender { get; set; }

        public Gender Gender { get; set; }

        public long? IdSpecialization { get; set; }

        public Specialization Specialization { get; set; }

        public string IdUser { get; set; }

        public AspNetUser AspNetUser { get; set; }

        public ICollection<Schedule> Schedules { get; set; }

        public string DoctorFullName
        {
            get {return $"{Surname} {Name} {Forename}";}
        }
    }
}