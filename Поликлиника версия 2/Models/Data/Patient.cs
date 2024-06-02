using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Polyclinic.Models.data
{
    [Table("Patient")]
    public partial class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string IdUser { get; set; }

        public AspNetUser AspNetUser { get; set; }

        public string Surname { get; set; }

        public string Name { get; set; }

        public string Forename { get; set; }

        public string PlaceResidence { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public DateTime DateOfBirth { get; set; }

        public long? IdGender { get; set; }

        public Gender Gender { get; set; }

        public long? NumberCard { get; set; }

        public string MHIpolicy { get; set; }

        public string VMIpolicy { get; set; }

        public string PassportSeries { get; set; }

        public string PassportNumber { get; set; }

        public DateTime DateOfIssue { get; set; }

        public ICollection<Record> Records { get; set; }

        public string PatientFullName 
        {
            get {return $"{Surname} {Name} {Forename}";}
        }
    }
}