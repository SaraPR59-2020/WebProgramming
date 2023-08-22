using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using webProgramiranje.DB;
using webProgramiranje.Models;

namespace webProgramiranje.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        private readonly JsonFileService<Student> _students;

        public AdminController()
        {
            _students = new JsonFileService<Student>("studenti.json");
        }

        // GET: Admin
        public ActionResult Index()
        {
            var allStudents = _students.ReadFromFile();
            var std = allStudents != null ? allStudents.ToList() : new List<Student>() ;
            return View(std);
        }

        public ActionResult CreateStudent()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateStudent(Student student)
        {
            var allStudents = _students.ReadFromFile()!=null? _students.ReadFromFile().ToList() : new List<Student>();

            if (allStudents.Any(s => s.ElektronskaPosta == student.ElektronskaPosta ||
                                     s.KorisnickoIme == student.KorisnickoIme ||
                                     s.BrojIndeksa == student.BrojIndeksa))
            {
                ModelState.AddModelError("", "Student sa ovom email adresom, korisničkim imenom ili brojem indeksa već postoji.");
                return View(student);
            }
            student.ListaIspita = new List<int>();
            allStudents.Add(student);
            _students.WriteToFile(allStudents);
            return RedirectToAction("Index");
        }

        public ActionResult EditStudent(string username)
        {
            var student = _students.ReadFromFile().FirstOrDefault(s => s.KorisnickoIme == username);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        [HttpPost]
        public ActionResult EditStudent(Student student)
        {
            var allStudents = _students.ReadFromFile();
            var existingStudent = allStudents.FirstOrDefault(s => s.KorisnickoIme == student.KorisnickoIme);

            if (existingStudent == null)
            {
                return HttpNotFound();
            }

            if (allStudents.Any(s => s.ElektronskaPosta == student.ElektronskaPosta && s.KorisnickoIme != student.KorisnickoIme))
            {
                ModelState.AddModelError("Email", "Ova email adresa već postoji.");
                return View(student);
            } if (allStudents.Any(s => s.BrojIndeksa == student.BrojIndeksa && s.KorisnickoIme != student.KorisnickoIme))
            {
                ModelState.AddModelError("BrojIndeksa", "Ovaj BrojIndeksa već postoji.");
                return View(student);
            }

            existingStudent.Ime = student.Ime;
            existingStudent.ElektronskaPosta = student.ElektronskaPosta;
            existingStudent.Prezime = student.Prezime;
            existingStudent.DatumRodjenja = student.DatumRodjenja;

            _students.WriteToFile(allStudents);
            return RedirectToAction("Index");
        }

        public ActionResult DeleteStudent(string username)
        {
            var allStudents = _students.ReadFromFile().ToList();
            var student = allStudents.FirstOrDefault(s => s.KorisnickoIme == username);

            if (student == null)
            {
                return HttpNotFound();
            }

            allStudents.Remove(student);
            _students.WriteToFile(allStudents);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult FilterAndSort(string imeFilter, string prezimeFilter, string indeksFilter, string sortCriteria, string sortDirection)
        {
            // Dobavljanje svih studenata
            var studenti = _students.ReadFromFile() !=null? _students.ReadFromFile().ToList() :  new List<Student>(); 

            // Filtriranje
            if (!string.IsNullOrEmpty(imeFilter))
            {
                studenti = studenti.Where(s => s.Ime.ToLower().Contains(imeFilter.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(prezimeFilter))
            {
                studenti = studenti.Where(s => s.Prezime.ToLower().Contains(prezimeFilter.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(indeksFilter))
            {
                studenti = studenti.Where(s => s.BrojIndeksa.ToLower().Contains(indeksFilter.ToLower())).ToList();
            }

            // Sortiranje
            switch (sortCriteria)
            {
                case "Ime":
                    studenti = (sortDirection == "asc") ? studenti.OrderBy(s => s.Ime).ToList() : studenti.OrderByDescending(s => s.Ime).ToList();
                    break;

                case "Prezime":
                    studenti = (sortDirection == "asc") ? studenti.OrderBy(s => s.Prezime).ToList() : studenti.OrderByDescending(s => s.Prezime).ToList();
                    break;

                case "BrojIndeksa":
                    studenti = (sortDirection == "asc") ? studenti.OrderBy(s => s.BrojIndeksa).ToList() : studenti.OrderByDescending(s => s.BrojIndeksa).ToList();
                    break;
            }

            // Vraćanje rezultata
            return View("Index", studenti);
        }


    }
}