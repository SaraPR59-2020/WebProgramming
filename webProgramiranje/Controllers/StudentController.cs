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
        private readonly JsonFileService<Student> _studenti; // pretpostavljajući da imate osnovni User model
        private readonly JsonFileService<Ispit> _ispiti; // pretpostavljajući da imate osnovni User model
        private readonly JsonFileService<RezultatIspita> _rezultatiIspita; // pretpostavljajući da imate osnovni User model


        public StudentController()
        {
            _studenti = new JsonFileService<Student>("studenti.json");
            _ispiti = new JsonFileService<Ispit>("ispiti.json");
            _rezultatiIspita = new JsonFileService<RezultatIspita>("rezultatiIspita.json");
        }

        // GET: Student
        public ActionResult Index()
        {
            var student = _studenti.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));
            var mojiIspiti = student.ListaIspita;
            return View(mojiIspiti);
        }

        public ActionResult PrijaviIspit()
        {
            // Pretpostavka je da su ne prijavljeni ispiti oni koji nemaju rezultat za tog studenta.
            var student = _studenti.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));
            var prijavljeniIspiti = student.ListaIspita.Select(i => i.Ispit).ToList();

            var sviIspiti = _ispiti.ReadFromFile();
            var neprijavljeniIspiti = sviIspiti != null ? sviIspiti.Where(i => !prijavljeniIspiti.Contains(i) && i.DatumIVremeOdrzavanja > DateTime.Now).ToList() : new List<Ispit>();

            return View(neprijavljeniIspiti);
        }

        [HttpPost]
        public ActionResult PrijaviIspit(int id)
        {
            Student student = _studenti.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));
            var ispitZaPrijavu = _ispiti.ReadFromFile().FirstOrDefault(i => i.Id == id);

            if (ispitZaPrijavu != null && ispitZaPrijavu.DatumIVremeOdrzavanja > DateTime.Now)
            {
                var noviRezultat = new RezultatIspita { Ispit = ispitZaPrijavu, Student = student.KorisnickoIme, Ocena = 5 }; // Početna ocena je 5 kao nepoložen
                student.ListaIspita.Add(noviRezultat);
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