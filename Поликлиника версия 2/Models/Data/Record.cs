using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Polyclinic.Models.data
{
    [Table("Record")]
    public partial class Record
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? IdSchedule { get; set; }

        public Schedule Schedule { get; set; }

        public long? IdPatient { get; set; }

        public Patient Patient { get; set; }

        public ICollection<Visit> Visits { get; set; }

        public string CabinetWorkTime 
        {
            get {return $"{Schedule.Cabinet} ({Schedule.TimeStart.ToShortTimeString()}-{Schedule.TimeEnd.ToShortTimeString()})";}
        }
    }
}