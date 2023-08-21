using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using webProgramiranje.DB;
using webProgramiranje.Models;

namespace webProgramiranje.Controllers
{
    public class StudentController : Controller
    {
        private readonly JsonFileService<Student> _studenti;
        private readonly JsonFileService<Ispit> _ispiti;
        private readonly JsonFileService<RezultatIspita> _rezultati;



        public StudentController()
        {
            _studenti = new JsonFileService<Student>("studenti.json");
            _ispiti = new JsonFileService<Ispit>("ispiti.json");
            _rezultati = new JsonFileService<RezultatIspita>("rezultatiIspita.json");
        }

        // GET: Student
        public ActionResult Index()
        {
            var student = _studenti.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));
            var rez = _rezultati.ReadFromFile();
            var mojiIspiti = rez != null ? rez.Where(r => student.ListaIspita.Contains(r.Id)).ToList() : new List<RezultatIspita>();
            return View(mojiIspiti);
        }

        public ActionResult PrijaviIspit()
        {
            var student = _studenti.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));

            // Ovde izdvajamo samo Id-jeve prijavljenih ispita za tog studenta
            var prijavljeniIspitiIds = _rezultati.ReadFromFile() != null ? _rezultati.ReadFromFile().Where(r => student.ListaIspita.Contains(r.Id)).Select(r => r.Ispit.Id).ToList() : new List<int>();

            var sviIspiti = _ispiti.ReadFromFile();

            // Filtriramo s obzirom na Id, umesto na celokupne objekte
            var neprijavljeniIspiti = sviIspiti != null
                ? sviIspiti.Where(i => !prijavljeniIspitiIds.Contains(i.Id) && i.DatumIVremeOdrzavanja > DateTime.Now).ToList()
                : new List<Ispit>();

            return View(neprijavljeniIspiti);
        }
        [HttpPost]
        public ActionResult PrijaviIspit(int id)
        {
            Student student = _studenti.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));
            var ispitZaPrijavu = _ispiti.ReadFromFile().FirstOrDefault(i => i.Id == id);

            if (ispitZaPrijavu != null && ispitZaPrijavu.DatumIVremeOdrzavanja > DateTime.Now)
            {
                var noviRezultat = new RezultatIspita { Ispit = ispitZaPrijavu, Student = student, Ocena = 5 }; // Početna ocena je 5 kao nepoložen
                var rez = _rezultati.ReadFromFile() != null ? _rezultati.ReadFromFile().ToList() : new List<RezultatIspita>();
                noviRezultat.Id = rez.Count;
                rez.Add(noviRezultat);
                _rezultati.WriteToFile(rez);
                student.ListaIspita.Add(noviRezultat.Id);
                List<Student> studenti = _studenti.ReadFromFile().Where(s => s.KorisnickoIme != student.KorisnickoIme).ToList();
                studenti.Add(student);
                _studenti.WriteToFile(studenti);
                TempData["SuccessMessage"] = "Uspešno ste prijavili ispit!";
            }
            else
            {
                TempData["ErrorMessage"] = "Greška prilikom prijave ispita!";
            }

            return RedirectToAction("PrijaviIspit");
        }

        [HttpPost]
        public ActionResult FilterAndSortExams(string rokFilter, string predmetFilter, string ocenaFilter, string sortCriteria, string sortOrder)
        {
            var ispiti = _rezultati.ReadFromFile().Where(r => r.Student.Equals(HttpContext.Session["Username"])).ToList();  // Ovo je pretpostavljena metoda za dobavljanje ispita

            // Filtriranje
            if (!string.IsNullOrEmpty(rokFilter))
                ispiti = ispiti.Where(i => i.Ispit.NazivIspitnogRoka.ToLower().Contains(rokFilter.ToLower())).ToList();

            if (!string.IsNullOrEmpty(predmetFilter))
                ispiti = ispiti.Where(i => i.Ispit.Predmet.ToLower().Contains(predmetFilter.ToLower())).ToList();

            if (!string.IsNullOrEmpty(ocenaFilter) && int.TryParse(ocenaFilter, out int ocena))
                ispiti = ispiti.Where(i => i.Ocena == ocena).ToList();

            Func<RezultatIspita, object> orderFunc = i => i.Ispit.NazivIspitnogRoka; // Defaultni poredak

            switch (sortCriteria)
            {
                case "Rok":
                    orderFunc = i => i.Ispit.NazivIspitnogRoka;
                    break;

                case "Predmet":
                    orderFunc = i => i.Ispit.Predmet;
                    break;

                case "Ocena":
                    orderFunc = i => i.Ocena;
                    break;
            }

            if (sortOrder == "Asc")
            {
                ispiti = ispiti.OrderBy(orderFunc).ToList();
            }
            else
            {
                ispiti = ispiti.OrderByDescending(orderFunc).ToList();
            }

            return View("Index", ispiti);
        }


    }
}