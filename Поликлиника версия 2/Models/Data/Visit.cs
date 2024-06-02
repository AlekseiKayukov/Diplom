using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Polyclinic.Models.data
{
    [Table("Visit")]
    public partial class Visit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime DateVisit { get; set; }

        public DateTime TimeVisit { get; set; }

        public string Complaint { get; set; }

        public string Appointments { get; set; }

        public long? IdDiagnosis { get; set; }

        public Diagnosis Diagnosis { get; set; }

        public long? IdRecord { get; set; }

        public Record Record { get; set; }

        public string Status { get; set; }

        public static implicit operator List<object>(Visit v)
        {
            throw new NotImplementedException();
        }
    }
}