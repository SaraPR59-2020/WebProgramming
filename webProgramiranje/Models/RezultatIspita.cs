using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace webProgramiranje.Models
{
    public class RezultatIspita
    {
        [Required]
        public Ispit Ispit { get; set; }

        [Required]
        public string Student { get; set; }

        [Required]
        [Range(5, 10)]
        [Display(Name = "Ocena")]
        public int Ocena { get; set; }
    }
}