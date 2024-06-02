using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Polyclinic.Models.data
{
    [Table("Schedule")]
    public partial class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime TimeStart { get; set; }

        public DateTime TimeEnd { get; set; }

        public long? Duration { get; set; }

        public string Cabinet { get; set; }

        public long? IdDoctor { get; set; }

        public Doctor Doctor { get; set; }

        public ICollection<Record> Records { get; set; }

        public string CabinetWorkTime 
        {
            get {return $"{Cabinet} ({TimeStart.ToShortTimeString()}-{TimeEnd.ToShortTimeString()})";}
        }
        public string CabinetAndDoctor
        {
            get {return $"{Cabinet} ({TimeStart.ToShortTimeString()}-{TimeEnd.ToShortTimeString()}) {Doctor.Surname} {Doctor.Name}";}
        }
    }
}