using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            var mojiIspiti = rez!=null? rez.Where(r => student.ListaIspita.Contains(r.Id)).ToList(): new List<RezultatIspita>();
            return View(mojiIspiti);
        }

        public ActionResult PrijaviIspit()
        {
            var student = _studenti.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));

            // Ovde izdvajamo samo Id-jeve prijavljenih ispita za tog studenta
            var prijavljeniIspitiIds = _rezultati.ReadFromFile()!=null?_rezultati.ReadFromFile().Where(r => student.ListaIspita.Contains(r.Id)).Select(r => r.Ispit.Id).ToList():new List<int>();

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

            if (ispitZaPrijavu != null && ispitZaPrijavu.DatumIVremeOdrzavanja > DateTime.Now )
            {
                var noviRezultat = new RezultatIspita { Ispit = ispitZaPrijavu, Student = student.KorisnickoIme, Ocena = 5 }; // Početna ocena je 5 kao nepoložen
                var rez = _rezultati.ReadFromFile()!= null?_rezultati.ReadFromFile().ToList(): new List<RezultatIspita>();
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


    }
}