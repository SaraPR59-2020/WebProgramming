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
            return View(allStudents);
        }

        public ActionResult CreateStudent()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateStudent(Student student)
        {
            var allStudents = _students.ReadFromFile().ToList();

            if (allStudents.Any(s => s.ElektronskaPosta == student.ElektronskaPosta ||
                                     s.KorisnickoIme == student.KorisnickoIme ||
                                     s.BrojIndeksa == student.BrojIndeksa))
            {
                ModelState.AddModelError("", "Student sa ovom email adresom, korisničkim imenom ili brojem indeksa već postoji.");
                return View(student);
            }

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
    }
}