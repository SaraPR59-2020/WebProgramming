using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace webProgramiranje.Models
{
    public class Ispit
    {
        public int Id { get; set; }

        [Required]
        public string Profesor { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Predmet")]
        public string Predmet { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum i vreme održavanja")]
        public DateTime DatumIVremeOdrzavanja { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Naziv učionice")]
        public string NazivUcionice { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Naziv ispitnog roka")]
        public string NazivIspitnogRoka { get; set; }
    }
}