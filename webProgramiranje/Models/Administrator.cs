﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace webProgramiranje.Models
{
    public class Administrator
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Korisničko ime")]
        [Key]
        public string KorisnickoIme { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Šifra")]
        public string Sifra { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Ime")]
        public string Ime { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Prezime")]
        public string Prezime { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Datum rođenja")]
        public DateTime DatumRodjenja { get; set; }
    }
}